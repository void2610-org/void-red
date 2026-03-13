# チュートリアルバトル実装：プレイヤー操作制約

> 関連ドキュメント: [敵AI差し替え](./tutorial-battle-enemy-ai.md)

## 概要

チュートリアルバトル（敵ID "alv"）で、プレイヤーが行える操作をフェーズ毎に制限する。
View 層の各メソッドにオプショナルパラメータを追加し、`BattlePresenter` から制約を渡す方式。
デフォルト値（null）のため既存の呼び出し箇所への影響なし。

---

## 制約の全体像

| フェーズ | 操作 | 制約内容 |
|---------|------|--------|
| 対話 | カードクリック | 指定カード1枚のみクリック可 |
| 入札：カード選択 | カードクリック→入札ウィンドウ | 指定カード1枚のみクリック可 |
| 入札：感情選択 | 車輪UIで感情切り替え | 切り替え禁止（初期感情固定） |
| 入札：ベット増減 | +ボタン | 必要量に達したら押せなくなる |
| 入札：確定 | 確定ボタン | ちょうど必要量のときのみ有効 |
| デッキ選択 | カードD&D | 指定カードのみドラッグ可 |
| カードバトル：カード伏せ | カードD&D | ラウンド毎に指定カードのみドラッグ可 |
| コインフリップ | （なし） | ラウンド毎に結果を固定 |

---

## 変更ファイル（6ファイル）

### 1. `Assets/Scripts/Game/Logic/BattlePresenter.cs`

#### プレイヤー側チュートリアルスクリプト定数を追加
```csharp
private bool _isTutorialBattle;
private int _battleRoundIndex;

private static class AlvTutorialPlayerScript
{
    // 対話フェーズ: このインデックスのカードのみ選択可（auctionCards内）
    public const int DialogueForcedCardIndex = 0;

    // 入札フェーズ
    public const int BidForcedCardIndex = 0;
    public const EmotionType BidForcedEmotion = EmotionType.Joy;
    public const int BidRequiredAmount = 3;

    // デッキ選択: 配置可能なカードのインデックス（WonCards内）
    public static readonly int[] DeckAllowedCardIndices = { 0, 1, 2 };

    // カードバトル: コインフリップ結果（true = プレイヤー先攻）
    public static readonly bool[] CoinFlipPerRound = { true, false, true };

    // カードバトル: ラウンド毎の強制カードインデックス（null = 自由選択）
    public static readonly int?[] PlayerCardPerRound = { 0, null, null };
}
```

#### `StartGame()` 冒頭でフラグセット
```csharp
_isTutorialBattle = _currentEnemyData.EnemyId == "alv";
```

#### `HandleDialoguePhase()` 変更
```csharp
_battleUIPresenter.StartDialogueSelection(
    _isTutorialBattle ? AlvTutorialPlayerScript.DialogueForcedCardIndex : (int?)null);
```

#### `HandleBiddingPhase()` 変更
```csharp
// 敵AI側は IEnemyAIController 経由で自動的にスクリプト動作
_enemyAI.DecideBids(_auctionCards);

// プレイヤー側制約を付与
await _battleUIPresenter.WaitForBiddingAsync(
    _auctionCards, _player.Bids, EmotionType.Joy, _player.EmotionResources,
    _isTutorialBattle ? AlvTutorialPlayerScript.BidForcedCardIndex : (int?)null,
    _isTutorialBattle ? AlvTutorialPlayerScript.BidForcedEmotion : (EmotionType?)null,
    _isTutorialBattle ? AlvTutorialPlayerScript.BidRequiredAmount : (int?)null);
```

#### `HandleDeckSelection()` 変更
```csharp
_battleUIPresenter.InitializeDeckSelection(
    _player.WonCards,
    _isTutorialBattle ? AlvTutorialPlayerScript.DeckAllowedCardIndices : null);
```

#### `HandleCardBattle()` のラウンドループ変更
```csharp
_battleRoundIndex = 0;

// ループ内...
// コインフリップ
if (_isTutorialBattle)
    handler.SetFirstPlayer(AlvTutorialPlayerScript.CoinFlipPerRound[_battleRoundIndex]);
else
    handler.DecideFirstPlayer();

// プレイヤーカード配置の制約
var forcedCard = _isTutorialBattle
    && _battleRoundIndex < AlvTutorialPlayerScript.PlayerCardPerRound.Length
    ? AlvTutorialPlayerScript.PlayerCardPerRound[_battleRoundIndex]
    : null;
await PlayerPlaceCard(handler, playerDeck, playerSkillSession, forcedCard);

_battleRoundIndex++;
```

#### `PlayerPlaceCard()` 変更
```csharp
private async UniTask<bool> PlayerPlaceCard(
    CardBattleHandler handler, BattleDeckModel playerDeck,
    PlayerBattleSkillSession playerSkillSession, int? forcedCardIndex = null)
{
    _battleUIPresenter.SetBattleInstruction("伏せるカードを選んでください");
    _battleUIPresenter.ShowPlayerHand(playerDeck.GetAvailableCards(), forcedCardIndex);
    // ...既存コード
}
```

---

### 2. `Assets/Scripts/Game/Logic/CardBattleHandler.cs`

コインフリップのオーバーライドメソッドを追加：

```csharp
// 既存
public void DecideFirstPlayer() => IsPlayerFirst = Random.value > 0.5f;
// 追加
public void SetFirstPlayer(bool isPlayerFirst) => IsPlayerFirst = isPlayerFirst;
```

---

### 3. `Assets/Scripts/UI/Auction/AuctionView.cs`

#### フィールド追加
```csharp
private int? _requiredBetAmount;
```

#### `StartDialogueSelection(int? forcedCardIndex = null)` に変更
指定インデックス以外のカードに `OnCardClicked`/`OnDialogueClicked` を購読しない：

```csharp
public void StartDialogueSelection(int? forcedCardIndex = null)
{
    foreach (var (card, index) in _auctionCardViews.Select((c, i) => (c, i)))
    {
        if (forcedCardIndex.HasValue && index != forcedCardIndex.Value) continue;
        card.OnCardClicked.Subscribe(OnDialogueCardClicked).AddTo(_dialogueDisposables);
        card.OnDialogueClicked.Subscribe(OnDialogueCardClicked).AddTo(_dialogueDisposables);
    }
}
```

#### `StartBidding(...)` にパラメータ追加
```csharp
public void StartBidding(
    IReadOnlyList<CardModel> auctionCards,
    BidModel playerBids,
    EmotionType initialEmotion,
    IReadOnlyDictionary<EmotionType, int> emotionResources,
    int? forcedCardIndex = null,
    EmotionType? forcedEmotion = null,
    int? requiredBetAmount = null)
```

- `forcedCardIndex` 指定時：そのカード以外の `OnCardClicked` を購読しない
- `forcedEmotion` 指定時：`emotionResourceDisplayView.OnEmotionSelected` を購読しない（感情固定）
- `requiredBetAmount` 指定時：`_requiredBetAmount` にセット、確定ボタンを初期無効化

#### `UpdateBettingButtonStates()` 追加
```csharp
private void UpdateBettingButtonStates()
{
    if (!_requiredBetAmount.HasValue) return;
    var total = _playerBids.GetTotalBidAmount();
    // ちょうど必要量のときのみ確定可能
    confirmBiddingButton.interactable = total == _requiredBetAmount.Value;
    // 必要量に達したら増加不可（超過防止）
    bidWindowView.SetIncreaseInteractable(total < _requiredBetAmount.Value);
}
```

`OnIncreaseBid()` / `OnDecreaseBid()` の末尾で `UpdateBettingButtonStates()` を呼ぶ。

---

### 4. `Assets/Scripts/UI/Auction/BidWindowView.cs`

増加ボタンの interactable を外部から制御するメソッドを追加：

```csharp
public void SetIncreaseInteractable(bool value) => increaseButton.interactable = value;
```

---

### 5. `Assets/Scripts/UI/Battle/DeckSelectionView.cs`

`Initialize` にオプショナルパラメータを追加。
`allowedCardIndices` 指定時、対象外カードの `CanvasGroup.blocksRaycasts = false`（イベント購読しない）：

```csharp
public void Initialize(IReadOnlyList<CardModel> wonCards, int[] allowedCardIndices = null)
{
    for (var i = 0; i < wonCards.Count; i++)
    {
        var draggableCard = Instantiate(draggableCardPrefab, handContainer);
        draggableCard.Initialize(wonCards[i], i);

        if (allowedCardIndices != null && !allowedCardIndices.Contains(i))
        {
            draggableCard.CanvasGroup.blocksRaycasts = false;
            continue;
        }
        // 既存のイベント購読
        draggableCard.OnDragStarted.Subscribe(OnCardDragStarted).AddTo(_disposables);
        // ...
    }
}
```

---

### 6. `Assets/Scripts/UI/Battle/CardBattleView.cs`

`ShowPlayerHand` にオプショナルパラメータを追加。
`forcedCardIndex` 指定時、対象以外のカードの `CanvasGroup.blocksRaycasts = false`（イベント購読しない）：

```csharp
public void ShowPlayerHand(IReadOnlyList<CardModel> availableCards, int? forcedCardIndex = null)
{
    ClearPlayerHand();
    handContainer.gameObject.SetActive(true);

    for (var i = 0; i < availableCards.Count; i++)
    {
        var draggableCard = Instantiate(draggableCardPrefab, handContainer);
        draggableCard.Initialize(availableCards[i], i);

        if (forcedCardIndex.HasValue && i != forcedCardIndex.Value)
        {
            // 指定以外のカードはドラッグ不可（見た目はそのまま）
            draggableCard.CanvasGroup.blocksRaycasts = false;
        }
        else
        {
            draggableCard.OnDragStarted.Subscribe(OnCardDragStarted).AddTo(_disposables);
            draggableCard.OnDragEnded.Subscribe(OnCardDragEnded).AddTo(_disposables);
            draggableCard.OnDragging.Subscribe(OnCardDragging).AddTo(_disposables);
        }
        _handCards.Add(draggableCard);
    }
    // ...既存コード
}
```

---

## BattleUIPresenter への影響

各メソッドに新パラメータを追加し、下位 View へ透過する（デフォルト null）：

```csharp
// 対話フェーズ
public void StartDialogueSelection(int? forcedCardIndex = null) =>
    _auctionView.StartDialogueSelection(forcedCardIndex);

// カードバトル
public void ShowPlayerHand(IReadOnlyList<CardModel> availableCards, int? forcedCardIndex = null) =>
    _cardBattleView.ShowPlayerHand(availableCards, forcedCardIndex);

// 入札
public async UniTask WaitForBiddingAsync(
    IReadOnlyList<CardModel> auctionCards,
    BidModel playerBids,
    EmotionType initialEmotion,
    IReadOnlyDictionary<EmotionType, int> emotionResources,
    int? forcedCardIndex = null,
    EmotionType? forcedEmotion = null,
    int? requiredBetAmount = null)
// → _auctionView.StartBidding(...) に全パラメータを透過

// デッキ選択
public void InitializeDeckSelection(IReadOnlyList<CardModel> wonCards, int[] allowedIndices = null) =>
    _deckSelectionView.Initialize(wonCards, allowedIndices);
```

---

## 実装時確認事項

- `AlvTutorialPlayerScript` の具体的な数値はゲームデザインに合わせて調整
- `StartDialogueSelection` の既存実装での `_auctionCardViews` のアクセス方法を確認
- `DeckSelectionView.Initialize` の既存シグネチャを確認（メソッド名が異なる場合は合わせる）

---

## 検証手順

1. `mcp__uLoopMCP__compile` (ForceRecompile=false) でコンパイル
2. `mcp__uLoopMCP__get-logs` (LogType=Error) でエラーなし確認
3. フォーマット修正コマンドを実行
4. "alv" 敵バトルを繰り返し実行し、以下を確認：
   - 対話フェーズ：指定カード以外クリック不可
   - 入札フェーズ：指定カードのみ選択可、感情変更不可、ちょうど指定量で確定可
   - デッキ選択：指定カードのみドラッグ可
   - カードバトル：コインフリップ固定、指定カードのみドラッグ可
   - 毎回同一の展開になること
