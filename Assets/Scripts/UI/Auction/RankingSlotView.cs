using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using R3;

/// <summary>
/// 順位スロットView
/// ドラッグされたカードを受け入れるドロップ先
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RankingSlotView : MonoBehaviour, IDropHandler
{
    [SerializeField] private int rank;
    [SerializeField] private Image highlightImage;

    public int Rank => rank;
    public DraggableCardView PlacedCard { get; private set; }
    public bool IsOccupied => PlacedCard;
    public Transform CardAnchor => this.transform;
    public Observable<(RankingSlotView slot, DraggableCardView card)> OnCardDropped => _onCardDropped;

    private readonly Subject<(RankingSlotView slot, DraggableCardView card)> _onCardDropped = new();

    /// <summary>
    /// スロットにカードがドロップされた時の処理
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var draggableCard = eventData.pointerDrag?.GetComponent<DraggableCardView>();
        if (!draggableCard) return;

        _onCardDropped.OnNext((this, draggableCard));
    }

    /// <summary>
    /// スロットにカードを配置
    /// </summary>
    public void PlaceCard(DraggableCardView card)
    {
        PlacedCard = card;
        card.SetSlot(this);
        SetHighlight(true);
    }

    /// <summary>
    /// スロットからカードを取り外す
    /// </summary>
    public DraggableCardView RemoveCard()
    {
        var card = PlacedCard;
        if (PlacedCard)
        {
            PlacedCard.SetSlot(null);
        }
        PlacedCard = null;
        SetHighlight(false);
        return card;
    }

    /// <summary>
    /// ハイライト表示設定
    /// </summary>
    private void SetHighlight(bool highlight)
    {
        highlightImage.gameObject.SetActive(highlight);
    }

    private void Awake()
    {
        highlightImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _onCardDropped.Dispose();
    }
}
