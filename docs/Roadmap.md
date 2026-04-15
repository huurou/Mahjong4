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

## Phase 1: 対局(Game)集約とステートマシンの器

**目的**: 対局全体を統括する「器」を用意する。点数精算ロジックと局内プレイヤー状態は Phase 2 に分離する。通知/応答の仕組みはこの時点では未着手で、テストは`GameStateContext`の直接駆動で行う。

- ファイル配置 — `Mahjong.Lib.Game/Games/` フォルダを新設(既存の`Rounds/`と並列)
- `GameRules` (仮称) — 対局前に決まるルール。対局形式(東風戦/東南戦、既定:東南戦) / 赤ドラ枚数(既定:赤五萬1/赤五筒1/赤五索1) / 初期持ち点(当面35000) / オーラス親あがり止め(1位単独確定のみ) / トビ終了点(天鳳準拠:0点未満) / 食いタン / 後付け / 連荘条件 / `IsRedDora(Tile)` 判定メソッド / 対局形式 `GameFormat.Tonpuu` / `Tonnan`(既定) / `SingleRound`
- `GameConfig` record — 対局固有パラメータ(`PlayerList` / `GameRules` / 起家`PlayerIndex` / タイムアウト設定 等)。起家は上位層で乱数決定した`PlayerIndex`を外部注入
- `PlayerList` record — `ImmutableArray<PlayerProfile>`(席情報のみの純データ)のラッパー、`PlayerIndex`でアクセス。**`IPlayer` 実体は保持しない**。Phase 1時点では **空の`IPlayer` interface**(マーカー)を`Players/`配下に用意し、Phase 4で本物に差し替える。`IPlayer[]` 実体は `GameManager` が別途 `IPlayerEndpoint[]` として保持する
- `Game` record (immutable) — 対局全体の状態(`PlayerList` / 局順 / 本場 / 供託 / 持ち点 / ルール等)
- `Round` 結果情報保持の追加 — 和了情報(誰が/誰から/役構成/符翻/点数)・流局情報(種別/テンパイ者)を`Round`に持たせる(点数計算は Phase 2 で接続)
- `GameState` 抽象 + 具象 (`GameInit` / `RoundInit` / `RoundEnd` / `GameEnd`)
- `GameStateContext` — `Round*`系と同じ非同期イベント駆動。内部で`RoundStateContext`を生成・破棄。Phase 1時点のテストは既存`RoundStateContext`と同様に`ResponseXxxAsync`系の直接駆動
- `GameManager` — コンストラクタで`GameConfig` + `IWallGenerator` + (Phase 2で注入予定の `IScoreCalculator`) 等を受け取る。対局開始処理・局間の引き継ぎ・対局終了判定を統括。`IPlayer[]` 実体の保持もここが担当
- `RoundManager` / 通信・集約レイヤーは Phase 5 で導入。Phase 1 では未着手

**Phase 1 完了の定義**:

- テストから直接駆動で `GameInit` → `RoundInit` → (Round 既存駆動で進行) → `RoundEnd` → 次局 `RoundInit` → ... → `GameEnd` の遷移が回る
- `GameRules` / `GameConfig` / `Game` / `PlayerList` / `GameState` / `GameStateContext` / `GameManager` が生成・破棄可能
- 点数精算・流局精算・連荘/本場処理は **スタブ**(固定値返却や手計算固定値)で十分。統合テストは状態遷移の流れのみ検証
- 対局終了条件(オーラス親あがり止め / トビ / 規定局数消化)の分岐が`GameManager`で評価される(実値判定ではなくフック点が存在するレベルで可)

## Phase 2: 点数精算と局内プレイヤー状態

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

## Phase 3: 通知・応答の型定義

**目的**: プレイヤー通知・応答の型体系を固める。型のみだが、Phase 4 の `IPlayer` 戻り値型は本フェーズの応答型に直接依存するため、Phase 4 の API 形状もあわせて想定して型設計する。

- `PlayerNotification` 抽象 + 種別別具象 (`HaipaiNotification` / `TsumoNotification` / `DahaiNotification` / `CallNotification` / `KanNotification` / `KanTsumoNotification` / `WinNotification` / `RyuukyokuNotification` / `DoraRevealNotification` / `GameStartNotification` / `GameEndNotification` / `RoundStartNotification` / `RoundEndNotification` …)
- `PlayerResponse` 抽象 + 種別別具象 (`OkResponse` / `DahaiResponse(Tile, isRiichi)` / `CallResponse` / `KanResponse` / `WinResponse` / `RyuukyokuResponse`) および Wire DTO 用 envelope
- `ResponseCandidate` 抽象 + 具象 (`OkCandidate` / `RonCandidate` / `ChiCandidate(HandTiles)` / `PonCandidate(HandTiles)` / `DaiminkanCandidate(HandTiles)` / `AnkanCandidate(Tile)` / `KakanCandidate(Tile)` / `TsumoCandidate` / `DahaiCandidate(Tile[], riichiAvailable)` / `KyuushuKyuuhaiCandidate`)
- `RoundDecisionSpec` — State が返す「何を誰に聞くか」の仕様
- `ResolvedRoundAction` — 優先順位適用後の採用済み結果。`ResolvedWinAction` は `Winners[]` / `Loser?` / `WinType` / `KyoutakuDistribution` / `HonbaDistribution` / `DealerContinues` を保持(詳細は Design.md 参照)
- `PlayerRoundView` — プレイヤー視点で情報フィルタ済みの卓情報

## Phase 4: IPlayer / PlayerBase 抽象

**目的**: プレイヤー側の抽象層を整備する。

- `IPlayer` interface — 種別ごとの通知メソッド (`OnGameStartAsync` / `OnRoundStartAsync` / `OnHaipaiAsync` / `OnTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnDoraRevealAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnRoundEndAsync` / `OnGameEndAsync`)。各メソッドの戻り値は通知ごとに型が異なる(Phase 3 の応答型を直接戻す)
- `PlayerBase` 抽象基底 — 視点卓情報(`PlayerRoundView`相当)の保持・更新、通知イベントの共通処理、C# API ⇔ Wire DTO 二層構造の変換基盤
- `FakePlayer` (test用) — テストシナリオ記述用の疑似プレイヤー実装

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

- `PlayerBase`継承の人間プレイヤー実装 (UI入力を待つ形)
- transport adapter (WebSocket / SignalR 等)
- `Mahjong.ApiService` / `Mahjong.Web` との統合
- 接続断/再接続処理
