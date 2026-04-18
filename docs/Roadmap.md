# 実装ロードマップ

対局ドメイン(`Mahjong.Lib.Game`)および周辺の実装段階を示す。各フェーズは原則として前フェーズの完了を前提とする。
設計の一次情報は [Design.md](Design.md) を参照。

## Phase 0: 既存実装 (完了済み)

- 牌: `Tile`(0-135) / `TileKind`(0-33) / `TileKindList`
- プレイヤー別配列: `HandArray` / `RiverArray` / `CallListArray` / `PointArray`
- 副露: `Call` / `CallList` / `CallType` (Chi / Pon / Ankan / Daiminkan / Kakan)
- 山: `Wall` / `IWallGenerator` / `WallGeneratorTenhou` (MT19937 + SHA512)
- 局集約: `Round` record と進行メソッド (`Haipai` / `Tsumo` / `Dahai` / `Chi` / `Pon` / `Daiminkan` / `Ankan` / `Kakan` / `RinshanTsumo` / `RevealDora` / `SetPoints` / `AddKyoutakuRiichi` / `ClearKyoutaku` / `NextTurn`)
- 局ステートマシン: `RoundState` / `RoundEvent` / `RoundStateContext` と各具象状態(Haipai / Tsumo / Dahai / Call / Kan / KanTsumo / AfterKanTsumo / Win / Ryuukyoku)

## Phase 1: 対局(Game)集約とステートマシンの器 (完了済み)

**目的**: 対局全体を統括する「器」を用意する。点数精算ロジックと局内プレイヤー状態は Phase 2 に分離する。通知/応答の仕組みはこの時点では未着手で、テストは`GameStateContext`の直接駆動で行う。

- ファイル配置 — `Mahjong.Lib.Game/Games/` フォルダ(既存の`Rounds/`と並列) / `Mahjong.Lib.Game/States/GameStates/`
- `GameRules` — 対局前に決まるルール。対局形式(東風戦/東南戦、既定:東南戦) / 赤ドラ枚数(既定:赤五萬1/赤五筒1/赤五索1) / 初期持ち点(当面35000) / オーラス親あがり止め(1位単独確定のみ) / トビ終了点(天鳳準拠:0点未満) / 食いタン / 後付け / 連荘条件 / `IsRedDora(Tile)` 判定メソッド / 対局形式 `GameFormat.Tonpuu` / `Tonnan`(既定) / `SingleRound`
- `Player` abstract record — プレイヤーの共通基底(`PlayerId` / `DisplayName`)。Phase 1 では識別情報のみ、Phase 4 で通知・応答メソッド(`OnHaipaiAsync` 等)を追加する。AI / 人間の実装は Phase 4 以降で `Player` を継承する
- `PlayerList` record — `ImmutableArray<Player>` のラッパー かつ `IEnumerable<Player>` 実装、`PlayerIndex`でアクセス。**index 0 が起家**という仕様。並び替え・起家決定は呼び出し側の責務
- `Game` record (immutable) — 対局全体の状態を直接フィールドで保持(`PlayerList` / `GameRules` / `RoundWind` / `RoundNumber` / `Honba` / `KyoutakuRiichiCount` / `PointArray`)。`GameConfig` のような中間コンテナは作らない(局順など対局中に変動する値と固定パラメータを同じrecordに混在させないため)
- 局終了イベント (`GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku`) のフィールドとして結果情報を保持(和了者 / 放銃者 / 和了種別 / 流局種別 / 連荘判定フラグ / 本場加算フラグ)。**`Round` record は変更しない**。Phase 2 以降でフィールドを拡張して役/符/翻/点数を追加
- `GameState` 抽象 + 具象 (`GameStateInit` / `GameStateRoundRunning` / `GameStateEnd`)。既存 `RoundStateXxx` の命名規約に揃える。Phase 1 では「局終了」を表す独立状態は持たず、局終了後の Game 更新・終了判定・次局決定は `GameStateRoundRunning.RoundEndedBy*` ハンドラ内で行い 直接 `GameStateRoundRunning`(次局)または `GameStateEnd` へ Transit する。プレイヤー向けの局終了通知(Phase 5)の実装時に必要なら `GameStateRoundEnd` 状態を再導入する
- `GameStateContext` — `Round*`系と同じ非同期イベント駆動。内部で`RoundStateContext`を生成・破棄。Phase 1時点のテストは既存`RoundStateContext`と同様に`ResponseXxxAsync`系の直接駆動
- `GameStateContext` と `RoundStateContext` の協調 — `GameStateRoundRunning.Entry` で `GameStateContext.StartRound(round)` を呼ぶ。`StartRound` は内部で RoundStateContext を生成し RoundStateChanged を購読登録してから Init を実行し、Init 完了後に `CurrentRoundContext` プロパティへ公開する(テストが初期化前の State を観測しないようにする)。購読ハンドラが `RoundStateWin` / `RoundStateRyuukyoku` を検知したら `NotifyRoundEndedAsync` で一度だけ内部イベント発行(多重抑止フラグ `roundEnded_` 付き)。Phase 5 で `RoundManager` 経由に差し替え予定
- `GameManager` — コンストラクタで`PlayerList` + `GameRules` + `IWallGenerator` を受け取る。対局開始処理・局間の引き継ぎ・対局終了判定を統括
- `RoundManager` / 通信・集約レイヤーは Phase 5 で導入。Phase 1 では未着手

**Phase 1 完了の定義**:

- テストから直接駆動で `GameStateInit` → `GameStateRoundRunning` → (Round 既存駆動で進行) → 次局 `GameStateRoundRunning` → ... → `GameStateEnd` の遷移が回る
- `GameRules` / `Game` / `PlayerList` / `GameState` / `GameStateContext` / `GameManager` が生成・破棄可能
- 点数精算・流局精算・連荘/本場処理は **スタブ**(固定値返却や手計算固定値)で十分。統合テストは状態遷移の流れのみ検証
- 対局終了条件(オーラス親あがり止め / トビ / 規定局数消化)の分岐が`GameEndPolicy.ShouldEndAfterRound`で評価される(実値判定ではなくフック点が存在するレベルで可)

## Phase 2: 点数精算と局内プレイヤー状態 (完了済み)

**目的**: Phase 1 の器に点数計算・流局精算・局内プレイヤー状態を結線する。

- `IScoreCalculator` 抽象 — 和了時の点数計算インタフェース。`Mahjong.Lib.Scoring`をラップする実装は上位層で用意(Lib.Game直接参照しない)。Phase 2では **仮実装**(固定値返却や手計算スタブ)で状態遷移テストを通す。本配線(Scoring 接続)は上位層の整備時に行う。具体IF(入出力型)は実装着手時に決める
- `PlayerRoundStatusArray` (仮) — `Round` 集約にプレイヤー別局内状態を追加。立直 / ダブル立直 / 一発可否 / 同巡フリテン / 永久フリテン / 門前 / 流し満貫資格 / 第一打前 / リンシャン中 等。合法候補列挙・和了判定・流局精算で必要
- `RoundState`の和了/流局時点数処理 — 和了時は`IScoreCalculator`経由で点数計算。流局時は天鳳ルール準拠でノーテン罰符・テンパイ料(荒牌平局:1人3000/2人1500・3000/3人1000・3000、途中流局は点数移動なし・本場+1)を計算。点数移動・供託処理・連荘/本場増加処理を行い`Round`を修正
- 連荘・本場・供託の局跨ぎ処理を`GameManager`に実装
- オーラス親あがり止め(1位単独確定のみ)の判定を`GameManager`に実装

**Phase 2 完了の定義**:

- テストからイベントを直接駆動して「東一〜オーラス親あがり止め」まで遷移できる
- 和了/流局/連荘/トビの各シナリオで点数が正しく動く(仮`IScoreCalculator`と天鳳ルール準拠の流局処理)
- 仮スコア計算器で統合テストが全パス

## Phase 3: 通知・応答の型定義 (完了済み)

**目的**: プレイヤー通知・応答の型体系を固める。型のみだが、Phase 4 の `Player` 通知メソッドの戻り値型は本フェーズの応答型に直接依存するため、Phase 4 の API 形状もあわせて想定して型設計する。

- C# API 層: `GameNotification`（対局レベル通知基底）/ `RoundNotification`（局内通知基底）+ 種別別具象 (`HaipaiNotification` / `TsumoNotification` / `OtherPlayerTsumoNotification` / `DahaiNotification` / `CallNotification` / `KanNotification` / `KanTsumoNotification` / `WinNotification` / `RyuukyokuNotification` / `DoraRevealNotification` / `GameStartNotification` / `GameEndNotification` / `RoundStartNotification` / `RoundEndNotification`)
- Wire DTO 層: `PlayerNotification`（共通 envelope）/ `PlayerResponseEnvelope`（応答 envelope）/ `ResponseBody` 抽象 + 具象 (`OkResponseBody` / `DahaiResponseBody` / `CallResponseBody` / `KanResponseBody` / `WinResponseBody` / `RyuukyokuResponseBody`)
- `PlayerResponse` 抽象 + **状態別中間抽象型** (`AfterDahaiResponse` / `AfterTsumoResponse` / `AfterKanResponse` / `AfterKanTsumoResponse`) + 具象 (`OkResponse` / `PassResponse` / `ChiResponse` / `PonResponse` / `DaiminkanResponse` / `RonResponse` / `DahaiResponse` / `AnkanResponse` / `KakanResponse` / `TsumoAgariResponse` / `KyuushuKyuuhaiResponse` / `ChankanRonResponse` / `KanPassResponse` / `RinshanTsumoResponse` / `KanTsumoAnkanResponse` / `KanTsumoDahaiResponse` / `KanTsumoKakanResponse`)
- `ResponseCandidate` 抽象 + 具象 (`OkCandidate` / `RonCandidate` / `ChiCandidate(HandTiles)` / `PonCandidate(HandTiles)` / `DaiminkanCandidate(HandTiles)` / `AnkanCandidate(Tiles)` / `KakanCandidate(Tile)` / `TsumoAgariCandidate` / `DahaiCandidate(Options)` / `KyuushuKyuuhaiCandidate`)
- `RoundDecisionSpec` / `RoundDecisionPhase` / `PlayerDecisionSpec` — State が返す「何を誰に聞くか」の仕様
- `ResolvedRoundAction` — 優先順位適用後の採用済み結果。`ResolvedWinAction` は `WinnerIndices(ResolvedWinner[])` / `LoserIndex?` / `WinType` / `KyoutakuRiichiAward?`(供託は上家取りの単一受取者) / `Honba`(WinType+和了者情報から配分導出可能) / `DealerContinues` を保持
- `PlayerRoundView` — プレイヤー視点で情報フィルタ済みの卓情報（`OwnRoundStatus` / `VisiblePlayerRoundStatus` で情報非対称性を表現）
- `YakuInfo` / `ScoreResult.YakuInfos` — 和了時の役情報（表示・ログ用）

## Phase 4: Player 拡張 (完了済み)

**目的**: プレイヤー側の抽象層を整備する。`Player` abstract class に通知・応答メソッドを追加し、`PlayerList` (= `IEnumerable<Player>`) 一本でプレイヤー情報と実体を管理する。

- `Player` を abstract record から **abstract class** に変更 — record + mutable property + equality の相互作用リスク回避、および人間プレイヤーと AI プレイヤーで状態保持要件が異なるため。`PlayerId` + `DisplayName` ベースのカスタム `Equals` / `GetHashCode` / `==` / `!=` を実装し `PlayerList` / `Game` の既存 value equality を維持
- `Player` に通知メソッド 14 個を追加 — すべて `public abstract` として定義。(`OnGameStartAsync` / `OnRoundStartAsync` / `OnHaipaiAsync` / `OnTsumoAsync` / `OnOtherPlayerTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnDoraRevealAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnRoundEndAsync` / `OnGameEndAsync`)。各メソッドの戻り値は通知ごとに型が異なる(Phase 3 の応答型を直接戻す)。全メソッドに `CancellationToken ct = default` を第2引数で付与し Phase 5 の RoundManager タイムアウト制御に備える
- `PlayerRoundView` のキャッシュ保持は基底クラスには持たせない — 人間プレイヤーは View を UI に反映するだけで保持不要。AI プレイヤーが必要な場合は各実装で独自に保持する(または Phase 5 で `PlayerSession` に分離する余地を残す)
- C# API ⇔ Wire DTO 二層構造の変換基盤は `this` 対象型ごとの拡張メソッドクラスに分離 — `GameNotificationExtensions.ToWire(...)` / `RoundNotificationExtensions.ToWire(...)` / `PlayerResponseEnvelopeExtensions.FromWire(RoundDecisionPhase)` / `PlayerResponseExtensions.ToBody()` を提供。Envelope 化 (`NotificationId` / `RoundRevision` / `PlayerIndex` の付与) と応答の合法性検証は Phase 5 の RoundManager 責務に残す
- `FakePlayer` (test用) — テストシナリオ記述用の疑似プレイヤー実装。`Func<TNotification, CancellationToken, TResponse>` デリゲート群を init プロパティで受け取り、未設定時は安全な既定応答(OK / 先頭 `DahaiCandidate` を打牌 / `PassResponse` / `KanPassResponse` 等)を返す。受信通知を `ReceivedNotifications` に記録

**Phase 4 完了の定義**:

- `Player` が abstract class であり `PlayerId` + `DisplayName` によるカスタム値等価 (`Equals` / `GetHashCode` は `sealed override`) を持ち、14 通知メソッドすべてに `CancellationToken ct = default` が付与されている
- `GameNotification` / `RoundNotification` / `PlayerResponseEnvelope` / `PlayerResponse` の Wire DTO 変換拡張メソッドが this 対象型ごとに分離されており、`FromWire(RoundDecisionPhase)` がフェーズ別に許可応答型のみを受理する
- Wire DTO は「`NotificationType` / `View` / `CandidateList` を運ぶ薄いエンベロープ」に留まる(通知固有ペイロード運搬は Phase 5 で対応 — 後述)
- `FakePlayer` が全通知に対する疑似応答と受信通知記録を提供し、Phase 5 の統合テスト記述基盤として機能する
- Phase 4 時点で Wire 経由プレイヤー接続は未対応。`NotificationId` / `RoundRevision` / `PlayerIndex` 付与・応答合法性検証・Wire ペイロード運搬・Game レベル ACK 経路は Phase 5 責務として残されている

**Phase 4 レビューで挙がり Phase 5 で対応する課題** (Codex CLI + Claude 共同レビュー由来):

- **【高】`PlayerNotification` の通知固有ペイロード運搬**: 現状の `PlayerNotification` は `NotificationType` / `View` / `CandidateList` のみを保持しており、`TsumoTile` (`TsumoNotification`) / `DiscardedTile` + `DiscarderIndex` (`DahaiNotification`) / `WinResult` (`WinNotification`) / `FinalPointArray` (`GameEndNotification`) / `PlayerList` + `Rules` (`GameStartNotification`) / `RoundWind` + `RoundNumber` + `Honba` + `DealerIndex` (`RoundStartNotification`) / `RyuukyokuResult` (`RyuukyokuNotification`) / `MadeCall` + `CallerIndex` (`CallNotification`) / `NewDoraIndicator` (`DoraRevealNotification`) 等が Wire 側で復元不能。別プロセス/通信プレイヤー接続時に Wire 経由で通知内容が届かない。通知種別ごとの Wire DTO もしくは `NotificationBody` 相当の discriminated union を `PlayerNotification` に追加して解消する
- **【高】`KanTsumo` 1 通知化に対応する RoundManager 分解契約**: `RoundStateKanTsumo` は `ResponseOk` / `ResponseWin` のみ処理し、打牌 / 暗槓 / 加槓は `RoundStateAfterKanTsumo` 側で処理する 2 段階構造。`PlayerResponseEnvelopeExtensions.FromWire(KanTsumo)` は `KanTsumoDahaiResponse` / `KanTsumoAnkanResponse` / `KanTsumoKakanResponse` を返すため、`RoundManager` が `RinshanTsumoResponse` は `RoundStateKanTsumo` に、それ以外 3 種は `RoundStateAfterKanTsumo` にディスパッチする責務を明記しテスト化する
- **【中】`ResolvedWinAction.LoserIndex` の境界変換**: `ResolvedWinAction` はツモ/嶺上で `LoserIndex == null` を強制するが、`RoundEventResponseWin` / `RoundEndedEventArgs` / `Round.SettleWin` では和了者自身 (`self`) を入れる規約。`RoundManager` がイベント系から `ResolvedWinAction` を生成する際に `WinType` がツモ/嶺上なら `self → null` へ正規化する変換を実装する
- **【中】Game レベル通知 ACK 経路の整備**: `PlayerResponseEnvelopeExtensions.FromWire(RoundDecisionPhase)` は局内意思決定フェーズ前提のため、`GameStartNotification` / `RoundStartNotification` / `RoundEndNotification` / `GameEndNotification` に対する Wire ACK (OK 応答) を受信する経路が無い。`NotificationType` ベースの OK 検証か `FromWireOk()` 相当を追加する
- **【高】`ResponseBody` / `ResponseCandidate` の多態シリアライズ設計**: `ResponseBody` と `ResponseCandidate` は abstract 基底で、`PlayerResponseEnvelope.Body` / `PlayerNotification.CandidateList` はこれらの基底型を保持する。現状は object-to-object 変換のラウンドトリップしかテストされていない。JSON 等でシリアライズする際には `System.Text.Json` の `[JsonPolymorphic]` / `[JsonDerivedType]` や独自 converter 等の多態ディスパッチ設計が必須。別プロセス/通信プレイヤー接続時のブロッカーとなるため Phase 5 で対応する
- **【中】`FakePlayer` responder の async 版追加**: 現状の `FakePlayer` は同期 `Func<TNotification, CancellationToken, TResponse>` 固定のため、Phase 5 の RoundManager で timeout/cancellation シナリオをテストする際に遅延応答・キャンセル待ち・例外発生タイミングの表現力が不足。`Func<TNotification, CancellationToken, Task<TResponse>>` も受けられる async responder を同期デリゲートと並存で追加する

## Phase 5: RoundManager と通信・集約レイヤー (実装中)

**目的**: プレイヤーとの通知・応答集約を`RoundManager`に集約する。既存の`RoundStateContext.ResponseXxxAsync`を`internal`化する。

- `RoundState`改修 — `CreateDecisionSpec(Round)` を追加
- `RoundStateContext.ResponseXxxAsync` を`internal`化 (`InternalsVisibleTo`でテストからアクセス維持)
- `RoundManager` — 1局ごとに生成・破棄。通知Id発行(UUIDv7)、視点射影、通知送信、応答収集(全員応答待ち)、検証(3段階)、タイムアウトフォールバック、優先順位適用
- `IRoundViewProjector` — `Round`から`PlayerRoundView`へ射影
- `IResponseCandidateEnumerator` (仮) — `Round`と`RoundDecisionSpec`から合法応答候補を列挙
- `IResponsePriorityPolicy` + 天鳳実装 — ロン > ポン/大明槓 > チー > OK、ダブロン対応、同順位衝突時は`PlayerIndex`小優先
- `IDefaultResponseFactory` — タイムアウト/プレイヤー例外時の既定応答
- `IGameTracer` + no-op 既定実装 — 全イベントのトレースインタフェース
- `GameManager`が内部で`RoundManager`を生成・破棄する形に統合
- Gameレベルの通知 — 対局開始 / 局開始 / 局終了 / 対局終了をプレイヤーに通知する仕組みを追加
- ロギング — `Microsoft.Extensions.Logging.Abstractions` を追加、`ILogger<T>`で警告/エラーを記録
- **包(責任払い)の記録・精算**: 大三元 / 大四喜 / 四槓子の役満確定副露を振った者を責任者として記録。`Call` 履歴から事後導出もしくは `Round` に責任者フィールドを追加(方針は本フェーズで確定)。槍槓の暗槓対応可否(国士ロン)もここで確定

### Phase 5 実装済み (feature/game-lib-phase5 ブランチ)

- 多態シリアライズ: `ResponseCandidate` / `ResponseBody` / `PlayerResponse` / `AfterXxxResponse` / `RoundNotification` / `GameNotification` / `ResolvedRoundAction` / `ResolvedKanAction` に `[JsonDerivedType]` 付与。ディスクリミネータは `nameof(具象型)` 形式で統一
- 通知ペイロード分離: `NotificationPayload` 抽象 + 13 派生 (`TsumoNotificationPayload` / `DahaiNotificationPayload` / `WinNotificationPayload` / `RyuukyokuNotificationPayload` / `CallNotificationPayload` / `KanNotificationPayload` / `KanTsumoNotificationPayload` / `DoraRevealNotificationPayload` / `OtherPlayerTsumoNotificationPayload` / `GameStartNotificationPayload` / `RoundStartNotificationPayload` / `RoundEndNotificationPayload` / `GameEndNotificationPayload`) を追加し、`PlayerNotification.Payload` に格納。`RoundNotificationExtensions.ToWire` / `GameNotificationExtensions.ToWire` で Payload 構築
- `NotificationType` enum を削除 — `PlayerNotification.Payload` の具象型で通知種別を判別できるため冗長
- `FromWireOk(envelope)` 追加 — 行動選択を伴わない Game レベル / 局内 OK 通知への Wire ACK を `OkResponse` に変換
- `FakePlayer` に async responder (`OnXxxHandler`) を 14 種追加 (既存の同期 `OnXxx` は維持)。優先順位: Async > Sync > 既定
- `RoundState.CreateDecisionSpec(round, enumerator)` を基底に追加 (virtual、既定 null)。6 具象状態 (`Haipai` / `Tsumo` / `Dahai` / `Kan` / `KanTsumo` / `AfterKanTsumo`) に実装。`Call` / `Win` / `Ryuukyoku` は null (自動遷移・終端)
- 中核抽象と実装を追加 (`src/Mahjong.Lib.Game/Rounds/Managing/`):
  - `IRoundViewProjector` + `RoundViewProjector`
  - `IResponseCandidateEnumerator` (具象実装は未着手)
  - `IResponsePriorityPolicy` + `TenhouResponsePriorityPolicy` (ロン > ポン/大明槓 > チー > OK、ダブロン対応、同順位衝突は PlayerIndex 小優先)
  - `IDefaultResponseFactory` + `DefaultResponseFactory`
  - `IGameTracer` + `NullGameTracer`
  - `ResolvedPlayerResponse` (採用応答)
- `RoundStateContext.ResponseOkAsync` / `ResponseDahaiAsync` / `ResponseCallAsync` / `ResponseKanAsync` / `ResponseWinAsync` / `ResponseRyuukyokuAsync` を `public` → `internal` に変更
- 打牌/加槓フェーズのスルーは `OkResponse` に統一。`Player.OnDahaiAsync` / `OnKanAsync` の戻り値型は `Task<PlayerResponse>` (スルー時は `OkResponse`、アクション時は `AfterDahaiResponse` / `AfterKanResponse` 派生)。`AfterDahaiResponse` / `AfterKanResponse` は**アクション応答のみ**の階層。`docs/Design.md` にも明記

### Phase 5 残作業

- **【高】`IResponseCandidateEnumerator` の具象実装**: 合法応答候補の列挙ロジック。`ResponseCandidateEnumerator` を新規作成し、Tsumo/Dahai/Kan/KanTsumo/AfterKanTsumo 各フェーズで合法候補を `ITenpaiChecker` を利用して列挙する。暗槓からの槍槓 (国士ロン) は `GameRules.AllowAnkanChankanForKokushi` 参照
- **【高】`RoundManager` 本体実装**: `src/Mahjong.Lib.Game/Rounds/Managing/RoundManager.cs` を新規作成。1 局ごとに `RoundStateContext` をホストし、`RoundStateChanged` を `Channel<RoundState>` で受けてメインループ駆動。各 state から `CreateDecisionSpec` を取得→全プレイヤーへ通知送信 (`Task.WhenAll` + タイムアウト) → 応答集約 → `priorityPolicy.Resolve` → `_context.ResponseXxxAsync` にディスパッチ。終端 (`RoundStateWin` / `RoundStateRyuukyoku`) では `RoundEnded` イベントを発火。通知送信時の NotificationId は `Guid.CreateVersion7()`
- **【高】KanTsumo 1 通知化の 2 段階ディスパッチ**: `AfterKanTsumoResponse` のうち `RinshanTsumoResponse` は `RoundStateKanTsumo` へ直接ディスパッチ、それ以外 3 種 (`KanTsumoDahaiResponse` / `KanTsumoAnkanResponse` / `KanTsumoKakanResponse`) は先に `ResponseOkAsync()` で `RoundStateAfterKanTsumo` に進めてからディスパッチ。RoundManager 内の `DispatchKanTsumoResponseAsync` に閉じ込める
- **【中】`ResolvedWinAction.LoserIndex` 境界変換**: `RoundEndedByWinEventArgs` では `WinType` がツモ/嶺上でも `LoserIndex = self` が入る。`ResolvedWinAction` に組み直す際に ツモ/嶺上で `self → null` に正規化する (RoundManager の受信側で)。現状 Phase 5 では `RoundEndedEventArgs` のまま RoundEnded を発火する設計で回避している。`ResolvedRoundAction` への正規化は Step 11 (GameManager 統合) で行う
- **【高】包 (責任払い) の記録と精算**: 新規 `src/Mahjong.Lib.Game/Players/PlayerResponsibilityArray.cs` (和了者 → 責任者の map) と `src/Mahjong.Lib.Game/Rounds/PaoDetector.cs` (大三元 / 大四喜 / 四槓子 判定) を追加。`Round` に `PaoResponsibleArray` フィールド追加、`Round.Pon` / `Round.Daiminkan` / `Round.Kakan` で役満確定副露を判定して記録。`Round.SettleWin` で包を参照し、ツモ時は責任者が全額負担、ロン時は放銃者と責任者で折半 (天鳳ルール)
- **【中】槍槓の暗槓対応 (国士ロン)**: `GameRules.AllowAnkanChankanForKokushi` を追加 (既定 `true`)。`RoundStateKan.cs` の暗槓分岐で国士テンパイ時のみ `ChankanRonResponse` を受理 (`IScoreCalculator` 側で国士無双以外の役を制限)
- **【高】GameManager 統合**: `GameManager` のコンストラクタに `IRoundViewProjector` / `IResponseCandidateEnumerator` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IGameTracer?` / `ILoggerFactory?` / `TimeSpan? defaultTimeout` を追加。`GameStateContext.StartRound` で `RoundStateContext` 直接生成 → `RoundManager` 生成・`StartAsync` に置き換え。`RoundManager.RoundEnded` を購読して `GameEventRoundEndedBy*` に変換
- **【中】Game レベル通知の送信経路**: `GameStateInit.Entry` で `GameStartNotification` を全員へ送信 → ACK 収集、`GameStateRoundRunning.Entry` で `RoundStartNotification` → ACK 収集 → RoundManager 起動、局終了ハンドラで `RoundEndNotification` → ACK 収集、`GameStateEnd.Entry` で `GameEndNotification` → ACK 収集。`FromWireOk` で受信
- **【中】Microsoft.Extensions.Logging.Abstractions 依存追加**: `Mahjong.Lib.Game.csproj` に追加、`ILogger<RoundManager>` / `ILogger<GameManager>` で警告・エラーを記録
- **【高】テスト追加**: `tests/Mahjong.Lib.Game.Tests/Rounds/Managing/` に RoundManager 統合テスト (ダブロン / 見逃し / タイムアウト / 優先順位衝突 / KanTsumo 2 段階 / 嶺上和了 / 槍槓) / `Wire/JsonRoundTripTests.cs` (全 Wire DTO ラウンドトリップ) / `Games/GameManager_IntegrationTests.cs` (4 人 FakePlayer 対局) / `PaoDetectorTests` / `RoundStateKan_ResponseWin_AnkanTests` (暗槓チャンカン)
- **【中】既存テストの整合**: `GameManager_*Tests` の新コンストラクタ引数対応、テストヘルパに既定実装注入ヘルパ追加

### Phase 5 タイムアウト既定値

| 層 | 既定 |
|---|---|
| `RoundManager.defaultTimeout` | 10 秒 |
| `RoundStateContext.DisposeTimeout` | 5 秒 (既存維持) |
| `GameStateContext.DisposeTimeout` | 5 秒 (既存維持) |

## Phase 6: AI実装と自動対局

**目的**: 4人AI自動対局で対局ループが回ることを確認し、統計取得基盤を作る。

- 簡易AI(ランダム打牌・副露しないなど最小実装)
- AI 4人で所定回数の自動対局をこなすコンソールアプリ (`samples/` or `tools/` 配下)
- `IGameTracer` 実装での統計集計(順位率 / 和了率 / 放銃率 / 立直率 / 副露率 / 平均順位 / 平均打点 / 役出現率 / 流局理由別出現率)
- 牌譜出力実装(天鳳牌譜 or 独自フォーマット)
- AI差し替え可能な構成 (DI / factory)

## Phase 7: 人間プレイヤー対応

**目的**: 人間プレイヤーが対局に参加できるようにする。

- `Player`継承の人間プレイヤー実装 (UI入力を待つ形)
- transport adapter (WebSocket / SignalR 等)
- `Mahjong.ApiService` / `Mahjong.Web` との統合
- 接続断/再接続処理
