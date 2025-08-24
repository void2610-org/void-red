using UnityEngine;
using VContainer;
using VContainer.Unity;
using Void2610.UnityTemplate;

/// <summary>
/// ルートレベルのLifetimeScope
/// 複数シーンで共有されるサービス（設定管理など）を登録
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private BgmManager bgmManager;
    [SerializeField] private SeManager seManager;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 設定管理とセーブデータ管理
        builder.Register<PersonalityLogService>(Lifetime.Singleton);
        builder.Register<SaveDataManager>(Lifetime.Singleton);
        builder.Register<SettingsManager>(Lifetime.Singleton);
        
        // シーン遷移管理
        builder.Register<SceneTransitionService>(Lifetime.Singleton);
        
        // サウンドマネージャーの初期化
        InitializeSoundManagers();
    }
    
    private void InitializeSoundManagers()
    {
        DontDestroyOnLoad(Instantiate(bgmManager).gameObject);
        DontDestroyOnLoad(Instantiate(seManager).gameObject);
    }
}
