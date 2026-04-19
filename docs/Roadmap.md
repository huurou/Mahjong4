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
- **【中】`AdoptedWinAction.LoserIndex` の扱い統一**: (Phase 6 初頭で解消済) かつては `AdoptedWinAction` だけがツモ/嶺上で `LoserIndex == null` を強制していたが、`RoundEventResponseWin` / `RoundEndedEventArgs` / `Round.SettleWin` / `GameEventRoundEndedByWin` は「ツモ/嶺上では和了者自身 (`self`)」の規約で統一されていた。ズレを埋めるためだけに `NormalizeLoserIndex` 正規化が 2 箇所 (`AdoptedRoundActionBuilder` / `GameStateRoundRunning.BuildAdoptedRoundAction`) で重複していたため、`AdoptedWinAction.LoserIndex` を non-nullable `PlayerIndex` に変更し、ツモ/嶺上判定は `WinType` 一本に統一して正規化コードは削除済
- **【中】`GameNotification` ACK 経路の整備**: `PlayerResponseEnvelopeExtensions.FromWire(RoundInquiryPhase)` は局内意思決定フェーズ前提のため、`GameStartNotification` / `RoundStartNotification` / `RoundEndNotification` / `GameEndNotification` に対する Wire ACK (OK 応答) を受信する経路が無い。`NotificationType` ベースの OK 検証か `FromWireOk()` 相当を追加する
- **【高】`ResponseBody` / `ResponseCandidate` の多態シリアライズ設計**: `ResponseBody` と `ResponseCandidate` は abstract 基底で、`PlayerResponseEnvelope.Body` / `PlayerNotification.CandidateList` はこれらの基底型を保持する。現状は object-to-object 変換のラウンドトリップしかテストされていない。JSON 等でシリアライズする際には `System.Text.Json` の `[JsonPolymorphic]` / `[JsonDerivedType]` や独自 converter 等の多態ディスパッチ設計が必須。別プロセス/通信プレイヤー接続時のブロッカーとなるため Phase 5 で対応する
- **【中】`FakePlayer` responder の async 版追加**: 現状の `FakePlayer` は同期 `Func<TNotification, CancellationToken, TResponse>` 固定のため、Phase 5 の RoundManager で timeout/cancellation シナリオをテストする際に遅延応答・キャンセル待ち・例外発生タイミングの表現力が不足。`Func<TNotification, CancellationToken, Task<TResponse>>` も受けられる async responder を同期デリゲートと並存で追加する

## Phase 5: 通知・応答パイプライン (完了)

**目的**: プレイヤーとの通知・応答集約を `RoundStateContext` に統合する (Phase 5 完了時点で `GameStateContext → RoundStateContext` の 2 層構造、API は `ctor` + `StartAsync` のみ)。`Response*Async` は `private` 化、`State` / `Round` は `internal set` で同 assembly のみ書換可。

### 実装済み

**通知・応答集約レイヤー** (`src/Mahjong.Lib.Game/States/RoundStates/` + `src/Mahjong.Lib.Game/Rounds/Managing/`)
- `RoundStateContext` (partial 3 分割: 本体 / `Runtime` / `PlayerIo`) — 1 局分の通知・応答集約、`Channel<RoundState>` リレー、`Task.WhenAll` 並列通知 + プレイヤー単位 10 秒タイムアウト、KanTsumo 2 段階ディスパッチ (`pendingAfterKanTsumoResponse_`)、合法応答検証 (`ResponseValidator`)。state 機械と通知・応答集約ループを 1 クラスに統合 (旧 `RoundManager` を吸収、3 層 → 2 層化)
- `IRoundViewProjector` + `RoundViewProjector`
- `IResponseCandidateEnumerator` + `ResponseCandidateEnumerator` (`ITenpaiChecker` / `GameRules` 注入、赤ドラ区別鳴き候補を `EnumerateRedCountVariants(pickCount)` で枚数組合せ列挙、立直中は槓候補抑制)
- `IResponsePriorityPolicy` + `TenhouResponsePriorityPolicy` (ロン > ポン/大明槓 > チー > OK、ダブロン対応、同順位は放銃者起点巡目順)
- `IDefaultResponseFactory` + `DefaultResponseFactory` (タイムアウト/例外時の既定応答)
- `IGameTracer` + `NullGameTracer` (`OnInvalidResponse` 含む全イベントトレース)
- `ResponseValidator` — 応答が候補リストに含まれるか検証、候補外は `DefaultResponseFactory` へフォールバック

**状態機械との連携**
- `RoundState.CreateInquirySpec(round, enumerator)` を基底に追加。6 具象状態 (`Haipai` / `Tsumo` / `Dahai` / `Kan` / `KanTsumo` / `AfterKanTsumo`) に実装
- `RoundStateContext.Response*Async` 6 個を `private` 化 (外部からの状態駆動経路は物理的に封鎖、race の構造的除去)
- `GameStateContext.StartRound` が `RoundStateContext` を直接生成・駆動 (旧 `RoundManager` 経由は撤廃、`RoundManager` プロパティも削除)
- `GameManager` の依存は `IRoundViewProjector` / `IResponseCandidateEnumerator` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IGameTracer` / `ILoggerFactory` (`IRoundNotificationBuilder` / `IResponseDispatcher` は廃止し `RoundStateContext.PlayerIo.cs` に inline 化済み)

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
- `RoundStateContext.ProcessRuntimeAsync` 内で `RoundStateCall` / `RoundStateWin` / `RoundStateRyuukyoku` 突入時に `CreateInquirySpec` が OkCandidate のみを返し、既存の CollectResponsesAsync ルート経由で `CallNotification` / `WinNotification` / `RyuukyokuNotification` を全員送信・OK 応答を集約する

**和了明細の伝搬 (通知層の役/点欠落対策)**
- `Round.SettleWin` に `out WinSettlementDetails` 版オーバーロード追加。和了者毎の `ResolvedWinner` (PlayerIndex / 和了牌 / `ScoreResult`) / 精算前本場 / `KyoutakuRiichiAward` を出力
- `RoundEndedByWinEventArgs` / `GameEventRoundEndedByWin` に `Winners` / `Honba` / `KyoutakuRiichiAward` フィールドを追加し通知層に伝搬

**包 (責任払い)**
- `PlayerResponsibilityArray` / `PaoDetector` 新規追加。`Round.PaoResponsibleArray` に記録
- 大三元・大四喜は新規牌種を増やす `Pon` / `Daiminkan` のみトリガ (`Kakan` 除外)、四槓子は `Daiminkan` / `Kakan` でトリガ
- `Round.SettleWin` で `YakuInfo.IsPaoEligible` を参照して役満素点のみ責任者負担 (本場・供託は通常精算、天鳳準拠)
- `GameRules.AllowAnkanChankanForKokushi` (既定 `true`) 追加。暗槓チャンカンは国士無双のみ成立 (役制限は `IScoreCalculator` に委譲)

**同巡フリテン**
- `RoundStateContext.DetectRonMissedFuritenPlayers` (private) が打牌フェーズでロン見逃しを検出し `Round.ApplyTemporaryFuriten` で対象プレイヤーに `IsTemporaryFuriten=true` を設定
- `Round.Tsumo` で自分の次ツモ時に自動解除

**その他**
- `Mahjong.Lib.Game.csproj` に `Microsoft.Extensions.Logging.Abstractions` 追加、`ILogger<T>` で警告/エラー記録
- `RoundStateContext.StartAsync` キャンセル時は `TrySetCanceled(ct)` で Task を確実に完了
- `GameStateContext.InvokeGameNotificationAsync` はプレイヤーメソッドの同期例外も try 内で受ける

**テスト追加** (`tests/Mahjong.Lib.Game.Tests/`)
- `States/RoundStates/`: `RoundStateContextRuntimeTestHelper` / `RoundStateContext_RuntimeStartAsyncTests` / `RoundStateContext_RuntimeDahaiFlowTests` / `RoundStateContext_RuntimeTimeoutTests` / `RoundStateContext_RuntimeCandidateValidationTests` / `RoundStateContext_RuntimeKanTsumoFlowTests`
- `Rounds/Managing/`: `ResponseCandidateEnumerator_EnumerateFor*Tests` / `TenhouResponsePriorityPolicy_ResolveTests` / `ResponseValidator_IsResponseInCandidatesTests`
- `Rounds/`: `PaoDetector_DetectTests` / `Round_PaoRecordTests` / `Round_SettleWinPaoTests` / `Round_TemporaryFuritenTests`
- `Games/`: `GameManager_GameLevelNotificationTests`
- state 単体テストは `Drive*` 同期駆動 (event queue 不経由) に書き換え済み (`RoundStateContextTestHelper`)

### タイムアウト既定値

| 層 | 既定 |
|---|---|
| `RoundStateContext.DefaultTimeout` (プレイヤー応答) | 10 秒 |
| `RoundStateContext.DisposeTimeout` | 5 秒 |
| `GameStateContext.DisposeTimeout` | 5 秒 |
| `GameStateContext.NotificationTimeout` | 10 秒 |

### Phase 5 レビューで持ち越した構造懸念 (解消済)

Phase 5 の実装レビュー (Claude + Codex gpt-5.4) で挙がった設計課題を Phase 6 冒頭で解消した。実装内容の要点:

- **`IGameTracer` の default interface method 化**: 全 9 メソッドに既定 no-op を付与。`NullGameTracer` はメソッド定義を削除し `Instance` の公開型を `IGameTracer` に変更
- **`Round.SettleWin` のタプル戻り値化 + `winTile` 明示**: `out WinSettlementDetails` を廃止し `(Round Settled, WinSettlementDetails Details)` タプル戻り値へ統一。和了牌は呼び出し側 (RoundState) が決定して渡す引数に変更し、`Round.ResolveWinTile` の best-guess フォールバックを削除
- **同巡フリテンの RoundState 移動 + `Round` 外部 setter 封鎖**: ロン見逃し検出は `RoundStateContext.DetectRonMissedFuritenPlayers` (private) が担当し、`ApplyTemporaryFuriten` (private) で `Round` を局所更新。`RoundStateContext.Round` は `internal set` (同 assembly のみ書換可)、状態遷移時の Round 更新は `Transit(Func<RoundState> createNextState, Func<Round, Round>? updateRound = null)` 経由 (Exit → updateRound 適用 → createNextState 評価 → State 差替 → Entry の意味論。updateRound が例外を投げた場合は Round が部分更新されないことを保証)
- **`PaoDetector.Detect` の戻り値整理**: `PaoDetectionResult` record へ変更 (`IsPao` プロパティで判定)。副作用位置は Round 内 (副露履歴と責任者更新の atomicity 優先) を維持
- **`RoundManager` を `RoundStateContext` に統合 (3 層 → 2 層化)**: 旧 `RoundManager` クラス (320 行) と `IRoundNotificationBuilder` + `RoundNotificationBuilder` (通知生成) / `IResponseDispatcher` + `ResponseDispatcher` (応答→イベント変換) はすべて `RoundStateContext` (partial 3 分割: 本体 / `Runtime` / `PlayerIo`) に inline 化。所有構造を `GameStateContext → RoundManager → RoundStateContext` から `GameStateContext → RoundStateContext` に縮小し、`InternalContext` 経由の二重参照と「同 ctx を 2 つの async ループ (`stateChannel_` in Manager / `eventChannel_` in Context) が奪い合う race」を構造的に除去。共有ヘルパ `AdoptedRoundActionBuilder` (internal static) は通知層とトレース層で再利用

## Phase 5 からの繰越 (Phase 6 冒頭で対応)

- **補助通知の未対応分**: `OtherPlayerTsumoNotification` / `DoraRevealNotification` の送信経路
- **`ResponseCandidateEnumerator` の立直中暗槓精緻化**: 待ち不変条件 (暗槓対象=ツモ牌 / 待ち牌種集合不変 / 順子解釈なし刻子) を満たす場合のみ候補提示
- **合法応答検証の 2 / 3 段目**: 現状は候補リスト membership のみ。意味的検証 (手牌との整合等) と副作用防止を追加
- **対局統合テストの再構築**: 旧 `GameStateContext_IntegrationTests` (流局/子ロン/親ツモ連荘/4 局消化) および `GameStateRoundRunning_RenchanTests` (連荘条件) は削除済 (旧 race の flaky 原因)。`RoundManager` を `RoundStateContext` に統合した本フェーズで「同 ctx を 2 つの async ループが奪い合う race」は構造的に消滅したが、対局レベル統合テストは `GameManager` + `FakePlayer` 経由で「ロン成立」「ツモ連荘で Honba+1」「4 局流局で対局終了」「1 位単独オーラス親あがり止め」「連荘条件別の親継続/親流れ判定」「途中流局 (九種九牌) での同一親本場加算」などのシナリオを Phase 6 の AI 実装と合わせて再構築する

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
