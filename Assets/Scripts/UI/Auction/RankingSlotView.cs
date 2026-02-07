using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 順位スロットView
/// ドラッグされたカードを受け入れるドロップ先
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class RankingSlotView : MonoBehaviour, IDropHandler
{
    [SerializeField] private int rank;
    [SerializeField] private Image highlightImage;

    public int Rank => rank;
    public DraggableCardView PlacedCard { get; private set; }
    public bool IsOccupied => PlacedCard;
    public Transform CardAnchor => this.transform;
    public CanvasGroup CanvasGroup => _canvasGroup;
    public Observable<(RankingSlotView slot, DraggableCardView card)> OnCardDropped => _onCardDropped;

    private const float FADE_DURATION = 0.15f;

    private readonly Subject<(RankingSlotView slot, DraggableCardView card)> _onCardDropped = new();
    private MotionHandle _fadeTween;
    private CanvasGroup _canvasGroup;

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

    private void SetHighlight(bool highlight)
    {
        _fadeTween.TryCancel();
        _fadeTween = highlight
            ? highlightImage.FadeIn(FADE_DURATION, Ease.OutQuad)
            : highlightImage.FadeOut(FADE_DURATION, Ease.OutQuad);
    }

    /// <summary>
    /// スロットにカードがドロップされた時の処理
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var draggableCard = eventData.pointerDrag?.GetComponent<DraggableCardView>();
        if (!draggableCard) return;

        _onCardDropped.OnNext((this, draggableCard));
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        highlightImage.SetAlpha(0f);
    }

    private void OnDestroy()
    {
        _fadeTween.TryCancel();
        _onCardDropped.Dispose();
    }
}
