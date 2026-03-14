# フェーズ3: チュートリアルデータと DI 切り替え

## 目的

チュートリアル専用の定数群と Presenter 切り替えを導入し、通常バトルとの責務分離を確定させる。

## 対象ファイル

- `Assets/Scripts/Game/Tutorial/TutorialBattlePlayerData.cs`
- `Assets/Scripts/VContainer/BattleLifetimeScope.cs`

## 実装タスク

### 1. TutorialBattlePlayerData 作成

- 入札フェーズの強制カード、感情、必要ベット量を定義する
- 競合フェーズの必要レイズ回数と強制感情を定義する
- デッキ選択で許可するカードインデックスを定義する
- ラウンドごとのコインフリップ固定値を定義する
- ラウンドごとの強制カード指定を定義する
- スキルを使わせるラウンドと感情を定義する

### 2. BattleLifetimeScope の登録切り替え

- 現在ノードから対象バトル情報を取得する
- 対象敵が `alv` かどうかをここで判定する
- `alv` 戦なら `TutorialBattlePresenter` を `BattlePresenter` として登録する
- それ以外は通常の `BattlePresenter` を登録する

### 3. 判定責務の集約確認

- チュートリアル判定が `BattleLifetimeScope` のみになっていることを確認する
- それ以外のクラスに `EnemyId == "alv"` が残っていないことを確認する

## 完了条件

- プレイヤー制約値が 1 か所にまとまる
- `alv` 戦のみ `TutorialBattlePresenter` が起動する
- 通常戦で `TutorialBattlePresenter` が登録されない

## 注意点

- 今回は `alv` 判定を設定駆動に一般化しない
- 定数値は仮置きでもよいが、意味が読み取れる名前にする
