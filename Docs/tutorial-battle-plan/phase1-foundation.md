# フェーズ1: 基盤拡張

## 目的

`TutorialBattlePresenter` を自然に差し込めるように、ゲーム進行ロジックの拡張点を先に整える。

## 対象ファイル

- `Assets/Scripts/Game/Logic/BattlePresenter.cs`
- `Assets/Scripts/Game/Logic/CardBattleHandler.cs`
- `Assets/Scripts/Game/Logic/AuctionProcessor.cs`
- `Assets/Scripts/Game/Logic/CompetitionPhaseRunner.cs` または同等の定義ファイル

## 実装タスク

### 1. BattlePresenter の責務整理

- チュートリアル専用の `EnemyId == "alv"` 判定を全て削除する
- `_battleUIPresenter`、`_player`、`_enemy`、`_enemyAI`、`_auctionCards` を `protected` に変更する
- `_competitionPhaseRunner`、`_auctionProcessor` を派生先で差し替えられる形にする
- コンストラクタの `_enemyAI` 初期化を通常版に固定する

### 2. BattlePresenter の拡張ポイント追加

- `HandleThemeAnnouncement()` を `protected virtual` に変更する
- `HandleBiddingPhase()` を `protected virtual` に変更する
- `HandleAuctionResult()` を新設し、既存のオークション結果処理をラップする
- `OnAfterCardsDisplayed()` を新設する
- `OnAfterResourceGaugesDisplayed()` を新設する
- `OnBeforeMemoryGrowthContinueAsync()` を新設する
- `GetAllowedDeckCardIndices()` を新設する
- `GetForcedCoinFlip(int round)` を新設する
- `GetForcedBattleCard(int round)` を新設する
- `GetSkillRound()` を新設する

### 3. BattlePresenter 内の呼び出し差し替え

- オークション結果処理を直接呼ばず `HandleAuctionResult()` 経由に変更する
- デッキ選択初期化で `GetAllowedDeckCardIndices()` の戻り値を渡す
- カードバトル中の先攻決定で `GetForcedCoinFlip(int round)` を参照する
- スキルボタン表示で `GetSkillRound()` を参照する
- プレイヤーカード選択で `GetForcedBattleCard(int round)` を参照する
- 報酬表示後と記憶成長前に新設フックを呼ぶ

### 4. 周辺ロジックの継承・制御対応

- `CardBattleHandler.SetFirstPlayer(bool)` を追加する
- `CompetitionPhaseRunner` を継承可能に変更する
- `CompetitionPhaseRunner.RunAsync()` を `virtual` 化する
- `AuctionProcessor` からチュートリアル用分岐と演出呼び出しを除去する

## 完了条件

- `BattlePresenter` 単体で通常バトルが成立する設計になる
- 派生クラスからチュートリアル制御を差し込むためのメソッドとフィールドが揃う
- `AuctionProcessor` と `BattlePresenter` の両方にチュートリアル判定が残っていない

## 注意点

- このフェーズでは `TutorialBattlePresenter` をまだ実装しない
- 基底クラスの API 追加は最小限に止め、不要な汎用化をしない
- 既存フローの順序を崩さない
