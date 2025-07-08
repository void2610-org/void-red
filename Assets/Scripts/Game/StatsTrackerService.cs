using System.Collections.Generic;

/// <summary>
/// プレイヤーと敵のStatsTrackerを管理するサービス
/// </summary>
public class StatsTrackerService
{
    private readonly Dictionary<string, StatsTracker> _trackers = new();
    
    /// <summary>
    /// 指定したIDのStatsTrackerを取得（存在しない場合は作成）
    /// </summary>
    public StatsTracker GetTracker(string ownerId)
    {
        if (!_trackers.ContainsKey(ownerId))
        {
            _trackers[ownerId] = new StatsTracker(ownerId);
        }
        return _trackers[ownerId];
    }
    
    /// <summary>
    /// プレイヤー用StatsTrackerを取得
    /// </summary>
    public StatsTracker PlayerTracker => GetTracker("Player");
    
    /// <summary>
    /// 敵用StatsTrackerを取得
    /// </summary>
    public StatsTracker EnemyTracker => GetTracker("Enemy");
}