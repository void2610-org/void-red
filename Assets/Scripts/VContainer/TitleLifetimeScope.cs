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
        builder.RegisterSettingsFeature();

        builder.RegisterComponentInHierarchy<TitleView>();
        builder.RegisterEntryPoint<TitlePresenter>();
        builder.RegisterComponentInHierarchy<DebugController>();
    }
}
