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
        builder.RegisterSettingsFeature();

        builder.RegisterComponentInHierarchy<TitleView>();
        builder.RegisterEntryPoint<TitlePresenter>();
        builder.RegisterComponentInHierarchy<DebugController>();

        // 展示モード: タイトルPV
        builder.RegisterComponentInHierarchy<TitlePVView>();
        builder.RegisterEntryPoint<TitleIdlePVPresenter>();
    }
}
