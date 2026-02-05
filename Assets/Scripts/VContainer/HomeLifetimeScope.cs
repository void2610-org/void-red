using UnityEngine;
using VContainer;
using VContainer.Unity;

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

        builder.RegisterSettingsFeature();
        builder.RegisterEntryPoint<HelpPresenter>();

        // ホームPresenter
        builder.RegisterEntryPoint<HomePresenter>();
    }
}
