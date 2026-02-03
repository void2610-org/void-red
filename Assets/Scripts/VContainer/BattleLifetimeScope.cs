using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BattleLifetimeScope : LifetimeScope
{
    [SerializeField] private AllAuctionData allAuctionData;

    private Player _player;
    private Enemy _enemy;

    protected override void Configure(IContainerBuilder builder)
    {
        // GameProgressServiceを親コンテナから取得
        var gameProgressService = Parent.Container.Resolve<GameProgressService>();

        // 全オークションデータを登録
        builder.RegisterInstance(allAuctionData);

        // Player Model・HandView の作成
        _player = new Player(gameProgressService);
        builder.RegisterInstance(_player).AsSelf();
        
        // Enemy Model・HandView の作成
        _enemy = new Enemy(gameProgressService);
        builder.RegisterInstance(_enemy).AsSelf();
        
        builder.RegisterEntryPoint<BattlePresenter>().AsSelf().As<ISceneInitializable>();
        builder.RegisterEntryPoint<BattleUIPresenter>().AsSelf();
        builder.RegisterEntryPoint<PausePresenter>().AsSelf();
        builder.RegisterEntryPoint<MentalPowerEffectController>();
        
        builder.RegisterEntryPoint<HelpPresenter>();

        // Settings機能を登録（バトル用：SettingButtonView無し）
        builder.RegisterSettingsFeatureForBattle();
    }
}
