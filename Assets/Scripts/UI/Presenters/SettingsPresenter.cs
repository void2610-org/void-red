using System.Linq;
using R3;
using UnityEngine;
using VContainer;

/// <summary>
/// SettingsManagerとSettingsViewの橋渡しを行うPresenterクラス
/// MVPパターンに基づいてViewとModelを分離
/// </summary>
public class SettingsPresenter : MonoBehaviour
{
    [SerializeField] private SettingsView settingsView;
    
    private SettingsManager _settingsManager;
    private readonly CompositeDisposable _disposables = new();
    
    [Inject]
    public void Construct(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }
    
    private void Start()
    {
        // Viewのイベントを監視
        SubscribeToViewEvents();
        
        // 初期設定データをViewに注入
        RefreshSettingsView();
    }
    
    /// <summary>
    /// 設定画面を表示
    /// </summary>
    public void ShowSettings()
    {
        if (settingsView)
        {
            RefreshSettingsView(); // 最新データで更新
            settingsView.ShowSettings();
        }
    }
    
    /// <summary>
    /// 設定画面を非表示
    /// </summary>
    public void HideSettings()
    {
        settingsView?.HideSettings();
    }
    
    /// <summary>
    /// ViewのイベントをSettingsManagerに接続
    /// </summary>
    private void SubscribeToViewEvents()
    {
        // スライダー変更イベント
        settingsView.OnSliderChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<SliderSetting>(data.settingName);
                if (setting != null)
                {
                    setting.CurrentValue = data.value;
                    Debug.Log($"スライダー設定更新: {data.settingName} = {data.value}");
                }
            })
            .AddTo(_disposables);
        
        // 列挙型変更イベント
        settingsView.OnEnumChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<EnumSetting>(data.settingName);
                if (setting != null)
                {
                    setting.CurrentValue = data.value;
                    Debug.Log($"列挙型設定更新: {data.settingName} = {data.value}");
                }
            })
            .AddTo(_disposables);
        
        // ボタンクリックイベント
        settingsView.OnButtonClicked
            .Subscribe(settingName => {
                var setting = _settingsManager.GetSetting<ButtonSetting>(settingName);
                if (setting != null)
                {
                    setting.ExecuteAction();
                    Debug.Log($"ボタン設定実行: {settingName}");
                }
            })
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// SettingsManagerのデータをViewの形式に変換してViewに設定
    /// </summary>
    private void RefreshSettingsView()
    {
        var settingsData = _settingsManager.Settings
            .Select(ConvertToDisplayData)
            .ToArray();
        
        settingsView.SetSettings(settingsData);
    }
    
    /// <summary>
    /// ISettingBaseをSettingDisplayDataに変換
    /// </summary>
    private SettingsView.SettingDisplayData ConvertToDisplayData(ISettingBase setting)
    {
        var data = new SettingsView.SettingDisplayData
        {
            name = setting.SettingName,
            displayName = setting.SettingName
        };
        
        switch (setting)
        {
            case SliderSetting sliderSetting:
                data.type = SettingsView.SettingType.Slider;
                data.floatValue = sliderSetting.CurrentValue;
                data.minValue = sliderSetting.MinValue;
                data.maxValue = sliderSetting.MaxValue;
                break;
                
            case EnumSetting enumSetting:
                data.type = SettingsView.SettingType.Enum;
                data.stringValue = enumSetting.CurrentValue;
                data.options = enumSetting.Options;
                data.displayNames = enumSetting.DisplayNames;
                break;
                
            case ButtonSetting buttonSetting:
                data.type = SettingsView.SettingType.Button;
                data.buttonText = buttonSetting.ButtonText;
                data.requiresConfirmation = buttonSetting.RequiresConfirmation;
                data.confirmationMessage = buttonSetting.ConfirmationMessage;
                break;
        }
        
        return data;
    }
    
    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}