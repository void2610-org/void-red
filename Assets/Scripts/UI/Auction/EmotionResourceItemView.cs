using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 個別の感情リソース表示アイテム
// 色インジケーター、残量テキスト、選択状態ボーダーを持つ
public class EmotionResourceItemView : MonoBehaviour
{
    [SerializeField] private Image colorIndicator;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image selectionBorder;

    private EmotionType _emotion;

    public void Initialize(EmotionType emotion)
    {
        _emotion = emotion;
        colorIndicator.color = emotion.GetColor();
        selectionBorder.gameObject.SetActive(false);
    }

    public void UpdateAmount(int amount)
    {
        amountText.text = amount.ToString();
    }

    public void SetSelected(bool selected)
    {
        selectionBorder.gameObject.SetActive(selected);
    }
}
