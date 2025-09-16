using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BattleLifetimeScope : LifetimeScope
{
    [SerializeField] private AllThemeData allThemeData;
    [SerializeField] private HandView playerHandView;
    [SerializeField] private HandView enemyHandView;
    [SerializeField] private TutorialData tutorialData;
    
    private Player _player;
    private Enemy _enemy;
    
    private void RegisterAllData()
    {
        #if UNITY_EDITOR
        allThemeData.RegisterAllThemes();
        #endif
    }
    
    protected override void Configure(IContainerBuilder builder)
    {
        // === プレイヤーの初期化（2層構造） ===
        
        // GameProgressServiceを親コンテナから取得
        var gameProgressService = Parent.Container.Resolve<GameProgressService>();
        
        // Player Model・HandView の作成
        _player = new Player(playerHandView, gameProgressService, 3); // 最大手札数3
        builder.RegisterInstance(_player).AsSelf();
        
        // Enemy Model・HandView の作成
        _enemy = new Enemy(enemyHandView, gameProgressService, 3); // 最大手札数3
        builder.RegisterInstance(_enemy).AsSelf();
        
        // === データとサービスの登録 ===
        
        builder.RegisterInstance(allThemeData);
        RegisterAllData();
        builder.Register<ThemeService>(Lifetime.Singleton);
        builder.Register<CardNarrationService>(Lifetime.Singleton);
        
        
        // === チュートリアルデータの登録 ===
        
        builder.RegisterInstance(tutorialData);
        
        // === エントリーポイントとPresenterの登録 ===
        
        builder.RegisterEntryPoint<UIPresenter>().AsSelf();
        builder.RegisterEntryPoint<PausePresenter>().AsSelf();
        builder.RegisterEntryPoint<GameManager>();
    }
}
