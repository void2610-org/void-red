using UnityEngine;
using UnityEngine.UI;

namespace VoidRed.UI.Views
{
    /// <summary>
    /// 各感情タイプのバーを表示するView
    /// </summary>
    public class EmotionGaugeView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        /// <summary>
        /// バーの値を設定（0.0〜1.0）
        /// </summary>
        public void SetValue(float normalizedValue)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalizedValue);
        }

        /// <summary>
        /// バーの色を設定
        /// </summary>
        public void SetColor(Color color)
        {
            fillImage.color = color;
        }
    }
}
