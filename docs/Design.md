# 設計

## モジュール構成

- **Mahjong.Lib.Scoring** — 点数計算専用ドメイン。`TileKind`(0-33、34種の絵柄)のみを扱う
- **Mahjong.Lib.Game** — 対局進行専用ドメイン。`Tile`(0-135、136枚を個別識別)を扱う
- 両ライブラリは相互参照しない。対局中に点数計算へ渡す際は `Tile.Kind`（= `Id / 4`）で牌種インデックスへ変換する
- 点数計算では同種牌を区別する必要がないため `TileKind` のみで十分だが、対局では赤ドラなど同種牌の個別識別が必要なため `Tile` を使う

## 設計方針

- ドメインモデルはすべて `record` + `ImmutableList` / `ImmutableArray` のイミュータブル設計。状態更新は常に新インスタンスを返す
- 局全体の状態は `Round` record に集約する。進行アクション（配牌/ツモ/打牌/副露/槓/etc.）はすべて新しい `Round` を返す
- 4プレイヤー分の状態は `HandArray` / `RiverArray` / `CallListArray` / `PointArray` といったプレイヤー別配列型に統一し、`PlayerIndex` でアクセスする
- 局の状態遷移は `System.Threading.Channels.Channel<T>` を使った非同期イベント駆動のステートマシンで実装する

## 牌

- 牌は牌(Tile)と牌種別(TileKind)に分ける
- 牌種別は牌の絵柄を表現する
  - 点数計算には牌の絵柄のみが必要なので
- 牌は同じ絵柄でも牌1枚1枚を区別して表現する
  - 実際のゲームでは牌を区別する必要があるので
- 赤ドラはルールによって異なるため、局ごとに管理し点数計算モジュールや表示モジュールに通知する

## 副露種類

- ポン
- チー
- 暗槓
- 大明槓
- 加槓
- 点数計算において大明槓と加槓には差がないため明槓にまとめる
- 対局において大明槓と加槓は表示上の違いがあるため区別する

## 点数計算

- 役と点数は天鳳準拠
- ルールは天鳳準拠だがローカルルールも多少込み

## 対局の状態遷移

```mermaid
stateDiagram-v2
    GameStateInit: 対局開始<br>席順決めたり持ち点決めたり
    GameStateRoundRunning: 局進行中<br>局の作成・進行を行い 終了時は Game に結果を反映する
    state ChoiceGameEnd <<choice>>
    GameStateEnd: 対局終了

    [*] --> GameStateInit
    GameStateInit --> GameStateRoundRunning: OK応答
    GameStateRoundRunning --> ChoiceGameEnd: 局終了 (RoundEndedBy{Win,Ryuukyoku})
    ChoiceGameEnd --> GameStateEnd: 対局終了判定=true
    ChoiceGameEnd --> GameStateRoundRunning: 対局終了判定=false (次局)
    GameStateEnd --> [*]
```

Phase 1 では「局終了」を表す独立状態は持たず、局終了時の Game 更新・終了判定・次局決定は `GameStateRoundRunning.RoundEndedBy*` ハンドラ内で行い、直接 `GameStateRoundRunning`(次局)または `GameStateEnd` へ遷移する。Phase 5 のプレイヤー向け局終了通知実装時に必要なら `GameStateRoundEnd` 状態を再導入する。

### RoundWind の循環仕様

`Game.AdvanceToNextRound` で `RoundNumber` が 3 を超える場合、`RoundWind` は東→南→西→北→東 と循環する仕様。通常ルールでは `GameEndPolicy` により規定局数消化時点で対局終了するため循環には到達しないが、対局形式を超えて続行するテストシナリオや、将来の西入/北入ルール実装(Phase 以降で検討)に備えた定義。

## 局の状態遷移

```mermaid
stateDiagram-v2
    Haipai: 配牌<br>局順・本場・供託・持ち点・配牌・ドラを全プレイヤーに通知
    Tsumo: ツモ<br>ツモ番のプレイヤーにはツモ牌を それ以外のプレイヤーにはツモしたことを通知
    Dahai: 打牌<br>手牌orツモ牌から河に牌を移し、全プレイヤーに打牌されたことを通知<br>和了>ポン/大明槓>チー>OK
    state ChoiceDahai <<choice>>
    Call: 副露<br>河から打牌を除去し副露したプレイヤーに副露を生成<br>全プレイヤーに副露したことを通知
    state ChoiceCall <<choice>>
    Kan: 槓(暗槓・加槓)<br>暗槓-手牌から牌を除去し副露を生成 加槓-ツモ牌を追加してポン副露を更新<br>全プレイヤーに槓されたことを通知<br>和了>OK
    KanTsumo: 槓ツモ<br>嶺上牌を引いて手牌に加える<br>全プレイヤーに槓ツモしたことを通知<br>嶺上ツモ和了判定あり
    AfterKanTsumo: 槓ツモ後<br>嶺上ツモ和了なし
    state ChoiceKanTsumo <<choice>>
    Win: 和了<br>全プレイヤーに和了による局終了を通知
    Ryuukyoku: 流局<br>全プレイヤーに流局による局終了を通知

    [*] --> Haipai
    Haipai --> Tsumo: OK応答
    Tsumo --> Dahai: 打牌応答
    Tsumo --> Kan: 槓応答
    Tsumo --> Win: 和了応答(ツモ和了)
    Tsumo --> Ryuukyoku: 流局応答(九種九牌など)
    Dahai --> Call: 副露応答
    Dahai --> Win: 和了応答(ロン和了)
    Dahai --> ChoiceDahai: OK応答
    ChoiceDahai --> Tsumo: 流局でない
    ChoiceDahai --> Ryuukyoku: 流局(荒牌平局、四家立直、三家和了、四風連打など)
    Call --> ChoiceCall: OK応答
    ChoiceCall --> Dahai: 槓でない
    ChoiceCall --> KanTsumo: 槓(大明槓)
    Kan --> KanTsumo: OK応答
    Kan --> Win: 和了応答(槍槓・加槓のみ)
    KanTsumo --> ChoiceKanTsumo: OK応答
    KanTsumo --> Win: 和了応答(嶺上ツモ)
    ChoiceKanTsumo --> Ryuukyoku: 四槓流れ
    ChoiceKanTsumo --> AfterKanTsumo: 四槓流れでない
    AfterKanTsumo --> Kan: 槓応答
    AfterKanTsumo --> Dahai: 打牌応答
    Win --> [*]: OK応答
    Ryuukyoku --> [*]: OK応答
```

- プレイヤーに通知後、全プレイヤーの応答を待つ
- 副露のチー・ポン・大明槓のそれぞれの処理はCallで行う
- 立直は打牌と統合する
  - 立直打牌に対して和了応答があった場合は供託しない
  - 立直打牌に対して和了応答がなかった場合は供託後、打牌に対する応答の処理を行う
- チーとポン/大明槓が同時にあった場合はポン/大明槓が優先
- 副露とロンが同時に合った場合はロンが優先
- ダブロン、トリプルロンはルールによる(天鳳ではダブロンあり、三家和了は途中流局。天鳳以外のルールへの対応も視野に入れている)
- 同時副露や同時ロンは図に書くと複雑すぎるので省略 集約したのち優先順位に従って遷移させる実装を行う
- 槓など複数のイベントでの遷移先があるものも同様に集約後優先順位に従って遷移させる実装を行う
- 槓ドラ表示タイミング: 天鳳では暗槓は即乗り、明槓/加槓は後めくり(打牌または続く嶺上の直前)
- 四槓流れは槓ツモ後和了でない場合に判定が行われる
- 抜きは一旦考えないが、点数計算ライブラリではあらかじめ考慮しておく

## 要素

対局に出てくる要素は以下

- プレイヤー
- 親
- 持ち点
- 山
  - ドラ表示牌
  - 裏ドラ表示牌
  - 嶺上牌
- 河
- 手牌・副露
- 局順 東一局とか
- 本場
- 供託(リーチ棒)
- ツモ番
- ルール
  - 赤が何枚かなども

## プレイヤー

- プレイヤーはAIと人間両方に対応する
- プレイヤーは抽象型を用意し、それぞれをAIと人間用で継承して使用する
- 通知は各プレイヤーに対して処理後の状態を通知し、全プレイヤーの応答が揃うのを待つ
- 通知にはユニークなIdを振り、応答にもそのIdを含めることで待機中のものと一致していることを確認する
- 通知の種類によっては不適格な応答種類もあるため検証が必要
- 全ての応答がそろったら優先順位に従って最も処理をする
- AIの場合は同一プロセス内で応答を待つかも知れないが、人間の場合は別プロセスクライアントの可能性が高いのでその辺りも考慮した待ち方にする

### プレイヤー側の階層構造

参考: 書籍『麻雀AI』(電脳麻将)のMajiang.Player階層。AIとUIプレイヤーを共通の抽象基底の派生として実装し、階層ごとに責務を切る。ただし書籍の「行動提示層」はプレイヤー側ではなく対局側(通知送信側)に置く。

| 層                 | 責務                                                          | Player基底 | AIプレイヤー | 人間プレイヤー |
| ------------------ | ------------------------------------------------------------- | ---------- | ------------ | -------------- |
| 1. 通知受信層      | ゲームからの通知を受信し、種別ごとに振り分け                  | ●          |              |                |
| 2. 状態管理層      | 自プレイヤー視点の卓情報を更新                                | ●          |              |                |
| 3. 応答決定層      | 通知に含まれる合法行動候補から1つ選び、ゲームへ応答送信       |            | ●            | ●              |
| 4. 思考ルーチン層  | 候補から行動を選択(AIのみ)。人間プレイヤーはUI入力で代替      |            | ●            |                |

- 第1層の入口は書籍では `action()` 単一メソッドだが、本プロジェクトでは通知種別ごとのメソッド(`OnHaipai` / `OnTsumo` / `OnDahai` / `OnCall` / `OnKan` / ...)に分割する。単一入口の`switch`肥大化を避けるため
- 第2層で保持する卓情報は「自分視点に射影済みの状態」。他家の手牌や山の中身など見えない情報は含めない
- **書籍との差分**: 書籍では第5層「行動提示層」(合法行動列挙)が `Majiang.Player` 基底に存在するが、本設計では対局側の責務とする(下記「行動提示はサーバー側責務」参照)

### 行動提示はサーバー側責務

合法行動の列挙はドメインルール依存のため、対局側(通知送信側)で一元的に生成し、通知(`PlayerNotification.Candidates`)に含めてプレイヤーへ渡す。

- **理由**:
  - プレイヤー側に持たせるとルール判定ロジックがサーバー/クライアント二重実装になり、判定ズレのリスクがある
  - 不正クライアント対策として、どのみちサーバー側で応答の合法性を再検証する必要がある
  - AI の思考ルーチンは「候補から選ぶ」ことに集中でき、実装がシンプルになる
- **実装位置**: `RoundManager` が `RoundInquirySpec` と現在の `Round` から、`IResponseCandidateEnumerator`(仮) に候補生成を委譲する。`Mahjong.Lib.Scoring` のシャンテン判定や役判定を流用できる場面もあるが、Lib.Game 側で独自にラップする(ライブラリ間の直接依存は避ける)
- **プレイヤー側の責務**: 候補の中から1つを選んで応答するだけ。候補を自前で再計算しない

### 対局(Game)レベル

局(Round)の上位である半荘/対局全体の状態遷移は、`Round`と同様の構成で管理する。実装は`Game`系を先行させ、その後`Round*`系の通信・集約レイヤーを追加する(実装順は末尾「実装段階」参照)。

- `Game` (record, immutable) — 対局全体の状態を直接フィールドで保持(`PlayerList` / `GameRules` / `RoundWind` / `RoundNumber` / `Honba` / `KyoutakuRiichiCount` / `PointArray`)。`GameConfig` のような中間コンテナは作らない(局順など対局中に変動する値と固定パラメータを同じrecordに混在させないため)
- `Player` (abstract record) — プレイヤーの共通基底。`PlayerId` / `DisplayName` を持つ。Phase 4 で通知・応答メソッドを追加し、AI / 人間の実装が継承する
- `PlayerList` (record) — 4人分の `Player` を `ImmutableArray<Player>` で保持するラッパーで `IEnumerable<Player>` を実装。`PlayerIndex`でアクセス。**index 0 が起家**という仕様。起家決定・並び替えは呼び出し側の責務(既存 `RoundNumber.ToDealer()` が `PlayerIndex(Value)` を返す前提と整合)。`Player` 実体そのものが `PlayerList` の要素であり、識別情報と実体を二重管理しない
- `GameState` / `GameStateContext` — `Round*`と同じ非同期イベント駆動ステートマシン。状態は`GameStateInit` / `GameStateRoundRunning` / `GameStateEnd` (既存 `RoundStateXxx` の命名規約に揃える)。局終了後の Game 更新・終了判定・次局決定は `GameStateRoundRunning.RoundEndedBy*` ハンドラ内で行い 直接 `GameStateRoundRunning`(次局)または `GameStateEnd` へ Transit する。プレイヤー向けの局終了通知(Phase 5)の実装時に必要なら `GameStateRoundEnd` 状態を再導入する
- `GameManager` — `RoundManager`と同様の役割を`Game`レベルで担う。対局開始処理、局間の引き継ぎ、対局終了判定を統括し、各局の`RoundStateContext` / `RoundManager` を内部で生成・破棄する(親子関係)。コンストラクタで `PlayerList`(index 0 が起家になるよう並び替え済み)+ `GameRules` + `IWallGenerator` を受け取る
- **局終了時の引き継ぎ**: `GameStateContext`は`RoundStateContext`を内部保持し、`GameStateRoundRunning.RoundEndedBy*` ハンドラ内で `RoundStateContext.Round` から持ち点・本場・供託等を読み取って`Game`に反映した上で破棄、続行なら次局用に新しい`RoundStateContext`を生成する
- **局終了結果の保持場所**: 独立した `RoundResult` record は作らず、局終了イベント (`GameEventRoundEndedByWin` / `GameEventRoundEndedByRyuukyoku`) のフィールドとして結果情報を保持する(和了者 / 放銃者 / 和了種別 / 流局種別 / 連荘判定フラグ / 本場加算フラグ)。**`Round` record は変更しない**。Phase 2-3 でフィールドを拡張して役/符/翻/点数などを追加
- **Gameレベルのプレイヤー通知**: 対局開始(`PlayerList`/持ち点/ルール)、局開始、局終了(結果)、対局終了を各プレイヤーへ通知する。`Player`メソッド名は`OnGameStartAsync` / `OnRoundStartAsync` / `OnRoundEndAsync` / `OnGameEndAsync`。局内の通知は`OnHaipaiAsync` / `OnTsumoAsync` / `OnDahaiAsync` / `OnCallAsync` / `OnKanAsync` / `OnKanTsumoAsync` / `OnWinAsync` / `OnRyuukyokuAsync` / `OnDoraRevealAsync`(カンドラ表示)
- **応答型の設計**: 各`On***Async`の戻り値は通知ごとに異なる型(`Task<OkResponse>` / `Task<AfterTsumoResponse>` / `Task<PlayerResponse>` 等)にする。通知時に取りうる応答が型で制約され、不正応答の多くをコンパイル時に弾ける
- **スルー (パス) 応答の統一**: 打牌/加槓通知に対するスルーは `OkResponse` を返す。これにより全通知でスルー応答の型が一貫し、Player 実装・Wire 変換 (`OkResponseBody`)・既定応答生成 (`DefaultResponseFactory`) が簡潔になる。`AfterDahaiResponse` / `AfterKanResponse` は**アクション応答のみ**の階層 (チー/ポン/大明槓/ロン / 槍槓ロン) とし、`Player.OnDahaiAsync` / `OnKanAsync` の戻り値型は `Task<PlayerResponse>` とする (スルー時は `OkResponse`、アクション時は `AfterDahaiResponse` / `AfterKanResponse` 派生を返す)
- **`GameRules`の責務**: 対局前に決まっている**ルール**(対局形式 東風戦/東南戦(デフォルト東南)/ 赤ドラ枚数 / 初期持ち点(当面35000) / オーラス親あがり止め / トビ終了点 / 食いタン / 後付け / 連荘条件 等)。起家 / タイムアウト等の対局固有パラメータは `GameManager` コンストラクタ引数または `GameRules` 内で管理する
- **ルール設定**: `Mahjong.Lib.Game` 独自に仮の`GameRules`(仮称、名前要検討)を定義。当面は`Mahjong.Lib.Scoring.GameRules`を模した最小構成で良い。将来的に両者の変換/共通化を検討
- **対局終了条件**: オーラス親あがり止め / トビ終了 / 西入 等は`GameRules`に持たせ、`GameEndPolicy.ShouldEndAfterRound`が`GameStateRoundRunning.RoundEndedBy*`から判定される(`AdvanceToNextRound` の**前**に評価することで、終了時の `Game` が「次局予定状態」ではなく「終了した局を保持した状態」となる)

### 対局側の責務分離

対局側は既存のステートマシン(`RoundStateContext` / `RoundState`)に加え、プレイヤーとの通信・集約レイヤーを分離する。

| コンポーネント             | 責務                                                                 |
| -------------------------- | -------------------------------------------------------------------- |
| `RoundState`               | 局面の問い合わせ仕様(`RoundInquirySpec`)を返し、採用結果を受けて次状態・次`Round`を決める。プレイヤー・通信・タイムアウトは知らない |
| `RoundStateContext`        | `RoundEvent`を直列処理する既存ステートマシン。採用応答を`RoundEvent`として受け取る               |
| `RoundManager` (新設)       | 通知Id発行、プレイヤー視点への射影、通知送信、応答収集、検証、タイムアウトフォールバック、優先順位適用。`ImmutableArray<Player>`(`GameManager`から渡される)を保持 |
| `IResponsePriorityPolicy`  | 同時応答の優先順位決定(ロン > ポン/大明槓 > チー > OK、ダブロン/トリプルロン、席順優先 等)            |
| `IRoundViewProjector`      | `Round`からプレイヤー別の公開情報ビュー(`PlayerRoundView`)を生成。情報非対称性をここに閉じ込める     |
| `IDefaultResponseFactory`  | 通知種別ごとのタイムアウト既定応答(打牌タイムアウト→ツモ切り、副露可否タイムアウト→OK 等)。プレイヤー例外時のフォールバックにも使用 |
| `Player` / `Player`   | プレイヤー抽象(interface) + 共通実装を持つ抽象基底クラス。通知メソッドは**種別ごと**(`OnHaipaiAsync`/`OnTsumoAsync`/`OnDahaiAsync`/`OnCallAsync`/`OnKanAsync`/...)に分割。`Player`は視点卓情報の更新など共通処理を持つ。`Player`自身は`PlayerIndex`を保持せず、`GameManager`が`ImmutableArray<Player>`として`PlayerIndex`→`Player`実体の対応表を保持する |
| `IGameTracer`              | 構造化イベント記録。**全イベント**(対局/局/配牌/ツモ/打牌/副露/槓/カンドラ/和了/流局/通知送信/応答受信/採用結果等)をトレース可能にし、牌譜・リプレイが再生成できるレベルを目指す。スコープは複数対局を跨ぐグローバル。統計集計は実装側で必要イベントを抽出。差し替え可能(no-op/メモリ/ファイル/DB 等) |

- State は「この局面で何を聞くか」の仕様(`RoundInquirySpec`)だけ返し、通信待ちは持たない。`Entry`/`Exit`は同期のまま保つ
- `RoundManager` が仕様を受け取り、`IRoundViewProjector` で視点フィルタ、各プレイヤーへ送信、集約、検証、優先順位適用を行い、採用済み結果(`AdoptedRoundAction`)を `RoundStateContext` にイベントとして渡す

#### 統一通知フロー

全通知 (配牌/ツモ/打牌/副露/槓/嶺上ツモ/嶺上ツモ後/和了/流局) は `RoundState.CreateInquirySpec` を起点とする**単一パイプライン**に乗せる。入力を要求する局面と「通知観測だけ」の局面 (配牌/副露/和了/流局) を個別ルートに分けず、候補を `OkCandidate` 1 択にして同じフローに流す方針。Phase 5 以前は「意思決定通知」と「観測通知」で別経路 (`progressChannel` の union / `Broadcast*NotificationAsync` / `CallPerformed` イベント) を持っていたが、これらは全撤去されている。

##### パイプライン全体像

`RoundManager.ProcessAsync` は 1 局分のメインループで、`stateChannel_` から次状態を取り出すたびに以下のステップを**直列**に実行する。各ステップは独立して差し替え可能 (`IResponseCandidateEnumerator` / `IRoundViewProjector` / `IResponsePriorityPolicy` / `IDefaultResponseFactory` / `IGameTracer`) な DI 境界を持つ。

```
[1] RoundStateContext.RoundStateChanged
      → stateChannel_ (Channel<RoundState>, SingleReader/SingleWriter)
[2] RoundManager.ProcessAsync が次状態を 1 件取り出す
[3] Round スナップショット解決
      state is RoundStateCall call && call.SnapshotRound != null
        ? call.SnapshotRound
        : context_.Round
[4] state.CreateInquirySpec(round, enumerator)
      → RoundInquirySpec { Phase, PlayerSpecs[], LoserIndex? }
[5] CollectResponsesAsync (全員並列 Task.WhenAll)
      各プレイヤーについて:
        a. NotificationId.NewId() (UUIDv7)
        b. BuildNotification(state, round, playerSpec)
             projector.Project(round, playerIndex) で視点射影
        c. tracer.OnNotificationSent
        d. InvokePlayerAsync (9 種の Player.On***Async)
             DefaultTimeout=10秒 で LinkedTokenSource
        e. tracer.OnResponseReceived
        f. ResponseValidator.IsResponseInCandidates で候補集合と照合
        g. 候補外 → tracer.OnInvalidResponse → defaultFactory にフォールバック
        h. OperationCanceledException → tracer.OnResponseTimeout → fallback
        i. その他 Exception → tracer.OnResponseException → fallback
[6] priorityPolicy.Resolve(spec, responses)
      → ImmutableArray<AdoptedPlayerResponse>
[7] ApplyTemporaryFuritenIfRonMissed (Dahai フェーズのみ)
      Ron 候補を提示されたが Ron 以外で応答したプレイヤーに同巡フリテンを付与
[8] tracer.OnAdoptedAction (採用応答を 1 件ずつ記録)
[9] DispatchAsync
      Phase に応じて context_.Response*Async を発火
      → RoundStateContext.Channel<RoundEvent> に積まれ、次状態へ遷移
      → [1] に戻る
```

終了条件は `RoundStateContext.RoundEnded` イベント (終端状態 `RoundStateWin` / `RoundStateRyuukyoku` の OK 応答で発火) で `stateChannel_.Writer.TryComplete()` を呼び、`ProcessAsync` の `await foreach` が自然終了する。

##### フェーズ別 `RoundInquirySpec`

| State                      | `RoundInquiryPhase` | 対象プレイヤー               | 候補 (`CandidateList`)                                                | `LoserIndex` | 採用後の遷移                           |
| -------------------------- | ------------------- | ---------------------------- | --------------------------------------------------------------------- | ------------ | -------------------------------------- |
| `RoundStateHaipai`         | `Haipai`            | 全員                         | `OkCandidate` のみ                                                    | null         | `ResponseOkAsync` → `Tsumo`            |
| `RoundStateTsumo`          | `Tsumo`             | 手番のみ (1 人)              | `DahaiCandidate` + (`TsumoAgariCandidate` / `AnkanCandidate[]` / `KakanCandidate[]` / `KyuushuKyuuhaiCandidate`) | null         | 応答種別に応じ個別 `Response*Async`   |
| `RoundStateDahai`          | `Dahai`             | 他家 3 人 (手番除外)         | `RonCandidate?` + (`ChiCandidate[]` / `PonCandidate[]` / `DaiminkanCandidate[]`) + `OkCandidate` | 手番 (放銃者候補) | 優先順位適用後、Ron (ダブロン対応) / 副露 / OK |
| `RoundStateKan` (加槓)     | `Kan`               | 他家 3 人                    | `ChankanRonCandidate?` + `OkCandidate`                                | 加槓者       | Chankan Ron (ダブロン対応) / OK       |
| `RoundStateKanTsumo`       | `KanTsumo`          | 手番のみ (1 人)              | `DahaiCandidate` + (`RinshanTsumoAgariCandidate` / `AnkanCandidate[]` / `KakanCandidate[]`) | null         | 2 段階ディスパッチ (後述)             |
| `RoundStateAfterKanTsumo`  | `AfterKanTsumo`     | 手番のみ (1 人)              | `DahaiCandidate` + (`AnkanCandidate[]` / `KakanCandidate[]`)          | null         | 応答種別に応じ個別 `Response*Async`   |
| `RoundStateCall`           | `Call`              | 全員                         | `OkCandidate` のみ (副露直後の通知観測点)                             | null         | `ResponseOkAsync` → `Dahai` or `KanTsumo` |
| `RoundStateWin`            | `Win`               | 全員                         | `OkCandidate` のみ (終端状態)                                         | null         | `ResponseOkAsync` → 局終了             |
| `RoundStateRyuukyoku`      | `Ryuukyoku`         | 全員                         | `OkCandidate` のみ (終端状態)                                         | null         | `ResponseOkAsync` → 局終了             |

`LoserIndex` は `Dahai` / `Kan` フェーズのみ非 null。ダブロン判定と「放銃者基準の巡目順」(下家=1 / 対面=2 / 上家=3) による同順位解決で使われる。

##### フェーズ別 `BuildNotification` マッピング

各 `RoundState` → 各 `RoundNotification` 派生型の対応:

| State                      | Notification                                    | 主な内容                                                    |
| -------------------------- | ----------------------------------------------- | ----------------------------------------------------------- |
| `RoundStateHaipai`         | `HaipaiNotification(view)`                      | 配牌直後の視点情報のみ (候補は spec 側)                     |
| `RoundStateTsumo`          | `TsumoNotification(view, tsumoTile, candidates)`| 手番は自ツモ牌を含む、他家は `OtherPlayerTsumoNotification` 相当 (視点射影で隠される) |
| `RoundStateDahai`          | `DahaiNotification(view, discardedTile, discarder, candidates)` | 直前の打牌を河末尾から取得                              |
| `RoundStateKan`            | `KanNotification(view, kanCall, caller, candidates)` | 加槓/暗槓の `Call` を抽出                                   |
| `RoundStateKanTsumo`       | `KanTsumoNotification(view, rinshanTile, candidates)` | 嶺上から手牌末尾へ追加した牌                                |
| `RoundStateAfterKanTsumo`  | `KanTsumoNotification(view, rinshanTile, candidates)` | `KanTsumo` と同じ通知型を再利用                             |
| `RoundStateCall`           | `CallNotification(view, madeCall, caller)`      | `SnapshotRound` から副露直後・嶺上ツモ前の `Round` を参照   |
| `RoundStateWin`            | `WinNotification(view, adoptedWinAction)`       | `BuildAdoptedActionForTrace(eventArgs)` で構築              |
| `RoundStateRyuukyoku`      | `RyuukyokuNotification(view, adoptedRyuukyokuAction)` | 同上                                                        |

`PlayerRoundView` は `IRoundViewProjector` が `Round` + 受信者 `PlayerIndex` から生成する。自分の手牌・非公開状態 (フリテン等) と、他家の公開情報 (河/副露/打点/立直棒) のみを含む。山の中身や他家の手牌は射影時に除外される。

##### `DispatchAsync` の分岐

```
switch (spec.Phase)
{
    // 観測フェーズ: 全員 OK 集約 → 単一 ResponseOkAsync
    case Haipai:
    case Call:
    case Win:
    case Ryuukyoku:
        await context_.ResponseOkAsync();
        break;

    // 入力要求フェーズ: 採用応答に応じ振り分け
    case Tsumo:          await DispatchTsumoAsync(adopted[0]);
    case Dahai:          await DispatchDahaiAsync(adopted, loserIndex);
    case Kan:            await DispatchKanAsync(adopted, loserIndex);
    case KanTsumo:       await DispatchKanTsumoAsync(adopted[0]);
    case AfterKanTsumo:  await DispatchAfterKanTsumoAsync(adopted[0].Response);
}
```

`Dahai` / `Kan` のダブロン判定は `DispatchDahaiAsync` / `DispatchKanAsync` 内で `adopted.Where(x => x.Response is RonResponse or ChankanRonResponse)` を配列のまま `context_.ResponseWinAsync(winners, loserIndex, winType)` に渡す (天鳳準拠でダブロン成立、トリプルロンはルール未確定)。

##### KanTsumo の 2 段階ディスパッチ

`RoundStateKanTsumo` で「嶺上ツモ和了 / 打牌 / 暗槓 / 加槓」を 1 通知・1 応答で受けるが、嶺上ツモ和了 (`RinshanTsumoResponse`) とそれ以外で遷移先が異なる:

- **嶺上ツモ和了**: `ResponseWinAsync([self], self, WinType.Rinshan)` を直接発火 → `RoundStateWin` へ
- **打牌/暗槓/加槓**: `pendingAfterKanTsumoResponse_` にセット → `ResponseOkAsync()` で `RoundStateAfterKanTsumo` に遷移 → メインループ [2] で state が `RoundStateAfterKanTsumo` に切り替わったタイミングで `pending` を消費して `DispatchAfterKanTsumoAsync` を呼ぶ

この 2 段階化により、`AfterKanTsumo` 状態での候補再計算 (四槓流れ除外後) を挟まずに済む。`pending` のセット漏れ防止のため `try/catch` で例外時は `null` に戻す。

##### 候補検証の規約 (`ResponseValidator`)

検証は `PlayerResponse` の具象型ごとに異なる規約で行う:

| 応答                                                          | 判定                                                                          |
| ------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `OkResponse` / `RonResponse` / `TsumoAgariResponse` / `KyuushuKyuuhaiResponse` / `ChankanRonResponse` / `RinshanTsumoResponse` | 対応 `Candidate` 型が候補リストに存在するだけで OK                          |
| `DahaiResponse` / `KanTsumoDahaiResponse`                     | `DahaiCandidate.DahaiOptionList` に `Tile` 完全一致 (Tile.Id)。立直時は `RiichiAvailable=true` が必要 |
| `AnkanResponse` / `KanTsumoAnkanResponse`                     | `AnkanCandidate.Tiles[0].Kind` と一致 (Tile.Kind、4 枚全部を使うため赤ドラの選択余地なし)    |
| `KakanResponse` / `KanTsumoKakanResponse`                     | `KakanCandidate.Tile` と完全一致 (Tile.Id、赤/非赤は別 `KakanCandidate` として列挙済み)       |
| `ChiResponse` / `PonResponse` / `DaiminkanResponse`           | `HandTiles` が `SequenceEqual` (順序と Id 全一致)                            |

検証漏れは「合法な候補が提示されたのに別 Id の牌で応答された」ケースで顕在化するため、赤ドラの取り扱いは候補列挙側と検証側で一貫させる (`KakanCandidate` は赤/非赤で別候補、`AnkanCandidate` は `Kind` 単位で 1 候補)。

##### 優先順位解決 (`TenhouResponsePriorityPolicy`)

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

##### 既定応答フォールバック (`DefaultResponseFactory`)

`ResponseValidator` 不合格 / タイムアウト (10 秒) / プレイヤー例外時に使用。呼び出しは `RoundManager.CollectSingleAsync` の `catch` ブロックと候補外判定の両方から行われる:

| フェーズ         | 既定応答                                                                    |
| ---------------- | --------------------------------------------------------------------------- |
| `Haipai`         | `OkResponse`                                                                |
| `Dahai`          | `OkResponse` (スルー)                                                       |
| `Kan`            | `OkResponse` (スルー)                                                       |
| `Tsumo`          | `DahaiResponse(DahaiCandidate.DahaiOptionList[0].Tile)` (先頭の打牌オプション = 実質ツモ切り) |
| `KanTsumo`       | `KanTsumoDahaiResponse(先頭 DahaiOption)`                                   |
| `AfterKanTsumo`  | `KanTsumoDahaiResponse(先頭 DahaiOption)`                                   |

`Tsumo` 系のフォールバックで `DahaiCandidate` が提示されていない状態は起こりえないため、空なら `InvalidOperationException` を投げる設計。接続断時は **進行継続** (天鳳の CPU 代打相当) を優先し、対局中断はしない。

##### `RoundStateCall` の `SnapshotRound` が必要な理由

`RoundStateCall` は「副露直後の通知観測点」として残されている (削除候補だったが、全通知を統一パイプラインに乗せる設計上、副露通知だけ別経路にすると整合性が崩れるため復活)。問題は大明槓のタイミング:

```
Dahai --(ResponseCall: Daiminkan)--> Call --(ResponseOk)--> KanTsumo
                                     │
                                     └── ここで context.Round は副露直後
                                         (RinshanTsumo 未実行)
```

`RoundStateCall.ResponseOk` が大明槓判定時に `context.Round = context.Round.RinshanTsumo()` を遷移アクションとして実行するため、`RoundStateCall.Entry` の時点では `Round` は副露直後だが、`RoundStateKanTsumo.Entry` が走った直後には嶺上ツモ後になる。

`stateChannel_` が `RoundStateCall` を流した時点で `context.Round` を読んでも、`RoundManager.ProcessAsync` が非同期ループで読む順序によっては RinshanTsumo 後の `Round` が観測される可能性がある (`RoundStateContext` の `Channel<RoundEvent>` と `stateChannel_` は独立キューのため)。副露通知に嶺上ツモ牌が混入するとクライアント表示がずれる。

解決策として `RoundStateCall.SnapshotRound` を `init` プロパティで持ち、遷移時に `Transit(context, () => new RoundStateCall { SnapshotRound = context.Round }, action)` の**ファクトリ版 `Transit`** で副露実行 (`action`) 後・`Entry` 前の `Round` を封入する。ファクトリ版は `Exit → action → nextStateFactory() → Entry` の順に評価するため、`nextStateFactory` の中で `context.Round` を参照した時点の値は `action` 適用後の副露直後 `Round` になる。

```csharp
// RoundState.cs
protected static void Transit(RoundStateContext context, Func<RoundState> nextStateFactory, Action? action = null)
{
    context.Transit(nextStateFactory, action);
}

// RoundStateContext.cs
internal void Transit(Func<RoundState> nextStateFactory, Action? action = null)
{
    State.Exit(this);
    action?.Invoke();           // 副露実行
    State = nextStateFactory();  // この時点の context.Round を SnapshotRound に封入
    State.Entry(this);
}
```

ファクトリ版が必要な本質的理由は「State パターンの `Exit → action → Entry` 順序を崩さずに、`action` 適用後の `context.Round` を次状態のコンストラクタ引数に使いたい」ため。`Immutable` 性維持のためではなく、遷移順序の契約を保ちつつ派生情報を伝搬するための追加オーバーロード。

##### 撤去された旧設計

Phase 5 レビュー前の設計で存在していた以下は全て撤去:

- `CallPerformed` イベント (`RoundStateContext` 上で副露時のみ発火されていた個別チャネル)
- `progressChannel` の union 型 (`OneOf<State, CallEvent>` 風に通知観測と状態遷移を混在させていた)
- `BroadcastCallNotificationAsync` / `BroadcastDahaiNotificationAsync` / `BroadcastWinNotificationAsync` の 3 メソッド (state なしで引数直接渡しだった観測通知専用の送信経路)

通知観測点と意思決定点は通知プロトコル上で区別せず、応答候補集合 (`OkCandidate` のみ vs それ以外) とフェーズ列挙値 (`RoundInquiryPhase`) だけで表現する。これにより Wire DTO 層も通知種別 9 種に閉じ、リプレイ・牌譜再生が一本化される。

### 通知・応答モデル

```
PlayerNotification
  - NotificationId        : Guid ユニークId
  - RoundRevision         : 局内連番 (古い応答の検出・リプレイ用)
  - Recipient             : PlayerIndex 通知先
  - View                  : PlayerRoundView 視点フィルタ済み卓情報
  - Candidates            : ResponseCandidate[] 合法応答候補(OKのみの場面もある)
  - Timeout               : TimeSpan タイムアウト

PlayerResponse
  - NotificationId        : 対応する通知Id
  - RoundRevision         : 対応する局Revision
  - PlayerIndex           : 応答者 (なりすまし防止のため必須)
  - Body                  : 応答内容 (OK / Dahai / Call / Kan / Win / Ryuukyoku)
```

- `ResponseCandidate` は「応答として選べる行動」を列挙する(例: 打牌後に他家が取りうる`OkCandidate` / `RonCandidate` / `ChiCandidate(handTiles)` / `PonCandidate(handTiles)` / `DaiminkanCandidate(handTiles)`)。クライアントは候補から選ぶだけでよい
- **応答はサーバー側で必ず再検証する**。候補の提示はUX/ショートカット用であり、信用してはいけない

#### C# API と Wire DTO の二層構造

プレイヤー通知/応答は **C# API 層** (`Player`) と **Wire DTO 層** (`PlayerNotification` / `PlayerResponse`) の二層で扱う。

- **C# API 層**: `Player.OnTsumoAsync(TsumoNotification) : Task<AfterTsumoResponse>` のように、通知ごとに**戻り値型を専用にする**。コンパイル時の型安全を優先
- **Wire DTO 層**: 別プロセス/別マシンとやりとりする場合に備え、`PlayerNotification` / `PlayerResponse` (Body は discriminated union 相当) を共通 envelope として定義。シリアライズ/通信基盤はこちらを使う
- **変換**: `Player` (または adapter) が C# API 層と Wire DTO 層を相互変換する。ローカル AI プレイヤーは C# API 層で直接呼び出し、別プロセスクライアントは Wire DTO 層経由

### 応答検証の3段階

1. **通信整合性検証**: `NotificationId`と`RoundRevision`が現在待機中のものと一致するか、応答元`PlayerIndex`が通知先と一致するか
2. **応答種別検証**: その通知で許可された種別か、OK以外を返してよいプレイヤーか(例: 打牌応答は手番プレイヤーのみ)
3. **ドメイン検証**: 手牌に指定牌があるか、チーは上家打牌に対してか、槓できる山残数があるか、和了条件を満たすか

`RoundEvent`のコンストラクタでは形状レベルの検証のみ行う。`Round`に依存する合法性検証は`RoundManager`側で`Round`とともに行う。

### 採用済み応答 (AdoptedRoundAction)

プレイヤーからの生応答(`PlayerResponse`)と、ルール適用後の採用結果(`AdoptedRoundAction`)は型を分ける。採用結果型は `Mahjong.Lib.Game.Adoptions` 名前空間に配置する (入力側の `Mahjong.Lib.Game.Inquiries` と対になる出力側)。

```
AdoptedRoundAction (abstract)
  ├ AdoptedOkAction
  ├ AdoptedDahaiAction(Tile)
  ├ AdoptedAnkanAction(Tile) / AdoptedKakanAction(Tile)   // 槓 (暗槓/加槓)
  ├ AdoptedCallAction(PlayerIndex, CallType, Tile[])      // Chi/Pon/Daiminkan
  ├ AdoptedWinAction                                      // ダブロン/トリプルロン対応
  │   {
  │     Winners             : AdoptedWinner[]   // 和了者(複数可)
  │     Loser               : PlayerIndex?      // ロン時の放銃者。ツモ時は null
  │     WinType             : Ron | Tsumo | Chankan | Rinshan
  │     KyoutakuDistribution: ...               // 供託棒の配分ルール(上家取り等)
  │     HonbaDistribution   : ...               // 本場の配分
  │     DealerContinues     : bool              // 親続行判定(連荘可否)
  │   }
  │   AdoptedWinner { PlayerIndex, WinTile, Yaku[], Fu, Han, Score }
  └ AdoptedRyuukyokuAction(RyuukyokuType)
```

- ロンは複数採用され得るため`Winners`は配列。各和了者ごとに`AdoptedWinner`でスコア情報(役/符/翻/点数)を保持
- ダブロン時の供託は**上家取り**、本場は放銃者が全和了者に支払う(天鳳準拠)。これらは`KyoutakuDistribution` / `HonbaDistribution` で表現
- 流局は種別(`SanchaHou` / `Suukaikan` / `Suufonrenda` / `SuuchaRiichi` / `KouhaiHeikyoku` / `KyuushuKyuuhai` 等)を持つ
- 三家和了は「採用しない(流局扱い)」ルールを採る場合、`AdoptedWinAction` ではなく `AdoptedRyuukyokuAction(SanchaHou)` に落とす

### 通知と応答

通知は対象プレイヤー全員へ送り、全員からの応答を待つ。応答候補(`Candidates`)が`OK`のみの場面(配牌通知や他家ツモ通知など)でも、接続状態把握のため全員からの`OK`応答を必須とする。

| 場面           | 応答候補の例                                                              |
| -------------- | ------------------------------------------------------------------------- |
| 配牌通知       | OK                                                                        |
| 自ツモ         | ツモ番: 打牌(Riichi付加可) / 暗槓 / 加槓 / ツモ和了 / 九種九牌、他家: OK  |
| 他家打牌       | 副露可プレイヤー: チー/ポン/大明槓/ロン/OK、不可プレイヤー: OK            |
| 槓(加槓)       | 槍槓可プレイヤー: ロン/OK、それ以外: OK                                   |
| 嶺上ツモ後     | ツモ番: ツモ和了 / 暗槓 / 加槓 / 打牌(リーチ付加可) を**1通知にまとめて**提示、他家: OK |

- 嶺上ツモ後は「ツモ和了/暗槓/加槓/打牌」を1通知・1応答単位で扱う(二段階に分けない)
- **四槓流れ判定**: 嶺上ツモ**直前**に4つ目の槓成立をチェックし、成立なら嶺上ツモに進まず流局(`Suukaikan`)へ遷移

### タイムアウトと既定応答

- タイムアウトは通知種別ごと、またはプレイヤー応答役割ごとに既定値を持つ
- 既定応答(`IDefaultResponseFactory`)の例:
  - 打牌待ちタイムアウト → ツモ切り(リーチ中はそもそもツモ切り強制)
  - 副露/ロン可否タイムアウト → OK(スルー)
  - ツモ番の選択タイムアウト → 打牌(ツモ切り)
  - 九種九牌選択 → OK(流局しない)
  - 接続断 → 上記の既定応答にフォールバック

### リプレイ/ログ

- 通知(全プレイヤー分) / 全応答 / 採用結果(`AdoptedRoundAction`) を順に保存できる形にしておく。天鳳牌譜検証ツールと同様、後でリプレイ・デバッグ可能に
- `RoundRevision`があれば「その時点のRound状態と、そこから採用アクションで遷移した次Round」を再現できる

### プロジェクト分離の方針

- `Mahjong.Lib.Game` — ルール、状態遷移、通知/応答/プレイヤー抽象、`RoundManager` / `GameManager`、各種Policy既定実装
- `Mahjong.Lib.Ai` (新設予定) — AI思考ルーチン、`Player` AI実装
- `Mahjong.ApiService` / `Mahjong.Web` — 人間プレイヤー用transport adapter(WebSocket/SignalR等)
- 最初期は tests や sample 側にランダム AI stub を置く程度で十分。本格 AI は別プロジェクトに切り出す

### 決定事項

- **通信・集約レイヤー名**: `RoundManager`
- **`RoundManager`のライフサイクル**: 1局ごとに新規作成して破棄(`IDisposable`)。持ち点・本場・供託等の引き継ぎは`Game`集約側が担当
- **応答収集**: 早期終了はなし。全プレイヤー(`OK`しか返せない者も含む)の応答が揃うかタイムアウトするまで待つ
- **通知方式**: 1通知に全合法候補を載せる(段階的通知は行わない)
- **リーチ宣言と打牌**: 1応答で表現する(例: `DahaiResponse(tile, isRiichi: true)`)
- **嶺上ツモ後の槓/打牌**: 1通知に両候補を載せ、1応答で遷移
- **ロギング**: `Microsoft.Extensions.Logging.ILogger<T>` (人向けテキスト)。`Mahjong.Lib.Game`に`Microsoft.Extensions.Logging.Abstractions`を追加
- **構造化トレース**: `IGameTracer` を別途用意。**全イベント**をトレースし牌譜・リプレイが再生成できるレベルを目指す。スコープは複数対局を跨ぐグローバル。統計集計(順位率/和了率/放銃率/立直率/副露率/平均順位/平均打点/役出現率/流局理由別出現率 等)は Tracer 実装側で必要イベントを抽出する
- **`IGameTracer`の購読方式**: `GameManager`には単一の`IGameTracer`を注入する。複数購読したい場合は利用側で`CompositeGameTracer`を構成
- **`Game`の局履歴**: `Game` record に過去局の履歴は持たない。履歴集計は`IGameTracer`実装側の責務
- **対局開始パラメータ**: `GameManager`コンストラクタで外部から直接注入(`PlayerList` / `GameRules` / `ImmutableArray<Player>` / `IWallGenerator`)。初期持ち点は`GameRules`(対局前に決まる要素)。当面の初期持ち点は全員35000点。起家は`PlayerList`の並びで表現する(index 0 が起家、並び替えは呼び出し側責務)
- **点数計算の接続**: `Mahjong.Lib.Game` に `IScoreCalculator` 抽象を定義し、`GameManager`に注入する。`Mahjong.Lib.Scoring` の `HandCalculator` をラップする実装は**上位層**(ApiService等)で作って注入する(`Lib.Game` は `Lib.Scoring` に直接依存しない方針を維持)。流局時のノーテン罰符・テンパイ料計算は点数計算器に含めず、`GameManager`/`RoundState`側で天鳳ルール準拠で処理
- **連荘・本場**: 親和了 → 連荘+本場+1、親テンパイ流局 → 連荘+本場+1、子和了/親ノーテン流局 → 親流れ+本場リセット。途中流局(九種九牌/四風連打/四家立直/三家和/四槓散了)は点数移動なし・親流れなし・本場+1(天鳳ルール)。ダブロンなど複雑ケースの扱いはルール実装時に再検討
- **オーラス親あがり止め**: 親がオーラス(南四局など対局形式の最終局)で和了し、その時点で**1位単独確定**の場合のみ止める。同点トップ/1位だが2位との点差で逆転余地がある場合は続行(連荘)。判定は`GameRules`に定義された原点/対局形式に基づき`GameEndPolicy.ShouldEndAfterRound`が局終了時に実施
- **流局時点数移動**: 荒牌平局のテンパイ料(1人:3000 / 2人:1500・3000 / 3人:1000・3000)、ノーテン罰符。いずれも天鳳ルール準拠
- **開始局の指定**: `GameManager` 引数では指定しない。テスト用に途中状態から始めたい場合は、任意の`Round`を注入する別経路で対応
- **対局形式**: `GameFormat { Tonpuu, Tonnan, SingleRound }` の3種類。既定は`Tonnan`。`GameRules`に持たせる
- **1局のみモード(`SingleRound`)**: 親は`PlayerList` index 0 固定、局名「東一局0本場」固定。和了/流局で即`GameStateEnd`遷移(連荘せず、本場も増やさない)。テスト・デバッグ用途
- **赤ドラ判定の配置**: `GameRules.IsRedDora(Tile)` で判定する(設計的に自然)。ドラ表示/点数計算/ログ等の複数箇所からこの関数経由で参照
- **局→対局の持ち点同期**: 局終了時(`GameStateRoundRunning.RoundEndedBy*` ハンドラ内)に`RoundStateContext.Round.PointArray`を読み取って`Game.PointArray`へ書き込む
- **供託の局跨ぎ**: `Game.KyoutakuRiichiCount`を対局通しての供託とし、局開始時に`Game`→`Round`へコピー、局終了時(和了なら和了者に加算、流局なら持ち越し)に`Round`→`Game`へ書き戻し
- **持ち点の保持**: `Game`(対局全体の持ち点) と `Round`(各局進行中の持ち点) の両方で保持する。局終了時に`Round`の最終`PointArray`を`Game`へ反映
- **赤ドラ規約**: 各スート(萬/筒/索)について、該当牌種のうち**下位`Tile.Id`から順に、`GameRules`で指定された枚数分**を赤ドラ扱いとする(天鳳準拠)。例: 赤五萬1枚指定 → `Tile.Id=16`(5m 1枚目)が赤
- **トビ終了点**: 天鳳ルール準拠(0点未満で即終了)
- **`PlayerList`の席順**: 対局開始時に確定し、対局中は固定。局進行における親の移動は`Round`側で管理
- **タイムアウト値**: 当面は構成で固定値とする(ルール由来ではなく`GameManager`引数または`GameRules`の実装時に詳細化)
- **山生成器**: `IWallGenerator`を`GameManager`に注入し、各局で共有利用。乱数シード指定も注入側で制御
- **プレイヤー抽象**: `Player` interface + `Player` 抽象基底クラス。AI/人間は`Player`継承
- **プレイヤー例外時の挙動**: 接続断/タイムアウト超過/不正応答など`Player`から例外が上がった場合、`IDefaultResponseFactory`の既定応答へフォールバックして進行継続。対局中断はしない(天鳳のCPU代打相当)
- **同順位衝突時の決定**: `PlayerIndex` が小さいものを優先。採用したものだけ処理し、それ以外は破棄した上でエラーログを残す
- **同一プレイヤーからの複数応答**: 最後の応答を採用する。古い応答を上書きする際に警告ログを残す
- **リーチ打牌供託のタイミング**: 打牌応答集約後にロン応答がなければリーチ棒を供託、あれば供託しない
- **通知Idの形式**: UUIDv7(`Guid.CreateVersion7()`)。外部通信でのシリアライズ互換性と時系列ソート性を両立
- **既存の`RoundStateContext.ResponseXxxAsync`**: 最終的に`internal`化し、外部公開APIは`RoundManager`経由のみにする。`Mahjong.Lib.Game.Tests`へは既存の`InternalsVisibleTo`で公開
- **3人麻雀対応**: 当面考慮しない。4人麻雀前提で実装する
- **テスト方針**: 単体テストは`InternalsVisibleTo`経由で`RoundStateContext`を直接駆動する既存方式を維持。`RoundManager`経由の応答シナリオテスト(疑似endpointで挙動記述)は別レイヤーとして追加する
- **ルール準拠の線引き**: 点数計算 / 流局処理 / 連荘 / 本場 / トビ / オーラス親あがり止め / 赤ドラ規約等は**天鳳ルール準拠**とする。ただし**初期持ち点は独自に全員35000点**(天鳳の25000点とは異なる意図的ローカル仕様)
- **起家決定**: `PlayerList` の index 0 を起家とする仕様。並び替え(乱数による起家決定を含む)は`Mahjong.Lib.Game`の責務外とし、上位層(ApiService / テストハーネス等)で決定してから渡す
- **嶺上ツモ後の応答単位**: 1通知に「ツモ和了 / 暗槓 / 加槓 / 打牌(リーチ付加可)」をまとめて提示し、1応答で遷移する(状態二段階に分けない)。四槓流れ判定は嶺上ツモの**直前**で行う
- **槍槓ルール**: 槍槓は**加槓のみ**対象とする。暗槓に対する国士無双ロンの可否はルール未確定(TODO: Phase 5 で詰める)。槍槓成立時はカンドラ表示・嶺上ツモ・加槓確定のいずれも行わず、加槓宣言した手牌から該当牌を除いてロン処理へ
- **リーチ供託の精算仕様**:
  - リーチは**鳴かれても成立**する(打牌が通れば成立、ロンが成立した場合のみ不成立)
  - リーチ宣言時点で手牌側から1000点を減算し、打牌応答集約後にロンなしならリーチ棒を供託へ加算する(ロンがあれば供託せず放銃者に流れる 点数計算は`AdoptedWinAction.KyoutakuDistribution`で表現)
  - 鳴き(他家の副露成立)で一発は消滅する
  - 複数和了(ダブロン等)の場合、供託棒は**上家取り**(放銃者から見て最初の和了者に全額)
  - 宣言可能点数は1000点以上(1000点未満はリーチ候補に入れない)
- **局内プレイヤー別状態 (`PlayerRoundStatus`)**: `Round`集約に`PlayerRoundStatusArray` (仮) を追加し、各プレイヤーごとに以下を保持する:
  - 立直 / ダブル立直 / 一発可否 / 同巡フリテン / 永久フリテン / 門前 / 流し満貫資格 / 第一打前(天和/地和/人和可否) / リンシャン中 等
  - 合法候補列挙・和了判定・流局精算で必要となるため、Phase 2 で導入する
- **包(責任払い)**: 大三元 / 大四喜 / 四槓子の役満確定副露(3枚目のポン・槓など)を振った者を責任者として記録する。実装方針は `Call` 履歴から事後導出、もしくは `Round` に責任者フィールドを持たせるかのどちらか。詳細設計は Phase 5 で詰める(TODO)
- **`Player` API 二層構造**: C# API 層は通知別に `Task<TResponse>` で型安全を担保し、Wire/transport 層は共通 `PlayerResponseEnvelope` (discriminated union 相当) でシリアライズ・通信する。変換は `Player` または transport adapter が担当(上記「C# API と Wire DTO の二層構造」参照)

### 実装段階

実装のフェーズ分割・各段階の完了定義は [Roadmap.md](Roadmap.md) を参照(Phase 0: 既存実装 / Phase 1: 対局集約の器 / Phase 2: 点数精算・局内プレイヤー状態 / Phase 3: 通知・応答型 / Phase 4: Player 拡張 / Phase 5: RoundManager / Phase 6: AI / Phase 7: 人間プレイヤー)。
