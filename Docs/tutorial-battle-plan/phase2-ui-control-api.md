# フェーズ2: UI 制御 API 追加

## 目的

チュートリアル制約を `TutorialBattlePresenter` から制御できるように、UI 層へ最小限の操作 API を追加する。

## 対象ファイル

- `Assets/Scripts/UI/Auction/AuctionView.cs`
- `Assets/Scripts/UI/Auction/AuctionCardView.cs`
- `Assets/Scripts/UI/Auction/BidWindowView.cs`
- `Assets/Scripts/UI/Main/BattleUIPresenter.cs`
- `Assets/Scripts/UI/Battle/CardBattleView.cs`
- `Assets/Scripts/UI/Battle/DeckSelectionView.cs`

## 実装タスク

### 1. AuctionView の公開 API 追加

- 感情選択イベントを公開する
- カードクリック時にインデックスで受け取れる Observable を追加する
- 全カードの操作可否を一括変更するメソッドを追加する
- 指定カード 1 枚の操作可否を変更するメソッドを追加する
- 確定ボタンの操作可否を変更するメソッドを追加する
- 入札増加イベントを公開する
- 入札増加ボタンの操作可否を変更するメソッドを追加する

### 2. AuctionCardView と BidWindowView の補助 API 追加

- `AuctionCardView.SetInteractable(bool)` を追加する
- 実装は `CanvasGroup.blocksRaycasts` を基本にする
- `BidWindowView.SetIncreaseInteractable(bool)` を追加する

### 3. BattleUIPresenter の委譲メソッド追加

- `StartAuctionBidding(...)` を追加する
- `SetAuctionCardInteractable(...)` を追加する
- `SetAuctionAllCardsInteractable(...)` を追加する
- `SetAuctionConfirmInteractable(...)` を追加する
- `SetAuctionBidIncreaseInteractable(...)` を追加する
- `OnAuctionEmotionSelected` を追加する
- `OnAuctionCardClicked` を追加する
- `OnAuctionBidIncreased` を追加する
- `OnAuctionBiddingConfirmed` を追加する

### 4. カード配置制限 API 追加

- `CardBattleView.ShowPlayerHand(...)` に強制カード指定のオプションを追加する
- 強制カード指定時は対象以外をドラッグ不可にする
- `DeckSelectionView.Initialize(...)` に許可カード配列のオプションを追加する
- 許可対象外のカードをドラッグ不可にする

## 完了条件

- `TutorialBattlePresenter` から入札 UI とカード選択 UI の操作制御が可能になる
- UI 内にチュートリアル専用ステートマシンが存在しない
- 通常フローでは追加パラメータ未指定時に従来通り動作する

## 注意点

- null チェック追加で守るのではなく、既存のセットアップ前提を維持する
- コメントを追加する場合は日本語にする
- 状態保持が必要なら既存フィールドを利用し、新しい状態列挙は増やさない
