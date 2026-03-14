# チュートリアルバトル実装：プレイヤー操作制約

> 関連ドキュメント: [敵AI差し替え](./tutorial-battle-enemy-ai.md)

## 概要

チュートリアルバトル（敵ID `alv`）では、`TutorialBattlePresenter` が通常の `BattlePresenter` を継承して進行を固定する。
通常バトル側に `EnemyId` 分岐は持ち込まず、必要な差分だけを `protected virtual` フックで差し替える。

## 現在の制約

| Step | フェーズ | 操作 | 現在の制約 |
|------|---------|------|-----------|
| 1 | 入札開始 | 対話 | 最初は指定カードの対話ボタンだけ有効 |
| 2 | 入札：感情選択 | 感情ホイール | `Joy` が選ばれるまで次へ進まない |
| 3 | 入札：カード選択 | カードクリック | 指定カード1枚のみ選択可 |
| 4 | 入札：ベット | `+` ボタン | 合計3ベットに達した時点で次へ進める |
| 5 | 入札：確定 | 確定ボタン | 合計3ベット到達後のみ有効 |
| 6 | オークション競合 | 感情選択・レイズ | `Trust` 固定、2回レイズ |
| 7 | デッキ選択 | カードD&D | 指定カードのみ選択可 |
| 8 | デッキ選択中スキル | スキルボタン | `Joy` 表示のまま使用不能 |
| 9 | カードバトル | カード選択 | ラウンドごとに指定カードだけ選択可 |
| 10 | バトル中スキル | スキルボタン | 2ラウンド目のみ `Joy` を使用可能 |
| 11 | バトル競合 | 感情選択・レイズ | `Trust` 固定、1回レイズ |
| - | コインフリップ | 演出 | ラウンドごとに結果固定 |

## 実装上の要点

### 入札フェーズ

`TutorialBattlePresenter.RunTutorialBiddingAsync()` が入札導線を固定している。

1. `StartAuctionBidding(...)` で入札UIを開く
2. 全カード・全対話・感情・ベット・確定を無効化する
3. 指定カードの対話ボタンだけ有効化する
4. 対話完了後に `Joy` の感情選択だけを待つ
5. 指定カードのカード本体だけ有効化する
6. `OnAuctionBidIncreased` で合計ベットが `3` に達した時点で確定へ進める

### デッキ選択

- `InitializeDeckSelection(..., DeckAllowedCardIndices)` で選択可能カードを制限する
- スキルボタンは通常UIとして表示したままにする
- `TutorialBattlePresenter.CanUseDeckSelectionSkill(...) => false` により、デッキ選択中は押せない
- `RequiresDeckSelectionSkillActivation(...) => false` なので、スキルを使わなくても先へ進める

### カードバトル

- `CoinFlipPerRound = { true, false, true }`
- `ForcedCardPerRound = { 0, null, null }`
- `SkillRoundIndex = 1`
- `BattleForcedSkillEmotion = Joy`

`TutorialBattlePresenter` では以下を差し替えている。

- `DecideFirstPlayer(...)`
- `CanUseBattleSkill(...)`
- `RequiresBattleSkillActivation(...)`
- `SelectBattleCardAsync(...)`
- `GetBattleVictoryCondition(...)`
- `GetEnemyBattleEmotionState(...)`

### 競合

競合ロジック自体は `TutorialCompetitionPhaseRunner` を使うが、種別判定は共通基盤には持ち込まない。

- オークション競合用 runner
- バトル競合用 runner

を `TutorialBattlePresenter` から別インスタンスで注入している。

## 現在の固定データ

`TutorialBattlePlayerData` の内容:

```csharp
BidForcedCardIndex = 0
BidForcedEmotion = Joy
BidRequiredAmount = 3

AuctionCompetitionRequiredRaises = 2
AuctionCompetitionForcedEmotion = Trust

BattleCompetitionRequiredRaises = 1
BattleCompetitionForcedEmotion = Trust

DeckAllowedCardIndices = { 0, 1, 2 }

BattleVictoryCondition = LowerWins
CoinFlipPerRound = { true, false, true }
ForcedCardPerRound = { 0, null, null }
SkillRoundIndex = 1
BattleForcedSkillEmotion = Joy
```

## 現在の注意点

- スキルボタンは「非表示にしない」前提で扱う
- デッキ選択中のスキルは表示のみで、使用させない
- バトル中のスキルは2ラウンド目だけ有効化する
- 競合の違いは `TutorialBattlePresenter` 側の注入で分け、共通基盤には概念を増やさない
