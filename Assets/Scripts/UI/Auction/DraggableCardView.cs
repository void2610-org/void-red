using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

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

    /// <summary>
    /// カードモデルで初期化
    /// </summary>
    public void Initialize(CardModel cardModel)
    {
        CardModel = cardModel;
        cardView.Initialize(cardModel.Data);
        cardView.SetInteractable(false);
    }

    /// <summary>
    /// スロット参照を設定
    /// </summary>
    public void SetSlot(RankingSlotView slot)
    {
        CurrentSlot = slot;
    }

    /// <summary>
    /// ドラッグ開始
    /// </summary>
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
        _scaleTween = LMotion.Create(transform.localScale, Vector3.one * dragScale, 0.1f)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(transform)
            .AddTo(gameObject);

        _onDragStarted.OnNext(this);
    }

    /// <summary>
    /// ドラッグ中（位置追従）
    /// </summary>
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

    /// <summary>
    /// ドラッグ終了
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;

        _scaleTween.TryCancel();
        _scaleTween = LMotion.Create(transform.localScale, Vector3.one, 0.1f)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(transform)
            .AddTo(gameObject);

        _onDragEnded.OnNext(this);
    }

    /// <summary>
    /// クリック時（配置済みカードの取り外し用）
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging) return;

        _onClicked.OnNext(this);
    }

    /// <summary>
    /// スロットへスナップするアニメーション
    /// </summary>
    public async UniTask PlaySnapToSlotAsync(Transform targetParent, Vector2 targetPosition)
    {
        transform.SetParent(targetParent);
        
        // 親のスケールに影響されないようにローカルスケールをリセット
        transform.localScale = Vector3.one;
        
        // アンカーとピボットを中央に設定
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _moveTween.TryCancel();
        await LMotion.Create(_rectTransform.anchoredPosition, targetPosition, SNAP_DURATION)
            .WithEase(Ease.OutCubic)
            .BindToAnchoredPosition(_rectTransform)
            .ToUniTask();
    }

    /// <summary>
    /// 手札に戻るアニメーション
    /// </summary>
    public async UniTask PlayReturnToHandAsync(Transform handParent)
    {
        // 現在のワールド位置を保存（アニメーション開始位置）
        var startWorldPos = _rectTransform.position;

        // 親を変更
        transform.SetParent(handParent);
        transform.localScale = Vector3.one;

        // アンカーとピボットを設定
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // LayoutGroupにレイアウトを再計算させる
        LayoutRebuilder.ForceRebuildLayoutImmediate(handParent as RectTransform);

        // LayoutGroupが計算した目標位置を取得
        var targetPosition = _rectTransform.anchoredPosition;

        // ワールド座標からローカル座標へ変換（アニメーション開始位置）
        _rectTransform.position = startWorldPos;
        var startPosition = _rectTransform.anchoredPosition;

        // 開始位置から目標位置へアニメーション
        _moveTween.TryCancel();
        await LMotion.Create(startPosition, targetPosition, RETURN_DURATION)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(_rectTransform)
            .ToUniTask();
    }

    /// <summary>
    /// 元の位置（ドラッグ開始前）に戻る
    /// </summary>
    public async UniTask PlayReturnToOriginalAsync()
    {
        transform.SetParent(_originalParent);
        
        // スケールとアンカーをリセット
        transform.localScale = Vector3.one;
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _moveTween.TryCancel();
        await LMotion.Create(_rectTransform.anchoredPosition, _originalPosition, RETURN_DURATION)
            .WithEase(Ease.OutBack)
            .BindToAnchoredPosition(_rectTransform)
            .ToUniTask();
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
