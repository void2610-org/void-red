using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// カードの表示と基本的なロジックを担当するViewクラス
/// 元のCard.csをベースに選択機能とアニメーション機能を追加した簡略化されたMVPパターン
/// </summary>
public class CardView : BaseCardView
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private Button cardButton;
    [SerializeField] private UIEffect edgeUIEffect;
    [SerializeField] private Image gaugeImage;

    public CardData CardData { get; private set; }
    public Observable<CardView> OnClicked { get; private set; }

    // BaseCardView 抽象プロパティの実装
    protected override Image CardImage => cardImage;
    protected override TextMeshProUGUI CardNameText => cardNameText;
    protected override Image CardFrame => cardFrame;
    protected override UIEffect EdgeUIEffect => edgeUIEffect;
    protected override Image GaugeImage => gaugeImage;
    protected override CardData GetCardData() => CardData;
    private Vector2 _originalPosition;
    private RectTransform _rectTransform;
    private CardDisplayState _displayState = CardDisplayState.Normal;

    // Tween管理用
    private MotionHandle _backTransitionTween;
    private MotionHandle _edgeColorTween;

    public void SetInteractable(bool interactable) => cardButton.interactable = interactable;

    /// <summary>
    /// カードデータを設定して初期化
    /// </summary>
    public void Initialize(CardData cardData)
    {
        CardData = cardData;
        _originalPosition = _rectTransform.anchoredPosition;
        UpdateCardDisplay(_displayState);
    }

    /// <summary>
    /// ハイライト表示
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        _rectTransform.ScaleTo(highlight ? Vector3.one * 1.1f : Vector3.one, 0.1f);
        if (highlight) SeManager.Instance.PlaySe("CardSelect");
    }

    /// <summary>
    /// プレイヤー側（画面下）へ移動するアニメーション
    /// </summary>
    public async UniTask PlayMoveToPlayerSideAsync(float duration = 0.5f)
    {
        var startY = _rectTransform.anchoredPosition.y;
        var targetY = startY - 500f;
        await LMotion.Create(startY, targetY, duration)
            .WithEase(Ease.OutCubic)
            .Bind(y => _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, y))
            .ToUniTask();
    }

    /// <summary>
    /// 敵側（画面上）へ移動するアニメーション
    /// </summary>
    public async UniTask PlayMoveToEnemySideAsync(float duration = 0.5f)
    {
        var startY = _rectTransform.anchoredPosition.y;
        var targetY = startY + 500f;
        await LMotion.Create(startY, targetY, duration)
            .WithEase(Ease.OutCubic)
            .Bind(y => _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, y))
            .ToUniTask();
    }

    /// <summary>
    /// フェードアウトアニメーション（入札なしカード用）
    /// </summary>
    public async UniTask PlayFadeOutAsync(float duration = 0.3f)
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        await LMotion.Create(1f, 0f, duration)
            .WithEase(Ease.OutCubic)
            .Bind(alpha => canvasGroup.alpha = alpha)
            .ToUniTask();
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        // R3のOnClickAsObservableでボタンクリックを購読し、CardView自身を発行
        OnClicked = cardButton.OnClickAsObservable().Select(_ => this);
    }

    private void OnDestroy()
    {
        // Tweenのクリーンアップ
        _edgeColorTween.TryCancel();
        _backTransitionTween.TryCancel();
    }

}
