using LitMotion;
using LitMotion.Extensions;
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
    [SerializeField] private float colorTransitionDuration = 0.5f;

    private Material _mentalFireMaterial;
    private MotionHandle _colorMotionHandle;
    
    /// <summary>
    /// 精神力表示を更新（現在値と割合でスプライトを切り替え）
    /// </summary>
    public void UpdateDisplay(int currentMentalPower, int maxMentalPower)
    {
        mentalPowerText.text = currentMentalPower.ToString();

        // 割合を計算（0.0～1.0）
        var ratio = (float)currentMentalPower / maxMentalPower;

        // 目標の色を決定
        Color targetColor;
        if (ratio <= 0.33f)
            targetColor = lowMentalColor;
        else if (ratio <= 0.66f)
            targetColor = midMentalColor;
        else
            targetColor = highMentalColor;

        // 前のアニメーションがあればキャンセル
        if (_colorMotionHandle.IsActive())
            _colorMotionHandle.Cancel();

        // LitMotionで色をアニメーション
        _colorMotionHandle = LMotion.Create(_mentalFireMaterial.color, targetColor, colorTransitionDuration)
            .BindToMaterialColor(_mentalFireMaterial, "_Color");
    }

    private void Awake()
    {
        _mentalFireMaterial = Instantiate(mentalStateImage.material);
        mentalStateImage.material = _mentalFireMaterial;
        
        UpdateDisplay(20, 20);
    }

    private void OnDestroy()
    {
        // アニメーションをキャンセル
        if (_colorMotionHandle.IsActive())
            _colorMotionHandle.Cancel();
    }
}