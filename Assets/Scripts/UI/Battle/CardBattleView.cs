using System.Collections.Generic;
using Auction;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// カードバトル画面のView
/// プレイヤーはD&Dでカードを場に出し、敵カードが開示されて横並びに表示される
/// </summary>
public class CardBattleView : BasePhaseView
{
    [Header("勝利条件")]
    [SerializeField] private TextMeshProUGUI victoryConditionText;

    [Header("プレイヤー手札")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private DraggableCardView draggableCardPrefab;

    [Header("フィールド")]
    [SerializeField] private DeckSlotView playerFieldSlot;
    [SerializeField] private Transform enemyCardContainer;
    [SerializeField] private BattleCardSlotView cardSlotPrefab;

    [Header("ドラッグ演出")]
    [SerializeField] private DragLineView dragLineView;

    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button skillButton;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Button nextButton;

    [Header("アニメーション")]
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    /// <summary>プレイヤーがカードを場に出した</summary>
    public Observable<BattleCardModel> OnCardSelected => _onCardSelected;

    /// <summary>スキルボタンが押された</summary>
    public Observable<Unit> OnSkillActivated => _onSkillActivated;

    /// <summary>次へボタンが押された</summary>
    public Observable<Unit> OnNextClicked => _onNextClicked;

    private readonly List<DraggableCardView> _handCards = new();
    private readonly Subject<BattleCardModel> _onCardSelected = new();
    private readonly Subject<Unit> _onSkillActivated = new();
    private readonly Subject<Unit> _onNextClicked = new();
    private CompositeDisposable _disposables = new();
    private RectTransform _handContainerRect;
    private BattleCardSlotView _enemyFieldCardSlot;

    /// <summary>指示テキストを設定</summary>
    public void SetInstruction(string text) => instructionText.text = text;

    /// <summary>スキルボタンの表示状態を設定</summary>
    public void SetSkillButtonVisible(bool visible) => skillButton.gameObject.SetActive(visible);

    /// <summary>バトルを初期化する</summary>
    public void Initialize(VictoryCondition condition, EmotionType playerSkill)
    {
        Show();

        victoryConditionText.text = condition == VictoryCondition.LowerWins
            ? "勝利条件: 数字が小さい方が勝利"
            : "勝利条件: 数字が大きい方が勝利";

        skillNameText.text = $"{playerSkill.ToJapaneseName()}\n{SkillEffectApplier.GetDescription(playerSkill)}";

        nextButton.gameObject.SetActive(false);
    }

    /// <summary>プレイヤーの手札をD&D可能カードとして表示する</summary>
    public void ShowPlayerHand(IReadOnlyList<BattleCardModel> availableCards)
    {
        ClearPlayerHand();
        handContainer.gameObject.SetActive(true);

        for (var i = 0; i < availableCards.Count; i++)
        {
            var card = availableCards[i];
            var draggableCard = Instantiate(draggableCardPrefab, handContainer);
            draggableCard.Initialize(card, i);

            draggableCard.OnDragStarted.Subscribe(OnCardDragStarted).AddTo(_disposables);
            draggableCard.OnDragEnded.Subscribe(OnCardDragEnded).AddTo(_disposables);
            draggableCard.OnDragging.Subscribe(OnCardDragging).AddTo(_disposables);

            _handCards.Add(draggableCard);
        }

        // レイアウト計算＋スライドインアニメーション
        cardStagger.Play();

        // フィールドスロットのドロップイベントを購読
        playerFieldSlot.OnCardDropped
            .Subscribe(tuple => OnCardDroppedToField(tuple.card))
            .AddTo(_disposables);

        // 確定ボタンでカード選択を確定
        nextButton.OnClickAsObservable()
            .Subscribe(_ => OnConfirmCardSelection())
            .AddTo(_disposables);
    }

    /// <summary>プレイヤーのカードを場に配置（確定後に手札をクリア）</summary>
    public void PlacePlayerCard(BattleCardModel card)
    {
        handContainer.gameObject.SetActive(false);
        ClearPlayerHand();
    }

    /// <summary>敵のカードを場に伏せる</summary>
    public void PlaceEnemyCard()
    {
        if (_enemyFieldCardSlot)
            Destroy(_enemyFieldCardSlot.gameObject);

        _enemyFieldCardSlot = Instantiate(cardSlotPrefab, enemyCardContainer);
        _enemyFieldCardSlot.ShowBack();
        _enemyFieldCardSlot.SetInteractable(false);
    }

    /// <summary>両者のカードをオープンする（横並びで比較表示）</summary>
    public void RevealCards(BattleCardModel playerCard, BattleCardModel enemyCard)
    {
        // プレイヤーカードの数字を更新（スキル効果による変更を反映）
        var placedCard = playerFieldSlot.PlacedCard;
        if (placedCard)
            placedCard.UpdateNumber(playerCard.Number);

        // 敵カードを開示
        if (_enemyFieldCardSlot)
        {
            _enemyFieldCardSlot.Initialize(enemyCard, showNumber: true);
            _enemyFieldCardSlot.ShowFront();
        }
    }

    /// <summary>次へボタンを表示して待機</summary>
    public async UniTask WaitForNextAsync()
    {
        nextButton.gameObject.SetActive(true);
        await _onNextClicked.FirstAsync();
        nextButton.gameObject.SetActive(false);
    }

    /// <summary>場のカードをクリアする</summary>
    public void ClearField()
    {
        // プレイヤーのスロットをクリア
        var placedCard = playerFieldSlot.RemoveCard();
        if (placedCard)
            Destroy(placedCard.gameObject);

        // 敵カードをクリア
        if (_enemyFieldCardSlot)
        {
            Destroy(_enemyFieldCardSlot.gameObject);
            _enemyFieldCardSlot = null;
        }
    }

    // === D&D関連 ===

    private void OnCardDragStarted(DraggableCardView card)
    {
        var handCenterWorld = _handContainerRect.position;
        dragLineView.Show(handCenterWorld);
    }

    private void OnCardDragEnded(DraggableCardView card)
    {
        dragLineView.Hide();

        if (card.IsPlaced)
        {
            // 配置済みカードはスロットに戻す
            card.PlaySnapToSlotAsync(card.CurrentSlot.CardAnchor, Vector2.zero).Forget();
            return;
        }

        // スロット外にドロップされた場合、手札に戻す
        ReturnCardToHand(card);
    }

    private void OnCardDragging(Vector3 cardWorldPos) => dragLineView.UpdateEndPosition(cardWorldPos);

    private void OnCardDroppedToField(DraggableCardView droppedCard)
    {
        // 既にカードがある場合は入れ替え
        if (playerFieldSlot.IsOccupied)
        {
            var existingCard = playerFieldSlot.RemoveCard();
            ReturnCardToHand(existingCard);
        }

        playerFieldSlot.PlaceCard(droppedCard);
        droppedCard.PlaySnapToSlotAsync(playerFieldSlot.CardAnchor, Vector2.zero).Forget();

        // カードを枠に配置した時のSE
        SeManager.Instance.PlaySe("SE_FRAME_LIGHT", pitch: 1f);

        // 確定ボタンを表示
        nextButton.gameObject.SetActive(true);
    }

    /// <summary>カード選択を確定する</summary>
    private void OnConfirmCardSelection()
    {
        if (!playerFieldSlot.IsOccupied) return;

        nextButton.gameObject.SetActive(false);
        _onCardSelected.OnNext(playerFieldSlot.PlacedCard.BattleCard);
    }

    /// <summary>カードを手札コンテナに戻してレイアウトを再計算する</summary>
    private void ReturnCardToHand(DraggableCardView card)
    {
        var rt = card.transform as RectTransform;
        card.transform.SetParent(handContainer);
        card.transform.localScale = card.OriginalScale;
        card.transform.localEulerAngles = Vector3.zero;

        // アンカーをリセット（ドラッグ中にrootCanvasに移動しているため）
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = card.OriginalSizeDelta;

        // HandIndexの順番に並べ直す
        card.transform.SetSiblingIndex(GetSiblingIndexForHandIndex(card.HandIndex));

        // StaggeredSlideInGroupでレイアウト再計算
        cardStagger.ApplyLayout();
    }

    /// <summary>HandIndexに基づいて正しいSiblingIndexを計算する</summary>
    private int GetSiblingIndexForHandIndex(int handIndex)
    {
        for (var i = 0; i < handContainer.childCount; i++)
        {
            var child = handContainer.GetChild(i).GetComponent<DraggableCardView>();
            if (child && child.HandIndex > handIndex)
                return i;
        }

        return handContainer.childCount;
    }

    // === Lifecycle ===

    private void ClearPlayerHand()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        foreach (var card in _handCards)
            if (card && !card.IsPlaced)
                Destroy(card.gameObject);

        _handCards.Clear();
    }

    protected override void Awake()
    {
        base.Awake();
        _handContainerRect = handContainer as RectTransform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        dragLineView.Initialize(canvas);

        skillButton.OnClickAsObservable()
            .Subscribe(_ => _onSkillActivated.OnNext(Unit.Default))
            .AddTo(this);

        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onCardSelected.Dispose();
        _onSkillActivated.Dispose();
        _onNextClicked.Dispose();
    }
}
