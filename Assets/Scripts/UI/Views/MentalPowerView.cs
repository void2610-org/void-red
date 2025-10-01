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
    [ColorUsage(false, true), SerializeField] private Color highMentalColor = Color.blue;
    [ColorUsage(false, true), SerializeField] private Color midMentalColor = Color.yellow;
    [ColorUsage(false, true), SerializeField] private Color lowMentalColor = Color.red;
    
    private Material _mentalFireMaterial;
    
    /// <summary>
    /// 精神力表示を更新（現在値と割合でスプライトを切り替え）
    /// </summary>
    public void UpdateDisplay(int currentMentalPower, int maxMentalPower)
    {
        mentalPowerText.text = currentMentalPower.ToString();
        
        // 割合を計算（0.0～1.0）
        var ratio = (float)currentMentalPower / maxMentalPower;
        
        if (ratio <= 0.33f)
            _mentalFireMaterial.color = lowMentalColor;
        else if (ratio <= 0.66f)
            _mentalFireMaterial.color = midMentalColor;
        else
            _mentalFireMaterial.color = highMentalColor;
    }

    private void Awake()
    {
        _mentalFireMaterial = Instantiate(mentalStateImage.material);
        mentalStateImage.material = _mentalFireMaterial;
    }
}