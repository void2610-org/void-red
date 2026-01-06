using System.Linq;
using Void2610.UnityTemplate;

/// <summary>
/// NPCクラス
/// AIによって制御される対戦相手を表す
/// </summary>
public class Enemy : PlayerPresenter
{
    private EnemyData _data;
    
    public Enemy(HandView handView, GameProgressService gameProgressService = null, int maxHandSize = 3) : base(handView, gameProgressService, maxHandSize) { }
    
    public void SetEnemyData(EnemyData data) => _data = data;

    public PlayStyle DecidePlayStyle()
    {
        var all = _data.PlaystyleWeights.Keys.ToList();
        return all.ChooseByWeight(p => _data.PlaystyleWeights[p]);
    }
    
    /// <summary>
    /// 敵はセーブしない（オーバーライドして無効化）
    /// </summary>
    public override void SaveDeckChanges()
    {
        // 敵のデッキ変更はセーブデータに影響しない
    }
}