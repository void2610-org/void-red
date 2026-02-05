using System.Collections.Generic;
using System.Linq;
using Auction;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 価値順位設定UI
/// プレイヤーがカードをドラッグ&ドロップして順位1-4を設定する
/// </summary>
public class ValueRankingView : MonoBehaviour
{
    [Header("スロット")]
    [SerializeField] private List<RankingSlotView> slots;

    [Header("手札エリア")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private DraggableCardView draggableCardPrefab;

    [Header("扇形配置")]
    [SerializeField] private float fanSpreadWidth = 400f;
    [SerializeField] private float fanHeightCurve = 30f;
    [SerializeField] private float fanMaxAngle = 10f;

    [Header("UI")]
    [SerializeField] private Button confirmButton;

    [Header("ドラッグ演出")]
    [SerializeField] private DragLineView dragLineView;

    [Header("アニメーション設定")]
    [SerializeField] private float slotStaggerDelay = 0.05f;
    [SerializeField] private float cardStaggerDelay = 0.03f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private float slotSlideOffset = 50f;
    [SerializeField] private float cardSlideOffset = -100f;

    public Observable<Unit> OnRankingComplete => _onRankingComplete;

    private readonly List<DraggableCardView> _handCards = new();
    private readonly List<CardModel> _rankedCards = new();
    private readonly Subject<Unit> _onRankingComplete = new();
    private readonly List<MotionHandle> _animHandles = new();
    private readonly Dictionary<RankingSlotView, Vector2> _slotOriginalPositions = new();
    private CompositeDisposable _disposables = new();
    private bool _wasDroppedToSlot;
    private RectTransform _handContainerRect;

    public void Show()
    {
        gameObject.SetActive(true);
        PlaySlotEnterAnimation();
    }

    public void Hide()
    {
        _animHandles.CancelAll();
        gameObject.SetActive(false);
    }

    public void StartRanking(IReadOnlyList<CardModel> cards)
    {
        Clear();

        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var draggableCard = Instantiate(draggableCardPrefab, handContainer);
            draggableCard.Initialize(card, i);

            // 初期位置・回転を設定
            SetCardToFanPosition(draggableCard, i, cards.Count);

            draggableCard.OnDragStarted
                .Subscribe(OnCardDragStarted)
                .AddTo(_disposables);

            draggableCard.OnDragEnded
                .Subscribe(OnCardDragEnded)
                .AddTo(_disposables);

            draggableCard.OnClicked
                .Subscribe(OnCardClicked)
                .AddTo(_disposables);

            draggableCard.OnDragging
                .Subscribe(OnCardDragging)
                .AddTo(_disposables);

            _handCards.Add(draggableCard);
        }

        // カードのスライドインアニメーション
        PlayCardEnterAnimation();

        foreach (var slot in slots)
        {
            slot.OnCardDropped
                .Subscribe(tuple => OnCardDroppedToSlot(tuple.slot, tuple.card))
                .AddTo(_disposables);
        }

        confirmButton.OnClickAsObservable()
            .Subscribe(_ => OnConfirmClicked())
            .AddTo(_disposables);

        UpdateConfirmButtonState();
    }

    public IReadOnlyList<CardModel> GetRankedCards()
    {
        _rankedCards.Clear();

        foreach (var slot in slots.OrderBy(s => s.Rank).Where(s => s.IsOccupied))
        {
            _rankedCards.Add(slot.PlacedCard.CardModel);
        }

        return _rankedCards;
    }

    public void Clear()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        foreach (var slot in slots)
        {
            slot.RemoveCard();
        }

        foreach (var card in _handCards)
        {
            Destroy(card.gameObject);
        }
        _handCards.Clear();
        _rankedCards.Clear();
    }

    private void OnCardDragStarted(DraggableCardView card)
    {
        _wasDroppedToSlot = false;

        // 手札エリアの中心からドラッグ線を表示
        var handCenterWorld = _handContainerRect.position;
        dragLineView.Show(handCenterWorld);
    }

    private void OnCardDragEnded(DraggableCardView card)
    {
        dragLineView.Hide();

        // スロットにドロップされた場合はOnCardDroppedToSlotで処理済み
        if (_wasDroppedToSlot) return;

        // スロット外にドロップされた場合、元のスロットから外して手札に戻す
        if (card.IsPlaced)
        {
            card.CurrentSlot.RemoveCard();
            UpdateConfirmButtonState();
        }

        var (position, rotation) = CalculateFanPosition(card.HandIndex, _handCards.Count);
        card.PlayReturnToHandAsync(handContainer, position, rotation).Forget();
    }

    private void OnCardDragging(Vector3 cardWorldPos)
    {
        dragLineView.UpdateEndPosition(cardWorldPos);
    }

    private void OnCardDroppedToSlot(RankingSlotView slot, DraggableCardView droppedCard)
    {
        _wasDroppedToSlot = true;

        var previousSlot = droppedCard.CurrentSlot;
        previousSlot?.RemoveCard();

        if (slot.IsOccupied)
        {
            var existingCard = slot.RemoveCard();

            if (previousSlot != null)
            {
                previousSlot.PlaceCard(existingCard);
                existingCard.PlaySnapToSlotAsync(previousSlot.CardAnchor, Vector2.zero).Forget();
            }
            else
            {
                var (pos, rot) = CalculateFanPosition(existingCard.HandIndex, _handCards.Count);
                existingCard.PlayReturnToHandAsync(handContainer, pos, rot).Forget();
            }
        }

        slot.PlaceCard(droppedCard);
        droppedCard.PlaySnapToSlotAsync(slot.CardAnchor, Vector2.zero).Forget();

        UpdateConfirmButtonState();
    }

    private void OnCardClicked(DraggableCardView card)
    {
        if (!card.IsPlaced) return;

        var slot = card.CurrentSlot;
        slot.RemoveCard();

        var (position, rotation) = CalculateFanPosition(card.HandIndex, _handCards.Count);
        card.PlayReturnToHandAsync(handContainer, position, rotation).Forget();

        UpdateConfirmButtonState();
    }

    private void OnConfirmClicked()
    {
        if (!IsAllSlotsOccupied()) return;

        _onRankingComplete.OnNext(Unit.Default);
    }

    private void UpdateConfirmButtonState()
    {
        confirmButton.interactable = IsAllSlotsOccupied();
    }

    private bool IsAllSlotsOccupied()
    {
        return slots.All(s => s.IsOccupied);
    }

    private (Vector2 position, float rotation) CalculateFanPosition(int index, int totalCount)
    {
        // -1 〜 1 の範囲に正規化（中央が0）
        var t = totalCount > 1
            ? (index - (totalCount - 1) / 2f) / ((totalCount - 1) / 2f)
            : 0f;

        var x = t * fanSpreadWidth / 2f;
        // 放物線で中央が高い
        var y = (1 - t * t) * fanHeightCurve;
        var rotation = -t * fanMaxAngle;

        return (new Vector2(x, y), rotation);
    }

    private void SetCardToFanPosition(DraggableCardView card, int index, int totalCount)
    {
        var (position, rotation) = CalculateFanPosition(index, totalCount);
        var cardRect = card.transform as RectTransform;

        cardRect.anchoredPosition = position;
        card.transform.localEulerAngles = new Vector3(0, 0, rotation);
    }

    /// <summary>
    /// スロットの順次スライド+フェードインアニメーション
    /// </summary>
    private void PlaySlotEnterAnimation()
    {
        _animHandles.CancelAll();

        for (var i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (!_slotOriginalPositions.TryGetValue(slot, out var originalPos)) continue;

            var slotRect = (RectTransform)slot.transform;
            var canvasGroup = slot.CanvasGroup;

            // 開始位置を上にオフセット、透明度を0に
            var startPos = originalPos + new Vector2(0, slotSlideOffset);
            slotRect.anchoredPosition = startPos;
            canvasGroup.alpha = 0f;

            // ディレイ付きでスライド+フェードイン
            var delay = slotStaggerDelay * i;

            var moveHandle = LMotion.Create(startPos, originalPos, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAnchoredPosition(slotRect)
                .AddTo(slotRect.gameObject);
            _animHandles.Add(moveHandle);

            var fadeHandle = LMotion.Create(0f, 1f, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAlpha(canvasGroup)
                .AddTo(canvasGroup.gameObject);
            _animHandles.Add(fadeHandle);
        }
    }

    /// <summary>
    /// カードの順次スライド+フェードインアニメーション
    /// </summary>
    private void PlayCardEnterAnimation()
    {
        Canvas.ForceUpdateCanvases();

        for (var i = 0; i < _handCards.Count; i++)
        {
            var card = _handCards[i];
            var cardRect = (RectTransform)card.transform;
            var canvasGroup = card.CanvasGroup;

            // ターゲット位置を取得
            var targetPos = cardRect.anchoredPosition;

            // 開始位置を下にオフセット、透明度を0に
            var startPos = targetPos + new Vector2(0, cardSlideOffset);
            cardRect.anchoredPosition = startPos;
            canvasGroup.alpha = 0f;

            // ディレイ付きでスライド+フェードイン
            var delay = cardStaggerDelay * i;

            var moveHandle = LMotion.Create(startPos, targetPos, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAnchoredPosition(cardRect)
                .AddTo(cardRect.gameObject);
            _animHandles.Add(moveHandle);

            var fadeHandle = LMotion.Create(0f, 1f, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAlpha(canvasGroup)
                .AddTo(canvasGroup.gameObject);
            _animHandles.Add(fadeHandle);
        }
    }

    private void Awake()
    {
        _handContainerRect = handContainer as RectTransform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        dragLineView.Initialize(canvas);

        Canvas.ForceUpdateCanvases();

        // スロットの初期位置を保存
        foreach (var slot in slots)
        {
            var slotRect = (RectTransform)slot.transform;
            _slotOriginalPositions[slot] = slotRect.anchoredPosition;
        }
    }

    private void OnDestroy()
    {
        _animHandles.CancelAll();
        _disposables.Dispose();
        _onRankingComplete.Dispose();
    }
}
