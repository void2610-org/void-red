using VContainer;
using VContainer.Unity;
using Void2610.SettingsSystem;

/// <summary>
/// IContainerBuilderの拡張メソッド
/// 共通の登録パターンをまとめる
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// 設定機能を登録
    /// </summary>
    public static void RegisterSettingsFeature(this IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<SettingsPresenter>().AsSelf();
        builder.RegisterComponentInHierarchy<SettingsWindowView>();
        builder.RegisterComponentInHierarchy<SettingButtonView>();
        builder.RegisterBuildCallback(container =>
        {
            var windowView = container.Resolve<SettingsWindowView>();
            var presenter = container.Resolve<SettingsPresenter>();
            var inputProvider = container.Resolve<InputActionsProvider>();
            var settingButton = container.Resolve<SettingButtonView>();
            windowView.Initialize(presenter, inputProvider, settingButton);
        });
    }
}
