# 局内通知パイプライン (Phase 5 以降)

本書は `Mahjong.Lib.Game` における「局(Round) 内のプレイヤー通知・応答集約」の実装詳細を解説する。全体の設計方針・対局レベル構成は [Design.md](Design.md) を参照。

## 位置付け

Phase 5 以前は「意思決定通知」と「観測通知」で別経路 (`progressChannel` の union / `Broadcast*NotificationAsync` / `CallPerformed` イベント) を持っていたが、これらは全撤去され、全通知 (配牌/ツモ/打牌/副露/槓/嶺上ツモ/嶺上ツモ後/和了/流局) を `RoundState.CreateInquirySpec` 起点の**単一パイプライン**に統合した。入力を要求する局面と「通知観測だけ」の局面 (配牌/副露/和了/流局) を個別ルートに分けず、候補を `OkCandidate` 1 択にして同じフローに流す。

## 通知は全員、問い合わせは一部

1 つの `RoundState` は `PlayerSpecs` に **常に 4 人分** の `PlayerInquirySpec` を生成し、通知自体は全員に届く (表示用)。そのうち「意味ある入力を期待するプレイヤー」は `RoundInquirySpec.InquiredPlayerIndices` で明示的に保持する。

- `PlayerInquirySpec.CandidateList`: 各プレイヤーが取れる合法応答。問い合わせ外プレイヤーは `[OkCandidate]` のみ
- `RoundInquirySpec.InquiredPlayerIndices`: 問い合わせ対象 (Tsumo/KanTsumo/AfterKanTsumo: 手番 1 人 / Dahai/Kan: 非手番 3 人 / Haipai/Call/Win/Ryuukyoku: 空)
- `RoundInquirySpec.IsInquired(PlayerIndex)` ヘルパで判定

この二重構造により、クライアントは `PlayerNotification.InquiredPlayerIndices` から「誰の判断待ちか」を UI 表示できる。

## 問い合わせ外プレイヤーの非 OK 応答は例外

`RoundManager.CollectSingleAsync` は各応答に対して以下の順で処理する:

1. `InvokePlayerAsync` (`OperationCanceledException` → timeout fallback / その他 `Exception` → exception fallback)
2. `ResponseValidator.IsResponseInCandidates` 候補集合照合
3. 候補外の場合:
   - **問い合わせ外** (`!spec.IsInquired(index)`): **`InvalidOperationException` を throw** (クライアント契約違反として進行を停止)
   - **問い合わせ対象** (`spec.IsInquired(index)`): `tracer.OnInvalidResponse` → `defaultFactory.CreateDefault` にフォールバック

問い合わせ外プレイヤーは「OK 応答のみ許可」という契約を持つため、非 OK 応答を silent fallback で隠蔽せず即例外化してバグ可視化する方針。タイムアウトや Player 例外は通信層の問題として従来通り fallback する。

## 私的情報の遮断

ツモ牌 / 嶺上ツモ牌は手番プレイヤー固有の私的情報のため、他家には**別通知型**で送る:

- 手番 (問い合わせ対象): `TsumoNotification(TsumoTile)` / `KanTsumoNotification(DrawnTile)` → `OnTsumoAsync` / `OnKanTsumoAsync`
- 他家 (問い合わせ外): `OtherPlayerTsumoNotification(tsumoPlayerIndex)` / `OtherPlayerKanTsumoNotification(kanTsumoPlayerIndex)` → `OnOtherPlayerTsumoAsync` / `OnOtherPlayerKanTsumoAsync` (戻り値 `Task<OkResponse>`)

`Dahai` / `Kan` は通知内容 (打牌/槓) が公開情報なので、打牌者/槓宣言者も同じ `DahaiNotification` / `KanNotification` を受け取る (ただし `CandidateList=[OkCandidate]` のため OK 応答しかできない)。

## パイプライン全体像

`RoundManager.ProcessAsync` は 1 局分のメインループで、`stateChannel_` から次状態を取り出すたびに以下のステップを**直列**に実行する。各ステップは独立して差し替え可能な DI 境界を持つ (`IResponseCandidateEnumerator` / `IRoundViewProjector` / `IRoundNotificationBuilder` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IResponseDispatcher` / `IGameTracer`)。

```
[1] RoundStateContext.RoundStateChanged
      → stateChannel_ (Channel<RoundState>, SingleReader/SingleWriter)
[2] RoundManager.ProcessAsync が次状態を 1 件取り出す
[3] KanTsumo pending 消費判定
      state is RoundStateAfterKanTsumo && pendingAfterKanTsumoResponse_ != null
        → dispatcher.DispatchAfterKanTsumoAsync(context, pending) で消費して continue
[4] Round スナップショット解決
      state is RoundStateCall call && call.SnapshotRound != null
        ? call.SnapshotRound
        : context_.Round
[5] state.CreateInquirySpec(round, enumerator)
      → RoundInquirySpec { Phase, PlayerSpecs[4人分], InquiredPlayerIndices, LoserIndex? }
[6] CollectResponsesAsync (全員並列 Task.WhenAll)
      各プレイヤーについて:
        a. NotificationId.NewId() (UUIDv7)
        b. notificationBuilder.Build(state, round, spec, playerSpec, projector)
             内部で projector.Project(round, playerIndex) により視点射影
             問い合わせ対象/非対象で通知型を切り替え (後述)
        c. tracer.OnNotificationSent
        d. InvokePlayerAsync (11 種の Player.On***Async にディスパッチ)
             DefaultTimeout = 10 秒 で LinkedTokenSource
        e. 例外制御:
             OperationCanceledException → tracer.OnResponseTimeout → defaultFactory
             その他 Exception → tracer.OnResponseException → defaultFactory
        f. tracer.OnResponseReceived
        g. ResponseValidator.IsResponseInCandidates で候補集合と照合
        h. 候補外:
             問い合わせ外 → InvalidOperationException を throw
             問い合わせ対象 → tracer.OnInvalidResponse → defaultFactory にフォールバック
[7] priorityPolicy.Resolve(spec, responses)
      → ImmutableArray<AdoptedPlayerResponse>
[8] DetectRonMissedFuritenPlayers (Dahai フェーズのみ)
      Ron 候補を提示されたが Ron 以外で応答したプレイヤーを同巡フリテン対象として検出
      ロン応答が 1 件でも含まれる場合は空配列
[9] tracer.OnAdoptedAction (採用応答を 1 件ずつ記録)
[10] RoundStateContext.ApplyTemporaryFuriten(temporaryFuritenPlayers) を同期直接呼び出し
      Dahai フェーズで同巡フリテン対象がある場合のみ、状態遷移を伴わない局所更新として Round を書き換える
[11] dispatcher.DispatchAsync(context, spec, adopted)
      Phase に応じて context_.Response*Async を発火
        → RoundStateContext.Channel<RoundEvent> に積まれ、次状態へ遷移
      KanTsumo でアクション応答が採用された場合は PlayerResponse? を返し、
        pendingAfterKanTsumoResponse_ にセット → [3] で後続 AfterKanTsumo 消費
      → [1] に戻る
```

終了条件は `RoundStateContext.RoundEnded` イベント (終端状態 `RoundStateWin` / `RoundStateRyuukyoku` の OK 応答で発火) で `stateChannel_.Writer.TryComplete()` を呼び、`ProcessAsync` の `await foreach` が自然終了する。

同巡フリテンは `RoundManager` が優先順位解決後・ディスパッチ前に `RoundStateContext.ApplyTemporaryFuriten` を**同期呼び出し**して `Round` を局所更新する。状態遷移を伴わない更新のためイベントキュー (`Channel<RoundEvent>`) は介さず、`private setter` 経由で `Round` を直接書き換える例外経路として設計されている。

## フェーズ別 `RoundInquirySpec`

`PlayerSpecs` は常に 4 人分、`InquiredPlayerIndices` で問い合わせ対象を明示する:

| State                      | `RoundInquiryPhase` | `InquiredPlayerIndices`      | 問い合わせ対象の候補                                                   | 問い合わせ外の候補 | `LoserIndex`      | 採用後の遷移                           |
| -------------------------- | ------------------- | ---------------------------- | --------------------------------------------------------------------- | ------------------ | ----------------- | -------------------------------------- |
| `RoundStateHaipai`         | `Haipai`            | `[]` (空)                     | —                                                                     | 全員 `OkCandidate` | `round.Turn` (便宜値) | `ResponseOkAsync` → `Tsumo`            |
| `RoundStateTsumo`          | `Tsumo`             | `[round.Turn]`               | `DahaiCandidate` + (`TsumoAgariCandidate` / `AnkanCandidate[]` / `KakanCandidate[]` / `KyuushuKyuuhaiCandidate`) | `[OkCandidate]`    | `round.Turn` (便宜値) | 応答種別に応じ個別 `Response*Async`   |
| `RoundStateDahai`          | `Dahai`             | 非手番 3 人                   | `RonCandidate?` + (`ChiCandidate[]` / `PonCandidate[]` / `DaiminkanCandidate[]`) + `OkCandidate` | `[OkCandidate]`    | 手番 (放銃者候補) | 優先順位適用後、Ron (ダブロン対応) / 副露 / OK |
| `RoundStateKan` (加槓)     | `Kan`               | 非手番 3 人                   | `ChankanRonCandidate?` + `OkCandidate`                                | `[OkCandidate]`    | 加槓者            | Chankan Ron (ダブロン対応) / OK       |
| `RoundStateKanTsumo`       | `KanTsumo`          | `[round.Turn]`               | `DahaiCandidate` + (`RinshanTsumoAgariCandidate` / `AnkanCandidate[]` / `KakanCandidate[]`) | `[OkCandidate]`    | `round.Turn` (便宜値) | 2 段階ディスパッチ (後述)             |
| `RoundStateAfterKanTsumo`  | `AfterKanTsumo`     | `[round.Turn]`               | `DahaiCandidate` + (`AnkanCandidate[]` / `KakanCandidate[]`)          | `[OkCandidate]`    | `round.Turn` (便宜値) | 応答種別に応じ個別 `Response*Async`   |
| `RoundStateCall`           | `Call`              | `[]` (空)                     | —                                                                     | 全員 `OkCandidate` | `round.Turn` (便宜値) | `ResponseOkAsync` → `Dahai` or `KanTsumo` |
| `RoundStateWin`            | `Win`               | `[]` (空)                     | —                                                                     | 全員 `OkCandidate` | `round.Turn` (便宜値) | `ResponseOkAsync` → 局終了             |
| `RoundStateRyuukyoku`      | `Ryuukyoku`         | `[]` (空)                     | —                                                                     | 全員 `OkCandidate` | `round.Turn` (便宜値) | `ResponseOkAsync` → 局終了             |

`LoserIndex` は non-null 型 (`PlayerIndex`) として便宜上全フェーズで設定されるが、意味を持つのは `Dahai` / `Kan` のみ (その他フェーズは `round.Turn` が入る便宜値)。ダブロン判定と「放銃者基準の巡目順」(下家=1 / 対面=2 / 上家=3) による同順位解決で使われる。

## フェーズ別通知マッピング

`IRoundNotificationBuilder` 実装 (`RoundNotificationBuilder`) はプレイヤーが問い合わせ対象か否かで通知型を切り替える (私的情報遮断のため):

| State                      | 問い合わせ対象 (手番) への通知                  | 問い合わせ外 (他家) への通知                           | 主な内容                                                    |
| -------------------------- | ----------------------------------------------- | ------------------------------------------------------ | ----------------------------------------------------------- |
| `RoundStateHaipai`         | `HaipaiNotification(view)`                      | 同上 (全員同じ)                                        | 配牌直後の視点情報のみ                                      |
| `RoundStateTsumo`          | `TsumoNotification(view, tsumoTile, candidates)`| `OtherPlayerTsumoNotification(view, turnIndex)`        | 手番は自ツモ牌を含む / 他家は誰がツモったかのみ              |
| `RoundStateDahai`          | `DahaiNotification(view, discardedTile, discarder, candidates)` | 同左 (candidates のみ OK 限定に差し替え) | 打牌は公開情報のため打牌者本人も同じ通知を受け取る           |
| `RoundStateKan`            | `KanNotification(view, kanCall, caller, candidates)` | 同左 (candidates のみ OK 限定に差し替え)            | 槓は公開情報のため槓宣言者本人も同じ通知を受け取る           |
| `RoundStateKanTsumo`       | `KanTsumoNotification(view, rinshanTile, candidates)` | `OtherPlayerKanTsumoNotification(view, turnIndex)` | 手番は嶺上ツモ牌を含む / 他家は誰が嶺上ツモしたかのみ        |
| `RoundStateAfterKanTsumo`  | `KanTsumoNotification(view, rinshanTile, candidates)` | `OtherPlayerKanTsumoNotification(view, turnIndex)` | `KanTsumo` と同じ通知型を再利用                             |
| `RoundStateCall`           | —                                               | `CallNotification(view, madeCall, caller)` (全員)      | `SnapshotRound` から副露直後・嶺上ツモ前の `Round` を参照   |
| `RoundStateWin`            | —                                               | `WinNotification(view, adoptedWinAction)` (全員)       | `AdoptedRoundActionBuilder.Build(eventArgs)` で構築         |
| `RoundStateRyuukyoku`      | —                                               | `RyuukyokuNotification(view, adoptedRyuukyokuAction)` (全員) | 同上                                                  |

`PlayerRoundView` は `IRoundViewProjector` が `Round` + 受信者 `PlayerIndex` から生成する。自分の手牌・非公開状態 (フリテン等) と、他家の公開情報 (河/副露/打点/立直棒) のみを含む。山の中身や他家の手牌は射影時に除外される。

## `DispatchAsync` の分岐

```
switch (spec.Phase)
{
    // 観測フェーズ: 全員 OK 集約 → 単一 ResponseOkAsync
    case Haipai:
    case Call:
    case Win:
    case Ryuukyoku:
        await context.ResponseOkAsync();
        break;

    // 入力要求フェーズ: 問い合わせ対象の応答を抽出して振り分け
    case Tsumo:          await DispatchTsumoAsync(FindInquiredResponse(spec, adopted));
    case Dahai:          await DispatchDahaiAsync(FilterInquiredResponses(spec, adopted), loserIndex);
    case Kan:            await DispatchKanAsync(FilterInquiredResponses(spec, adopted), loserIndex);
    case KanTsumo:       return await DispatchKanTsumoAsync(FindInquiredResponse(spec, adopted));
    case AfterKanTsumo:  await DispatchAfterKanTsumoAsync(FindInquiredResponse(spec, adopted).Response);
}
```

`FindInquiredResponse` は単一対象フェーズで `spec.InquiredPlayerIndices[0]` の応答を引く。`FilterInquiredResponses` は複数対象フェーズで問い合わせ対象の応答だけを残す (非対象プレイヤーの OK は優先順位解決前に除外される)。

`Dahai` / `Kan` のダブロン判定は `DispatchDahaiAsync` / `DispatchKanAsync` 内で `adopted.Where(x => x.Response is RonResponse or ChankanRonResponse)` を配列のまま `context.ResponseWinAsync(winners, loserIndex, winType)` に渡す (天鳳準拠でダブロン成立、トリプルロンはルール未確定)。

## KanTsumo の 2 段階ディスパッチ

`RoundStateKanTsumo` で「嶺上ツモ和了 / 打牌 / 暗槓 / 加槓」を 1 通知・1 応答で受けるが、嶺上ツモ和了 (`RinshanTsumoResponse`) とそれ以外で遷移先が異なる:

- **嶺上ツモ和了**: `ResponseWinAsync([self], self, WinType.Rinshan)` を直接発火 → `RoundStateWin` へ。`DispatchKanTsumoAsync` は `null` を返す
- **打牌/暗槓/加槓**: `DispatchKanTsumoAsync` が採用応答を返し、`RoundManager` が `pendingAfterKanTsumoResponse_` にセット → `ResponseOkAsync()` で `RoundStateAfterKanTsumo` に遷移 → メインループ [3] で state が `RoundStateAfterKanTsumo` に切り替わったタイミングで `pending` を消費して `DispatchAfterKanTsumoAsync` を呼ぶ

この 2 段階化により、`AfterKanTsumo` 状態での候補再計算 (四槓流れ除外後) を挟まずに済む。`pending` のセット漏れ防止のため `try/catch` で例外時は `null` に戻す。

## 候補検証の規約 (`ResponseValidator`)

検証は `PlayerResponse` の具象型ごとに異なる規約で行う:

| 応答                                                          | 判定                                                                          |
| ------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `OkResponse` / `RonResponse` / `TsumoAgariResponse` / `KyuushuKyuuhaiResponse` / `ChankanRonResponse` / `RinshanTsumoResponse` | 対応 `Candidate` 型が候補リストに存在するだけで OK                          |
| `DahaiResponse` / `KanTsumoDahaiResponse`                     | `DahaiCandidate.DahaiOptionList` に `Tile` 完全一致 (Tile.Id)。立直時は `RiichiAvailable=true` が必要 |
| `AnkanResponse` / `KanTsumoAnkanResponse`                     | `AnkanCandidate.Tiles[0].Kind` と一致 (Tile.Kind、4 枚全部を使うため赤ドラの選択余地なし)    |
| `KakanResponse` / `KanTsumoKakanResponse`                     | `KakanCandidate.Tile` と完全一致 (Tile.Id、赤/非赤は別 `KakanCandidate` として列挙済み)       |
| `ChiResponse` / `PonResponse` / `DaiminkanResponse`           | `HandTiles` が `SequenceEqual` (順序と Id 全一致)                            |

検証漏れは「合法な候補が提示されたのに別 Id の牌で応答された」ケースで顕在化するため、赤ドラの取り扱いは候補列挙側と検証側で一貫させる (`KakanCandidate` は赤/非赤で別候補、`AnkanCandidate` は `Kind` 単位で 1 候補)。

## 優先順位解決 (`TenhouResponsePriorityPolicy`)

- **Dahai フェーズ**:
  1. `RonResponse` が 1 件以上あれば、全員採用 (ダブロン)。放銃者基準の巡目順 (下家→対面→上家) でソート
  2. `PonResponse` / `DaiminkanResponse` があれば、放銃者に最も近い (= 下家) を 1 件採用
  3. `ChiResponse` があれば、下家から 1 件採用 (通常は下家のみ提示)
  4. いずれも無ければ全員 OK
- **Kan フェーズ**:
  1. `ChankanRonResponse` が 1 件以上あれば、全員採用 (ダブロン対応、巡目順)
  2. 無ければ全員 OK
- **他フェーズ**: 単一プレイヤー応答 (`Tsumo` / `KanTsumo` / `AfterKanTsumo`) または全員 OK (`Haipai` / `Call` / `Win` / `Ryuukyoku`) のため、ソート不要で `responses` をそのまま返す

トリプルロンの扱いはルール依存のため `IResponsePriorityPolicy` を差し替えて調整する (本実装は 3 件採用まで許容)。三家和了による途中流局化は採用段階ではなく後段の `RoundState` 側で判定する方針。

## 既定応答フォールバック (`DefaultResponseFactory`)

`ResponseValidator` 不合格 (問い合わせ対象のみ) / タイムアウト (10 秒) / プレイヤー例外時に使用。現実装は spec 駆動で、`DahaiCandidate` が含まれない spec には常に `OkResponse` を返す:

| 条件                              | 既定応答                                                                    |
| --------------------------------- | --------------------------------------------------------------------------- |
| `DahaiCandidate` なし             | `OkResponse` (観測のみ / Dahai/Kan スルー)                                  |
| `Tsumo` + `DahaiCandidate` あり   | `DahaiResponse(DahaiCandidate.DahaiOptionList[0].Tile)` (先頭 = 実質ツモ切り) |
| `KanTsumo` + `DahaiCandidate` あり| `KanTsumoDahaiResponse(先頭 DahaiOption)`                                   |
| `AfterKanTsumo` + 同上            | `KanTsumoDahaiResponse(先頭 DahaiOption)`                                   |

接続断時は **進行継続** (天鳳の CPU 代打相当) を優先し、対局中断はしない。

## `RoundStateCall` の `SnapshotRound` が必要な理由

`RoundStateCall` は「副露直後の通知観測点」として残されている (削除候補だったが、全通知を統一パイプラインに乗せる設計上、副露通知だけ別経路にすると整合性が崩れるため復活)。問題は大明槓のタイミング:

```
Dahai --(ResponseCall: Daiminkan)--> Call --(ResponseOk)--> KanTsumo
                                     │
                                     └── ここで context.Round は副露直後
                                         (RinshanTsumo 未実行)
```

`RoundStateCall.ResponseOk` が大明槓判定時に `Transit(context, new RoundStateKanTsumo(), round => round.RinshanTsumo())` を呼ぶため、`RoundStateCall.Entry` の時点では `Round` は副露直後だが、`RoundStateKanTsumo.Entry` が走った直後には嶺上ツモ後になる。

`stateChannel_` が `RoundStateCall` を流した時点で `context.Round` を読んでも、`RoundManager.ProcessAsync` が非同期ループで読む順序によっては RinshanTsumo 後の `Round` が観測される可能性がある (`RoundStateContext` の `Channel<RoundEvent>` と `stateChannel_` は独立キューのため)。副露通知に嶺上ツモ牌が混入するとクライアント表示がずれる。

解決策として `RoundStateCall.SnapshotRound` を `init` プロパティで持ち、遷移時に `RoundStateDahai.ResponseCall` が `Transit(context, () => new RoundStateCall { SnapshotRound = context.Round }, round => ...)` の**ファクトリ版 `Transit`** を使い、副露実行後・`Entry` 前の `Round` を封入する。

`RoundState.Transit` は 3 オーバーロード:

```csharp
// [1] Round 更新なしの遷移
protected static void Transit(RoundStateContext context, RoundState nextState);

// [2] Round を updateRound で更新してから遷移
protected static void Transit(RoundStateContext context, RoundState nextState, Func<Round, Round> updateRound);

// [3] Round 更新後に状態ファクトリで次状態を生成して遷移 (SnapshotRound 等で使用)
protected static void Transit(RoundStateContext context, Func<RoundState> nextStateFactory, Func<Round, Round> updateRound);
```

`RoundStateContext.Transit` の内部実装は `Exit → updateRound → nextStateFactory() → Entry` の順で評価するため、ファクトリ版の中で `context.Round` を参照した時点の値は `updateRound` 適用後の副露直後 `Round` になる。

ファクトリ版が必要な本質的理由は「State パターンの `Exit → 副作用 → Entry` 順序を崩さずに、副作用適用後の `context.Round` を次状態のコンストラクタ引数に使いたい」ため。Immutable 性維持のためではなく、遷移順序の契約を保ちつつ派生情報を伝搬するための追加オーバーロード。

## 撤去された旧設計

Phase 5 レビュー前の設計で存在していた以下は全て撤去:

- `CallPerformed` イベント (`RoundStateContext` 上で副露時のみ発火されていた個別チャネル)
- `progressChannel` の union 型 (`OneOf<State, CallEvent>` 風に通知観測と状態遷移を混在させていた)
- `BroadcastCallNotificationAsync` / `BroadcastDahaiNotificationAsync` / `BroadcastWinNotificationAsync` の 3 メソッド (state なしで引数直接渡しだった観測通知専用の送信経路)
- `ApplyTemporaryFuritenIfRonMissed` として `RoundManager` が直接 `context.Round` を書き換えていた経路 (現在は `DetectRonMissedFuritenPlayers` が `PlayerIndex[]` を返し、`RoundManager` が `RoundStateContext.ApplyTemporaryFuriten` を同期直接呼び出しする。`RoundStateContext` 側で `private setter` 経由の局所更新として吸収しており、状態遷移を伴わない更新のためイベントキューは介さない明示的な例外経路)

通知観測点と意思決定点は通知プロトコル上で区別せず、応答候補集合 (`OkCandidate` のみ vs それ以外) とフェーズ列挙値 (`RoundInquiryPhase`) だけで表現する。これにより Wire DTO 層も通知種別に閉じ、リプレイ・牌譜再生が一本化される。
