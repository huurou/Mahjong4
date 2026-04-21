# AI プレイヤー設計

`Mahjong.Lib.Game` の AI プレイヤー実装は、`Mahjong.Lib.Game/Players/Impl/` 配下にバージョン系列で蓄積する。設計思想・アルゴリズム詳細・統計比較はフェーズ計画 ([Roadmap.md](Roadmap.md)) から独立した継続タスクとして本文書で管理する。

## 全体方針

- **命名規則**: `AI_v{major}_{minor}_{patch}_{識別名}` (例: `AI_v0_1_0_ランダム` / `AI_v0_2_0_有効牌`)。識別名は日本語可 (クラス名・ファイル名・表示名すべて同じ日本語を使う)
- **実装場所**: [src/Mahjong.Lib.Game/Players/Impl/](../src/Mahjong.Lib.Game/Players/Impl/)。1 バージョン = 1 ファイル、Factory クラスは同ファイル末尾に定義
- **Factory ペア**: 各 AI は `{AI名}Factory : IPlayerFactory(int seed)` を必ず提供する。`Create(PlayerIndex, PlayerId)` で席ごとに異なる `Random` を `new Random(HashCode.Combine(seed, index.Value))` で注入し、同一 seed に対する再現性を保証する
- **評価基盤**: [tools/Mahjong.Lib.Game.AutoPlay/](../tools/Mahjong.Lib.Game.AutoPlay/) で一括対局 + `StatsTracer` により順位分布・和了率・放銃率・立直率・副露率・平均打点・役出現率等を取得する
- **混在対局**: [MixedPlayerFactory](../tools/Mahjong.Lib.Game.AutoPlay/MixedPlayerFactory.cs) に 4 つの `IPlayerFactory` を渡すと、対局ごとに席配置をシャッフルしながら異なる AI を同卓させる。新バージョンをベースライン (ひとつ前の版や `AI_v0_1_0_ランダム`) と対戦させて差分を見るのが標準の評価手順
- **判断に使えるヘルパー** (すべて `Mahjong.Lib.Game` 内から直接参照可能):
  - [ShantenHelper](../src/Mahjong.Lib.Game/Tenpai/ShantenHelper.cs) — `CalcShanten(Hand)` / `EnumerateUsefulTileKinds(Hand)` (有効牌 = 引くとシャンテン数が減る牌種)
  - [TenpaiHelper](../src/Mahjong.Lib.Game/Tenpai/TenpaiHelper.cs) — `IsTenpai` / `EnumerateWaitTileKinds` / `IsKoutsuOnlyInAllInterpretations` (立直中暗槓の送り槓判定)
  - [VisibleTileCounter](../src/Mahjong.Lib.Game/Tenpai/VisibleTileCounter.cs) — `CountUnseen(PlayerRoundView, TileKind)` で自分視点の未見枚数を取得
  - [ScoringHelper](../src/Mahjong.Lib.Game/Games/ScoringHelper.cs) — 和了時点数計算 (AI 判断でも試算に使える)

## 今後のバージョンへ

新バージョンを追加するときは、本文書の末尾に以下のテンプレートで章を追加する。

```markdown
## vX.Y.Z AI_vX_Y_Z_識別名

**目的**: 一つ前のバージョンに対して何を改善するか (例: 役選択 / 守備判断 / 押し引き)。

**アルゴリズム**:
- 打牌選択・副露判断・立直判断・和了判断それぞれの挙動を箇条書きで明記
- 参照する情報 (自手牌 / 河 / 副露 / 他家リーチ / ドラ / 局状況 等) を列挙

**クラス構成**:
- [AI_vX_Y_Z_識別名.cs](../src/Mahjong.Lib.Game/Players/Impl/AI_vX_Y_Z_識別名.cs)

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
  - `AI_v0_1_0_ランダムFactory(int seed) : IPlayerFactory`

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
  - `AI_v0_2_0_有効牌Factory(int seed) : IPlayerFactory`

**使用ヘルパー**: `ShantenHelper.CalcShanten` / `ShantenHelper.EnumerateUsefulTileKinds` / `VisibleTileCounter.CountUnseen`

**既知の限界** (次バージョンへの課題):

- 役を考慮していない (役なし形でもテンパイを目指す) ため、リーチ以外の役がつかず門前テンパイでもリーチ後以外の打点が伸びない
- 副露しないので、対子・刻子手・染め手を手牌で作れない
- ドラ / 赤ドラの価値を評価しないため、有効牌枚数同点時の選択がドラ切り偏重になることがある
- 他家のリーチ / テンパイ気配に対する守備判断がない (放銃率が高い)

## v0.3.0 AI_v0_3_0_評価値

**目的**: v0.2.0 の「有効牌同点時はランダム選択」というタイブレーカーを、「対象牌を孤立牌と見立てたときの面子完成ポテンシャル (評価値) が低い牌を優先して切る」書籍『麻雀 AI』由来のロジックに置き換える。将来ターツに発展しやすい牌 (特にドラ・役牌) を手元に残す指向に改善し、v0.2.0 比で平均順位・立直率・平均打点に差を出すことを狙う。

**アルゴリズム**:

v0.2.0 の [SelectBestDahai 内シャンテン→有効牌] 選抜後、`finalists.Count >= 2` の場合にだけ評価値最小グループへ絞り、そこから `Random.Next` でランダム選択する。

**評価値の定義**:

```
評価値(tile) = Σ (useful(x, kind) × adjDoraMultiplier(x, kind)) × outerMultiplier(tile)
               x ∈ EnumerateAdjacents(kind)
               kind = tile.Kind
```

- **くっつき範囲** `EnumerateAdjacents(kind)`: 数牌は同スート内で `kind-2〜kind+2` (`TileKind.TryGetAtDistance` で判定)、字牌は自身のみ
- **使える枚数** `useful(x, kind)`: 書籍 5m 例で「3m:2, 4m:2, 5m:1, 6m:3, 7m:3」の `min` 構造に従う
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
  - `AI_v0_3_0_評価値Factory(int seed) : IPlayerFactory`

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
