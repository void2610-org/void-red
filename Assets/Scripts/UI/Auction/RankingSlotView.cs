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
public class RankingSlotView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int rank;
    [SerializeField] private Image highlightImage;

    public int Rank => rank;
    public DraggableCardView PlacedCard { get; private set; }
    public bool IsOccupied => PlacedCard;
    public Transform CardAnchor => this.transform;
    public Observable<(RankingSlotView slot, DraggableCardView card)> OnCardDropped => _onCardDropped;

    private readonly Subject<(RankingSlotView slot, DraggableCardView card)> _onCardDropped = new();
    private bool _isDragActive;

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
    /// ポインターがスロット上に入った時
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isDragActive) return;
        SetHighlight(true);
    }

    /// <summary>
    /// ポインターがスロットから離れた時
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    /// <summary>
    /// ドラッグ状態を設定（ドラッグ中のみハイライト可能にする）
    /// </summary>
    public void SetDragActive(bool active)
    {
        _isDragActive = active;
        if (!active)
        {
            SetHighlight(false);
        }
    }

    /// <summary>
    /// スロットにカードを配置
    /// </summary>
    public void PlaceCard(DraggableCardView card)
    {
        PlacedCard = card;
        card.SetSlot(this);
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
