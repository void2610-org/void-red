using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BattleLifetimeScope : LifetimeScope
{
    [SerializeField] private AllThemeData allThemeData;
    [SerializeField] private HandView playerHandView;
    [SerializeField] private HandView enemyHandView;
    
    private Player _player;
    private Enemy _enemy;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // GameProgressServiceを親コンテナから取得
        var gameProgressService = Parent.Container.Resolve<GameProgressService>();
        
        // Player Model・HandView の作成
        _player = new Player(playerHandView, gameProgressService, 3); // 最大手札数3
        builder.RegisterInstance(_player).AsSelf();
        
        // Enemy Model・HandView の作成
        _enemy = new Enemy(enemyHandView, gameProgressService, 3); // 最大手札数3
        builder.RegisterInstance(_enemy).AsSelf();
        
        builder.Register<CardNarrationService>(Lifetime.Singleton);
        builder.Register<PersonalityLogService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<VolumeController>();
        
        allThemeData.RegisterAllThemes();
        builder.RegisterInstance(allThemeData).AsSelf();

        builder.RegisterEntryPoint<BattlePresenter>().AsSelf().As<ISceneInitializable>();
        builder.RegisterEntryPoint<BattleUIPresenter>().AsSelf();
        builder.RegisterEntryPoint<PausePresenter>().AsSelf();
        builder.RegisterEntryPoint<MentalPowerEffectController>();
    }
}
