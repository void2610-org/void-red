using R3;
using UnityEngine;
using Void2610.SettingsSystem;

/// <summary>
/// 設定ウィンドウのラッパー
/// BaseWindowViewを継承し、入力イベントとSettingsPresenterのイベントでShow/Hideを制御
/// </summary>
public class SettingsWindowView : BaseWindowView
{
    [SerializeField] private SettingsView settingsView;

    private SettingsPresenter _settingsPresenter;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(SettingsPresenter settingsPresenter, InputActionsProvider inputProvider, SettingButtonView settingButtonView)
    {
        _settingsPresenter = settingsPresenter;

        // トグル入力（Pauseキー）
        inputProvider.UI.Pause.OnPerformedAsObservable()
            .Subscribe(_ =>
            {
                if (IsShowing) Hide();
                else Show();
            })
            .AddTo(Disposables);

        // 設定ボタンクリック
        settingButtonView.OnButtonClicked
            .Subscribe(_ => Show())
            .AddTo(Disposables);

        // 閉じるボタン（SettingsPresenter経由）
        _settingsPresenter.OnHideRequested
            .Subscribe(_ => Hide())
            .AddTo(Disposables);
    }

    protected override GameObject GetPreferredNavigationTarget()
    {
        // 最初の設定項目を優先、なければcloseButton
        return settingsView.FirstSettingItem ?? base.GetPreferredNavigationTarget();
    }
}
