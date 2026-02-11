using VContainer;
using VContainer.Unity;

/// <summary>
/// 展示モード感謝画面のLifetimeScope
/// </summary>
public class ThanksLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ThanksPresenter>();
    }
}
