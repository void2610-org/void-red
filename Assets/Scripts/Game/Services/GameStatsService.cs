using UnityEngine;

/// <summary>
/// プレイヤーと敵の統計データを管理するサービス
/// </summary>
public class GameStatsService
{
    private readonly SaveDataManager _saveDataManager;
    
    public GameStatsService(SaveDataManager saveDataManager)
    {
        _saveDataManager = saveDataManager;
        PlayerSaveData = _saveDataManager.LoadPlayerData();
    }
    
    public EnemyStats EnemyStats { get; private set; } = new();
    public PlayerSaveData PlayerSaveData { get; private set; }

    /// <summary>
    /// 敵の統計をリセット（新しい敵との戦闘開始時に使用）
    /// </summary>
    public void ResetEnemyStats()
    {
        EnemyStats = new EnemyStats();
    }
    
    /// <summary>
    /// プレイヤーセーブデータを再読み込み（デバッグ用）
    /// </summary>
    public void ReloadPlayerSaveData()
    {
        PlayerSaveData = _saveDataManager.LoadPlayerData();
        Debug.Log($"[GameStatsService] プレイヤーセーブデータを再読み込み: {PlayerSaveData.GetStatsString()}");
    }
}