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
    [SerializeField] private TextMeshProUGUI attributeText;
    [SerializeField] private TextMeshProUGUI scoreMultiplierText;
    [SerializeField] private TextMeshProUGUI collapseThresholdText;
    [SerializeField] private TextMeshProUGUI keywordsText;
    [SerializeField] private TextMeshProUGUI evolutionInfoText;
    
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
        
        // 詳細情報を更新
        attributeText.text = $"属性: {cardData.Attribute.ToJapaneseName()}";
        scoreMultiplierText.text = $"スコア倍率: {cardData.ScoreMultiplier:F1}x";
        collapseThresholdText.text = $"崩壊閾値: {cardData.CollapseThreshold}";

        // キーワード情報
        UpdateKeywordsInfo(cardData);

        // 進化情報
        UpdateEvolutionInfo(cardData);
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
        var keywordNames = cardData.Keywords
            .Where(k => k != KeywordType.None)
            .Select(k => k.GetJapaneseName());

        keywordsText.text = $"キーワード: {string.Join(", ", keywordNames)}";
    }

    /// <summary>
    /// 進化情報を更新
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    private void UpdateEvolutionInfo(CardData cardData)
    {
        var evolutionInfo = "";
        
        if (cardData.CanEvolve)
        {
            evolutionInfo += $"進化先: {cardData.EvolutionTarget?.CardName}\n";
        }
        
        if (cardData.CanDegrade)
        {
            evolutionInfo += $"劣化先: {cardData.DegradationTarget?.CardName}\n";
        }
        
        if (string.IsNullOrEmpty(evolutionInfo))
        {
            evolutionInfo = "進化・劣化なし";
        }
        
        evolutionInfoText.text = evolutionInfo.TrimEnd('\n');
    }
}