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
    
    [Header("色設定")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color collapsedColor = new(0.5f, 0.5f, 0.5f, 0.7f);
    
    public CardData CardData { get; private set; }
    
    /// <summary>
    /// カードデータを設定して表示を更新
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    /// <param name="isActive">使用可能かどうか</param>
    /// <param name="isCollapsed">崩壊しているかどうか</param>
    public void Initialize(CardData cardData, bool isActive, bool isCollapsed)
    {
        CardData = cardData;
        UpdateDisplay(isActive, isCollapsed);
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay(bool isActive, bool isCollapsed)
    {
        if (!CardData) return;
        
        // カード画像と名前を設定
        cardImage.sprite = CardData.CardImage;
        cardImage.color = CardData.CardImage ? Color.white : Color.clear;
        cardNameText.text = CardData.CardName;
        
        // 状態に応じて色を変更
        if (isCollapsed)
        {
            // 崩壊したカード：暗い色で半透明
            backgroundImage.color = collapsedColor;
        }
        else if (isActive)
        {
            // 使用可能なカード：通常の白色
            backgroundImage.color = activeColor;
        }
        else
        {
            // その他：少し暗め
            backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }
    }
}