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

## Phase 4: Player 拡張

**目的**: プレイヤー側の抽象層を整備する。`Player` abstract record に通知・応答メソッドを追加し、`PlayerList` (= `IEnumerable<Player>`) 一本でプレイヤー情報と実体を管理する。

- `Player` に通知メソッドを追加 — 種別ごとの通知メソッド (`OnGameStartAsync` / `OnRoundStartAsync` / `OnHaipaiAsync` / `OnTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnDoraRevealAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnRoundEndAsync` / `OnGameEndAsync`) を virtual / abstract で定義する。各メソッドの戻り値は通知ごとに型が異なる(Phase 3 の応答型を直接戻す)
- 視点卓情報(`PlayerRoundView`相当)の保持・更新、通知イベントの共通処理、C# API ⇔ Wire DTO 二層構造の変換基盤は `Player` の共通実装として持たせる
- `FakePlayer` (test用) — テストシナリオ記述用の疑似プレイヤー実装 (`Player` のサブタイプ)

## Phase 5: RoundManager と通信・集約レイヤー

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
