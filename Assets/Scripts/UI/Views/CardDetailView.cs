using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// カード詳細情報を表示するモーダルViewクラス
/// 既存のDeckCardViewを活用してカード表示の一貫性を保つ
/// </summary>
public class CardDetailView : BaseWindowView
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button playButton;
    [SerializeField] private DeckCardView cardView;

    public Observable<Unit> PlayButtonClicked => playButton.OnClickAsObservable();

    /// <summary>
    /// カード詳細を表示
    /// </summary>
    public void ShowCardDetail(CardData cardData, bool isPlayable)
    {
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel);
        playButton.gameObject.SetActive(isPlayable);
        Show();
        // カード詳細表示SE
        SeManager.Instance.PlaySe("SE_CARD_OPEN", pitch: 1f);
    }
}
