using UnityEngine;

/// <summary>
/// 敵の進行を管理するサービスクラス
/// VContainerによりシングルトンとして管理される
/// </summary>
public class EnemyProgressService
{
    public int CurrentEnemyIndex { get; private set; } = 0;

    public int TotalEnemyCount => _allEnemyData.Count;
    private bool IsAllEnemiesDefeated => CurrentEnemyIndex >= _allEnemyData.Count;
    
    private readonly AllEnemyData _allEnemyData;

    /// <summary>
    /// コンストラクタ（AllEnemyDataをDIで受け取る）
    /// </summary>
    /// <param name="allEnemyData">全敵データ</param>
    public EnemyProgressService(AllEnemyData allEnemyData)
    {
        _allEnemyData = allEnemyData;
        _allEnemyData.RegisterAllEnemies();
        CurrentEnemyIndex = 0;
    }
    
    /// <summary>
    /// 現在の敵データを取得
    /// </summary>
    /// <returns>現在の敵データ（全て倒した場合はnull）</returns>
    public EnemyData GetCurrentEnemy()
    {
        if (IsAllEnemiesDefeated) return null;
        return _allEnemyData.GetEnemyByIndex(CurrentEnemyIndex);
    }
    
    /// <summary>
    /// 次の敵に進む
    /// </summary>
    /// <returns>次の敵データ（全て倒した場合はnull）</returns>
    public EnemyData AdvanceToNextEnemy()
    {
        if (IsAllEnemiesDefeated) return null;
        CurrentEnemyIndex++;
        return GetCurrentEnemy();
    }
}