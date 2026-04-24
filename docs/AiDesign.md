# AI プレイヤー設計

`Mahjong.Lib.Game` の AI プレイヤー実装は、`Mahjong.Lib.Game/Players/Impl/` 配下にバージョン系列で蓄積する。設計思想・アルゴリズム詳細・統計比較はフェーズ計画 ([Roadmap.md](Roadmap.md)) から独立した継続タスクとして本文書で管理する。

## 全体方針

- **命名規則**: `AI_v{major}_{minor}_{patch}_{識別名}` (例: `AI_v0_1_0_ランダム` / `AI_v0_2_0_有効牌`)。識別名は日本語可 (クラス名・ファイル名・表示名すべて同じ日本語を使う)
- **実装場所**: [src/Mahjong.Lib.Game/Players/Impl/](../src/Mahjong.Lib.Game/Players/Impl/)。1 バージョン = 1 ファイル、Factory クラスは同ファイル末尾に定義
- **Factory ペア**: 各 AI は `{AI名}Factory : AiPlayerFactoryBase<{AI名}>(int seed, string displayName)` を必ず提供する。`Create(PlayerIndex, PlayerId)` での席別 `Random` 注入と Fibonacci hashing によるシード派生は [AiPlayerFactoryBase](../src/Mahjong.Lib.Game/Players/AiPlayerFactoryBase.cs) が共通実装する。派生 Factory は `CreatePlayer(PlayerId, PlayerIndex, Random)` の 1 メソッドだけを override する
- **評価基盤**: [tools/Mahjong.Lib.Game.AutoPlay/](../tools/Mahjong.Lib.Game.AutoPlay/) で一括対局 + `StatsTracer` により順位分布・和了率・放銃率・立直率・副露率・平均打点・役出現率等を取得する
- **混在対局**: [MixedPlayerFactory](../tools/Mahjong.Lib.Game.AutoPlay/MixedPlayerFactory.cs) に 4 つの `IPlayerFactory` を渡すと、対局ごとに席配置をシャッフルしながら異なる AI を同卓させる。新バージョンをベースライン (ひとつ前の版や `AI_v0_1_0_ランダム`) と対戦させて差分を見るのが標準の評価手順
- **判断に使えるヘルパー** (すべて `Mahjong.Lib.Game` 内から直接参照可能):
  - [ShantenHelper](../src/Mahjong.Lib.Game/Tenpai/ShantenHelper.cs) — `CalcShanten(Hand)` / `EnumerateUsefulTileKinds(Hand)` (有効牌 = 引くとシャンテン数が減る牌種)
  - [TenpaiHelper](../src/Mahjong.Lib.Game/Tenpai/TenpaiHelper.cs) — `IsTenpai` / `EnumerateWaitTileKinds` / `IsKoutsuOnlyInAllInterpretations` (立直中暗槓の送り槓判定)
  - [VisibleTileCounter](../src/Mahjong.Lib.Game/Tenpai/VisibleTileCounter.cs) — `CountUnseen(PlayerRoundView, TileKind)` で自分視点の未見枚数を取得
  - [ScoringHelper](../src/Mahjong.Lib.Game/Games/ScoringHelper.cs) — 和了時点数計算 (AI 判断でも試算に使える)

## 今後のバージョンへ

新バージョンを追加するときは、本文書の末尾に以下のテンプレートで章を追加する。Factory は `AiPlayerFactoryBase<TPlayer>` を派生させて `CreatePlayer` のみ実装する。

```markdown
## vX.Y.Z AI_vX_Y_Z_識別名

**目的**: 一つ前のバージョンに対して何を改善するか (例: 役選択 / 守備判断 / 押し引き)。

**アルゴリズム**:
- 打牌選択・副露判断・立直判断・和了判断それぞれの挙動を箇条書きで明記
- 参照する情報 (自手牌 / 河 / 副露 / 他家リーチ / ドラ / 局状況 等) を列挙

**クラス構成**:
- [AI_vX_Y_Z_識別名.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_vX_Y_Z_識別名.cs)
  - `AI_vX_Y_Z_識別名(PlayerId, PlayerIndex, Random) : Player`
  - `AI_vX_Y_Z_識別名Factory(int seed) : AiPlayerFactoryBase<AI_vX_Y_Z_識別名>(seed, AI_vX_Y_Z_識別名.DISPLAY_NAME)`

**使用ヘルパー**: ShantenHelper / TenpaiHelper / ScoringHelper 等のどれを使うか

**統計結果** (AutoPlay で取得):
- ベースライン: 一つ前の AI と 4 人卓、シード固定 N 局
- 比較対象: 一つ前の AI (例: AI_v0_2_0_有効牌 と対戦)
- 主要指標: 平均順位 / 和了率 / 放銃率 / 立直率 / 平均打点 / 役出現分布

**既知の限界**: 次バージョンの出発点になる未対応項目を列挙
```

**評価手順**:

1. `tools/Mahjong.Lib.Game.AutoPlay/Program.cs` で `MixedPlayerFactory` にベースライン AI と新 AI を 2:2 (または 1:3) で並べる
2. `--games N --seed S --write-paifu` で大量対局を実行、`StatsReport` で順位分布等を取得
3. 同一 seed で複数回実行して再現性を確認
4. 本文書の該当節に統計結果を記録し、前版との差分を明示する

## v0.1.0 AI_v0_1_0_ランダム

**目的**: 対局ループが回ることを確認するためのベースライン AI。以後の AI バージョンが比較する最底辺。

**アルゴリズム**:

- 通知系 11 種 (`OnGameStart` / `OnRoundStart` / `OnHaipai` / `OnOtherPlayerTsumo` / `OnDahai` (ロン候補なし時) / `OnCall` / `OnDoraReveal` / `OnWin` / `OnRyuukyoku` / `OnRoundEnd` / `OnGameEnd` / `OnOtherPlayerKanTsumo`): 常に `OkResponse`
- `OnTsumoAsync`: `TsumoAgariCandidate` があれば和了、なければ `DahaiCandidate.DahaiOptionList` からランダム選択で打牌 (立直・暗槓・加槓・九種九牌は選ばない)
- `OnDahaiAsync`: `RonCandidate` があればロン、なければスルー (副露しない)
- `OnKanAsync`: 常にスルー (槍槓もしない)
- `OnKanTsumoAsync`: `RinshanTsumoAgariCandidate` があれば和了、なければランダム選択で打牌

**クラス構成**:

- [AI_v0_1_0_ランダム.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_1_0_ランダム.cs)
  - `AI_v0_1_0_ランダム(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_1_0_ランダムFactory(int seed) : AiPlayerFactoryBase<AI_v0_1_0_ランダム>(seed, AI_v0_1_0_ランダム.DISPLAY_NAME)`

**使用ヘルパー**: なし (候補リストの中からランダムに選ぶだけ)

## v0.2.0 AI_v0_2_0_有効牌

**目的**: シャンテン数を最小化しつつ、同シャンテンなら未見有効牌が最多の打牌を選ぶ最小限のテンパイ指向 AI。v0.1.0 に対して和了率・立直率・平均順位で優位に立つことを示す。

**アルゴリズム**:

- シャンテン数が減らない牌を切る
- シャンテン数が減らない牌の中で、それを切ったあと、見えていない有効牌の枚数が最も多いものを切る
- **有効牌**とは引いたときにシャンテン数が減る牌のことである
- 有効牌の牌種別を求めるメソッドと、牌種別を指定して見えていない枚数を求めるメソッドを用意して牌を判断する
- 有効牌枚数が同じ打牌候補が複数ある場合、ランダムに選択する
- リーチ可能ならリーチする
- 和了可能なら和了する (ツモ・ロン・槍槓・嶺上ツモすべて)
- 副露 (チー / ポン / 大明槓) / 暗槓 / 加槓 / 九種九牌は行わない

**打牌選択の詳細** (`SelectBestDahai`):

1. `DahaiOptionList` の各候補 `option` について、捨てた後の 13 枚手牌 `hand13 = hand14.RemoveTile(option.Tile)` を作る
2. `ShantenHelper.CalcShanten(hand13)` でシャンテン数を取得、最小シャンテンの候補群に絞る
3. 各候補について `ShantenHelper.EnumerateUsefulTileKinds(hand13)` で有効牌種集合を取得し、各牌種について `VisibleTileCounter.CountUnseen(view, kind)` で未見枚数を合計する
4. 合計スコア最大の候補群から `Random.Next` で 1 つを選択
5. 選択した `DahaiOption` の `RiichiAvailable` をそのまま `DahaiResponse` / `KanTsumoDahaiResponse` に渡して立直する

**最適化**: 同一 `Tile.Kind` の候補 (例: 萬1 を 3 枚持つ場合の各牌) は `hand13` が同形になるため、`Kind` をキーにシャンテン / 有効牌種集合 / 未見枚数をメモ化して `ShantenCalculator` の呼び出し回数を抑える。

**クラス構成**:

- [AI_v0_2_0_有効牌.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_2_0_有効牌.cs)
  - `AI_v0_2_0_有効牌(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_2_0_有効牌Factory(int seed) : AiPlayerFactoryBase<AI_v0_2_0_有効牌>(seed, AI_v0_2_0_有効牌.DISPLAY_NAME)`

**使用ヘルパー**: `ShantenHelper.CalcShanten` / `ShantenHelper.EnumerateUsefulTileKinds` / `VisibleTileCounter.CountUnseen`

**既知の限界** (次バージョンへの課題):

- 役を考慮していない (役なし形でもテンパイを目指す) ため、リーチ以外の役がつかず門前テンパイでもリーチ後以外の打点が伸びない
- 副露しないので、対子・刻子手・染め手を手牌で作れない
- ドラ / 赤ドラの価値を評価しないため、有効牌枚数同点時の選択がドラ切り偏重になることがある
- 他家のリーチ / テンパイ気配に対する守備判断がない (放銃率が高い)

## v0.3.0 AI_v0_3_0_評価値

**目的**: v0.2.0 の「有効牌同点時はランダム選択」というタイブレーカーを、「対象牌を孤立牌と見立てたときの面子完成ポテンシャル (評価値) が低い牌を優先して切る」書籍由来のロジックに置き換える。将来ターツに発展しやすい牌 (特にドラ・役牌) を手元に残す指向に改善し、v0.2.0 比で平均順位・立直率・平均打点に差を出すことを狙う。

**アルゴリズム**:

v0.2.0 の [SelectBestDahai 内シャンテン→有効牌] 選抜後、`finalists.Count >= 2` の場合にだけ評価値最小グループへ絞り、そこから `Random.Next` でランダム選択する。

**評価値の定義**:

```
評価値(tile) = Σ (useful(x, kind) × adjDoraMultiplier(x, kind)) × outerMultiplier(tile)
               x ∈ EnumerateAdjacents(kind)
               kind = tile.Kind
```

- **くっつき範囲** `EnumerateAdjacents(kind)`: 数牌は同スート内で `kind-2〜kind+2` (`TileKind.TryGetAtDistance` で判定)、字牌は自身のみ
- **使える枚数** `useful(x, kind)`: 5m 例で「3m:2, 4m:2, 5m:1, 6m:3, 7m:3」の `min` 構造に従う
  - `x == kind` (対子形成): `unseen(kind)`
  - `x == kind ± 2` (嵌張): 順子中間牌の未見と min → `min(unseen(x), unseen(kind±1))`
  - `x == kind ± 1` (両面/辺張): 順子 3 枚目候補 2 種の max と min → `min(unseen(x), max(unseen(kind-2 or -1), unseen(kind+1 or +2)))`。端牌で片方のみ同スート内に収まる場合は 1 候補のみ
- **項倍率** `adjDoraMultiplier(x, kind)`: `x != kind` のとき `2^(表ドラ指示回数)` (通常ドラ ×2、ダブドラ ×4)。対象牌自身の項 (`x == kind`) には項倍率を適用しない
- **外側倍率** `outerMultiplier(tile)`: 以下の乗算
  - `2^(doraIndicatorCount[tile.Kind])` (表ドラ ×2、ダブドラ ×4)
  - `GameRules.IsRedDora(tile) ? 2 : 1` (赤ドラ ×2)
  - `tile.Kind が役牌 (三元牌/場風/自風) ? 2 : 1`

**書籍 3 例との整合**:

| 状況 | 計算 | 評価値 |
| --- | --- | --- |
| 發 (ドラ+役牌)、發未見3枚 | `3 × (2 × 2) = 12` | 12 |
| 赤5m、卓面ニュートラル | `(4+4+3+4+4) × 2 = 38` | 38 |
| 5m (ドラ)、6m がドラ、3m:3 / 4m:2 / 5m:1 / 6m:4 / 7m:3 | `(2+2+1+3×2+3) × 2 = 28` | 28 |

**その他の挙動**: 和了/副露/立直/通知応答は v0.2.0 と同一 (ロン・ツモ・槍槓・嶺上ツモすべて和了、副露しない、リーチ可能時は必ずリーチ)。

**使用ヘルパー**: `ShantenHelper.CalcShanten` / `ShantenHelper.EnumerateUsefulTileKinds` / `VisibleTileCounter.CountUnseen` / `TileKind.GetActualDora` / `TileKind.TryGetAtDistance` / `GameRules.IsRedDora` / `RoundNumber.ToDealer`。`GameRules` は `OnGameStartAsync` で通知された `notification.Rules` を `private GameRules? rules_;` に保持して参照する。

**クラス構成**:

- [AI_v0_3_0_評価値.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_3_0_評価値.cs)
  - `AI_v0_3_0_評価値(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_3_0_評価値Factory(int seed) : AiPlayerFactoryBase<AI_v0_3_0_評価値>(seed, AI_v0_3_0_評価値.DISPLAY_NAME)`

**統計結果** (AutoPlay で取得):

- ベースライン: `AI_v0_2_0_有効牌 ×2 + AI_v0_3_0_評価値 ×2` の混在卓、`MixedPlayerFactory` で席シャッフル
- 対局数: **1000 局 × 2 シード** (`--seed 1` / `--seed 2`)、`--parallel 4` で実行 (seed 1: 45 分 / seed 2: 46 分)

| シード | AI | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | v0.2.0 有効牌 | 2.534 | 22.0% | 16.5% | 42.9% | 0.0% | 6468 |
| 1 | v0.3.0 評価値 | **2.466** | **22.9%** | 16.8% | **45.1%** | 0.0% | **6782** |
| 2 | v0.2.0 有効牌 | 2.534 | 22.5% | 17.0% | 43.3% | 0.0% | 6429 |
| 2 | v0.3.0 評価値 | **2.466** | 22.4% | **16.3%** | **44.6%** | 0.0% | **6799** |

両シードで **v0.3.0 が平均順位 -0.068** (2.534 → 2.466)、**平均打点 +300〜350** の差で一貫して優位。2000 局規模で揺らぎが収束し、50 局時の「シード 1 タイ / シード 2 有意差」のようなばらつきは消えた。立直率 +1.3〜2.2pt、和了率 +0.4〜0.9pt。放銃率はシード 1 で僅かに v0.3.0 が高い / シード 2 で v0.3.0 が低いで方向性はつかない (守備判断がないため当然の結果)。役分布は双方とも立直 99%、赤ドラ / ドラ / 裏ドラ 各 40% 前後、一発 30%、門前清自摸和 / 平和 25% で大きな差はなく、打点差は主に表ドラ・赤ドラ保持の傾向差 (評価値で高ドラ牌を残す) に由来すると推定される。

**既知の限界** (次バージョンへの課題):

- 副露 (チー / ポン) をしないため、対子手・染め手・役牌手を作れない (v0.2.0 から未解決)
- 立直以外の役を積極的に狙わないため、食いタン/染め手/三色/一気通貫などの打点源が育たない
- 他家のリーチ / テンパイ気配に対する守備判断がないため放銃率は v0.2.0 比ほぼ同水準
- **くっつき先の赤ドラは考慮していない**: 隣接牌種の未見に含まれる赤ドラ Tile の存在は評価値に反映しない (対象牌自身の赤ドラのみ反映)。改善余地あり
- 終盤の受け入れ優先度・押し引き判断がないため、リーチ後のベタオリ / 現物選択などは行えない

## v0.4.0 AI_v0_4_0_回し打ち

**目的**: v0.3.0 までの AI は他家リーチに対して何も警戒しない (放銃率 16-17%)。書籍に準拠して、他家リーチ時のみ「シャンテン数と切りたい牌の危険度」で押し引きを判定する最小限の守備ロジックを追加する。v0.3.0 の「有効牌最大化 + 評価値タイブレーカー」攻撃ロジックはそのまま維持し、リーチ者検出時のみ危険度フィルタを被せる。

**ドメイン拡張**:

- [PlayerRoundStatus](../src/Mahjong.Lib.Game/Rounds/PlayerRoundStatus.cs) に `SafeKindsAgainstRiichi: ImmutableHashSet<TileKind>?` を追加 (立直前は null、立直宣言者のみ値を持つ)
- [Round.ConfirmRiichi](../src/Mahjong.Lib.Game/Rounds/Round.cs) で立直者自身の河牌種 + 鳴かれて河から消えた自分の捨て牌 (`TilesCalledFromRiver`) の牌種で初期化する。フリテン判定 (`EvaluateFuriten`) と同じ現物集合
- [Round.Dahai](../src/Mahjong.Lib.Game/Rounds/Round.cs) で全立直中プレイヤー (打牌者本人を含む) の `SafeKindsAgainstRiichi` に打牌牌種を追加する。立直者自身のツモ切りも振り聴ルールで現物扱いになるため本人を含める
- [OwnRoundStatus](../src/Mahjong.Lib.Game/Views/OwnRoundStatus.cs) / [VisiblePlayerRoundStatus](../src/Mahjong.Lib.Game/Views/VisiblePlayerRoundStatus.cs) にも露出し、[RoundViewProjector](../src/Mahjong.Lib.Game/Rounds/Managing/RoundViewProjector.cs) が射影する
- 集合方式のため副露で河から消えた牌も情報を失わず保持される (河長スナップショット方式と違い鳴かれた安全牌が復元可能)
- 書籍の「リーチ宣言後は他家の捨て牌も現物扱い」を、AI 内部状態を持たずに View から読むだけで実現する設計

**アルゴリズム**:

- リーチ者がいない局面: v0.3.0 と同じ (有効牌最大化 + 評価値タイブレーカー)
- リーチ者がいる局面: 自シャンテン数で分岐
  - テンパイ (`s == 0`): 通常打牌 (リーチ可能ならリーチ = 押し)
  - 1 シャンテン (`s == 1`): 攻撃打牌の危険度 ≤ 5 ならそれを切る (回し打ち、運よくテンパイすればリーチ)、> 5 ならベタオリ
  - 2 シャンテン以上: ベタオリ (全打牌候補から危険度最小の牌)
- ベタオリ時はリーチしない (`IsRiichi=false`)
- 副露 / 暗槓 / 加槓 / 九種九牌は行わない (v0.3.0 同様)
- 和了 (ツモ / ロン / 槍槓 / 嶺上ツモ) は必ず行う

**危険度計算** (`DangerEvaluator`):

- 対象リーチ者の現物 (`safeKinds` に含まれる牌種) → **0**
  - `safeKinds` = リーチ者自身の河牌種 ∪ 鳴かれた自分の捨て牌牌種 ∪ リーチ宣言後に全プレイヤー (自分含む) が捨てた全牌種
  - `view.OtherPlayerStatuses[i].SafeKindsAgainstRiichi` から直接取得 (AI 内部状態なし)
- 字牌 → `min(unseen, 3)` (ラス牌=0 / 3 枚見え=1 / 2 枚見え=2 / 生牌=3)
- 数牌 → `safeKinds` にスジ牌 (`k±3` 同スート内) が含まれるかで判定 (書籍の `_dapai[l]` と同じ集合)
- 複数リーチ者がいる場合は各リーチ者に対する危険度の **最大値**

**危険度表**:

| 牌の種類 | 無スジ | 片スジ | スジ | 生牌 | 2枚見え | 3枚見え | ラス牌 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 字牌 | — | — | — | 3 | 2 | 1 | 0 |
| 1・9 牌 | 6 | — | 3 | — | — | — | — |
| 2・8 牌 | 8 | — | 4 | — | — | — | — |
| 3・7 牌 | 8 | — | 5 | — | — | — | — |
| 4・5・6 牌 | 12 | 8 | 4 | — | — | — | — |

**クラス構成**:

- [AI_v0_4_0_回し打ち.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_4_0_回し打ち.cs)
  - `AI_v0_4_0_回し打ち(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_4_0_回し打ちFactory(int seed) : AiPlayerFactoryBase<AI_v0_4_0_回し打ち>(seed, AI_v0_4_0_回し打ち.DISPLAY_NAME)`
  - `internal static class DangerEvaluator` — 危険度計算 (テストから直接呼べるよう internal)

**使用ヘルパー**: v0.3.0 と同じ一式 + `TileKind.TryGetAtDistance(±3)` / `TileKind.IsHonor` / `TileKind.Number` / `VisibleTileCounter.CountUnseen`

**統計結果** (AutoPlay で取得):

- ベースライン: `AI_v0_3_0_評価値 ×2 + AI_v0_4_0_回し打ち ×2` の混在卓、`MixedPlayerFactory` で席シャッフル
- 対局数: **1000 局 × 2 シード** (`--seed 1` / `--seed 2`)、`--parallel 4` で実行 (seed 1: 約 50 分 / seed 2: 約 65 分)

| シード | AI | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | v0.3.0 評価値 | 2.614 | 21.9% | 19.2% | 48.4% | 0.0% | 6901 |
| 1 | v0.4.0 回し打ち | **2.386** | 19.3% | **7.4%** | 36.3% | 0.0% | 6812 |
| 2 | v0.3.0 評価値 | 2.583 | 22.2% | 19.0% | 48.2% | 0.0% | 6865 |
| 2 | v0.4.0 回し打ち | **2.417** | 18.8% | **7.5%** | 35.4% | 0.0% | 6856 |

両シードで **v0.4.0 が平均順位 -0.20 (2.60 → 2.40)**、**放銃率 -11.7pt (19.1% → 7.45%、v0.3.0 比およそ 4 割弱)** を一貫して示し、守備ロジックが明確に効いている。和了率は -3.0pt、立直率は -12.5pt で、ベタオリ時にリーチしない分が差となって現れる一方、平均打点はほぼ同等 (-25) で「押し時の打点は維持されている」ことを裏付ける。参考書籍の同等アルゴリズム同士 4 人対戦 (放銃率 7.0% / 立直率 36.7%) と放銃率・立直率とも極めて近い水準で、書籍 0102 にほぼ忠実に再現できている。

**既知の限界** (次バージョンへの課題):

- リーチ者以外の聴牌気配 (染め手・役牌対子の鳴き / ドラ切り / テンパイ気配等) に対する警戒がない
- 回し打ち時の危険度 5 / 6 の閾値は書籍通りでチューニング未検証
- テンパイ時の押しは無条件 (安牌があるテンパイでも必ず押す)
- 副露しないため、相手の鳴きに対する押し引きは発生しない (自分の鳴き判断がそもそも無い)
- 壁・ワンチャンスなど、河以外からの安牌推定は行っていない (参考書籍の同等アルゴリズムも同様)

## v0.5.0 AI_v0_5_0_鳴き

**目的**: v0.4.0 までは副露を一切行わず、打点源が実質「立直＋ドラ/裏ドラ/一発」に集中し、翻牌・対々和・一色手などの鳴きで完成する手役が実質ゼロだった。v0.5.0 で「役に対するシャンテン数」と「副露マーク (+/-) 付き有効牌」による評価を導入し、役なしテンパイを避けつつチー/ポン/大明槓/暗槓/加槓を採否する。v0.4.0 の回し打ち守備はそのまま維持する。

**アルゴリズム**:

- **役に対するシャンテン数** ([YakuAwareShantenHelper](../src/Mahjong.Lib.Game/Tenpai/YakuAwareShantenHelper.cs)): 以下 7 経路の min を「実質シャンテン」として扱う。いずれも成立しえない場合は Infinity (= 99)
  - **門前 (Menzen)**: 非暗槓 Call があれば Infinity、それ以外は `ShantenCalculator.Calc(list, knownCallMeldCount=暗槓数)` の通常シャンテン (暗槓は門前維持)
  - **翻牌 (Yakuhai, 白/發/中 + 場風 + 自風)**: 対象 kind が刻子確定 (Call に pon/kan or 手牌 3 枚以上) → 通常シャンテン / 対子のみ → 対子除去後 `knownCallMeldCount+1` でシャンテン計算し +1 補正 / どちらもなし → Infinity
  - **断么九 (Tanyao)**: `!KuitanAllowed && 非暗槓 Call` or Call に么九含む → Infinity、それ以外は手牌から么九除去してシャンテン計算し、除去枚数を下限ペナルティとして加算 (除去された么九牌は断么九では使えず最終的に必ず打牌する必要があるため、過小評価を防ぐ安全側補正)
  - **対々和 (Toitoi)**: Call に順子あれば Infinity、それ以外は直接公式 `8 - 2*刻子数 - 対子数` (ブロック超過時 `対子数 = 5 - 刻子数` に補正)
  - **一色手 (Isshoku, m/p/s の 3 経路)**: Call に対象スートでも字牌でもない牌あれば Infinity、それ以外は手牌から他スート数牌除去 (字牌は残す) してシャンテン計算し、除去枚数を下限ペナルティとして加算 (断么九経路と同じ理由)
- **副露マーク付き有効牌** ([YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark](../src/Mahjong.Lib.Game/Tenpai/YakuAwareShantenHelper.cs)): 34 牌種のうち「引くと役ありシャンテンが減る」有効牌 K について、手牌に K が 2 枚以上ありポン仮想でも役ありシャンテンが進めば `Pon` マーク、K を含む順子仮想 (手牌から 2 枚寄与) で進めば `Chi` マーク。両方成立なら Pon 優先。
- **打牌選択**: `DahaiOption` ごとに `hand13 = hand14.RemoveTile(option.Tile)` で役ありシャンテンを計算し、最小候補に絞る。その中で評価値 `ev = Σ unseen(K) × multiplier(Mark)` (Pon=×4 / Chi=×2 / None=×1) 最大の候補をランダム選択。`DahaiOption.RiichiAvailable` をそのまま採用。
- **副露判断** (`OnDahaiAsync`): ロン優先 → 自テンパイなら副露しない → 他家リーチ & 自シャンテン>1 ならベタオリ継続 → 大明槓は副露後シャンテン維持で採用、ポン/チーは進めば採用。
- **暗槓/加槓判断** (`OnTsumoAsync` / `OnKanTsumoAsync`): 和了優先 → 他家リーチ & 自非テンパイならカンしない → 副露後シャンテン維持で採用。
- **守備判断 (回し打ち)**: v0.4.0 の [DangerEvaluator](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_4_0_回し打ち.cs) をそのまま再利用。他家リーチ時は自シャンテン別に (テンパイ: 押し / 1 シャンテン: 危険度 ≤ 5 で回し打ち / 2 シャンテン以上: ベタオリ) 分岐。

**クラス構成**:

- [AI_v0_5_0_鳴き.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_5_0_鳴き.cs)
  - `AI_v0_5_0_鳴き(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_5_0_鳴きFactory(int seed) : AiPlayerFactoryBase<AI_v0_5_0_鳴き>`
- [YakuAwareShantenHelper.cs](../src/Mahjong.Lib.Game/Tenpai/YakuAwareShantenHelper.cs) — 内部ヘルパー (`Calc` + `EnumerateUsefulTileKindsWithCallMark`)
- [ShantenCalculator.Calc(list, knownCallMeldCount)](../src/Mahjong.Lib.Scoring/Shantens/ShantenCalculator.cs) — 明示的な確定面子数を指定できる overload を新設。`(14-N)/3` 自動推定では役別経路 (幺九除去・他スート除去・対子除去) のフィルタ済み手牌でズレるため

**フレームワーク差分**:

- 副露 (チー/ポン) 直後に副露者へ打牌を要求する遷移がこれまで未実装で、`RoundStateCall → RoundStateDahai` と直結していた結果、打牌者の河が空のまま `RoundStateDahai.CreateInquirySpec` が例外になっていた。v0.5.0 実装時に判明し、本系列で [RoundStateAfterCall](../src/Mahjong.Lib.Game/States/RoundStates/Impl/RoundStateAfterCall.cs) を新設 (`RoundInquiryPhase.AfterCall` + `EnumerateForAfterCall` + `FromWire` + `DefaultResponseFactory` 含む)。副露者への問い合わせ候補は打牌のみ (暗槓・加槓・ツモ和了・九種九牌は不可)。遷移図は [docs/Design.md](Design.md) を参照。
- 副露者向け通知は [AfterCallNotification](../src/Mahjong.Lib.Game/Notifications/AfterCallNotification.cs) (`CalledTile` を運搬)、他家向けは [OtherPlayerAfterCallNotification](../src/Mahjong.Lib.Game/Notifications/OtherPlayerAfterCallNotification.cs) の専用型。`Player` 抽象は [`OnAfterCallAsync`](../src/Mahjong.Lib.Game/Players/Player.cs) (戻り値 `Task<DahaiResponse>`、非門前のため立直不可) と `OnOtherPlayerAfterCallAsync` (`Task<OkResponse>`) を追加。従来 `RoundStateAfterCall` で `TsumoNotification` を再利用していた中途半端な実装は撤去済み。

**使用ヘルパー**: `YakuAwareShantenHelper.Calc` / `YakuAwareShantenHelper.EnumerateUsefulTileKindsWithCallMark` / `VisibleTileCounter.CountUnseen` / `DangerEvaluator` (v0.4.0 から再利用) / `ShantenCalculator.Calc(list, knownCallMeldCount)`

**統計結果** (AutoPlay で取得):

- ベースライン: `AI_v0_4_0_回し打ち ×2 + AI_v0_5_0_鳴き ×2` の混在卓、`MixedPlayerFactory` で席シャッフル
- 対局数: **1000 局 × 2 シード** (`--seed 1` / `--seed 2`)、`--parallel 4 --no-paifu` で実行

| シード | AI | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | v0.4.0 回し打ち | 2.572 | 14.5% | 9.4% | 34.3% | 0.0% | 7053 |
| 1 | v0.5.0 鳴き | **2.428** | **21.9%** | 9.7% | 27.7% | **23.2%** | 5193 |
| 2 | v0.4.0 回し打ち | 2.556 | 14.8% | 9.7% | 34.5% | 0.0% | 6992 |
| 2 | v0.5.0 鳴き | **2.444** | **21.6%** | 9.4% | 27.6% | **23.5%** | 5196 |

両シードで **v0.5.0 が平均順位 -0.13 (2.56 → 2.44)**、**和了率 +7.1pt (14.7% → 21.8%)** を一貫して示し、副露による手作りが順位・和了率の両面で優位に働いている。副露率は 0% → 約 23% に乗り、立直率は -6.8pt (34.4% → 27.7%)、平均打点は -1828 (7022 → 5194) で、鳴いて門前役 (立直/裏ドラ/一発) を捨てる分打点が落ちることと整合する。放銃率は ±0pt でほぼ互角 — v0.4.0 の回し打ち守備を維持しつつ和了量で稼ぐ構図になっている。役分布では翻牌 (白/發/中/場風/自風 合計) が v0.4.0 の 約 7% から v0.5.0 では 約 41% に拡大し、断么九も 12.3% → 14.5% に上昇。書籍 0202 が意図する「鳴きで作れる役を狙う」挙動がそのまま現れている。

**既知の限界** (次バージョンへの課題):

- 翻牌の対子ポン仮想は 14 牌状態での +1 補正が微小にズレうる (`ShantenCalculator` の境界計算)
- 断么九・一色手の除去枚数ペナルティは下限保証のみで過剰補正になりうる (例: 么九牌と有効牌が同じターンで入れ替わる場合、実シャンテンは除去枚数ほど遠くない)
- 字一色専用経路は持たず、一色手 (m/p/s) の字牌包含形からのみ評価される
- 副露後の放銃率上昇は不可避 (立直していない相手から鳴ける牌の過剰受けが生じうる)
- 副露による打点低下は避けられない (立直/赤ドラ/裏ドラ/一発のボーナスを失うため、子の平均打点が v0.4.0 比で下がる想定)
- くっつき有効牌の未見枚数評価は 34 牌種全体をなめるため打牌候補数が多い局面でやや重い (Kind 単位のメモ化で緩和)
- 攻撃時のタイブレーク評価値 `ev = Σ unseen(K) × multiplier(Mark)` に、対象牌自身のドラ/赤ドラ/役牌倍率 (v0.3.0 の `outerMultiplier`) が反映されていない。有効牌 EV 同点時に赤 5 や役牌を切りうる退行。v0.5.1 以降でタイブレーカーに `outerMultiplier` を戻す方針
- 翻牌経路 (`YakuAwareShantenHelper.CalcYakuhai`) は対子以上のみ評価する近似。孤立 1 枚の役牌は `INFINITE` 扱いになるため、混在副露手で「まだ 1 枚だけ残る役牌」を過剰に切りやすい

## v0.6.0 AI_v0_6_0_手作り

**目的**: v0.5.0 までは「役あり有効牌枚数」で打牌・副露を決めていたため、打点差が大きい選択肢 (例: 両面 2 種 8 枚 30 符 1 翻 / 嵌張 1 種 4 枚 40 符 3 翻+ドラ) でも枚数優先で安い手を選んでしまう。v0.6.0 は kobalab/majiang-ai の 0301〜0305 を C# に移植し、**和了打点を再帰評価値として打牌・副露・槓を選ぶ**。v0.4.0 守備 + v0.5.0 深いシャンテンのフォールバックを維持したハイブリッド構成。

**ハイブリッド構成** (役ありシャンテン `yakuShanten` で分岐):

| 状況 | 打牌選択 | 守備 | 副露判断 |
|---|---|---|---|
| 他家リーチ & 非テンパイ & 危険度 > 5 | v0.4.0 ベタオリ | ✓ | しない |
| 他家リーチ & 1 シャンテン & 危険度 ≤ 5 | 攻撃ルート | ✓ | しない |
| 通常 & `yakuShanten` ≥ 3 | v0.5.0 相当 (有効牌 × 副露マーク × 0305 染め重み) | — | v0.5.0 (役ありシャンテン減少で採用) |
| 通常 & `yakuShanten` ≤ 2 | **0301+0302 評価値ベース** | — | **0304 評価値ベース** |
| テンパイ | ツモ和了 / 通常打牌 | — | しない |

**アルゴリズム**:

- **0301 再帰評価値** ([HandShapeEvaluator.EvaluateHand13 / EvaluateHand14](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluator.cs)):
  - `EvaluateHand14(hand14)`: 和了形なら `CalcHandScore` (後述)、それ以外は全打牌候補 (シャンテン戻らないもの) の `EvaluateHand13` の max
  - `EvaluateHand13(hand13)`: シャンテン数に応じた補正係数 × Σ (unseen(K) × EvaluateHand14(hand13 + K)) over 有効牌 K
  - シャンテン補正: テンパイ ×18 / 1 シャンテン ×3 / 2 シャンテン ×1 (書籍の分母 216/12/72/216 を約分した long スケール整数、異なるシャンテン数間の比較が厳密に可能)
  - 再帰打ち切り: `yakuShanten ≥ 3` は v0.5.0 相当のフォールバック
- **0302 シャンテン戻し** ([HandShapeEvaluator.EvaluateBaktrack](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluator.cs)): シャンテンが戻る打牌候補は通常打牌最大値 × 2 を閾値に、各有効牌 14 枚評価値がそれを超えたものだけ加算。引き戻し除外 (打牌した牌種を有効牌から除く) とフリテン除外 (EvaluateHand14 で `winCandidate == back` なら 0 返却) を実装
- **0303 副露評価値** ([HandShapeEvaluator.EvaluateFulouForTile](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluator.cs)): `EvaluateHand13` の有効牌 K 加算時に「他家から K が出たときのポン/チー成立時評価値」を加算。ポン ×3 (3 家から成立可能) / チー ×1 (下家のみ) / 両立時は `peng×2 + chi` の書籍合算式。副露後打牌はテンパイ形なら待ち牌 × `CalcHandScore` の期待値で 1 段限定の軽量評価 (仮想副露の積み重ね爆発を防ぐ再帰ガード `inFulouRecursion_` + `EvaluateFulouPostDahaiTerminal`)
- **0304 副露判断ハイブリッド** ([AI_v0_6_0_手作り.SelectFulouByEvaluation](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_6_0_手作り.cs)): `yakuShanten ≤ 2` のとき、副露しない場合の評価値と大明槓/ポン/チー各候補の副露後評価値 (`EvaluateFulouCandidate`) を比較し最大値の行動を選択。`yakuShanten ≥ 3` は v0.5.0 相当 (役ありシャンテン減少で採用)
- **0305 一色手/大三元/四喜和狙い** ([TileWeights](../src/Mahjong.Lib.Game/Players/Impl/TileWeights.cs)): 各スート+字牌の合計 ≥10 枚なら同スート ×2・字牌 ×4 / 三元牌合計 ≥6 枚なら三元 ×8 / 風牌合計 ≥9 枚なら風 ×8 の牌種別重み。`yakuShanten ≥ 3` のフォールバックルートの評価値 `ev = Σ unseen(K) × multiplier(Mark) × TileWeights.Of(K)` に乗算する (書籍では一色手・大三元・四喜和に寄せた打牌を促す補正)

**CalcHandScore** ([HandShapeEvaluator.CalcHandScore](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluator.cs)): ツモ前提 + メンゼンならリーチ付与 + 一発/裏ドラなしで [HandCalculator.Calc](../src/Mahjong.Lib.Scoring/HandCalculating/HandCalculator.cs) を呼び、`Score.Main + Sub*2` を返す (点数総移動)。赤ドラは手牌側に含まれる枚数のみを `AkadoraCount` に反映 (ツモ引きの赤ドラ期待値は近似しない)。役なしエラー (非メンゼン × 役なし) は 0 返却。

**キャッシュ戦略**:

- **AI インスタンス単位 + 局スコープ (シャンテン/有効牌含む)**:
  - `shantenCacheGlobal_` (HandSignature + meldCount → shanten) / `usefulCacheGlobal_` (同 → 有効牌集合) は純粋関数だが、長時間 AutoPlay (数百局以上) でプロセス共有にすると無制限に膨張して GC スラッシング / OOM を起こしたため、AI インスタンス単位の `Dictionary` で保持し `ClearBetweenRounds` (局開始・配牌・局終了時) でクリア。局内はドラ開示・副露・打牌が走ってもクリアしない (局面独立な純粋関数のため)
  - `handScoreCache_` (HandSignature + akadora + winTile + menzen + Calls → Score): ドラ/赤ドラ状態を含むため対局内。`ClearAll` で局開始・ドラ開示・副露でクリア
  - `evalCache_` (HandSignature + Calls + BackMarker + BackKind + akadora → long): 未見枚数依存のため `ClearEvalOnly` で毎ツモ・打牌でクリア。`akadora` はキーに必須 — `HandSignature` / `CallsSignature` が意図的に赤黒を区別しない (34 牌種) ため、赤 5 含む hand と含まない hand の評価値が衝突するのを防ぐ
  - `fulouCache_` (hand13 + p + Calls + akadora → long): 同一 (hand13, p, ctx.Calls, akadora) の副露評価を共有。`ClearEvalOnly` で毎ツモ・打牌でクリア
- `HandSignature` は 34 牌種 × 3 bit パックの 128 bit 構造体 (`ulong lo + ulong hi`)。`CallsSignature` は 64 bit FNV-1a ハッシュ。`ImmutableList`/`SequenceEqual` ベースのキーより高速
- `useful` 列挙は [ShantenHelper.EnumerateUsefulTileKinds](../src/Mahjong.Lib.Game/Tenpai/ShantenHelper.cs) を使わず **インライン化**。34 候補の shanten 計算結果を `shantenCacheGlobal_` に格納し、続く `EvaluateHand14` での shanten lookup がキャッシュヒットするよう二重計算を除去

**クラス構成**:

- [AI_v0_6_0_手作り.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_6_0_手作り.cs)
  - `AI_v0_6_0_手作り(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_6_0_手作りFactory(int seed) : AiPlayerFactoryBase<AI_v0_6_0_手作り>`
- [HandShapeEvaluator.cs](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluator.cs) — 評価値計算器の本体
- [HandShapeEvaluatorContext.cs](../src/Mahjong.Lib.Game/Players/Impl/HandShapeEvaluatorContext.cs) — 局面不変情報 (Rules / 風 / ドラ / Calls / GetUnseen / TileWeights / BackMarker) + 遅延派生キャッシュ (CallsSignature / ScoringCallList / ScoringGameRules / ScoringDoraIndicators)
- [HandSignature.cs](../src/Mahjong.Lib.Game/Players/Impl/HandSignature.cs) — 34 牌種 × 3 bit パック署名
- [CallsSignature.cs](../src/Mahjong.Lib.Game/Players/Impl/CallsSignature.cs) — 64 bit FNV-1a 副露ハッシュ
- [TileWeights.cs](../src/Mahjong.Lib.Game/Players/Impl/TileWeights.cs) — 0305 の牌種別重み

**元実装 (kobalab/majiang-ai 0305) との差異**:

- **赤ドラ**: 手牌側の枚数のみを `CalcHandScore` に反映。ツモ引き時の赤ドラ期待値は近似しない (評価値計算中に追加される "仮想和了牌" は `RepresentativeNonRedTile` で常に非赤 Id を使用)
- **一発・裏ドラ**: 和了打点計算で考慮しない (元実装と同じ)
- **役なし 0 点**: `HandCalculator.Calc` の `ErrorMessage != null` のとき 0 を返す。メンゼン手は強制リーチ付与で回避されるが、非メンゼン × 役なしは 0
- **Kind レベルの赤ドラ曖昧性**: 内部再帰が 34 牌種レベルで進むため、count=0 の赤ドラ/非赤ドラ識別は元 hand13 から継承する近似 (iterateし直さない)
- **シャンテン閾値**: `yakuShanten ≤ 2` のみ評価値計算、`yakuShanten ≥ 3` は v0.5.0 相当フォールバック (元実装 0305 と同じ)
- **ハイブリッド構成**: v0.4.0 守備ロジック (他家リーチ時の押し/回し/ベタオリ) を保持 (元実装 0305 にはない独自拡張)
- **副露再帰ガード**: `EvaluateFulouForTile` が再帰中は eval_fulou 加算をスキップ (仮想副露の `CallsSignature` 爆発を抑える近似)

**統計結果** (AutoPlay 1000 局 × 2 シード):

- ベースライン: `AI_v0_5_0_鳴き ×2 + AI_v0_6_0_手作り ×2` の混在卓、`MixedPlayerFactory` で席シャッフル
- 対局数: 1000 局 × 2 シード (`--seed 1` / `--seed 2`)、`--parallel 4 --no-paifu` で実行

| シード | AI | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | v0.5.0 鳴き | 2.126 | 25.9% | 9.0% | 32.6% | 25.5% | 5225 |
| 1 | v0.6.0 手作り | 2.874 | 8.9% | 9.5% | 17.9% | 9.8% | 8804 |
| 2 | v0.5.0 鳴き | 2.136 | 25.8% | 8.6% | 33.0% | 25.3% | 5197 |
| 2 | v0.6.0 手作り | 2.864 | 8.6% | 9.8% | 17.5% | 9.6% | 8735 |
| 平均 | v0.5.0 鳴き | 2.131 | 25.85% | 8.8% | 32.8% | 25.4% | 5211 |
| 平均 | v0.6.0 手作り | 2.869 | 8.75% | 9.65% | 17.7% | 9.7% | 8770 |

**考察**:

- **平均打点**: v0.6.0 は v0.5.0 比 +68.3% (5211 → 8770) を達成し、打点最適化の方向性は統計的に確認できた (書籍 0301〜0305 の想定通り)
- **和了率の激減**: v0.6.0 は 25.85% → 8.75% (-66.2%) に落ちた。**平均順位は 2.131 → 2.869 と大幅悪化**し、打点向上で順位を取り返せなかった。副露率 9.7% (v0.5.0 の 25.4% の 4 割弱) / 立直率 17.7% (v0.5.0 の 54% 弱) から、門前高打点に固執し和了機会自体を失っていることが明確
- **役構成**: v0.6.0 の和了のうち立直 86.5-87.4% / 門前清自摸和 49.4-50.2% / 平和 37.6-40.3% と高打点門前役に偏重 (v0.5.0 は立直 53-54% / 門前自摸 30% 程度)。混一色は 1.6-1.8% と想定ほど出ておらず、0305 染め重みが発動する局面は限定的
- **放銃率**: v0.6.0 は 9.65% と v0.5.0 の 8.8% より微増。攻撃局面の短縮で守備不能局面が減ったが、評価値ルートで危険牌を切る判断が発生している可能性あり (v0.4.0 守備は継承しているが、1 シャンテン押し込みの判定基準が v0.5.0 と異なる)
- **元実装との乖離**: kobalab/majiang-ai の 0305 公開スコアは 0304 比で平均打点 +1200 / 順位 -0.05 程度の改善報告。本実装の打点ゲインは方向として一致するが、和了率ロスが支配的で **ネット順位は大きく悪化**。この差は (a) 赤ドラ期待値の近似誤差、(b) 副露判定 1 段打ち切り、(c) シャンテン戻し閾値 `min_ev × 2` が厳しすぎる可能性、のいずれか / 組合せと考えられる
- **結論**: v0.6.0 単体では実用 AI にならず、次イテレーションで和了率回復 (シャンテン戻し閾値の再調整、副露評価の深さ拡張、テンパイ維持優先度の再検討) が必要

**既知の限界** (次バージョンへの課題):

- Hand 表現が `ImmutableList<Tile>` ベースのため、`EvaluateHand14` の discard ループで tile × 14 回の `RemoveTile` alloc が発生し、2 シャンテン決定で 100ms オーダーの実行時間。count[34]-array ベースの内部表現に置き換えれば 3〜5 倍の高速化余地
- 赤ドラ期待値の近似: 評価値再帰で追加する仮想牌が非赤として扱われるため、赤牌が手牌にない時の `AkadoraCount` が理論値より小さくなる
- フリテン判定は `back` 引数で EvaluateHand14 の和了形を弾くだけで、永続フリテン (河に待ち牌がある状態) は考慮しない
- 副露判定評価値は 1 段限定 (`EvaluateFulouPostDahaiTerminal` がテンパイのみ評価)。書籍 0303 オリジナルは再帰するが、仮想副露の `CallsSignature` 爆発で計算量 10x になるため本実装では打ち切り
- 一発/裏ドラ未考慮: リーチ時の期待値を低めに見積もる
- シャンテン/有効牌キャッシュはインスタンス単位で局境界にクリアするため、局を跨いだ warm-up によるヒット率向上は得られない (静的共有を試したところ長時間 AutoPlay で OOM を起こしたため断念)

## v0.6.1 AI_v0_6_1_手作り

[AI_v0_6_1_手作り.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_6_1_手作り.cs)

**目的**: v0.6.0 の「和了率 25.85%→8.75% / 順位 2.131→2.869」の大幅退行を、書籍 との乖離を解消することで回復する。[docs/BookDigest_手作り.md](BookDigest_手作り.md) に書籍原典の整理を別ファイルとして切り出し、plan / 実装判断の一次情報源とした。

**v0.6.0 との差分** (共有評価器 `HandShapeEvaluator` / `TileWeights` への書籍準拠化):

| 変更点 | 書籍 | v0.6.0 旧実装 | v0.6.1 実装 |
|---|---|---|---|
| **A. 副露後打牌評価の深さ** (`EvaluateFulouPostDahaiTerminal`) | 再帰 (全深さ) | テンパイ 1 段のみ (`shanten > 0` は 0 返却) | **1 シャンテンまで拡張**。副露後 1 シャンテン手について、打牌 → テンパイ hand13 → 待ち牌 × `CalcHandScore` の期待値を 2 段限定で評価 |
| **B1. TileWeights 三元牌閾値** | 三元 ≥ 3 枚 | 三元 ≥ 6 枚 | **≥ 3 枚** に修正 |
| **B1. TileWeights 風牌閾値** | 風 ≥ 2 枚 | 風 ≥ 9 枚 | **≥ 2 枚** に修正 |
| **B2. TileWeights 乗数** | 染め ×4・字 ×1・風 ×4 | 染め ×2・字 ×4・風 ×8 | **染め ×4・字 ×1・風 ×4** に修正 (染めが字牌を乗算しなくなる) |
| **B3. TileWeights 適用範囲** | `eval_shoupai` 本体 (低シャンテン再帰含む) | `SelectDahaiByUsefulTileScore` フォールバック専用 | **`EvaluateHand13` / `EvaluateBacktrack` / `EvaluateFulouPostDahaiTerminal` の低シャンテン再帰でも乗算**。書籍 `useful(K) × paijia(K)` 相当 |

**Codex レビューで修正した実装上の細部**:

- **副露後 1 シャンテン評価 (A) で 15 枚手を採点しないよう修正**: 単純に `hand14 = handAfterDahai + K`, `hand15 = hand14 + winKind` で `CalcHandScore` すると副露込み 15 枚相当になり役なしエラー (0 返却) になる。正しくは `hand14` (テンパイ) から最良打牌で `hand13` を作り、その `hand13 + winKind` を採点
- **低シャンテン再帰で TileWeights を毎 hand13 再構築**: 親 hand14 の染め条件が子 hand13 で外れるケースを正しく評価するため、`EvaluateHand13` / `EvaluateBacktrack` / `EvaluateFulouPostDahaiTerminal` 冒頭で `TileWeights.Build(hand13, ctx.Calls)` を呼び直す
- **シャンテン戻し枝刈りを weights 乗算後の値で判定**: v0.6.1 では返り値が `ev * weight * unseen` なので、枝刈りも `ev * weight > minEvPerTile` で判定する。そうしないと weight > 1 の牌で本来採用すべきシャンテン戻し候補が刈り取られる

**重要な設計上の注記 (Codex 指摘の別側面)**:

変更は共有の `HandShapeEvaluator` / `TileWeights` に入っているため、`AI_v0_6_0_手作り` も同じ評価器を参照し**暗黙的に書籍準拠化される**。したがって以下の統計の「v0.6.0 (書籍準拠評価器)」列は純粋な旧 v0.6.0 (書籍乖離実装) ではなく「書籍準拠評価器を共有した v0.6.0 コード」であることに注意。v0.6.0 クラスは参考実装として残してあるが、評価器の差し替えで挙動自体が v0.6.1 に寄っている。

**クラス構成**:

- [AI_v0_6_1_手作り.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_v0_6_1_手作り.cs)
  - `AI_v0_6_1_手作り(PlayerId, PlayerIndex, Random) : Player`
  - `AI_v0_6_1_手作りFactory(int seed) : AiPlayerFactoryBase<AI_v0_6_1_手作り>`
- ロジック本体は v0.6.0 と同一 (共通評価器の書籍準拠化で効果が出る構造)

**統計結果** (AutoPlay 1000 局 × 2 シード、v0.6.0 × 2 席 + v0.6.1 × 2 席の混在対局):

| Seed | AI | 平均順位 | 和了率 | 放銃率 | 立直率 | 副露率 | 平均打点 |
|---|---|---|---|---|---|---|---|
| 1 | v0.6.0 (書籍準拠評価器) | 2.511 | 5.1% | 1.4% | 12.5% | 10.1% | 9549 |
| 1 | v0.6.1 | 2.489 | 5.3% | 1.3% | 12.7% | 10.3% | 9891 |
| 2 | v0.6.0 (書籍準拠評価器) | 2.488 | 5.1% | 1.4% | 12.9% | 10.1% | 9941 |
| 2 | v0.6.1 | 2.512 | 5.1% | 1.3% | 12.4% | 10.4% | 9547 |
| 平均 | v0.6.0 (書籍準拠評価器) | 2.500 | 5.10% | 1.40% | 12.7% | 10.1% | 9745 |
| 平均 | v0.6.1 | 2.501 | 5.20% | 1.30% | 12.55% | 10.35% | 9719 |

参考として旧 v0.6.0 (書籍乖離実装) と v0.5.0 の統計:

| AI | 平均順位 | 和了率 | 副露率 | 平均打点 |
|---|---|---|---|---|
| 旧 v0.6.0 (書籍乖離) | 2.869 | 8.75% | 9.7% | 8770 |
| v0.5.0 鳴き | 2.131 | 25.85% | 25.4% | 5211 |

**考察**:

- **書籍準拠化の効果**: 旧 v0.6.0 比で順位が **-0.37** (2.869 → 2.500) と大きく改善。平均打点も 9745 と旧 v0.6.0 の 8770 から上積みを維持。放銃率は 1.4% (旧 v0.6.0 9.65% から大幅改善) と、対戦相手となった旧 v0.5.0 鳴き想定と比べても極めて低い値まで落ちた
- **v0.6.0 vs v0.6.1 の差はほぼ誤差**: 2 シード平均で順位 0.001 / 和了率 0.1% / 副露率 0.25% / 打点 -26 しか差がない。共有評価器の変更効果が両方に波及するため、Player クラスの薄いラッパー差は順位統計に現れない
- **plan 合否判定は NG**: 目標 (和了率 ≥ 15% / 副露率 ≥ 15%) には届かず。和了率 5.2% は v0.5.0 の 25.85% / 書籍 0305 の 21.3% から大きく乖離
- **低和了率の要因考察**:
  - **副露率 10.35% (書籍 32.4%)** — 書籍の 1/3 程度にとどまる。立直前提の打点期待値 (一発・裏ドラなしでも 8000〜12000) が、副露後の非メンゼン鳴き手打点 (1000〜5000) を常に上回り、`SelectFulouByEvaluation` で「副露しない」が優先される構造。**Codex の P1 指摘 (副露後 1 シャンテン 15 枚採点バグ) 修正後も副露率は大きくは上がらなかった**ため、副露率の根本改善にはさらに別方向のアプローチ (副露後の立直断念ペナルティを持たせない打点補正、または立直付与を有効牌加算前提で期待値補正) が必要
  - **門前高打点への偏重** — 和了 972 回のうち立直 94.5%, 門前清自摸和 72.8%, 裏ドラ 40.8% と高打点役に極端に寄っており、手牌発展の待ち牌枚数を捨てて面子変換 (シャンテン戻し) を連発している挙動が示唆される
  - **シャンテン戻し閾値 `baselineMax * 2`** — plan Phase 3 C の対象。今回は未実施。`factor ∈ {1.75, 1.5}` で緩めれば和了率向上の可能性はあるが、v0.5.0 の 25.85% まで届くとは考えづらい
  - **TileWeights を低シャンテン再帰に乗算した副作用** — 染め/三元/風牌の評価値が相対的に大きくなり、通常打牌の和了価値が相対的に下がる。書籍 `(ev - max) * useful < min_ev * 2` の差分ベース閾値を現行実装は絶対値 `baselineMax * 2` で代替しており、weight 乗算で評価値の絶対スケールが ×4〜×8 倍膨らむと閾値の意味が変わる
- **放銃率の大幅低下** (9.65% → 1.4%) は副作用としてプラス。守備の v0.4.0 ロジックは維持しているが、攻撃局面で危険牌を切る頻度が激減したため (和了形成前に河に並べる牌の選択が安全牌寄りに変わった可能性)
- **結論**: v0.6.1 は v0.6.0 比の「和了率低下を書籍準拠で補正する」という当初目標は達成できず (むしろ和了率はさらに低下)、**共有評価器の書籍準拠化自体は有効だが、書籍 AI の和了率 21.3% に到達するには v0.6.x 系の更なる改修 (副露判定の期待値比較方法、シャンテン戻し閾値、1 シャンテンで副露を積極採用する特殊条件) が必要**

**既知の限界** (v0.6.1 でも残存):

- 副露後評価の再帰深さは 2 シャンテン以深で打ち切り (仮想副露の `CallsSignature` 爆発回避)。書籍オリジナルは全深さ再帰
- 赤ドラ期待値の近似は v0.6.0 と同じ (評価値再帰の仮想牌は非赤扱い)
- 永続フリテン未考慮
- 一発・裏ドラ未考慮 (書籍と整合)
- シャンテン戻し閾値 `baselineMax * 2` は Phase 3 C チューニング未実施のまま。`factor ∈ {1.75, 1.5}` で緩める検証は今後の TODO
