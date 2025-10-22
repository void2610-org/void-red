using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ノベルシーン用のLifetimeScope
/// ノベルシーン固有のコンポーネントを登録
/// </summary>
public class NovelLifetimeScope : LifetimeScope
{
    [SerializeField] private NovelSeManager novelSeManager;
    
    [SerializeField] private bool useLocalExcel = true; // trueでExcel、falseでスプレッドシート（useAlphaHardcode=falseの時に有効）
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(novelSeManager);
        
        builder.RegisterEntryPoint<NovelPresenter>().WithParameter(useLocalExcel).AsSelf();
        builder.RegisterEntryPoint<PausePresenter>().AsSelf();
    }
}
