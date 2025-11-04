using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitMotion;
using LitMotion.Extensions;

/// <summary>
/// スライダー形式の設定項目UI
/// 左右ナビゲーションでスライダー値を変更
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderSettingItem : MonoBehaviour, ISettingItemNavigatable
{
    private readonly Subject<(string settingName, object value)> _onValueChanged = new();
    private Slider _slider;
    private string _settingName;
    private float _minValue;
    private float _maxValue;

    private const float NAVIGATION_STEP = 0.1f;

    public GameObject SelectableGameObject => _slider.gameObject;
    public Observable<(string settingName, object value)> OnValueChanged => _onValueChanged;

    /// <summary>
    /// 設定項目を初期化
    /// </summary>
    public void Initialize(string settingName, float minValue, float maxValue, float currentValue)
    {
        _settingName = settingName;
        _minValue = minValue;
        _maxValue = maxValue;

        _slider = GetComponentInChildren<Slider>();

        // スライダーの設定
        _slider.minValue = minValue;
        _slider.maxValue = maxValue;
        _slider.value = currentValue;

        // スライダー変更イベントのリスニング
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// <summary>
    /// 左右ナビゲーション（スライダー値の増減）
    /// </summary>
    public void OnNavigateHorizontal(float direction)
    {
        if (Mathf.Abs(direction) < 0.1f) return;

        var newValue = _slider.value + (direction > 0 ? NAVIGATION_STEP : -NAVIGATION_STEP);
        _slider.value = Mathf.Clamp(newValue, _minValue, _maxValue);
    }

    public void OnSubmit() { }

    /// <summary>
    /// スライダー値変更時のハンドラー
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        _onValueChanged.OnNext((_settingName, value));
    }

    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        _onValueChanged?.Dispose();
    }
}
