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
    [SerializeField] private AllCardData allCardData;
    [SerializeField] private ConfirmationDialogView confirmationDialogView;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // データの登録と初期化
        builder.RegisterInstance(allEnemyData);
        builder.RegisterInstance(allCardData);
        RegisterAllData();
        
        // カードプールサービス（GameProgressServiceが依存）
        builder.Register<CardPoolService>(Lifetime.Singleton);
        
        // セーブデータ管理
        builder.Register<SaveDataManager>(Lifetime.Singleton);
        
        // シーン遷移管理（クロスフェード機能付き）
        builder.Register<SceneTransitionManager>(Lifetime.Singleton);
        
        // ゲーム進行管理（全機能統合）
        builder.Register<GameProgressService>(Lifetime.Singleton);
        
        // その他の設定管理
        builder.Register<SettingsManager>(Lifetime.Singleton);
        builder.Register<ConfirmationDialogService>(Lifetime.Singleton)
            .WithParameter(confirmationDialogView);
        
        // Steam統合サービス
        builder.RegisterEntryPoint<SteamService>().AsSelf();
        // Discord統合サービス
        builder.Register<DiscordService>(Lifetime.Singleton);
        
        // サウンドマネージャーの初期化
        InitializeSoundManagers();
    }
    
    private void RegisterAllData()
    {
        #if UNITY_EDITOR
        allCardData.RegisterAllCards();
        allEnemyData.RegisterAllEnemies();
        #endif
    }
    
    private void InitializeSoundManagers()
    {
        DontDestroyOnLoad(Instantiate(bgmManager).gameObject);
        DontDestroyOnLoad(Instantiate(seManager).gameObject);
    }
}
