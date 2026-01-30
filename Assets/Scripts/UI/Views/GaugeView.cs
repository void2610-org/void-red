using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ImageのfillAmountを使用したゲージ表示View
/// </summary>
public class GaugeView : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetValue(float value) => fillImage.fillAmount = Mathf.Clamp01(value);
}
