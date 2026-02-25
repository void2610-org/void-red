# リアルタイム競りシステム実装プラン

## 概要
新リアルタイムオークションシステムの実装
- 6枚の共有カード
- 感情リソース 8種類×各3枚 = 合計24枚
- 1カードあたり1種類の感情リソースのみベット可能
- リアルタイム競合システム（10秒タイマー）

---

## 実装済みフェーズ

### ✅ Phase 1: 基盤変更（データモデル・定数）
- GameConstants 変更（DEFAULT_EMOTION_VALUE=3, AUCTION_CARD_COUNT=6, COMPETITION_TIMEOUT_SECONDS=10f）
- GameState enum 変更（CardDistribution/ValueRanking削除、CompetitionPhase追加）
- BidModel に1カード1感情制約を追加
- AuctionData を6枚構成に変更（playerCards+enemyCards → auctionCards）

### ✅ Phase 2: 価値順位システムの削除
- ValueRankingModel.cs 削除
- PlayerPresenter/Enemy から参照削除
- UI削除: ValueRankingView.cs, DraggableCardView.cs, RankingSlotView.cs
- Prefab削除: ValueRankingView.prefab, ValueRanking/ フォルダ
- RewardCalculator をスタブ実装に書き換え

### ✅ Phase 3: フェーズ順序変更
- BattlePresenter.StartGame() のフロー書き換え
- HandleCardReveal を6枚共有表示に変更
- HandleAuctionResult: 勝者のみリソース消費、敗者はリソース返却

### ✅ Phase 4: 旧対話システム削除 + 仮実装
- 旧対話システム完全削除（DialoguePhaseView等）
- AuctionCardView.cs 新規作成（CardView + CardBidInfoView + DialogueButton）

### ✅ Phase 5: Prefab統合 + AuctionCardView組み込み
- AuctionCardView.prefab 作成
- AuctionView を AuctionCardView ベースに書き換え
- PlayerCardContainer+EnemyCardContainer → 単一CardContainer統合

### ✅ Phase 6: 競合システム
- CompetitionHandler.cs 新規作成（R3 Subject通知、タイマー、上乗せ管理）
- CompetitionView.cs 新規作成（天秤+タイマー+感情選択+上乗せボタン）
- CompetitionView.prefab 作成
- BattlePresenter に競合処理組み込み
- 敵AI の競合時ロジック実装

---

## 実装完了

Phase 1-6の実装により、リアルタイム競りシステムのコア機能は完了しました。

### 今後の検討事項（別タスクとして実装予定）
- 対話フェーズの詳細実装（現在は仮実装）
- 報酬計算ロジックの再設計（現在はスタブ実装）
- 特殊効果カードの実装

---

## 備考
- ブランチ: impl-realtime-auction
- 最新コミット: 6716e258 Add BalanceView and update CompetitionView with documentation
- ゲームオーバー条件: 不要（実装しない）
- 詳細設計: Docs/new-realtime-auction/implementation-plan.md 参照
