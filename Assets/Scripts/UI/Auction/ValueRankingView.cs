using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using R3;
using Cysharp.Threading.Tasks;
using Auction;

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

    [Header("UI")]
    [SerializeField] private Button confirmButton;

    [Header("ドラッグ演出")]
    [SerializeField] private DragLineView dragLineView;

    public Observable<Unit> OnRankingComplete => _onRankingComplete;

    private readonly List<DraggableCardView> _handCards = new();
    private readonly List<CardModel> _rankedCards = new();
    private readonly Subject<Unit> _onRankingComplete = new();
    private CompositeDisposable _disposables = new();
    private bool _wasDroppedToSlot;
    private RectTransform _handContainerRect;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void Awake()
    {
        _handContainerRect = handContainer as RectTransform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        dragLineView.Initialize(canvas);
    }

    public void StartRanking(IReadOnlyList<CardModel> cards)
    {
        Clear();

        foreach (var card in cards)
        {
            var draggableCard = Instantiate(draggableCardPrefab, handContainer);
            draggableCard.Initialize(card);

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

        card.PlayReturnToHandAsync(handContainer).Forget();
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
                existingCard.PlayReturnToHandAsync(handContainer).Forget();
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

        card.PlayReturnToHandAsync(handContainer).Forget();

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

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onRankingComplete.Dispose();
    }
}
