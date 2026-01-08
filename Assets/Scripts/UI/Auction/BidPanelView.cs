using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

// 入札パネルView
// カード選択時に表示される入札額調整UI
public class BidPanelView : MonoBehaviour
{
    [Header("入札操作")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI remainingResourceText;

    [Header("感情切り替え")]
    [SerializeField] private Button emotionCycleButton;
    [SerializeField] private Image currentEmotionColorIndicator;
    [SerializeField] private TextMeshProUGUI currentEmotionNameText;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();
    public Observable<Unit> OnConfirm => confirmButton.OnClickAsObservable();
    public Observable<Unit> OnEmotionCycle => emotionCycleButton.OnClickAsObservable();

    public void UpdateRemainingResource(int remaining)
    {
        remainingResourceText.text = $"残り: {remaining}";
    }

    public void UpdateCurrentEmotion(EmotionType emotion)
    {
        currentEmotionColorIndicator.color = emotion.GetColor();
        currentEmotionNameText.text = emotion.ToJapaneseName();
    }
}
