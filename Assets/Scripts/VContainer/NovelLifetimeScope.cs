using VContainer;
using VContainer.Unity;

/// <summary>
/// ノベルシーン用のLifetimeScope
/// ノベルシーン固有のコンポーネントを登録
/// </summary>
public class NovelLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // UI Presenter
        builder.RegisterComponentInHierarchy<NovelUIPresenter>();
        builder.RegisterEntryPoint<PausePresenter>().AsSelf();
        
        // Excel読み込み関連サービス
        builder.Register<ExcelDialogLoader>(Lifetime.Singleton);
    }
}
