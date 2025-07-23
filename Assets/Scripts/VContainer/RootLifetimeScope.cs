using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ルートレベルのLifetimeScope
/// 複数シーンで共有されるサービス（設定管理など）を登録
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // === 全シーン共通のサービス登録 ===
        
        // 設定管理とセーブデータ管理
        builder.Register<SaveDataManager>(Lifetime.Singleton);
        builder.Register<SettingsManager>(Lifetime.Singleton);
        
        Debug.Log("RootLifetimeScope: 共通サービスを登録しました");
    }
}
