using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 精神力表示を担当するViewクラス（プレイヤー・敵共通）
/// </summary>
public class MentalPowerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mentalPowerText;
    [SerializeField] private Image mentalStateImage;
    
    [Header("精神力状態スプライト")]
    [SerializeField] private Sprite lowMentalSprite;   // 低精神力時のスプライト（0-33%）
    [SerializeField] private Sprite midMentalSprite;   // 中精神力時のスプライト（34-66%）
    [SerializeField] private Sprite highMentalSprite;  // 高精神力時のスプライト（67-100%）
    
    /// <summary>
    /// 精神力表示を更新（現在値と割合でスプライトを切り替え）
    /// </summary>
    public void UpdateDisplay(int currentMentalPower, int maxMentalPower)
    {
        mentalPowerText.text = currentMentalPower.ToString();
        
        // 割合を計算（0.0～1.0）
        float ratio = (float)currentMentalPower / maxMentalPower;
        
        // 割合に応じて適切なスプライトを設定
        if (!mentalStateImage) return;
        
        if (ratio <= 0.33f)
        {
            mentalStateImage.sprite = lowMentalSprite;
        }
        else if (ratio <= 0.66f)
        {
            mentalStateImage.sprite = midMentalSprite;
        }
        else
        {
            mentalStateImage.sprite = highMentalSprite;
        }
    }
}