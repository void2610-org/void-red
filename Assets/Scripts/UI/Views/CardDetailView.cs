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
    /// <param name="isPlayable">プレイボタンを表示するか</param>
    /// <param name="themeData">現在のテーマ（nullの場合は色分けなし）</param>
    public void ShowCardDetail(CardData cardData, bool isPlayable, ThemeData themeData = null)
    {
        // カード詳細情報を設定
        UpdateCardDisplay(cardData, themeData);
        playButton.gameObject.SetActive(isPlayable);

        // パネルを表示
        Show();
    }
    
    /// <summary>
    /// カード表示を更新
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    /// <param name="themeData">現在のテーマ（nullの場合は色分けなし）</param>
    private void UpdateCardDisplay(CardData cardData, ThemeData themeData)
    {
        // 新しいカードViewを作成
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel);

        // キーワード情報
        UpdateKeywordsInfo(cardData, themeData);

        // 詳細情報を更新
        attributeText.text = $"属性: {cardData.Attribute.ToJapaneseName()}";
        collapseThresholdText.text = $"崩壊閾値: {cardData.CollapseThreshold}";
    }
    
    /// <summary>
    /// キーワード情報を更新
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    /// <param name="themeData">現在のテーマ（nullの場合は色分けなし）</param>
    private void UpdateKeywordsInfo(CardData cardData, ThemeData themeData)
    {
        if (cardData.Keywords == null || cardData.Keywords.Count == 0)
        {
            keywordsText.text = "キーワード: なし";
            return;
        }

        // テーマのキーワードと一致するものを赤色で表示
        var formattedKeywords = cardData.Keywords.Select(keyword =>
        {
            // テーマが存在し、かつテーマのキーワードに含まれる場合は赤色
            if (themeData != null && themeData.Keywords.Contains(keyword))
            {
                return $"<color=#FF0000>{keyword}</color>";
            }
            return keyword;
        });

        keywordsText.text = $"キーワード: {string.Join(", ", formattedKeywords)}";
    }
}