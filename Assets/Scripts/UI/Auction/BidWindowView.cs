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

    // 入札値から炎レベル(0〜2)を判定
    public static int GetFlameLevel(int bidAmount) => bidAmount switch
    {
        <= 4 => 0,
        <= 9 => 1,
        _ => 2,
    };

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

    private void UpdateFlame(int amount)
    {
        var level = GetFlameLevel(amount);
        flameImage.sprite = flameSprites[level];
    }
}
