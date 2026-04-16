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
- **Mahjong.Lib.Game** — 麻雀の対局ロジック。Mahjong.Lib.Scoringには依存せず、対局進行ドメインとして独立している。牌・手牌・山・河・副露・プレイヤー情報・**対局(Game)と局(Round)の二層状態遷移**を管理する
- **Mahjong.Lib.Scoring.Tests** — Mahjong.Lib.Scoringのテスト（InternalsVisibleToで内部メンバーにアクセス可能）
- **Mahjong.Lib.Game.Tests** — Mahjong.Lib.Gameのテスト。状態遷移の統合テストと各状態の単体テストを含む
- **Mahjong.Lib.Scoring.SampleApp** — `samples/`配下のコンソールアプリ。`HandCalculator.Calc`の代表的な入力例を実行して結果を表示する動作確認用サンプル
- **Mahjong.Lib.Scoring.TenhouPaifuValidation**（`tools/`配下）— 天鳳牌譜をダウンロード・解析し、`HandCalculator`の点数計算結果を実データと突き合わせる検証コンソールアプリ
- **Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests** — 上記ツールのテスト

ソリューションフォルダ構成は `Mahjong4.slnx` を参照。

### ドメイン分割の要点

- **Mahjong.Lib.Scoring** は点数計算専用で、牌種別(`TileKind`、34種)のみを扱う。**Mahjong.Lib.Game** は対局進行専用で、牌1枚を個別識別する`Tile`(0-135)を扱う。両者は相互参照しない。必要なら`Tile.Kind`(= `Id/4`) 経由でLib.Scoring側の牌種インデックスへ変換する
- **Lib.Game → Lib.Scoring の依存境界**: Lib.Game は Lib.Scoring に直接依存しない。和了点計算は `IScoreCalculator`、テンパイ/待ち判定は `ITenpaiChecker` を Lib.Game 側の抽象として定義し、Lib.Scoring をラップする実装は上位層（ApiService等）から注入する
- 牌種別(TileKind)と牌(Tile)の設計意図、副露種類、対局/局の状態遷移図は [docs/Design.md](docs/Design.md) が一次情報
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
- **局内プレイヤー状態**: `Round` は `PlayerRoundStatusArray` を保持し、各プレイヤーの立直保留/確定/取消、一発、門前、流し満貫資格、同巡/永久フリテン、嶺上中、第一打前(天和/地和判定用)などを進行メソッド内で更新する
- **点数精算**: 和了時は `Round.SettleWin` が `IScoreCalculator` 経由で点数計算・移動を行い、流局時は `Round.SettleRyuukyoku` が天鳳ルール準拠でノーテン罰符・テンパイ料を計算する。いずれも終端状態への遷移アクション内で呼ばれる
- **カンドラ表示のタイミング**: 暗槓は即時 `RevealDora`、加槓・大明槓は `PendingDoraReveal=true` で保留し、次の `RinshanTsumo` 直前にめくる
- **副露種類の扱い**: `CallType` は Chi/Pon/Ankan/Daiminkan/Kakan。Lib.Scoring側と違い大明槓と加槓を区別する（表示上の差があるため — 詳細は [docs/Design.md](docs/Design.md)）
- **状態遷移の実装** ([Mahjong.Lib.Game/States/RoundStates/](src/Mahjong.Lib.Game/States/RoundStates/)): 非同期イベント駆動のステートマシン。`System.Threading.Channels.Channel<T>` によるイベントキューでスレッドセーフに状態遷移する
  - `RoundState` — 抽象基底。`ResponseOk` / `ResponseWin` / `ResponseDahai` / `ResponseKan` / `ResponseCall` / `ResponseRyuukyoku` の仮想メソッドと `Entry` / `Exit` ライフサイクルを持つ
  - `RoundEvent` — 抽象基底。6種の具象実装（ResponseOk, ResponseDahai, ResponseCall, ResponseWin, ResponseKan, ResponseRyuukyoku）
  - `RoundStateContext` — ステートマシン本体。コンストラクタで `ITenpaiChecker` と `IScoreCalculator` を受け取る。`Init()` で `RoundStateHaipai` から開始。`RoundStateChanged` イベントで遷移通知。`IDisposable` で 5秒タイムアウト付きシャットダウン
  - **局終了通知**: 終端状態（`RoundStateWin` / `RoundStateRyuukyoku`）がOK応答時に `RoundStateContext.RoundEnded` イベント（`RoundEndedEventArgs`）を一度だけ発火する。`GameStateContext` はこれを購読し `GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku` に変換して対局レベル遷移に昇格させる
  - **不正応答**: 現状態で受け付けない応答が来た場合、例外で処理ループを止めず `InvalidEventReceived` イベントに通知して続行する
  - 状態遷移フローの詳細（配牌→ツモ→打牌サイクル、副露/ロン/流局の分岐、槓サイクル、終端状態）は [docs/Design.md](docs/Design.md) の状態遷移図を参照

#### 通知・応答型（Phase 3）

プレイヤーとの入出力境界は型で分離する。現時点では型定義のみであり、通知送信・応答集約・優先順位解決は後続の `RoundManager` 層（Phase 5）で扱う。

- **応答候補** ([Mahjong.Lib.Game/Candidates/](src/Mahjong.Lib.Game/Candidates/)) — サーバーが提示する合法応答候補。UX/選択肢提示用であり、受信応答は必ずサーバー側で再検証する
- **プレイヤー応答** ([Mahjong.Lib.Game/Responses/](src/Mahjong.Lib.Game/Responses/)) — C# API 層の応答型。`AfterTsumoResponse` / `AfterDahaiResponse` / `AfterKanResponse` / `AfterKanTsumoResponse` など局面ごとに戻り値型を分け、コンパイル時の型安全を優先する
- **Wire DTO** ([Mahjong.Lib.Game/Notifications/](src/Mahjong.Lib.Game/Notifications/)) — `PlayerNotification` / `PlayerResponseEnvelope` は通知ID(UUIDv7)・局Revision・応答者を含む通信・シリアライズ用エンベロープ。C# API 層と Wire DTO 層の変換は `Player`（または adapter）が担う
- **プレイヤー視点フィルタ** ([Mahjong.Lib.Game/Views/](src/Mahjong.Lib.Game/Views/)) — `PlayerRoundView` は自分の手牌・非公開状態（フリテン等）と、他家の公開情報を分離する情報射影
- **決定仕様** ([Mahjong.Lib.Game/Decisions/](src/Mahjong.Lib.Game/Decisions/)) — `RoundDecisionSpec` は「誰に何を聞くか」、`ResolvedRoundAction` は応答集約・優先順位適用後の採用結果を表す
- **点数結果型** ([Mahjong.Lib.Game/Games/Scoring/](src/Mahjong.Lib.Game/Games/Scoring/)) — `ScoreResult` / `YakuInfo` は Lib.Game が Lib.Scoring に依存しないための境界型。成立役は `Yaku` ではなく番号・名称・翻数・役満倍数の明細として保持する

#### 対局(Game)レベル

- **対局の集約**: [Mahjong.Lib.Game/Games/Game.cs](src/Mahjong.Lib.Game/Games/Game.cs) が対局全体の状態を保持するイミュータブルrecord（PlayerList/Rules/RoundWind/RoundNumber/Honba/KyoutakuRiichiCount/PointArray）。`Create` / `CreateRound` / `ApplyRoundResult` / `AdvanceToNextRound` で進行
- **プレイヤー**: [Mahjong.Lib.Game/Players/](src/Mahjong.Lib.Game/Players/) の `Player`（`PlayerId` + 表示名）と `PlayerList`（4人固定、index 0 が**起家**。並び替えは呼び出し側責務）
- **対局ルール**: [Mahjong.Lib.Game/Games/GameRules.cs](src/Mahjong.Lib.Game/Games/GameRules.cs) に対局形式（`GameFormat`: SingleRound/Tonpuu/Tonnan）、赤ドラ集合、初期持ち点、トビ閾値、食いタン/後付け、連荘条件（`RenchanCondition`）を集約
- **対局終了判定**: [Mahjong.Lib.Game/Games/GameEndPolicy.cs](src/Mahjong.Lib.Game/Games/GameEndPolicy.cs) `ShouldEndAfterRound(game, event, dealerContinues)` で判定。呼び出し順は **ApplyRoundResult → ShouldEndAfterRound → (false なら AdvanceToNextRound)**
- **対局の外部入口**: [Mahjong.Lib.Game/Games/GameManager.cs](src/Mahjong.Lib.Game/Games/GameManager.cs) が `Start()` で `GameStateContext` を生成し `IDisposable` で管理。コンストラクタで `PlayerList` / `GameRules` / `IWallGenerator` / `IScoreCalculator` / `ITenpaiChecker` を受け取る
- **対局レベル状態機械** ([Mahjong.Lib.Game/States/GameStates/](src/Mahjong.Lib.Game/States/GameStates/)): Round層と同じ Channel ベースのイベント駆動。`GameState`（Init/RoundRunning/End）と `GameEvent`（ResponseOk/RoundEndedByWin/RoundEndedByRyuukyoku）
  - `GameStateContext` は `RoundStateContext` をホストし、`RoundEnded` イベントを購読して `RoundEndedEventArgs` を `GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku` に変換・内部発行して対局レベル遷移に昇格させる（多重発火抑止付き）
  - `GameStateRoundRunning` の `RoundEndedByWin` / `RoundEndedByRyuukyoku` ハンドラ内で `Game.ApplyRoundResult` → `GameEndPolicy.ShouldEndAfterRound` → （続行なら `Game.AdvanceToNextRound` + 次局 `RoundStateContext` 生成、終了なら `GameStateEnd` 遷移）の順で処理する

### 点数計算検証ツール（tools/Mahjong.Lib.Scoring.TenhouPaifuValidation/）

`HandCalculator` の実装を天鳳の実牌譜に対して総当たり検証するコンソールアプリ。`Microsoft.Extensions.DependencyInjection` でサービスを組み立て、対話的に入力された日付（YYYYMMDD）の牌譜を処理する。

`UseCase` を起点に2段階のパイプライン:
1. **AnalysisPaifu** — `PaifuDownloadService`（天鳳からダウンロード） → `RoundDataExtractService`（局単位に分解） → `InitParseService` / `AgariParseService` / `MeldParseService`（XMLタグ→ドメインモデル） → `AgariInfoBuildService`（`AgariInfo` 生成）
2. **ValidateCalc** — `CalcValidateService` が `AgariInfo` から `HandCalculator.Calc` を呼び出し、実データの符・翻・点数と突き合わせて `ValidateResult` を返す

テストデータは [tests/Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests/TestData/](tests/Mahjong.Lib.Scoring.TenhouPaifuValidation.Tests/TestData/) 配下のXML牌譜（途中流局有無・ダブロン等のバリエーション）。

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
- テストはfeatureドメインごとのディレクトリに整理する（Lib.Scoring.Tests: Tiles/, Calls/, Fus/, Games/, Yakus/, Shantens/, HandCalculating/　Lib.Game.Tests: Tiles/, Calls/, Candidates/, Decisions/, Games/, Hands/, Notifications/, Players/, Responses/, Rivers/, Rounds/, States/, Views/, Walls/）
- `HandCalculator.Calc`のテストは入力カテゴリ別にクラスを分割（例: `HandCalculator_CalcTests_Shuntsu`、`_Koutsu`、`_Kokushimusou`、`_Dora`、`_Error`、`_Formless`、`_Tenhou`、`_Others`）
