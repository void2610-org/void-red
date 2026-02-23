using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 入札ウィンドウView
// カードクリック時にモーダル表示される入札額調整UI
public class BidWindowView : BaseWindowView
{
    [Header("情報表示")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text emotionNameText;
    [SerializeField] private Image emotionIndicator;

    [Header("炎演出")]
    [SerializeField] private Image flameImage;
    [SerializeField] private Sprite[] flameSprites;

    [Header("入札操作")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text bidAmountText;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();
    public Observable<Unit> OnClose => closeButton.OnClickAsObservable();

    public void SetCardName(string name) => cardNameText.text = name;

    public void UpdateBidAmount(int amount)
    {
        bidAmountText.text = amount.ToString();
        UpdateFlame(amount);
    }

    public void SetEmotion(EmotionType emotion)
    {
        emotionIndicator.color = emotion.GetColor();
        emotionNameText.text = emotion.ToJapaneseName();
    }

    // 入札値に応じた炎スプライトに差し替え
    private void UpdateFlame(int amount)
    {
        var index = Mathf.Clamp(amount, 0, flameSprites.Length - 1);
        flameImage.sprite = flameSprites[index];
    }
}
