# 設計

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
    GameInit: 対局開始<br>席順決めたり持ち点決めたり
    RoundInit: 局開始<br>局の作成 局順・本場・供託・持ち点などを引き継ぐ
    RoundEnd: 局終了
    state ChoiceGameEnd <<choice>>
    GameEnd: 対局終了

    [*] --> GameInit
    GameInit --> RoundInit
    RoundInit --> RoundEnd
    RoundEnd --> ChoiceGameEnd: 局終了
    ChoiceGameEnd --> GameEnd: 対局終了
    ChoiceGameEnd --> RoundInit: 対局終了でない
    GameEnd --> [*]
```

## 局の状態遷移

```mermaid
stateDiagram-v2
    Haipai: 配牌<br>局順・本場・供託・持ち点・配牌・ドラを全プレイヤーに通知
    Tsumo: ツモ<br>ツモ番のプレイヤーにはツモ牌を それ以外のプレイヤーにはツモしたことを通知
    Dahai: 打牌<br>手牌orツモ牌から河に牌を移し、全プレイヤーに打牌されたことを通知<br>和了>ポン/大明槓>チー>OK
    state ChoiceDahai <<choice>>
    Call: 副露<br>河から打牌を除去し副露したプレイヤーに副露を生成
    state ChoiceCall <<choice>>
    Kan: 槓(暗槓・加槓)<br>暗槓-手牌から牌を除去し副露を生成 加槓-ツモ牌を追加してポン副露を更新<br>全プレイヤーに槓されたことを通知<br>和了>OK
    KanTsumo: 槓ツモ<br>嶺上牌を引いて手牌に加える 全プレイヤーに槓ツモしたことを通知
    AfterKanTsumo: 槓ツモ後
    state ChoiceKanTsumo <<choice>>
    Win: 和了
    Ryuukyoku: 流局

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
    Call --> ChoiceCall: 無条件遷移
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
    Win --> [*]
    Ryuukyoku --> [*]
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
