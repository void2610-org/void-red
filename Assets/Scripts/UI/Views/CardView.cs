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
    public enum CardBidState
    {
        None,       // 入札なし
        PlayerBid, // プレイヤーが入札
        EnemyBid,   // 敵が入札
        DrawBid    // 引き分け入札
    }
    
    [Header("UIコンポーネント")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private Button cardButton;
    [SerializeField] private Image gaugeImage;
    [SerializeField] private Image growImage;

    public CardData CardData { get; private set; }
    public Observable<CardView> OnClicked { get; private set; }

    // BaseCardView 抽象プロパティの実装
    protected override Image CardImage => cardImage;
    protected override TextMeshProUGUI CardNameText => cardNameText;
    protected override Image CardFrame => cardFrame;
    protected override Image GaugeImage => gaugeImage;

    private Vector2 _originalPosition;
    private RectTransform _rectTransform;
    private CardDisplayState _displayState = CardDisplayState.Normal;
    // Tween管理用
    private MotionHandle _backTransitionTween;
    private MotionHandle _edgeColorTween;
    private Material _instancedGrowMaterial;

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
    
    public void SetGrowEffect(CardBidState state, Color enemyColor)
    {
        _instancedGrowMaterial.SetFloat("_Alpha", 0f);
        switch (state)
        {
            case CardBidState.PlayerBid:
                _instancedGrowMaterial.SetFloat("_Value", 0f);
                CardImage.material = _instancedGrowMaterial;
                break;
            case CardBidState.EnemyBid:
                _instancedGrowMaterial.SetColor("_Color2", enemyColor);
                _instancedGrowMaterial.SetFloat("_Value", 1f);
                CardImage.material = _instancedGrowMaterial;
                break;
            case CardBidState.DrawBid:
                _instancedGrowMaterial.SetColor("_Color2", enemyColor);
                _instancedGrowMaterial.SetFloat("_Value", 0.5f);
                CardImage.material = _instancedGrowMaterial;
                break;
        }
    }

    protected override CardData GetCardData() => CardData;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _instancedGrowMaterial = Instantiate(growImage.material);

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
