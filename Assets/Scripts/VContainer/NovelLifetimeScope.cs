using UnityEngine;
using VContainer;
using VContainer.Unity;
using VoidRed.Game.Services;

/// <summary>
/// ノベルシーン用のLifetimeScope
/// ノベルシーン固有のコンポーネントを登録
/// </summary>
public class NovelLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // ノベルシーン用サービスを登録
        builder.Register<CardDialogService>(Lifetime.Singleton);
        
        // UIプレゼンターを登録
        builder.RegisterComponentInHierarchy<NovelUIPresenter>();
    }
}
