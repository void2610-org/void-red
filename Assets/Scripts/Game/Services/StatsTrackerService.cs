/// <summary>
/// プレイヤーと敵のStatsTrackerを管理するサービス
/// </summary>
public class StatsTrackerService
{
    private readonly PlayerSaveData _playerSaveData;
    private EnemyStats _enemyStats = new();
    private StatsTracker _playerTracker;
    private StatsTracker _enemyTracker;
    
    public StatsTrackerService(SaveDataManager saveDataManager)
    {
        _playerSaveData = saveDataManager.LoadPlayerData();
    }

    /// <summary>
    /// プレイヤー用StatsTrackerを取得
    /// </summary>
    public StatsTracker PlayerTracker
    {
        get
        {
            _playerTracker ??= new StatsTracker(_playerSaveData, "Player");
            return _playerTracker;
        }
    }
    
    /// <summary>
    /// 敵用StatsTrackerを取得
    /// </summary>
    public StatsTracker EnemyTracker
    {
        get
        {
            _enemyTracker ??= new StatsTracker(_enemyStats, "Enemy");
            return _enemyTracker;
        }
    }
    
    /// <summary>
    /// プレイヤーのセーブデータを直接取得（将来のセーブ機能用）
    /// </summary>
    public PlayerSaveData PlayerSaveData => _playerSaveData;
    
    /// <summary>
    /// 敵の統計をリセット（新しい敵との戦闘開始時に使用）
    /// </summary>
    public void ResetEnemyStats()
    {
        _enemyStats = new EnemyStats();
        _enemyTracker = null; // 次回アクセス時に新しいTrackerを作成
    }
}