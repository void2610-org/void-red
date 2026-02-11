using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの顔アイコンと状態ゲージを表示するView
/// </summary>
public class PlayerFaceView : MonoBehaviour
{
    [SerializeField] private GaugeView painGauge;
    [SerializeField] private GaugeView dilutionGauge;

    public void UpdatePainGauge(float value) => painGauge.SetValue(value);
    public void UpdateDilutionGauge(float value) => dilutionGauge.SetValue(value);
}
