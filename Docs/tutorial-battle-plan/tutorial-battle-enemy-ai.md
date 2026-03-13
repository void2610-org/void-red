# チュートリアルバトル実装：敵AI差し替え

> 関連ドキュメント: [プレイヤー操作制約](./tutorial-battle-player-restrictions.md)

## 概要

敵AIは `IEnemyAIController` で抽象化し、通常バトルでは `EnemyAIController`、`alv` 戦では `TutorialEnemyAIController` を使う。
チュートリアルでは、入札・デッキ選択・感情・カード配置・スキル・競合上乗せを固定値で進める。

## 現在の実装

### 差し替え位置

- `BattleLifetimeScope` が `alv` 戦を判定する
- `TutorialBattlePresenter` のコンストラクタで `EnemyAI = new TutorialEnemyAIController(Enemy)` に差し替える

### 現在のスクリプト値

`TutorialEnemyAIController` の固定値:

```csharp
EnemyBids = { (0, Fear, 3) }
EnemyDeckCardIndices = { 0, 1, 2 }
EnemyEmotionPerRound = { Fear, Joy, Sadness }
EnemyCardIndexPerRound = { 0, 1, 2 }
EnemySkillPerRound = { false, true, false }
EnemyCompetitionDoRaise = true
```

### 各メソッドの役割

- `SelectDeck(...)`
  - `EnemyDeckCardIndices` の順でデッキを組む
  - 範囲外インデックスは `Debug.LogError` を出して無視する
- `DecideBids(...)`
  - 指定カードへ指定感情・指定量で入札する
  - 範囲外インデックスは `Debug.LogError` を出して無視する
- `DecideEmotionState()`
  - ラウンドごとの固定感情を返す
- `PlaceCard(...)`
  - `enemyDeck.Cards` の固定インデックスを使う
  - 使用済みや範囲外は `Debug.LogError` を出して中断する
- `TryActivateSkill(...)`
  - ラウンドごとの固定値を返す
  - 呼び出し時に `_roundIndex` を進める
- `TryCompetitionRaise(...)`
  - `Fear` で上乗せする

## 注意点

- カード配置は `GetAvailableCards()` ベースではなく `enemyDeck.Cards` の固定位置を見る
- これにより、残り札の並び順に依存しない
- プレイヤー側も同様に、`TutorialBattlePresenter` がデッキ順を正規化している
- 現在 `TutorialEnemyAIController.cs` には `CS0162` warning が1件残っている
