using System.Linq;
using Void2610.UnityTemplate;

/// <summary>
/// NPCクラス
/// AIによって制御される対戦相手を表す
/// </summary>
public class Enemy : PlayerPresenter
{
    private EnemyData _data;
    
    public Enemy(GameProgressService gameProgressService = null) : base(gameProgressService) { }
    
    public void SetEnemyData(EnemyData data) => _data = data;
}