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
- `RoundInquirySpec` / `RoundInquiryPhase` / `PlayerInquirySpec` — State が返す「何を誰に聞くか」の仕様
- `ResolvedRoundAction` — 優先順位適用後の採用済み結果。`ResolvedWinAction` は `WinnerIndices(ResolvedWinner[])` / `LoserIndex?` / `WinType` / `KyoutakuRiichiAward?`(供託は上家取りの単一受取者) / `Honba`(WinType+和了者情報から配分導出可能) / `DealerContinues` を保持
- `PlayerRoundView` — プレイヤー視点で情報フィルタ済みの卓情報（`OwnRoundStatus` / `VisiblePlayerRoundStatus` で情報非対称性を表現）
- `YakuInfo` / `ScoreResult.YakuInfos` — 和了時の役情報（表示・ログ用）

## Phase 4: Player 拡張 (完了済み)

**目的**: プレイヤー側の抽象層を整備する。`Player` abstract class に通知・応答メソッドを追加し、`PlayerList` (= `IEnumerable<Player>`) 一本でプレイヤー情報と実体を管理する。

- `Player` を abstract record から **abstract class** に変更 — record + mutable property + equality の相互作用リスク回避、および人間プレイヤーと AI プレイヤーで状態保持要件が異なるため。`PlayerId` + `DisplayName` ベースのカスタム `Equals` / `GetHashCode` / `==` / `!=` を実装し `PlayerList` / `Game` の既存 value equality を維持
- `Player` に通知メソッド 14 個を追加 — すべて `public abstract` として定義。(`OnGameStartAsync` / `OnRoundStartAsync` / `OnHaipaiAsync` / `OnTsumoAsync` / `OnOtherPlayerTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnDoraRevealAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnRoundEndAsync` / `OnGameEndAsync`)。各メソッドの戻り値は通知ごとに型が異なる(Phase 3 の応答型を直接戻す)。全メソッドに `CancellationToken ct = default` を第2引数で付与し Phase 5 の RoundManager タイムアウト制御に備える
- `PlayerRoundView` のキャッシュ保持は基底クラスには持たせない — 人間プレイヤーは View を UI に反映するだけで保持不要。AI プレイヤーが必要な場合は各実装で独自に保持する(または Phase 5 で `PlayerSession` に分離する余地を残す)
- C# API ⇔ Wire DTO 二層構造の変換基盤は `this` 対象型ごとの拡張メソッドクラスに分離 — `GameNotificationExtensions.ToWire(...)` / `RoundNotificationExtensions.ToWire(...)` / `PlayerResponseEnvelopeExtensions.FromWire(RoundInquiryPhase)` / `PlayerResponseExtensions.ToBody()` を提供。Envelope 化 (`NotificationId` / `RoundRevision` / `PlayerIndex` の付与) と応答の合法性検証は Phase 5 の RoundManager 責務に残す
- `FakePlayer` (test用) — テストシナリオ記述用の疑似プレイヤー実装。`Func<TNotification, CancellationToken, TResponse>` デリゲート群を init プロパティで受け取り、未設定時は安全な既定応答(OK / 先頭 `DahaiCandidate` を打牌 / `PassResponse` / `KanPassResponse` 等)を返す。受信通知を `ReceivedNotifications` に記録

**Phase 4 完了の定義**:

- `Player` が abstract class であり `PlayerId` + `DisplayName` によるカスタム値等価 (`Equals` / `GetHashCode` は `sealed override`) を持ち、14 通知メソッドすべてに `CancellationToken ct = default` が付与されている
- `GameNotification` / `RoundNotification` / `PlayerResponseEnvelope` / `PlayerResponse` の Wire DTO 変換拡張メソッドが this 対象型ごとに分離されており、`FromWire(RoundInquiryPhase)` がフェーズ別に許可応答型のみを受理する
- Wire DTO は「`NotificationType` / `View` / `CandidateList` を運ぶ薄いエンベロープ」に留まる(通知固有ペイロード運搬は Phase 5 で対応 — 後述)
- `FakePlayer` が全通知に対する疑似応答と受信通知記録を提供し、Phase 5 の統合テスト記述基盤として機能する
- Phase 4 時点で Wire 経由プレイヤー接続は未対応。`NotificationId` / `RoundRevision` / `PlayerIndex` 付与・応答合法性検証・Wire ペイロード運搬・`GameNotification` ACK 経路は Phase 5 責務として残されている

**Phase 4 レビューで挙がり Phase 5 で対応する課題** (Codex CLI + Claude 共同レビュー由来):

- **【高】`PlayerNotification` の通知固有ペイロード運搬**: 現状の `PlayerNotification` は `NotificationType` / `View` / `CandidateList` のみを保持しており、`TsumoTile` (`TsumoNotification`) / `DiscardedTile` + `DiscarderIndex` (`DahaiNotification`) / `WinResult` (`WinNotification`) / `FinalPointArray` (`GameEndNotification`) / `PlayerList` + `Rules` (`GameStartNotification`) / `RoundWind` + `RoundNumber` + `Honba` + `DealerIndex` (`RoundStartNotification`) / `RyuukyokuResult` (`RyuukyokuNotification`) / `MadeCall` + `CallerIndex` (`CallNotification`) / `NewDoraIndicator` (`DoraRevealNotification`) 等が Wire 側で復元不能。別プロセス/通信プレイヤー接続時に Wire 経由で通知内容が届かない。通知種別ごとの Wire DTO もしくは `NotificationBody` 相当の discriminated union を `PlayerNotification` に追加して解消する
- **【高】`KanTsumo` 1 通知化に対応する RoundManager 分解契約**: `RoundStateKanTsumo` は `ResponseOk` / `ResponseWin` のみ処理し、打牌 / 暗槓 / 加槓は `RoundStateAfterKanTsumo` 側で処理する 2 段階構造。`PlayerResponseEnvelopeExtensions.FromWire(KanTsumo)` は `KanTsumoDahaiResponse` / `KanTsumoAnkanResponse` / `KanTsumoKakanResponse` を返すため、`RoundManager` が `RinshanTsumoResponse` は `RoundStateKanTsumo` に、それ以外 3 種は `RoundStateAfterKanTsumo` にディスパッチする責務を明記しテスト化する
- **【中】`ResolvedWinAction.LoserIndex` の境界変換**: `ResolvedWinAction` はツモ/嶺上で `LoserIndex == null` を強制するが、`RoundEventResponseWin` / `RoundEndedEventArgs` / `Round.SettleWin` では和了者自身 (`self`) を入れる規約。`RoundManager` がイベント系から `ResolvedWinAction` を生成する際に `WinType` がツモ/嶺上なら `self → null` へ正規化する変換を実装する
- **【中】`GameNotification` ACK 経路の整備**: `PlayerResponseEnvelopeExtensions.FromWire(RoundInquiryPhase)` は局内意思決定フェーズ前提のため、`GameStartNotification` / `RoundStartNotification` / `RoundEndNotification` / `GameEndNotification` に対する Wire ACK (OK 応答) を受信する経路が無い。`NotificationType` ベースの OK 検証か `FromWireOk()` 相当を追加する
- **【高】`ResponseBody` / `ResponseCandidate` の多態シリアライズ設計**: `ResponseBody` と `ResponseCandidate` は abstract 基底で、`PlayerResponseEnvelope.Body` / `PlayerNotification.CandidateList` はこれらの基底型を保持する。現状は object-to-object 変換のラウンドトリップしかテストされていない。JSON 等でシリアライズする際には `System.Text.Json` の `[JsonPolymorphic]` / `[JsonDerivedType]` や独自 converter 等の多態ディスパッチ設計が必須。別プロセス/通信プレイヤー接続時のブロッカーとなるため Phase 5 で対応する
- **【中】`FakePlayer` responder の async 版追加**: 現状の `FakePlayer` は同期 `Func<TNotification, CancellationToken, TResponse>` 固定のため、Phase 5 の RoundManager で timeout/cancellation シナリオをテストする際に遅延応答・キャンセル待ち・例外発生タイミングの表現力が不足。`Func<TNotification, CancellationToken, Task<TResponse>>` も受けられる async responder を同期デリゲートと並存で追加する

## Phase 5: RoundManager と通信・集約レイヤー (完了)

**目的**: プレイヤーとの通知・応答集約を `RoundManager` に集約する。既存の `RoundStateContext.ResponseXxxAsync` を `internal` 化する。

### 実装済み

**RoundManager 抽象・実装** (`src/Mahjong.Lib.Game/Rounds/Managing/`)
- `RoundManager` 本体 (sealed) — 1 局分の通知・応答集約、`Channel<RoundState>` リレー、`Task.WhenAll` 並列通知 + プレイヤー単位 10 秒タイムアウト、KanTsumo 2 段階ディスパッチ (`pendingAfterKanTsumoResponse_`)、合法応答検証 (`ResponseValidator`)、`NormalizeLoserIndex` で Tsumo/Rinshan 時の self→null 正規化
- `IRoundViewProjector` + `RoundViewProjector`
- `IResponseCandidateEnumerator` + `ResponseCandidateEnumerator` (`ITenpaiChecker` / `GameRules` 注入、赤ドラ区別鳴き候補を `EnumerateRedCountVariants(pickCount)` で枚数組合せ列挙、立直中は槓候補抑制)
- `IResponsePriorityPolicy` + `TenhouResponsePriorityPolicy` (ロン > ポン/大明槓 > チー > OK、ダブロン対応、同順位は放銃者起点巡目順)
- `IDefaultResponseFactory` + `DefaultResponseFactory` (タイムアウト/例外時の既定応答)
- `IGameTracer` + `NullGameTracer` (`OnInvalidResponse` 含む全イベントトレース)
- `ResponseValidator` — 応答が候補リストに含まれるか検証、候補外は `DefaultResponseFactory` へフォールバック

**状態機械との連携**
- `RoundState.CreateInquirySpec(round, enumerator)` を基底に追加。6 具象状態 (`Haipai` / `Tsumo` / `Dahai` / `Kan` / `KanTsumo` / `AfterKanTsumo`) に実装
- `RoundStateContext.ResponseXxxAsync` を `internal` 化 (`InternalsVisibleTo` 経由でテストアクセス維持)
- `GameStateContext.StartRound` が `RoundManager` 経路 (依存フル注入時) と直接駆動経路 (旧コンストラクタ) を両対応
- `GameManager` に `RoundManager` 依存 (`IRoundViewProjector` / `IResponseCandidateEnumerator` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IGameTracer` / `ILoggerFactory`) を必須引数として追加

**通知・応答の型基盤**
- 多態シリアライズ: `ResponseCandidate` / `ResponseBody` / `PlayerResponse` / `AfterXxxResponse` / `RoundNotification` / `GameNotification` / `ResolvedRoundAction` / `ResolvedKanAction` に `[JsonDerivedType]` 付与 (ディスクリミネータは `nameof(具象型)`)
- 通知ペイロード分離: `NotificationPayload` 抽象 + 13 派生を追加し Wire 経由で通知内容を復元可能に
- `ChankanRonCandidate` / `RinshanTsumoAgariCandidate` 新規追加
- `FromWireOk(envelope)` 追加 — OK 応答のみ受理する通知用
- `FakePlayer` に async responder (`OnXxxHandler`) 14 種を追加 (同期版と並存、優先順位: Async > Sync > 既定)
- 打牌/加槓フェーズのスルーは `OkResponse` に統一。`AfterDahaiResponse` / `AfterKanResponse` はアクション応答のみの階層

**Game レベル通知経路**
- `GameState.EntryAsync` / `ResponseOkAsync` / `RoundEndedBy*Async` 非同期版を追加
- `GameStateInit.EntryAsync` → `GameStartNotification` 送信、`GameStateRoundRunning.EntryAsync` → `RoundStartNotification` 送信 → `StartRound`、`RoundEndedBy*Async` → `RoundEndNotification` 送信 → 次局/終了、`GameStateEnd.EntryAsync` → `GameEndNotification` 送信
- `RoundManager.ProcessAsync` 内で `RoundStateCall` / `RoundStateWin` / `RoundStateRyuukyoku` 突入時に `CreateInquirySpec` が OkCandidate のみを返し、既存の CollectResponsesAsync ルート経由で `CallNotification` / `WinNotification` / `RyuukyokuNotification` を全員送信・OK 応答を集約する

**和了明細の伝搬 (通知層の役/点欠落対策)**
- `Round.SettleWin` に `out WinSettlementDetails` 版オーバーロード追加。和了者毎の `ResolvedWinner` (PlayerIndex / 和了牌 / `ScoreResult`) / 精算前本場 / `KyoutakuRiichiAward` を出力
- `RoundEndedByWinEventArgs` / `GameEventRoundEndedByWin` に `Winners` / `Honba` / `KyoutakuRiichiAward` フィールドを追加し通知層に伝搬

**包 (責任払い)**
- `PlayerResponsibilityArray` / `PaoDetector` 新規追加。`Round.PaoResponsibleArray` に記録
- 大三元・大四喜は新規牌種を増やす `Pon` / `Daiminkan` のみトリガ (`Kakan` 除外)、四槓子は `Daiminkan` / `Kakan` でトリガ
- `Round.SettleWin` で `YakuInfo.IsPaoEligible` を参照して役満素点のみ責任者負担 (本場・供託は通常精算、天鳳準拠)
- `GameRules.AllowAnkanChankanForKokushi` (既定 `true`) 追加。暗槓チャンカンは国士無双のみ成立 (役制限は `IScoreCalculator` に委譲)

**同巡フリテン**
- `RoundManager` が打牌フェーズでロン見逃しを検出し `Round.SetTemporaryFuriten` で対象プレイヤーに `IsTemporaryFuriten=true` を設定
- `Round.Tsumo` で自分の次ツモ時に自動解除

**その他**
- `Mahjong.Lib.Game.csproj` に `Microsoft.Extensions.Logging.Abstractions` 追加、`ILogger<T>` で警告/エラー記録
- `RoundManager.StartAsync` キャンセル時は `TrySetCanceled(ct)` で Task を確実に完了
- `GameStateContext.InvokeGameNotificationAsync` はプレイヤーメソッドの同期例外も try 内で受ける

**テスト追加** (`tests/Mahjong.Lib.Game.Tests/`)
- `Rounds/Managing/`: `RoundManagerTestHelper` / `RoundManager_NormalizeLoserIndexTests` / `RoundManager_StartAsyncTests` / `RoundManager_DahaiFlowTests` / `RoundManager_TimeoutTests` / `RoundManager_CandidateValidationTests` / `RoundManager_KanTsumoFlowTests` / `ResponseCandidateEnumerator_EnumerateFor*Tests`
- `Rounds/`: `PaoDetector_DetectTests` / `Round_PaoRecordTests` / `Round_SettleWinPaoTests` / `Round_TemporaryFuritenTests`
- `Games/`: `GameManager_GameLevelNotificationTests`

### タイムアウト既定値

| 層 | 既定 |
|---|---|
| `RoundManager.DefaultTimeout` | 10 秒 |
| `RoundStateContext.DisposeTimeout` | 5 秒 |
| `GameStateContext.DisposeTimeout` | 5 秒 |
| `GameStateContext.NotificationTimeout` | 10 秒 |

## Phase 5 からの繰越 (Phase 6 冒頭で対応)

- **補助通知の未対応分**: `OtherPlayerTsumoNotification` / `DoraRevealNotification` の送信経路
- **`ResponseCandidateEnumerator` の立直中暗槓精緻化**: 待ち不変条件 (暗槓対象=ツモ牌 / 待ち牌種集合不変 / 順子解釈なし刻子) を満たす場合のみ候補提示
- **合法応答検証の 2 / 3 段目**: 現状は候補リスト membership のみ。意味的検証 (手牌との整合等) と副作用防止を追加

### Phase 5 レビューで持ち越した構造懸念

Phase 5 の実装レビュー (Claude + Codex gpt-5.4) で挙がった、実害は出ないが中長期的に解消したい設計課題。Phase 6 以降で順次着手する。

- **`RoundManager` の責務分離**: 現在 660 行で「状態遷移集約 / 通知生成 / 応答検証・優先順位解決 / ディスパッチ / リソース管理」を一手に担う。`INotificationBuilder` / `IResponseDispatcher` 相当で分離し、テスト粒度を細かくする
- **`Round` の外部 setter 書き換え**: `RoundManager.ApplyTemporaryFuritenIfRonMissed` が `context_.Round = context_.Round.SetTemporaryFuriten(...)` で Round を書き換えている。イミュータブル集約を `RoundStateContext` 越しに外部改変する構造になっており、副作用を RoundState 内部に閉じ込める方向で見直す
- **`IGameTracer.OnInvalidResponse` の default interface method 化**: 新規メソッド追加時に全実装へ no-op 追加が必要になる。C# default interface method で既定 no-op を提供し、テストダブル・ロギング実装の更新負荷を下げる
- **`Round.ResolveWinTile` の best-guess フォールバック削除**: 完全な状態を構築せず `SettleWin` を呼ぶ単体テストのために「和了者の手牌末尾を best-guess で返す」経路があるが、本番で誤った牌が通知層に流れるリスクを残す。`SettleWin` の API に和了牌を明示引き渡す形へ整理する (Chankan の加槓牌解決 (#C5) の堅牢化もここに集約できる)
- **`Round.SettleWin` の out パラメータ**: 通知明細を常に集計するなら `(Round, WinSettlementDetails)` タプルまたは複合レコード戻り値のほうが関数型スタイルに合う
- **`PaoDetector` の副作用位置**: 現在 `Round.Pon` / `Daiminkan` / `Kakan` 内で呼び出して `PaoResponsibleArray` を書き換えている。Round に「パオ検出」という責務が混ざるため、呼び出し側 (RoundManager or RoundState) で検出するか、結果タプルで分離する案を検討

## Phase 6: AI実装と自動対局

**目的**: 4人AI自動対局で対局ループが回ることを確認し、統計取得基盤を作る。

- 簡易AI(ランダム打牌・副露しないなど最小実装)
- AI 4人で所定回数の自動対局をこなすコンソールアプリ (`tools/` 配下)
- `IGameTracer` 実装での統計集計(順位率 / 和了率 / 放銃率 / 立直率 / 副露率 / 平均順位 / 平均打点 / 役出現率 / 流局理由別出現率)
- 牌譜出力実装(天鳳牌譜 or 独自フォーマット)
- AI差し替え可能な構成 (DI / factory)

## Phase 7: 人間プレイヤー対応

**目的**: 人間プレイヤーが対局に参加できるようにする。

- `Player`継承の人間プレイヤー実装 (UI入力を待つ形)
- transport adapter (WebSocket / SignalR 等)
- `Mahjong.ApiService` / `Mahjong.Web` との統合
- 接続断/再接続処理
