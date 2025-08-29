/// <summary>
/// NPCクラス
/// AIによって制御される対戦相手を表す
/// </summary>
public class Enemy : PlayerPresenter
{
    public Enemy(HandView handView, GameProgressService gameProgressService = null, int maxHandSize = 3) 
        : base(handView, gameProgressService, maxHandSize) { }
}