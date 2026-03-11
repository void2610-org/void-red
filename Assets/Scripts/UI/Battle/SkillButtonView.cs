using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// スキルボタンのView
/// Canvas直下に1つだけ配置し、DeckSelection/CardBattle両フェーズで共有する
/// </summary>
public class SkillButtonView : MonoBehaviour
{
    [SerializeField] private Button skillButton;
    [SerializeField] private Image emotionIcon;
    [SerializeField] private TextMeshProUGUI emotionNameText;
    [SerializeField] private TextMeshProUGUI skillDescText;
    [SerializeField] private SerializableDictionary<EmotionType, Sprite> emotionSprites = new();

    /// <summary>スキルボタンが押された</summary>
    public Observable<Unit> OnActivated => skillButton.OnClickAsObservable();

    /// <summary>表示/非表示を切り替える</summary>
    public void SetVisible(bool visible) => gameObject.SetActive(visible);

    /// <summary>押下可否を切り替える</summary>
    public void SetInteractable(bool isInteractable) => skillButton.interactable = isInteractable;

    /// <summary>感情タイプに応じてアイコン・スキル名・説明テキストを設定</summary>
    public void Initialize(EmotionType emotion)
    {
        if (emotionSprites.TryGetValue(emotion, out var sprite))
            emotionIcon.sprite = sprite;

        emotionNameText.text = emotion.ToJapaneseName();
        skillDescText.text = SkillEffectApplier.GetDescription(emotion);
    }

    private void Awake()
    {
        gameObject.SetActive(false);
        skillButton.interactable = true;
    }
}
