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
    [SerializeField] private AllEnemyData allEnemyData;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // データの登録
        builder.RegisterInstance(allEnemyData);
        
        // セーブデータ管理（最初に登録）
        builder.Register<SaveDataManager>(Lifetime.Singleton);
        
        // PersonalityLogService（SaveDataManagerに依存）
        builder.Register<PersonalityLogService>(Lifetime.Singleton);
        
        // GameStatsService（SaveDataManagerとPersonalityLogServiceに依存）
        builder.Register<GameStatsService>(Lifetime.Singleton);
        
        // GameProgressService（すべてのサービスに依存、最後に登録）
        builder.Register<GameProgressService>(Lifetime.Singleton);
        
        // その他の設定管理
        builder.Register<SettingsManager>(Lifetime.Singleton);
        
        // サウンドマネージャーの初期化
        InitializeSoundManagers();
    }
    
    private void InitializeSoundManagers()
    {
        DontDestroyOnLoad(Instantiate(bgmManager).gameObject);
        DontDestroyOnLoad(Instantiate(seManager).gameObject);
    }
}
