using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// ドラッグ可能なカードView
/// CardViewをラップしてドラッグ&ドロップ機能を追加
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private CardView cardView;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private float dragAlpha = 0.8f;
    [SerializeField] private float dragScale = 1.05f;

    public BattleCardModel BattleCard { get; private set; }
    public DeckSlotView CurrentSlot { get; private set; }
    public bool IsPlaced => CurrentSlot;
    public int HandIndex { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    public Observable<DraggableCardView> OnDragStarted => _onDragStarted;
    public Observable<DraggableCardView> OnDragEnded => _onDragEnded;
    public Observable<DraggableCardView> OnClicked => _onClicked;
    public Observable<Vector3> OnDragging => _onDragging;

    private const float SNAP_DURATION = 0.2f;
    private const float RETURN_DURATION = 0.3f;

    private readonly Subject<DraggableCardView> _onDragStarted = new();
    private readonly Subject<DraggableCardView> _onDragEnded = new();
    private readonly Subject<DraggableCardView> _onClicked = new();
    private readonly Subject<Vector3> _onDragging = new();
    private RectTransform _rectTransform;
    private Transform _originalParent;
    private Vector2 _originalPosition;
    private Canvas _rootCanvas;
    private bool _isDragging;
    private MotionHandle _moveTween;
    private MotionHandle _scaleTween;
    private MotionHandle _rotateTween;

    public void SetSlot(DeckSlotView slot) => CurrentSlot = slot;

    /// <summary>スキル効果等で変更された数字を反映する</summary>
    public void UpdateNumber(int number)
    {
        if (numberText)
            numberText.text = number.ToString();
    }

    public void Initialize(BattleCardModel battleCard, int handIndex)
    {
        BattleCard = battleCard;
        HandIndex = handIndex;

        // カードデータを表示
        if (battleCard.Card != null)
            cardView.Initialize(battleCard.Card.Data);

        // 数字表示
        if (numberText)
            numberText.text = battleCard.Number.ToString();
    }

    public async UniTask PlaySnapToSlotAsync(Transform targetParent, Vector2 targetPosition)
    {
        // 現在のワールド位置を保存
        var startWorldPos = _rectTransform.position;

        transform.SetParent(targetParent);
        transform.localScale = Vector3.one;

        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // ワールド座標からローカル座標へ変換して開始位置を取得
        _rectTransform.position = startWorldPos;
        var startPosition = _rectTransform.anchoredPosition;

        _moveTween.TryCancel();
        _moveTween = _rectTransform.MoveToAnchoredFrom(startPosition, targetPosition, SNAP_DURATION, Ease.OutCubic);
        await _moveTween.ToUniTask();
    }

    public async UniTask PlayReturnToHandAsync(Transform handParent, Vector2 targetPosition, float targetRotation)
    {
        var startWorldPos = _rectTransform.position;
        var startRotation = transform.localEulerAngles.z;

        transform.SetParent(handParent);
        transform.localScale = Vector3.one;

        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _rectTransform.position = startWorldPos;
        var startPosition = _rectTransform.anchoredPosition;

        _moveTween.TryCancel();
        _rotateTween.TryCancel();

        _moveTween = _rectTransform.MoveToAnchoredFrom(startPosition, targetPosition, RETURN_DURATION, Ease.OutBack);
        _rotateTween = LMotion.Create(startRotation, targetRotation, RETURN_DURATION)
            .WithEase(Ease.OutBack)
            .Bind(z => transform.localEulerAngles = new Vector3(0, 0, z))
            .AddTo(gameObject);

        await _moveTween.ToUniTask();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DraggableCardView] OnBeginDrag: {gameObject.name}, blocksRaycasts={CanvasGroup.blocksRaycasts}");
        _isDragging = true;
        _originalParent = transform.parent;
        _originalPosition = _rectTransform.anchoredPosition;

        // ドラッグ開始SEを再生
        if (BattleCard?.Card != null)
            SeManager.Instance.PlaySe(BattleCard.Card.Data.MemoryType.ToHoverSeName());

        transform.SetParent(_rootCanvas.transform);
        transform.SetAsLastSibling();

        CanvasGroup.alpha = dragAlpha;
        CanvasGroup.blocksRaycasts = false;

        // ドラッグ中は回転をリセット
        _rotateTween.TryCancel();
        transform.localEulerAngles = Vector3.zero;

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
            _onDragging.OnNext(_rectTransform.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DraggableCardView] OnEndDrag: {gameObject.name}, pointerCurrentRaycast={eventData.pointerCurrentRaycast.gameObject?.name}");
        _isDragging = false;

        CanvasGroup.alpha = 1f;
        CanvasGroup.blocksRaycasts = true;

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
        CanvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    private void OnDestroy()
    {
        _moveTween.TryCancel();
        _scaleTween.TryCancel();
        _rotateTween.TryCancel();
        _onDragStarted.Dispose();
        _onDragEnded.Dispose();
        _onClicked.Dispose();
        _onDragging.Dispose();
    }
}
