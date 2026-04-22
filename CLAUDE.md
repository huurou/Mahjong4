# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## ビルド・テスト

```bash
# ビルド
dotnet build

# 全テスト実行
dotnet test

# 単一テストクラス実行
dotnet test --filter "FullyQualifiedName~TileKind_ConstructorTests"

# 単一テストメソッド実行
dotnet test --filter "FullyQualifiedName~TileKind_ConstructorTests.有効値0から33_正常に作成される"

# Aspireアプリ起動
dotnet run --project src/Mahjong.AppHost/Mahjong.AppHost.csproj

# 点数計算サンプルアプリ実行（Mahjong.Lib.Scoringの動作確認用コンソール）
dotnet run --project samples/Mahjong.Lib.Scoring.SampleApp/Mahjong.Lib.Scoring.SampleApp.csproj

# 点数計算検証ツール実行（対話的にYYYYMMDDの日付を入力）
dotnet run --project tools/Mahjong.Lib.Scoring.TenhouPaifuValidation/Mahjong.Lib.Scoring.TenhouPaifuValidation.csproj

# 自動対局ツール実行（4人AI対局を指定回数繰り返し、順位統計を出力）
# 牌譜出力は既定で有効。統計計測時は --no-paifu で無効化するとディスク I/O 分だけ速くなる
dotnet run --project tools/Mahjong.Lib.Game.AutoPlay/Mahjong.Lib.Game.AutoPlay.csproj -- --games 100 --seed 1 --no-paifu --parallel 4

# コードカバレッジ計測（PowerShell）
pwsh scripts/TestCoverage.ps1
```

テストランナーはMicrosoft.Testing.Platform（global.jsonで設定済み）。

## アーキテクチャ

.NET 10.0 + .NET Aspire構成の麻雀アプリケーション。設計思想の一次情報は [docs/Design.md](docs/Design.md) / [docs/Requirements.md](docs/Requirements.md)、実装フェーズ計画は [docs/Roadmap.md](docs/Roadmap.md) を参照。

### プロジェクト構成

- **Mahjong.AppHost** — Aspireオーケストレーター。ApiServiceとWebを起動・管理する
- **Mahjong.ApiService** — REST API（OpenAPI対応）。ServiceDefaultsを参照
- **Mahjong.Web** — Blazor Serverフロントエンド。ApiServiceをサービスディスカバリ経由で参照
- **Mahjong.ServiceDefaults** — OpenTelemetry、サービスディスカバリ、レジリエンス等の共有設定
- **Mahjong.Lib.Scoring** — 麻雀の点数計算ドメインロジック。外部NuGet依存なし（追加時は慎重に判断する）
- **Mahjong.Lib.Game** — 麻雀の対局ロジック。Mahjong.Lib.Scoring に `ProjectReference` で直接依存し、点数計算・テンパイ判定を静的ヘルパー (`ScoringHelper` / `TenpaiHelper` / `ShantenHelper`) 経由で呼び出す。牌・手牌・山・河・副露・プレイヤー情報・**対局(Game)と局(Round)の二層状態遷移**を管理する
- **Mahjong.Lib.Scoring.Tests** — Mahjong.Lib.Scoringのテスト（InternalsVisibleToで内部メンバーにアクセス可能）
- **Mahjong.Lib.Game.Tests** — Mahjong.Lib.Gameのテスト。状態遷移の統合テストと各状態の単体テストを含む
- **Mahjong.Lib.Scoring.SampleApp** — `samples/`配下のコンソールアプリ。`HandCalculator.Calc`の代表的な入力例を実行して結果を表示する動作確認用サンプル
- **Mahjong.Lib.Scoring.TenhouPaifuValidation**（`tools/`配下）— 天鳳牌譜をダウンロード・解析し、`HandCalculator`の点数計算結果を実データと突き合わせる検証コンソールアプリ
- **Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests** — 上記ツールのテスト
- **Mahjong.Lib.Game.AutoPlay**（`tools/`配下）— 4人AI自動対局を指定回数繰り返し、順位統計(`StatsReport`) と JSONL 牌譜を出力する検証・評価コンソールアプリ。`AutoPlayRunner` が `GameStateContext` をループ駆動し、`Tracing/StatsTracer` / `Tracing/ProgressTracer` / `TenhouJsonPaifuRecorder` を `CompositeGameTracer` で束ねて流し込む
- **Mahjong.Lib.Game.AutoPlay.Tests** — 上記ツールのテスト（`AutoPlayRunner_SmokeTests` / `StatsTracer_BuildTests` / `MixedPlayerFactory_CreateTests`）

ソリューションフォルダ構成は `Mahjong4.slnx` を参照。

### ドメイン分割の要点

- **Mahjong.Lib.Scoring** は点数計算専用で、牌種別 (`TileKind`、34種) を扱う。**Mahjong.Lib.Game** は対局進行専用で、牌1枚を個別識別する`Tile` (0-135) を扱う。`Tile.Kind` は Lib.Scoring の `TileKind` 型 (`TileKind.All[Id / 4]`) を直接返し、Lib.Game と Lib.Scoring は `TileKind` を共通語彙として共有する
- **Lib.Game → Lib.Scoring の依存境界**: Lib.Game は Lib.Scoring に**直接依存する**（`ProjectReference`）。点数計算・テンパイ判定・向聴計算は静的ヘルパー経由で Lib.Scoring を呼び出す:
  - 和了点計算: [ScoringHelper.Calculate](src/Mahjong.Lib.Game/Games/ScoringHelper.cs) が `HandCalculator.Calc` をラップし、親子・ツモロン分配込みの `ScoreResult` を返す。`HandResult.ErrorMessage` が返った場合は `InvalidOperationException` を throw し、`RoundStateContext.ProcessEventAsync` の catch 経由で `InvalidEventReceived` に回送される (役なしロンの 0 点黙認を防ぐ)
  - テンパイ判定・待ち牌列挙・立直中暗槓の送り槓判定: [TenpaiHelper](src/Mahjong.Lib.Game/Tenpai/TenpaiHelper.cs) が `ShantenCalculator` / `HandDivider` を呼び出す
  - 向聴計算・有効牌列挙: [ShantenHelper](src/Mahjong.Lib.Game/Tenpai/ShantenHelper.cs) が `ShantenCalculator` を呼び出す
  - 役ありシャンテン計算: [YakuAwareShantenHelper](src/Mahjong.Lib.Game/Tenpai/YakuAwareShantenHelper.cs) は 7 経路 (門前 / 翻牌 / 断么九 / 対々和 / 一色手 3 種) の最小シャンテンを返し、副露前提の AI (v0.5.0 鳴き) が鳴き採否と打牌評価値を判断する基盤になる
  - 型変換: [ScoringConversions](src/Mahjong.Lib.Game/Games/ScoringConversions.cs) が `Hand` → `TileKindList`、`Call` → `Scoring.Calls.Call`、`GameRules` → `Scoring.Games.GameRules` の境界変換を提供する
- 以前存在した `Mahjong.Lib.Game.Scoring` ブリッジプロジェクトおよび `IScoreCalculator` / `ITenpaiChecker` / `IShantenEvaluator` 抽象は撤廃済み。将来「特殊ルールで点数計算を差し替える」要件が発生した場合は静的ヘルパーを抽象化し直す
- 牌種別 (TileKind) と牌 (Tile) の設計意図、副露種類、対局/局の状態遷移図は [docs/Design.md](docs/Design.md) が一次情報
- 両ライブラリとも record + `ImmutableList`/`ImmutableArray` のイミュータブル設計を採用

### 点数計算ドメイン（Mahjong.Lib.Scoring）

設計の全体像は [docs/Design.md](docs/Design.md) を参照。以下はコード上の横断的な約束事のみ:

- **シングルトン型**: `TileKind` / `Fu` / `Yaku` はstaticプロパティでシングルトンを公開し、コンストラクタはinternal。常に `TileKind.Man1` / `Fu.Futei` / `Yaku.Pinfu` のようにプロパティ参照で取得する（`new`禁止）
- **コレクション式対応**: `TileKindList` / `TileKindListList` / `CallList` / `Hand` / `FuList` / `YakuList` は `[CollectionBuilder]` 属性 + ネストされた`Builder`クラスで `[.. ]` 構文に対応
- **公開入口**:
  - [Mahjong.Lib.Scoring/Shantens/ShantenCalculator.cs](src/Mahjong.Lib.Scoring/Shantens/ShantenCalculator.cs) — `Calc(TileKindList, useRegular, useChiitoitsu, useKokushi)` で最小向聴数を返す（`SHANTEN_TENPAI=0`, `SHANTEN_AGARI=-1`）
  - [Mahjong.Lib.Scoring/HandCalculating/HandCalculator.cs](src/Mahjong.Lib.Scoring/HandCalculating/HandCalculator.cs) — `Calc(tileKindList, winTile, callList?, doraIndicators?, uradoraIndicators?, winSituation?, gameRules?)` で `HandResult` を返す
- **和了計算パイプライン**: `HandValidator`（入力検証） → `SpecialHandEvaluator`（流し満貫・国士無双等） → `HandDivider`（面子分解、複数解） → 分解候補ごとに `YakuEvaluator` / `FuCalculator` / `ScoreCalculator` → **高点法**で最高点を選択
- **結果型**: `HandResult`（Fu/Han/Score/YakuList/FuList/ErrorMessage）。`HandResult.Create` は0翻時に自動で `Error("役がありません。")` を返す。`Score(Main, Sub)` の意味は和了方（親子・ツモロン）で変わる（XMLコメント参照）
- **役実装規約**: [Mahjong.Lib.Scoring/Yakus/Impl/](src/Mahjong.Lib.Scoring/Yakus/Impl/) 配下の具象役は `static Valid(...)` メソッドで成立判定。引数は役ごとに必要なものだけ（Hand / CallList / WinSituation / GameRules 等）
- **符計算の特殊ケース**: 七対子は常に25符固定、それ以外は10の位に切り上げ

### 対局ドメイン（Mahjong.Lib.Game）

対局/局の状態遷移図、状態の意味、副露の扱いは [docs/Design.md](docs/Design.md) が一次情報。Lib.Game は **局(Round)** と **対局(Game)** の二層構造で、対局レベル状態機械が局レベル状態機械をホストする。以下はコード上の構造のみ:

#### 共通

- **牌ID規約**: `Tile.Id` は 0-135、`Tile.Kind` は `Id / 4` で 0-33 の牌種ID
- **プレイヤー別配列**: `HandArray` / `RiverArray` / `CallListArray` / `PointArray` は `ImmutableArray` + `PlayerIndex` でインデックス参照するイミュータブルラッパー。更新メソッドは新インスタンスを返す
- **山生成**: [Mahjong.Lib.Game/Walls/IWallGenerator.cs](src/Mahjong.Lib.Game/Walls/IWallGenerator.cs) が抽象、`WallGeneratorTenhou` が Mersenne Twister(MT19937) + SHA512 の天鳳互換実装

#### 局(Round)レベル

- **局の集約**: [Mahjong.Lib.Game/Rounds/Round.cs](src/Mahjong.Lib.Game/Rounds/Round.cs) が局の全状態を保持するイミュータブルrecord。`Haipai` / `Tsumo` / `Dahai` / `NextTurn` / `Chi` / `Pon` / `Daiminkan` / `Ankan` / `Kakan` / `RinshanTsumo` / `RevealDora` / `SetPoints` / `AddKyoutakuRiichi` / `ClearKyoutaku` / `PendRiichi` / `ConfirmRiichi` / `CancelRiichi` / `EvaluateFuriten` / `SettleWin` / `SettleRyuukyoku` の各メソッドで進行。すべて新しい `Round` を返す
- **局内プレイヤー状態**: `Round` は `PlayerRoundStatusArray` を保持し、各プレイヤーの立直保留/確定/取消、一発、門前、流し満貫資格、同巡/永久フリテン、嶺上中、第一打前(天和/地和判定用)、`SafeKindsAgainstRiichi`（立直宣言者から見た現物牌種集合 — `ConfirmRiichi` で自河を初期投入し、以後 `Dahai` で他家打牌の牌種を追加。副露で河から消えた牌も保持）などを進行メソッド内で更新する。`SafeKindsAgainstRiichi` は守備型 AI の安全牌判定の元情報として `OwnRoundStatus` / `VisiblePlayerRoundStatus` 経由でビュー層にも露出する
- **点数精算**: 和了時は呼び出し側 (`RoundStateDahai` / `RoundStateTsumo` / `RoundStateKan` / `RoundStateKanTsumo`) が `ScoringHelper.Calculate` で各和了者の `ScoreResult` を算出し、`Round.SettleWin` に `ImmutableArray<ScoreResult>` を渡して点数移動を適用する。流局時は `Round.SettleRyuukyoku` が天鳳ルール準拠でノーテン罰符・テンパイ料を計算する。いずれも終端状態への遷移アクション内で呼ばれる
- **カンドラ表示のタイミング**: 暗槓は即時 `RevealDora`、加槓・大明槓は `PendingDoraReveal=true` で保留し、次の `RinshanTsumo` 直前にめくる
- **副露種類の扱い**: `CallType` は Chi/Pon/Ankan/Daiminkan/Kakan。Lib.Scoring側と違い大明槓と加槓を区別する（表示上の差があるため — 詳細は [docs/Design.md](docs/Design.md)）
- **状態遷移の実装** ([Mahjong.Lib.Game/States/RoundStates/](src/Mahjong.Lib.Game/States/RoundStates/)): 非同期イベント駆動のステートマシン。`System.Threading.Channels.Channel<T>` によるイベントキューでスレッドセーフに状態遷移する
  - `RoundState` — 抽象基底。`ResponseOk` / `ResponseWin` / `ResponseDahai` / `ResponseKan` / `ResponseCall` / `ResponseRyuukyoku` の仮想メソッドと `Entry` / `Exit` ライフサイクルを持つ
  - `RoundEvent` — 抽象基底。6種の具象実装（ResponseOk, ResponseDahai, ResponseCall, ResponseWin, ResponseKan, ResponseRyuukyoku）
  - `RoundStateContext` — 局進行コンテキスト (partial 3 分割: 本体 / `Runtime` / `PlayerIo`)。状態機械 (`eventChannel_` / `Transit`) と通知・応答集約ループ (`StartAsync` / `ProcessRuntimeAsync`) を 1 クラスに統合。コンストラクタで `PlayerList` / `IRoundViewProjector` / `IResponseCandidateEnumerator` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `GameRules` / `IGameTracer` / `ILogger<RoundStateContext>` の 8 引数を受け取る。公開 API は `ctor` と `StartAsync(round, ct)` の 2 つのみ。`State` / `Round` プロパティは `internal set` で同 assembly のみ書換可。`Response*Async` 6 個はすべて `private` (外部からの状態駆動経路は物理的に封鎖、race の構造的除去)。`RoundStateChanged` / `RoundEnded` / `InvalidEventReceived` イベントで遷移通知。`IDisposable` で 5 秒タイムアウト付きシャットダウン
  - **局終了通知**: 終端状態（`RoundStateWin` / `RoundStateRyuukyoku`）がOK応答時に `RoundStateContext.RoundEnded` イベント（`RoundEndedEventArgs`）を一度だけ発火する。`GameStateContext` はこれを購読し `GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku` に変換して対局レベル遷移に昇格させる
  - **不正応答**: 現状態で受け付けない応答が来た場合、例外で処理ループを止めず `InvalidEventReceived` イベントに通知して続行する
  - 状態遷移フローの詳細（配牌→ツモ→打牌サイクル、副露/ロン/流局の分岐、槓サイクル、終端状態）は [docs/Design.md](docs/Design.md) の状態遷移図を参照

#### 通知・応答型・プレイヤー抽象（Phase 3-4）

プレイヤーとの入出力境界は型で分離する。通知送信・応答集約・優先順位解決は `RoundStateContext` の通知・応答集約ループ (`StartAsync` / `ProcessRuntimeAsync`、partial: `RoundStateContext.Runtime.cs` / `RoundStateContext.PlayerIo.cs`) で扱う。

- **応答候補** ([Mahjong.Lib.Game/Candidates/](src/Mahjong.Lib.Game/Candidates/)) — サーバーが提示する合法応答候補。UX/選択肢提示用であり、受信応答は必ずサーバー側で再検証する
- **プレイヤー応答** ([Mahjong.Lib.Game/Responses/](src/Mahjong.Lib.Game/Responses/)) — C# API 層の応答型。`AfterTsumoResponse` / `AfterDahaiResponse` / `AfterKanResponse` / `AfterKanTsumoResponse` など局面ごとに戻り値型を分け、コンパイル時の型安全を優先する
- **Wire DTO** ([Mahjong.Lib.Game/Notifications/](src/Mahjong.Lib.Game/Notifications/)) — `PlayerNotification` / `PlayerResponseEnvelope` は通知ID(UUIDv7)・局Revision・応答者を含む通信・シリアライズ用エンベロープ
- **C# API ⇔ Wire DTO 変換**: `this` 対象型ごとの拡張メソッドクラスに分離。[GameNotificationExtensions.ToWire](src/Mahjong.Lib.Game/Notifications/GameNotificationExtensions.cs) / [RoundNotificationExtensions.ToWire](src/Mahjong.Lib.Game/Notifications/RoundNotificationExtensions.cs) / [PlayerResponseEnvelopeExtensions.FromWire(RoundInquiryPhase)](src/Mahjong.Lib.Game/Notifications/PlayerResponseEnvelopeExtensions.cs) / [PlayerResponseExtensions.ToBody](src/Mahjong.Lib.Game/Notifications/PlayerResponseExtensions.cs)。`FromWire` は `RoundInquiryPhase` を引数に取り Tsumo/Dahai/Kan/KanTsumo/AfterKanTsumo/AfterCall 各フェーズで許可される応答本体のみを受理する。Envelope 化 (`NotificationId` / `RoundRevision` / `PlayerIndex` 付与) と合法性検証は `RoundStateContext` の通知・応答集約ループの責務
- **プレイヤー視点フィルタ** ([Mahjong.Lib.Game/Views/](src/Mahjong.Lib.Game/Views/)) — `PlayerRoundView` は自分の手牌・非公開状態（フリテン等）と、他家の公開情報を分離する情報射影
- **問い合わせ仕様** ([Mahjong.Lib.Game/Inquiries/](src/Mahjong.Lib.Game/Inquiries/)) — `RoundInquirySpec` は「誰に何を聞くか」
- **採用結果** ([Mahjong.Lib.Game/Adoptions/](src/Mahjong.Lib.Game/Adoptions/)) — `AdoptedRoundAction` は応答集約・優先順位適用後の採用結果 (和了/流局/打牌/副露/槓 等)。和了者 (`AdoptedWinner`) と供託リーチの扱い (`KyoutakuRiichiAward`) もここに属する
- **対局進行の抽象層** ([Mahjong.Lib.Game/Rounds/Managing/](src/Mahjong.Lib.Game/Rounds/Managing/)) — `IRoundViewProjector` (視点射影) / `IResponseCandidateEnumerator` (合法応答候補列挙) / `IResponsePriorityPolicy` (応答優先順位) / `IDefaultResponseFactory` (タイムアウト時既定応答) / `IGameTracer` (対局トレーサー) の 5 抽象と Tenhou/Default/Null 実装。`AdoptedRoundActionBuilder` / `ResolvedPlayerResponse` / `ResponseValidator` も通知・応答集約ループの補助型としてここにある
- **点数結果型** ([Mahjong.Lib.Game/Games/ScoreResult.cs](src/Mahjong.Lib.Game/Games/ScoreResult.cs)) — `ScoreResult(Han, Fu, PointDeltas, YakuList)` は `ScoringHelper.Calculate` の戻り値。成立役は `Mahjong.Lib.Scoring.Yakus.YakuList` (= `Yaku` 具象型の集合) をそのまま保持する。包 (責任払い) 判定は [YakuPaoExtensions.HasPaoEligibleYaku](src/Mahjong.Lib.Game/Games/YakuPaoExtensions.cs) で `Daisangen` / `Daisuushii` / `DaisuushiiDouble` / `Suukantsu` を pattern match する
- **プレイヤー抽象** ([Mahjong.Lib.Game/Players/Player.cs](src/Mahjong.Lib.Game/Players/Player.cs)) — `Player` は abstract **class**（record ではない: 可変状態を持つ AI/人間実装での record + mutable property の相互作用リスク回避）。`PlayerId` + `DisplayName` ベースのカスタム `Equals` / `GetHashCode` / `==` / `!=` を実装し `PlayerList` / `Game` の value equality を維持。通知メソッド 17 種 (`OnGameStartAsync` / `OnRoundStartAsync` / `OnHaipaiAsync` / `OnTsumoAsync` / `OnOtherPlayerTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnAfterCallAsync` / `OnOtherPlayerAfterCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnOtherPlayerKanTsumoAsync` / `OnDoraRevealAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnRoundEndAsync` / `OnGameEndAsync`) を `public abstract` で定義し、戻り値は通知ごとに異なる応答型。`OnAfterCallAsync` は副露 (チー/ポン) 直後の副露者打牌要求で戻り値 `DahaiResponse` (非門前のため立直不可)。全メソッドに `CancellationToken ct = default` を付与 (`RoundStateContext` の通知・応答集約ループのタイムアウト制御用)
- **テスト用疑似プレイヤー** ([tests/Mahjong.Lib.Game.Tests/Players/FakePlayer.cs](tests/Mahjong.Lib.Game.Tests/Players/FakePlayer.cs)) — `Func<TNotification, CancellationToken, TResponse>` デリゲート群を init プロパティで受け取る `Player` 実装。未設定時は安全な既定応答（OK / 先頭 `DahaiCandidate` を打牌 / `PassResponse` / `KanPassResponse` 等）を返し、受信通知を `ReceivedNotifications` に記録する
- **AI プレイヤー実装** ([src/Mahjong.Lib.Game/Players/Impl/](src/Mahjong.Lib.Game/Players/Impl/)) — 命名は `AI_v{major}_{minor}_{patch}_{識別名}` 形式。v0.1.0 ランダム打牌、v0.2.0 有効牌（`ShantenHelper` で向聴数を下げる候補を選ぶ）、v0.3.0 評価値（有効牌枚数同点時のタイブレーカーとして、対象牌を孤立牌と見立てた面子完成ポテンシャル評価値が低い牌を優先）、v0.4.0 回し打ち（v0.3.0 の打牌選択に加え、他家リーチ時のみ自分のシャンテンと攻撃打牌の危険度で押し/回し/ベタオリを切り替える守備層。危険度は内部の `DangerEvaluator` が `SafeKindsAgainstRiichi` を元に現物・スジ・字牌見え枚数で算出）、v0.5.0 鳴き（v0.4.0 の回し打ち守備を維持しつつ `YakuAwareShantenHelper` の役ありシャンテンに基づきチー/ポン/大明槓/暗槓/加槓の採否を判断、打牌評価値は `ev = Σ unseen(K) × {Pon:×4 / Chi:×2 / None:×1}`）。AI の設計思想・評価式は [docs/AiDesign.md](docs/AiDesign.md) を参照
- **AI Factory 共通基底** ([src/Mahjong.Lib.Game/Players/AiPlayerFactoryBase.cs](src/Mahjong.Lib.Game/Players/AiPlayerFactoryBase.cs)) — 各 AI バージョンの `{AI名}Factory` は `AiPlayerFactoryBase<TPlayer>(int seed, string displayName)` を派生して `CreatePlayer(PlayerId, PlayerIndex, Random)` のみを実装する。基底側で seed と席 index を Fibonacci hashing (Knuth multiplicative `0x9E3779B9u`) で合成し、席ごとに独立かつ決定的な `Random` を生成する。`HashCode.Combine` はプロセス起動時のランダム salt を含み再現性を壊すため使わない

#### 対局(Game)レベル

- **対局の集約**: [Mahjong.Lib.Game/Games/Game.cs](src/Mahjong.Lib.Game/Games/Game.cs) が対局全体の状態を保持するイミュータブルrecord（PlayerList/Rules/RoundWind/RoundNumber/Honba/KyoutakuRiichiCount/PointArray）。`Create` / `CreateRound` / `ApplyRoundResult` / `AdvanceToNextRound` で進行
- **プレイヤー**: [Mahjong.Lib.Game/Players/](src/Mahjong.Lib.Game/Players/) の `Player`（`PlayerId` + 表示名）と `PlayerList`（4人固定、index 0 が**起家**。並び替えは呼び出し側責務）
- **対局ルール**: [Mahjong.Lib.Game/Games/GameRules.cs](src/Mahjong.Lib.Game/Games/GameRules.cs) に対局形式（`GameFormat`: SingleRound/Tonpuu/Tonnan）、赤ドラ集合、初期持ち点、トビ閾値、食いタン/後付け、連荘条件（`RenchanCondition`）を集約
- **対局終了判定**: [Mahjong.Lib.Game/Games/GameEndPolicy.cs](src/Mahjong.Lib.Game/Games/GameEndPolicy.cs) `ShouldEndAfterRound(game, event, dealerContinues)` で判定。呼び出し順は **ApplyRoundResult → ShouldEndAfterRound → (false なら AdvanceToNextRound)**
- **対局の外部入口**: [Mahjong.Lib.Game/States/GameStates/GameStateContext.cs](src/Mahjong.Lib.Game/States/GameStates/GameStateContext.cs) が `StartAsync(ct)` で対局を開始し `IDisposable` で管理。公開 API は `ctor` と `StartAsync(CancellationToken)` の 2 つのみ (RoundStateContext と対称)。コンストラクタで `PlayerList` / `GameRules` / `IWallGenerator` / `IRoundViewProjector` / `IResponseCandidateEnumerator` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IGameTracer` / `ILogger<GameStateContext>` / `ILogger<RoundStateContext>` の 10 引数を受け取り、`RoundStateContext` が必要とする依存を各局生成時に供給する
- **対局レベル状態機械** ([Mahjong.Lib.Game/States/GameStates/](src/Mahjong.Lib.Game/States/GameStates/)): Round層と同じ Channel ベースのイベント駆動。`GameState`（Init/RoundRunning/End）と `GameEvent`（ResponseOk/RoundEndedByWin/RoundEndedByRyuukyoku）
  - `GameStateContext` は `RoundStateContext` をホストし、`RoundEnded` イベントを購読して `RoundEndedEventArgs` を `GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku` に変換・内部発行して対局レベル遷移に昇格させる（多重発火抑止付き）
  - `GameStateRoundRunning` の `RoundEndedByWin` / `RoundEndedByRyuukyoku` ハンドラ内で `Game.ApplyRoundResult` → `GameEndPolicy.ShouldEndAfterRound` → （続行なら `Game.AdvanceToNextRound` + 次局 `RoundStateContext` 生成、終了なら `GameStateEnd` 遷移）の順で処理する

### 点数計算検証ツール（tools/Mahjong.Lib.Scoring.TenhouPaifuValidation/）

`HandCalculator` の実装を天鳳の実牌譜に対して総当たり検証するコンソールアプリ。`Microsoft.Extensions.DependencyInjection` でサービスを組み立て、対話的に入力された日付（YYYYMMDD）の牌譜を処理する。

`UseCase` を起点に2段階のパイプライン:
1. **AnalysisPaifu** — `PaifuDownloadService`（天鳳からダウンロード） → `RoundDataExtractService`（局単位に分解） → `InitParseService` / `AgariParseService` / `MeldParseService`（XMLタグ→ドメインモデル） → `AgariInfoBuildService`（`AgariInfo` 生成）
2. **ValidateCalc** — `CalcValidateService` が `AgariInfo` から `HandCalculator.Calc` を呼び出し、実データの符・翻・点数と突き合わせて `ValidateResult` を返す

テストデータは [tests/Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests/TestData/](tests/Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests/TestData/) 配下のXML牌譜（途中流局有無・ダブロン等のバリエーション）。

### 自動対局ツール（tools/Mahjong.Lib.Game.AutoPlay/）

AI同士の4人対局を一括実行して統計を取るコンソールアプリ。`Microsoft.Extensions.DependencyInjection` で `GameStateContext` に必要な抽象を組み立てる。

- **実行時オプション** (`AutoPlayOptions`): `--games` 回数 / `--seed` 乱数シード / `--output` 出力先 / `--no-paifu` JSONL 牌譜出力の無効化フラグ (既定は出力あり。統計だけ取りたい時は指定して I/O を省く) / `--parallel` 並列 worker 数（0 以下で `Environment.ProcessorCount`、既定値は 4。ベンチマーク結果で wall-clock 最小だった設定）
- **対局ランナー** (`AutoPlayRunner`): 並列 worker 数に対局を均等分配し、各 worker が独立 `StatsTracer` で集計した結果を最後に `StatsTracer.Merge` で統合する（`Parallel.ForEachAsync` ベース）。1 対局ごとに `PlayerList` を生成し `GameStateContext` を `using` で起動、`GameStateEnd` を監視して次対局へ進む。対局単位の 5 分タイムアウトと例外時のスキップ＋次局継続を担う
- **プレイヤー構成**: [MixedPlayerFactory](tools/Mahjong.Lib.Game.AutoPlay/MixedPlayerFactory.cs) が `IPlayerFactory[]` を受け取り、各対局開始時に席配置をシャッフル。`Program.cs` で AI を組み替えると混在対局の統計が取れる
- **トレーサー合成**: [CompositeGameTracer](src/Mahjong.Lib.Game/Rounds/Managing/CompositeGameTracer.cs) に [StatsTracer](tools/Mahjong.Lib.Game.AutoPlay/Tracing/StatsTracer.cs)（順位・和了率・流局率集計）、[ProgressTracer](tools/Mahjong.Lib.Game.AutoPlay/Tracing/ProgressTracer.cs)（局進行のログ出力）、必要に応じて [TenhouJsonPaifuRecorder](src/Mahjong.Lib.Game/Paifu/TenhouJsonPaifuRecorder.cs)（天鳳 JSONL 牌譜エディタ互換形式で書き出し。ファイル生成は [TenhouPaifuFileSink](tools/Mahjong.Lib.Game.AutoPlay/Paifu/TenhouPaifuFileSink.cs) が担う）を束ねて `IGameTracer` として渡す
- **山牌生成**: AutoPlay は [WallGeneratorTenhou](src/Mahjong.Lib.Game/Walls/WallGeneratorTenhou.cs) を直接使用し、シード文字列を `options.Seed` と `gameNumber` から決定的に派生させて 2496 バイトの乱数列を生成することで対局ごとに独立かつ再現可能な山を作る（`AutoPlayRunner.RunOneGameAsync` 内）
- **出力**: `StatsReportFormatter.Format` が AI種別別の順位分布・平均順位・和了/放銃/立直/副露率・平均打点に加え、役出現上位20件と流局種別別回数を整形表示する

## 作業上の注意

- `.claude/settings.json`のStopフックにより、セッション終了時にソースファイルがUTF-8 BOM付きに自動変換される

## コーディング規約

- プライベートフィールド: キャメルケース + `_`サフィックス（例: `myField_`）
- 定数（const）: UPPER_SNAKE_CASE
- LINQの引数にはx, y, zを使用

## テスト規約

- フレームワーク: xUnit v3（`xunit.v3.mtp-v2`）
- テストクラス命名: `対象クラス_対象メソッドTests`（例: `TileKind_ConstructorTests`）
- テストメソッド命名: 日本語で「条件_期待される挙動」（例: `有効値0から33_正常に作成される`）
- AAA（Arrange-Act-Assert）パターンを使用
- 例外テストは`Record.Exception`を使用（`Assert.Throws`は使わない）
- Fluent Assertionは使用禁止
- テストコードにドキュメントコメントを付与しない
- テストはfeatureドメインごとのディレクトリに整理する（Lib.Scoring.Tests: Tiles/, Calls/, Fus/, Games/, Yakus/, Shantens/, HandCalculating/　Lib.Game.Tests: Adoptions/, Calls/, Candidates/, Games/, Hands/, Inquiries/, Notifications/, Paifu/, Players/, Responses/, Rivers/, Rounds/, States/, Tenpai/, Views/, Walls/）
- `HandCalculator.Calc`のテストは入力カテゴリ別にクラスを分割（例: `HandCalculator_CalcTests_Shuntsu`、`_Koutsu`、`_Kokushimusou`、`_Dora`、`_Error`、`_Formless`、`_Tenhou`、`_Others`）
