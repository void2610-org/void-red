using R3;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// オークション用カードのラッパーView
/// CardView + CardBidInfoView + 対話ボタンを統合
/// </summary>
public class AuctionCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardBidInfoView cardBidInfoView;
    [SerializeField] private Button dialogueButton;

    public CardView CardView => cardView;
    public CardBidInfoView BidInfoView => cardBidInfoView;
    public CardModel CardModel { get; private set; }

    // カードクリックイベント
    public Observable<AuctionCardView> OnCardClicked =>
        cardView.OnClicked.Select(_ => this);

    // 対話ボタンクリックイベント
    public Observable<AuctionCardView> OnDialogueClicked =>
        dialogueButton.OnClickAsObservable().Select(_ => this);

    /// <summary>
    /// 対話ボタンの表示/非表示
    /// </summary>
    public void SetDialogueButtonVisible(bool visible) =>
        dialogueButton.gameObject.SetActive(visible);

    /// <summary>
    /// カードデータで初期化
    /// </summary>
    public void Initialize(CardModel cardModel)
    {
        CardModel = cardModel;
        cardView.Initialize(cardModel.Data);
        cardBidInfoView.ShowPlayerBidOnly(0);
        cardBidInfoView.HideResult();
    }

    /// <summary>
    /// カードの操作可否を設定
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        cardView.SetInteractable(interactable);
        dialogueButton.interactable = interactable;
    }
}
