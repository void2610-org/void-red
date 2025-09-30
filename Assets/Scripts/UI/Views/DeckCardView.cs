using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    [Header("色設定")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color collapsedColor = new(0.5f, 0.5f, 0.5f, 0.7f);
    [SerializeField] private Color unviewedColor = new(0.2f, 0.2f, 0.2f, 0.9f);
    
    public CardModel CardModel { get; private set; }

    /// <summary>
    /// カードモデルを設定して表示を更新
    /// </summary>
    /// <param name="cardModel">表示するカードモデル</param>
    /// <param name="isVeiled">カードが隠されているか</param>
    public void Initialize(CardModel cardModel, bool isVeiled = false)
    {
        CardModel = cardModel;
        UpdateDisplay(isVeiled || CardModel.IsCollapsed);
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay(bool isVeiled)
    {
        if (!CardModel?.Data) return;
        
        // カード画像と名前を設定
        cardImage.sprite = CardModel.Data.CardImage;
        cardImage.color = CardModel.Data.CardImage ? Color.white : Color.clear;
        cardNameText.text = CardModel.Data.CardName;
        cardNameText.color = CardModel.Data.IsTextColorBlack ? Color.black : Color.white;
        
        // バナーとフレームの色をカードの色に設定
        cardTextBanner.color = CardModel.Data.Color;
        cardFrame.color = CardModel.Data.Color;

        // 未閲覧カード：黒く暗い表示
        if (isVeiled)
        {
            backgroundImage.color = unviewedColor;
            cardImage.color = CardModel.Data.CardImage ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.clear;
            cardTextBanner.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            cardNameText.color = Color.black;
            cardFrame.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            cardNameText.text = "???";
        }
    }
}