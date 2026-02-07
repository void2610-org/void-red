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
    private static readonly int _alpha = Shader.PropertyToID("_Alpha");
    private static readonly int _value = Shader.PropertyToID("_Value");
    private static readonly int _color2 = Shader.PropertyToID("_Color2");

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
        _instancedGrowMaterial.SetFloat(_alpha, state != CardBidState.None ? 0.4f : 0f);
        switch (state)
        {
            case CardBidState.PlayerBid:
                _instancedGrowMaterial.SetFloat(_value, 1f);
                break;
            case CardBidState.EnemyBid:
                _instancedGrowMaterial.SetColor(_color2, enemyColor);
                _instancedGrowMaterial.SetFloat(_value, 0f);
                break;
            case CardBidState.DrawBid:
                _instancedGrowMaterial.SetColor(_color2, enemyColor);
                _instancedGrowMaterial.SetFloat(_value, 0.5f);
                break;
        }
    }

    protected override CardData GetCardData() => CardData;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _instancedGrowMaterial = Instantiate(growImage.material);
        growImage.material = _instancedGrowMaterial;

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
