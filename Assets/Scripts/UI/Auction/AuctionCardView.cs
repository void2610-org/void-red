using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// オークション用カードのラッパーView
/// CardView + CardBidInfoView + 対話ボタンを統合
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AuctionCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardBidInfoView cardBidInfoView;
    [SerializeField] private Button dialogueButton;

    public CardView CardView => cardView;
    public CardBidInfoView BidInfoView => cardBidInfoView;
    public CardModel CardModel { get; private set; }

    public Observable<AuctionCardView> OnCardClicked => cardView.OnClicked.Select(_ => this);
    public Observable<AuctionCardView> OnDialogueClicked => dialogueButton.OnClickAsObservable().Select(_ => this);

    public void SetCardInteractable(bool interactable) => cardView.SetInteractable(interactable);

    public void SetDialogueInteractable(bool interactable) => dialogueButton.interactable = interactable;

    public void SetInteractable(bool interactable)
    {
        SetCardInteractable(interactable);
        SetDialogueInteractable(interactable);
    }

    /// <summary>
    /// カードデータで初期化
    /// </summary>
    public void Initialize(CardModel cardModel)
    {
        CardModel = cardModel;
        cardView.Initialize(cardModel.Data);
        cardBidInfoView.HideResult();
    }

    public async UniTask FadeOutAsync()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        await canvasGroup.FadeOut(0.25f).ToUniTask();
    }
}
