using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// ドラッグ可能なカードView
/// CardViewをラップしてドラッグ&ドロップ機能を追加
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private CardView cardView;
    [SerializeField] private float dragAlpha = 0.8f;
    [SerializeField] private float dragScale = 1.05f;

    public CardView CardView => cardView;
    public CardModel CardModel { get; private set; }
    public RankingSlotView CurrentSlot { get; private set; }
    public bool IsPlaced => CurrentSlot;
    public Observable<DraggableCardView> OnDragStarted => _onDragStarted;
    public Observable<DraggableCardView> OnDragEnded => _onDragEnded;
    public Observable<DraggableCardView> OnClicked => _onClicked;

    private const float SNAP_DURATION = 0.2f;
    private const float RETURN_DURATION = 0.3f;

    private readonly Subject<DraggableCardView> _onDragStarted = new();
    private readonly Subject<DraggableCardView> _onDragEnded = new();
    private readonly Subject<DraggableCardView> _onClicked = new();
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Transform _originalParent;
    private Vector2 _originalPosition;
    private Canvas _rootCanvas;
    private bool _isDragging;
    private MotionHandle _moveTween;
    private MotionHandle _scaleTween;

    public void SetSlot(RankingSlotView slot) => CurrentSlot = slot;

    public void Initialize(CardModel cardModel)
    {
        CardModel = cardModel;
        cardView.Initialize(cardModel.Data);
        cardView.SetInteractable(false);
    }

    public async UniTask PlaySnapToSlotAsync(Transform targetParent, Vector2 targetPosition)
    {
        transform.SetParent(targetParent);
        transform.localScale = Vector3.one;

        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _moveTween.TryCancel();
        _moveTween = _rectTransform.MoveToAnchored(targetPosition, SNAP_DURATION, Ease.OutCubic);
        await _moveTween.ToUniTask();
    }

    public async UniTask PlayReturnToHandAsync(Transform handParent)
    {
        var startWorldPos = _rectTransform.position;

        transform.SetParent(handParent);
        transform.localScale = Vector3.one;

        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // LayoutGroupにレイアウトを再計算させる
        LayoutRebuilder.ForceRebuildLayoutImmediate(handParent as RectTransform);

        var targetPosition = _rectTransform.anchoredPosition;

        // ワールド座標からローカル座標へ変換
        _rectTransform.position = startWorldPos;
        var startPosition = _rectTransform.anchoredPosition;

        _moveTween.TryCancel();
        _moveTween = _rectTransform.MoveToAnchoredFrom(startPosition, targetPosition, RETURN_DURATION, Ease.OutBack);
        await _moveTween.ToUniTask();
    }

    public async UniTask PlayReturnToOriginalAsync()
    {
        transform.SetParent(_originalParent);
        transform.localScale = Vector3.one;

        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _moveTween.TryCancel();
        _moveTween = _rectTransform.MoveToAnchored(_originalPosition, RETURN_DURATION, Ease.OutBack);
        await _moveTween.ToUniTask();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _originalParent = transform.parent;
        _originalPosition = _rectTransform.anchoredPosition;

        transform.SetParent(_rootCanvas.transform);
        transform.SetAsLastSibling();

        _canvasGroup.alpha = dragAlpha;
        _canvasGroup.blocksRaycasts = false;

        _scaleTween.TryCancel();
        _scaleTween = transform.ScaleTo(Vector3.one * dragScale, 0.1f, Ease.OutQuad);

        _onDragStarted.OnNext(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                _rootCanvas.worldCamera,
                out var localPoint))
        {
            _rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        _scaleTween.TryCancel();
        _scaleTween = transform.ScaleTo(Vector3.one, 0.1f, Ease.OutQuad);

        _onDragEnded.OnNext(this);
    }

    /// <summary>
    /// 配置済みカードの取り外し用
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging) return;

        _onClicked.OnNext(this);
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    private void OnDestroy()
    {
        _moveTween.TryCancel();
        _scaleTween.TryCancel();
        _onDragStarted.Dispose();
        _onDragEnded.Dispose();
        _onClicked.Dispose();
    }
}
