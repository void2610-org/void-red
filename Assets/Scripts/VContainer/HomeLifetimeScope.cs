using VContainer;
using VContainer.Unity;
using UnityEngine;
using Void2610.SettingsSystem;

/// <summary>
/// ホームシーン用のLifetimeScope
/// ホームシーン固有のコンポーネントを登録
/// </summary>
public class HomeLifetimeScope : LifetimeScope
{
    [SerializeField] private AllCardData allCardData;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<HomeView>();
        builder.RegisterInstance(allCardData);
        builder.Register<CardPoolService>(Lifetime.Singleton);

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
        builder.RegisterEntryPoint<HelpPresenter>();

        // ホームPresenter
        builder.RegisterEntryPoint<HomePresenter>();
    }
}
