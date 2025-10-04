using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

/// <summary>
/// カードの表示状態
/// </summary>
public enum CardDisplayState
{
    Normal,    // 通常状態
    Veiled,    // 隠されている状態
    Collapsed  // 崩壊状態
}

/// <summary>
/// デッキ表示専用の簡易カードViewクラス
/// CardViewのサブセットで表示のみを担当
/// </summary>
public class DeckCardView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardTextBanner;
    [SerializeField] private Image cardFrame;
    [SerializeField] private UIEffect backUIEffect;

    [Header("色設定")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color collapsedColor = new(0.5f, 0.5f, 0.5f, 0.7f);
    [SerializeField] private Color veiledColor = new(0.2f, 0.2f, 0.2f, 0.9f);

    public CardModel CardModel { get; private set; }

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
        if (!CardModel?.Data) return;

        // カード画像と名前を設定
        cardImage.sprite = CardModel.Data.CardImage;
        cardNameText.text = CardModel.Data.CardName;

        switch (displayState)
        {
            case CardDisplayState.Normal:
                // 通常表示
                backgroundImage.color = activeColor;
                cardImage.color = CardModel.Data.CardImage ? Color.white : Color.clear;
                cardTextBanner.color = CardModel.Data.Color;
                cardFrame.color = CardModel.Data.Color;
                cardNameText.color = CardModel.Data.IsTextColorBlack ? Color.black : Color.white;
                break;

            case CardDisplayState.Veiled:
                // 隠されている表示：黒く暗い表示
                backgroundImage.color = veiledColor;
                cardImage.color = CardModel.Data.CardImage ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.clear;
                cardTextBanner.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                cardFrame.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                cardNameText.color = Color.black;
                cardNameText.text = "???";
                break;

            case CardDisplayState.Collapsed:
                // 崩壊表示：グレーアウト
                backgroundImage.color = collapsedColor;
                cardImage.color = CardModel.Data.CardImage ? new Color(0.5f, 0.5f, 0.5f, 0.7f) : Color.clear;
                cardTextBanner.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                cardFrame.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                cardNameText.color = CardModel.Data.IsTextColorBlack ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);
                break;
        }
    }
}