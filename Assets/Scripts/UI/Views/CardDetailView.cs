using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using System.Linq;

/// <summary>
/// カード詳細情報を表示するモーダルViewクラス
/// 既存のDeckCardViewを活用してカード表示の一貫性を保つ
/// </summary>
public class CardDetailView : BaseWindowView
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button playButton;
    [SerializeField] private DeckCardView cardView;
    [SerializeField] private TextMeshProUGUI keywordsText;
    [SerializeField] private TextMeshProUGUI attributeText;
    [SerializeField] private TextMeshProUGUI collapseThresholdText;
    
    public Observable<Unit> PlayButtonClicked => playButton.OnClickAsObservable();

    /// <summary>
    /// カード詳細を表示
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    public void ShowCardDetail(CardData cardData, bool isPlayable)
    {
        // カード詳細情報を設定
        UpdateCardDisplay(cardData);
        playButton.gameObject.SetActive(isPlayable);

        // パネルを表示
        Show();
    }
    
    /// <summary>
    /// カード表示を更新
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    private void UpdateCardDisplay(CardData cardData)
    {
        // 新しいカードViewを作成
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel);
        
        // キーワード情報
        UpdateKeywordsInfo(cardData);
        
        // 詳細情報を更新
        attributeText.text = $"属性: {cardData.Attribute.ToJapaneseName()}";
        collapseThresholdText.text = $"崩壊閾値: {cardData.CollapseThreshold}";
    }
    
    /// <summary>
    /// キーワード情報を更新
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    private void UpdateKeywordsInfo(CardData cardData)
    {
        if (cardData.Keywords == null || cardData.Keywords.Count == 0)
        {
            keywordsText.text = "キーワード: なし";
            return;
        }

        // キーワードを日本語名に変換してカンマ区切りで表示
        keywordsText.text = $"キーワード: {string.Join(", ", cardData.Keywords)}";
    }
}