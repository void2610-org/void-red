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
    [SerializeField] private AllThemeData allThemeData;
    [SerializeField] private AllCardData allCardData;
    [SerializeField] private AllTutorialData allTutorialData;
    [SerializeField] private AllHelpData allHelpData;
    [SerializeField] private ConfirmationDialogView confirmationDialogView;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // データの登録と初期化
        builder.RegisterInstance(allEnemyData);
        builder.RegisterInstance(allThemeData);
        builder.RegisterInstance(allCardData);
        builder.RegisterInstance(allTutorialData);
        builder.RegisterInstance(allHelpData);
        RegisterAllData();

        // カードプールサービス（GameProgressServiceが依存）
        builder.Register<CardPoolService>(Lifetime.Singleton);

        // セーブデータ管理
        builder.Register<SaveDataManager>(Lifetime.Singleton);

        // シーン遷移管理（クロスフェード機能付き）
        builder.Register<SceneTransitionManager>(Lifetime.Singleton);

        // ゲーム進行管理（全機能統合）
        builder.Register<GameProgressService>(Lifetime.Singleton);

        // InputSystem管理
        builder.Register<InputActionsProvider>(Lifetime.Singleton);

        // UIナビゲーション管理
        builder.RegisterEntryPoint<MouseHoverUISelector>();
        builder.RegisterEntryPoint<SafeNavigationManager>();

        // その他の設定管理
        builder.Register<SettingsManager>(Lifetime.Singleton);
        builder.RegisterEntryPoint<HelpPresenter>();
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
        allThemeData.RegisterAllThemes();
        allEnemyData.RegisterAllEnemies();
        allTutorialData.RegisterAllTutorials();
        allHelpData.RegisterAllHelps();
        #endif
    }
    
    private void InitializeSoundManagers()
    {
        DontDestroyOnLoad(Instantiate(bgmManager).gameObject);
        DontDestroyOnLoad(Instantiate(seManager).gameObject);
    }
}
