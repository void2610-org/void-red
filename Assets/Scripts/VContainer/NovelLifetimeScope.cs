using UnityEngine;
using VContainer;
using VContainer.Unity;
using VoidRed.Game.Services;
using Void2610.UnityTemplate;

/// <summary>
/// ノベルシーン用のLifetimeScope
/// ノベルシーン固有のコンポーネントを登録
/// </summary>
public class NovelLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 基礎サービスを登録
        builder.Register<SaveDataManager>(Lifetime.Singleton);
        builder.Register<CardPoolService>(Lifetime.Singleton);
        
        // ゲーム進行管理サービスを登録
        builder.Register<GameProgressService>(Lifetime.Singleton);
        
        // シーン遷移管理サービスを登録
        builder.Register<SceneTransitionManager>(Lifetime.Singleton);
        
        // ノベルシーン用サービスを登録
        builder.Register<NovelDialogService>(Lifetime.Singleton);
        
        // Addressables画像読み込みサービスを登録
        builder.Register<AddressableCharacterImageLoader>(Lifetime.Singleton);
        
        // UIプレゼンターを登録
        builder.RegisterComponentInHierarchy<NovelUIPresenter>();
    }
}
