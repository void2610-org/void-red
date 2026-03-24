using Coffee.UIEffects;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カードの表示状態
/// </summary>
public enum CardDisplayState
{
    /// <summary> 通常状態 </summary>
    Normal,
    /// <summary> 隠されている状態 </summary>
    Veiled,
    /// <summary> 崩壊状態 </summary>
    Collapsed,
    /// <summary> 裏面状態 </summary>
    Backside
}

/// <summary>
/// デッキ表示専用の簡易カードViewクラス
/// CardViewのサブセットで表示のみを担当
/// </summary>
public class DeckCardView : BaseCardView
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button cardButton;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardTextBanner;
    [SerializeField] private Image cardFrame;

    [Header("色設定")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color collapsedColor = new(0.5f, 0.5f, 0.5f, 0.7f);
    [SerializeField] private Color veiledColor = new(0.2f, 0.2f, 0.2f, 0.9f);

    public CardModel CardModel { get; private set; }
    public Observable<CardData> OnCardClicked => _onCardClicked;

    private readonly Subject<CardData> _onCardClicked = new();
    protected override CardData GetCardData() => CardModel?.Data;

    // BaseCardView 抽象プロパティの実装
    protected override Image CardImage => cardImage;
    protected override TextMeshProUGUI CardNameText => cardNameText;
    protected override Image CardFrame => cardFrame;

    /// <summary>
    /// カードモデルを設定して表示を更新
    /// </summary>
    /// <param name="cardModel">表示するカードモデル</param>
    /// <param name="displayState">カードの表示状態</param>
    public void Initialize(CardModel cardModel, CardDisplayState displayState = CardDisplayState.Normal)
    {
        CardModel = cardModel;
        UpdateDisplay(displayState);
    }

    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay(CardDisplayState displayState)
    {
        // 基底クラスの共通表示ロジックを呼び出し
        UpdateCardDisplay(displayState);

        // DeckCardView固有の backgroundImage の色設定
        switch (displayState)
        {
            case CardDisplayState.Normal:
                backgroundImage.color = activeColor;
                break;

            case CardDisplayState.Veiled:
                backgroundImage.color = veiledColor;
                break;

            case CardDisplayState.Collapsed:
                backgroundImage.color = collapsedColor;
                break;

            case CardDisplayState.Backside:
                backgroundImage.color = Color.clear;
                break;
        }
    }

    /// <summary>
    /// カードボタンがクリックされた時の処理
    /// </summary>
    private void OnCardButtonClicked()
    {
        if (CardModel?.Data)
        {
            _onCardClicked.OnNext(CardModel.Data);
        }
    }

    private void Awake()
    {
        // カードボタンのクリックイベントを購読
        cardButton.OnClickAsObservable()
            .Subscribe(_ => OnCardButtonClicked())
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onCardClicked?.Dispose();
    }
}
