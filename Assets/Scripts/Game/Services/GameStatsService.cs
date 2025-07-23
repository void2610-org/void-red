/// <summary>
/// プレイヤーと敵の統計データを管理するサービス
/// </summary>
public class GameStatsService
{
    public GameStatsService(SaveDataManager saveDataManager)
    {
        PlayerSaveData = saveDataManager.LoadPlayerData();
    }
    
    public EnemyStats EnemyStats { get; private set; } = new();
    public PlayerSaveData PlayerSaveData { get; }

    /// <summary>
    /// 敵の統計をリセット（新しい敵との戦闘開始時に使用）
    /// </summary>
    public void ResetEnemyStats()
    {
        EnemyStats = new EnemyStats();
    }
}