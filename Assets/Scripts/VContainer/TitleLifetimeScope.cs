using UnityEngine;
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
        // === タイトルシーン固有のコンポーネント登録 ===
        
        // === UI Presentersの登録 ===
        builder.RegisterComponentInHierarchy<SettingsPresenter>();
        
        // TODO: タイトルシーン固有の他のコンポーネントがあれば追加
        // builder.RegisterComponentInHierarchy<TitleMenuView>();
        
        Debug.Log("TitleLifetimeScope: タイトルシーン用コンポーネントを登録しました");
    }
}
