using VContainer;
using VContainer.Unity;
using Void2610.SettingsSystem;

/// <summary>
/// タイトルシーン用のLifetimeScope
/// タイトルシーン固有のコンポーネントを登録
/// </summary>
public class TitleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 設定機能
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

        builder.RegisterComponentInHierarchy<TitleView>();
        builder.RegisterEntryPoint<TitlePresenter>();
        builder.RegisterComponentInHierarchy<DebugController>();
    }
}
