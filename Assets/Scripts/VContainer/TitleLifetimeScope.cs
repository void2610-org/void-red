using VContainer;
using VContainer.Unity;

/// <summary>
/// タイトルシーン用のLifetimeScope
/// タイトルシーン固有のコンポーネントを登録
/// </summary>
public class TitleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<SettingsPresenter>();
        builder.RegisterComponentInHierarchy<TitleView>();
        builder.RegisterEntryPoint<TitlePresenter>();
        builder.RegisterComponentInHierarchy<DebugController>();
    }
}
