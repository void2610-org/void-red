using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// キャラクター記憶ビュー
/// 右側パネルでキャラクターとカードを表示
/// </summary>
public class MemoryDetailView : BaseWindowView, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("カード表示")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private MemoryCardItemView cardItemPrefab;
    [SerializeField] private Transform centerThemeImage; // 中央のテーマ表示Image

    [Header("円形配置パラメータ")]
    [SerializeField] private float radiusX = 300f;
    [SerializeField] private float radiusY = 100f;
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.0f;

    [Header("ドラッグ回転")]
    [SerializeField] private float dragSensitivity = 0.5f;

    [Header("慣性・スナップ")]
    [SerializeField] private float inertiaDecay = 5f;
    [SerializeField] private float snapThreshold = 0.1f;
    [SerializeField] private float snapDuration = 0.2f;

    private readonly List<MemoryCardItemView> _cardViews = new();
    private IReadOnlyList<CardModel> _currentCards;
    private float _currentAngleOffset;
    private float _dragStartAngleOffset;
    private Vector2 _dragStartLocalPoint;
    private RectTransform _containerRectTransform;
    private float _angularVelocity;
    private bool _isInertiaActive;
    private Vector2 _previousLocalPoint;
    private MotionHandle _snapMotionHandle;

    /// <summary>
    /// 既存テーマのカードを表示（リストクリック時）
    /// </summary>
    public async UniTask ShowAndWaitClose(AcquiredTheme theme)
    {
        foreach (var cardView in _cardViews.Where(cardView => cardView)) Destroy(cardView.gameObject);
        _cardViews.Clear();

        ShowCardsInCircle(theme.AcquiredCards);
        base.Show();

        await closeButton.OnClickAsync();
        base.Hide();
    }

    /// <summary>
    /// カードを円形に配置して表示
    /// </summary>
    private void ShowCardsInCircle(IReadOnlyList<CardModel> cards)
    {
        _currentCards = cards;
        _currentAngleOffset = 0f;

        var count = cards.Count;
        if (count == 0) return;

        // カードビューを生成
        for (var i = 0; i < count; i++)
        {
            var cardView = Instantiate(cardItemPrefab, cardContainer);
            cardView.Initialize(cards[i]);
            _cardViews.Add(cardView);
        }

        UpdateCardPositions();
    }

    /// <summary>
    /// カードの位置を更新（ドラッグ回転に対応）
    /// </summary>
    private void UpdateCardPositions()
    {
        var count = _cardViews.Count;
        if (count == 0) return;

        // 円形配置の計算用リスト（深度でソート用）
        var cardPositions = new List<(int index, float angle, float depth)>();

        // 2アイテムの場合は左右配置（0から開始）、それ以外は上から開始
        var startAngle = count == 2 ? 0f : Mathf.PI / 2f;

        for (var i = 0; i < count; i++)
        {
            // 角度オフセットを適用
            var angle = startAngle - (2f * Mathf.PI / count) * i + _currentAngleOffset;
            // 深度: sin(angle)が-1（下）のとき手前、1（上）のとき奥
            // normalizedDepth: 0=手前, 1=奥
            var normalizedDepth = (Mathf.Sin(angle) + 1f) / 2f;
            cardPositions.Add((i, angle, normalizedDepth));
        }

        // 奥から手前順にソート（描画順用）
        var sortedByDepth = cardPositions.OrderByDescending(p => p.depth).ToList();

        // 描画順: 奥側カード → centerThemeImage → 手前側カード
        var backCardCount = sortedByDepth.Count(p => p.depth > 0.5f);
        var backIndex = 0;
        var frontIndex = 0;

        foreach (var (index, angle, depth) in sortedByDepth)
        {
            var cardView = _cardViews[index];

            // 楕円上の位置を計算
            var x = radiusX * Mathf.Cos(angle);
            var y = radiusY * Mathf.Sin(angle);
            var position = new Vector2(x, y);

            // スケール: 奥（depth=1）が小さく、手前（depth=0）が大きい
            var scale = Mathf.Lerp(maxScale, minScale, depth);

            // 奥側（depth > 0.5）はcenterより前、手前側はcenterより後
            var sortOrder = depth > 0.5f
                ? backIndex++
                : backCardCount + 1 + frontIndex++;

            cardView.SetCircularPosition(position, scale, sortOrder);
        }

        // centerThemeImageを奥側カードと手前側カードの間に配置
        centerThemeImage.SetSiblingIndex(backCardCount);
    }

    /// <summary>
    /// 最も近いカードを正面にスナップするアニメーションを開始
    /// </summary>
    private void StartSnapAnimation()
    {
        var count = _cardViews.Count;
        if (count == 0) return;

        var anglePerCard = 2f * Mathf.PI / count;
        var startAngle = count == 2 ? 0f : Mathf.PI / 2f;

        // 正面位置: sin(angle) = -1 → angle = -π/2
        var targetFrontAngle = -Mathf.PI / 2f;
        var bestSnapOffset = _currentAngleOffset;
        var minDiff = float.MaxValue;

        // 正面に最も近いカードを見つける
        for (var i = 0; i < count; i++)
        {
            var cardBaseAngle = startAngle - anglePerCard * i;
            // このカードを正面に持ってくるために必要なオフセット
            var neededOffset = targetFrontAngle - cardBaseAngle;
            // 現在のオフセットとの差分（-π〜πに正規化）
            var diff = NormalizeAngle(neededOffset - _currentAngleOffset);

            if (Mathf.Abs(diff) < minDiff)
            {
                minDiff = Mathf.Abs(diff);
                bestSnapOffset = _currentAngleOffset + diff;
            }
        }

        // LitMotionでスムーズにスナップ
        var startOffset = _currentAngleOffset;
        _snapMotionHandle = LMotion.Create(0f, 1f, snapDuration)
            .WithEase(Ease.OutCubic)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .Bind(t =>
            {
                _currentAngleOffset = Mathf.Lerp(startOffset, bestSnapOffset, t);
                UpdateCardPositions();
            });
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > Mathf.PI) angle -= 2f * Mathf.PI;
        while (angle < -Mathf.PI) angle += 2f * Mathf.PI;
        return angle;
    }

    protected override void Awake()
    {
        base.Awake();
        _containerRectTransform = cardContainer as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_cardViews.Count == 0) return;

        // スナップ中ならキャンセル
        _snapMotionHandle.TryCancel();
        _isInertiaActive = false;
        _angularVelocity = 0f;

        _dragStartAngleOffset = _currentAngleOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _containerRectTransform, eventData.position,
            eventData.pressEventCamera, out _dragStartLocalPoint);
        _previousLocalPoint = _dragStartLocalPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_cardViews.Count == 0) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _containerRectTransform, eventData.position,
                eventData.pressEventCamera, out var currentLocalPoint)) return;

        // 水平移動量を角度に変換
        var deltaX = currentLocalPoint.x - _dragStartLocalPoint.x;
        var angleDelta = (deltaX / radiusX) * Mathf.PI * dragSensitivity;

        _currentAngleOffset = _dragStartAngleOffset + angleDelta;
        UpdateCardPositions();

        // 角速度を計算（慣性用）
        var frameDeltaX = currentLocalPoint.x - _previousLocalPoint.x;
        if (Time.unscaledDeltaTime > 0f)
        {
            _angularVelocity = (frameDeltaX / radiusX) * Mathf.PI * dragSensitivity / Time.unscaledDeltaTime;
        }
        _previousLocalPoint = currentLocalPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_cardViews.Count == 0) return;
        _isInertiaActive = true;
    }

    private void Update()
    {
        if (!_isInertiaActive || _cardViews.Count == 0) return;

        // 慣性による回転
        _currentAngleOffset += _angularVelocity * Time.unscaledDeltaTime;
        _angularVelocity = Mathf.MoveTowards(_angularVelocity, 0f, inertiaDecay * Time.unscaledDeltaTime);
        UpdateCardPositions();

        // 速度が閾値以下になったらスナップ開始
        if (Mathf.Abs(_angularVelocity) < snapThreshold)
        {
            _isInertiaActive = false;
            StartSnapAnimation();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _snapMotionHandle.TryCancel();
    }
}
