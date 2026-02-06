using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 入札ウィンドウView
// カードクリック時にモーダル表示される入札額調整UI
public class BidWindowView : BaseWindowView
{
    [Header("感情表示")]
    [SerializeField] private Image emotionIndicator;

    [Header("入札操作")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text bidAmountText;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();

    public void SetEmotion(EmotionType emotion) => emotionIndicator.color = emotion.GetColor();

    public void UpdateBidAmount(int amount) => bidAmountText.text = amount.ToString();
}
