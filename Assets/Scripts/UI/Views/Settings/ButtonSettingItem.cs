using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

/// <summary>
/// ボタン形式の設定項目UI
/// Submit操作でボタンクリックを実行
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSettingItem : MonoBehaviour, ISettingItemNavigatable
{
    private readonly Subject<(string settingName, object value)> _onValueChanged = new();
    private string _settingName;
    private Button _button;

    public Observable<(string settingName, object value)> OnValueChanged => _onValueChanged;

    /// <summary>
    /// 設定項目を初期化
    /// </summary>
    public void Initialize(string settingName, string buttonText)
    {
        _settingName = settingName;
        _button = GetComponent<Button>();

        // ボタンテキストの設定
        var textComponent = _button.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = buttonText;

        // ボタンのイベント設定
        _button.onClick.AddListener(OnButtonClicked);
    }

    public void OnNavigateHorizontal(float direction) { }
    public void OnSubmit() => OnButtonClicked();

    /// <summary>
    /// ボタンクリック時のハンドラー
    /// </summary>
    private void OnButtonClicked()
    {
        // クリックイベントを通知
        _onValueChanged.OnNext((_settingName, Unit.Default));
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
        _onValueChanged?.Dispose();
    }
}
