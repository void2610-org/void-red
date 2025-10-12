using System;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Object = UnityEngine.Object;

/// <summary>
/// SettingsManagerとSettingsViewの橋渡しを行うPresenterクラス
/// MVPパターンに基づいてViewとModelを分離
/// </summary>
public class SettingsPresenter : IStartable, IDisposable
{
    private SettingsView _settingsView;
    private SettingButtonView _settingButtonView;
    private readonly SettingsManager _settingsManager;
    private readonly ConfirmationDialogService _confirmationDialogService;
    private readonly InputActionsProvider _inputActionsProvider;
    private readonly CompositeDisposable _disposables = new();

    public SettingsPresenter(
        SettingsManager settingsManager,
        ConfirmationDialogService confirmationDialogService,
        InputActionsProvider inputActionsProvider)
    {
        _settingsManager = settingsManager;
        _confirmationDialogService = confirmationDialogService;
        _inputActionsProvider = inputActionsProvider;
    }

    public void Start()
    {
        // ビューの取得
        _settingsView = Object.FindFirstObjectByType<SettingsView>();
        _settingButtonView = Object.FindFirstObjectByType<SettingButtonView>();

        // Pauseアクションの購読
        _inputActionsProvider.UI.Pause.performed += OnPauseActionPerformed;
        _inputActionsProvider.UI.Enable();

        // 設定ボタンのイベント設定
        if (_settingButtonView != null)
        {
            _settingButtonView.OnButtonClicked.Subscribe(
                _ => ShowSettings())
                .AddTo(_disposables);
        }

        SubscribeToViewEvents();
        RefreshSettingsView();
        HideSettings(); // 初期状態では非表示にする
    }

    private void OnPauseActionPerformed(InputAction.CallbackContext context)
    {
        ToggleSettings();
    }

    private void ToggleSettings()
    {
        if (_settingsView.IsShowing)
        {
            HideSettings();
        }
        else
        {
            ShowSettings();
        }
    }
    
    /// <summary>
    /// 設定画面を表示
    /// </summary>
    public void ShowSettings()
    {
        RefreshSettingsView(); // 最新データで更新
        _settingsView.ShowSettings();
    }
    
    /// <summary>
    /// 設定画面を非表示
    /// </summary>
    public void HideSettings()
    {
        _settingsView.HideSettings();
    }
    
    /// <summary>
    /// ViewのイベントをSettingsManagerに接続
    /// </summary>
    private void SubscribeToViewEvents()
    {
        // スライダー変更イベント
        _settingsView.OnSliderChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<SliderSetting>(data.settingName);
                if (setting != null) setting.CurrentValue = data.value;
            })
            .AddTo(_disposables);
        
        // 列挙型変更イベント
        _settingsView.OnEnumChanged
            .Subscribe(data => {
                var setting = _settingsManager.GetSetting<EnumSetting>(data.settingName);
                if (setting != null) setting.CurrentValue = data.value;
            })
            .AddTo(_disposables);
        
        // ボタンクリックイベント
        _settingsView.OnButtonClicked
            .Subscribe(settingName => {
                var setting = _settingsManager.GetSetting<ButtonSetting>(settingName);
                setting?.ExecuteAction();
            })
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// SettingsManagerのデータをViewの形式に変換してViewに設定
    /// </summary>
    private void RefreshSettingsView()
    {
        var settingsData = _settingsManager.Settings.Select(ConvertToDisplayData).ToArray();
        _settingsView.SetSettings(settingsData, _confirmationDialogService);
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
    
    public void Dispose()
    {
        // Pauseアクションの購読解除
        _inputActionsProvider.UI.Pause.performed -= OnPauseActionPerformed;
        _disposables?.Dispose();
    }
}