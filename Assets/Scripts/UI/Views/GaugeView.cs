using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ImageのfillAmountを使用したゲージ表示View
/// </summary>
public class GaugeView : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private static readonly int _valueProperty = Shader.PropertyToID("_Value");
    private static readonly int _tillingProperty = Shader.PropertyToID("_Tilling");

    public void SetValue(float value) => fillImage.material.SetFloat(_valueProperty, value * GetTillingY());

    private float GetTillingY()
    {
        return fillImage.material.GetVector(_tillingProperty).y;
    }
}
