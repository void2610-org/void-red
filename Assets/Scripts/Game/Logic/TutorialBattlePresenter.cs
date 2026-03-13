/// <summary>
/// チュートリアル用のバトル進行を担当するPresenter
/// </summary>
public class TutorialBattlePresenter : BattlePresenter
{
    protected readonly TutorialBattlePlayerData TutorialBattlePlayerData;

    public TutorialBattlePresenter(
        BattleUIPresenter battleUIPresenter,
        Player player,
        Enemy enemy,
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllAuctionData allAuctionData,
        TutorialBattlePlayerData tutorialBattlePlayerData)
        : base(
            battleUIPresenter,
            player,
            enemy,
            gameProgressService,
            sceneTransitionManager,
            allAuctionData)
    {
        TutorialBattlePlayerData = tutorialBattlePlayerData;
        EnemyAI = new TutorialEnemyAIController(Enemy);
        CompetitionPhaseRunner = new CompetitionPhaseRunner(Player, EnemyAI, BattleUIPresenter);
        AuctionProcessor = new AuctionProcessor(Player, Enemy, BattleUIPresenter, CompetitionPhaseRunner);
    }
}
