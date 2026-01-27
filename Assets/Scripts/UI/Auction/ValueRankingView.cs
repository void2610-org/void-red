using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using R3;
using Cysharp.Threading.Tasks;

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

    public Observable<Unit> OnRankingComplete => _onRankingComplete;

    private readonly List<DraggableCardView> _handCards = new();
    private readonly List<CardModel> _rankedCards = new();
    private readonly Subject<Unit> _onRankingComplete = new();
    private CompositeDisposable _disposables = new();

    /// <summary>
    /// カードを表示して順位選択を開始
    /// </summary>
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

    /// <summary>
    /// 設定結果を取得（順位順のカードリスト）
    /// </summary>
    public IReadOnlyList<CardModel> GetRankedCards()
    {
        _rankedCards.Clear();

        foreach (var slot in slots.OrderBy(s => s.Rank))
        {
            if (slot.PlacedCard)
            {
                _rankedCards.Add(slot.PlacedCard.CardModel);
            }
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

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnCardDragStarted(DraggableCardView card)
    {
        foreach (var slot in slots)
        {
            slot.SetDragActive(true);
        }
    }

    private void OnCardDragEnded(DraggableCardView card)
    {
        foreach (var slot in slots)
        {
            slot.SetDragActive(false);
        }

        if (!card.IsPlaced)
        {
            card.PlayReturnToHandAsync(handContainer).Forget();
        }
    }

    private void OnCardDroppedToSlot(RankingSlotView slot, DraggableCardView droppedCard)
    {
        var previousSlot = droppedCard.CurrentSlot;
        if (previousSlot != null)
        {
            previousSlot.RemoveCard();
        }

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
