using UnityEngine;

/// <summary>
/// 敵の進行を管理するサービスクラス
/// VContainerによりシングルトンとして管理される
/// </summary>
public class EnemyProgressService
{
    public bool IsAllEnemiesDefeated(int chapter) => chapter >= _allEnemyData.Count;
    
    private readonly AllEnemyData _allEnemyData;

    /// <summary>
    /// コンストラクタ（AllEnemyDataをDIで受け取る）
    /// </summary>
    /// <param name="allEnemyData">全敵データ</param>
    public EnemyProgressService(AllEnemyData allEnemyData)
    {
        _allEnemyData = allEnemyData;
        _allEnemyData.RegisterAllEnemies();
    }
    
    /// <summary>
    /// 指定チャプターの敵データを取得
    /// </summary>
    /// <param name="chapter">チャプター番号（0ベース）</param>
    /// <returns>指定チャプターの敵データ（範囲外の場合はnull）</returns>
    public EnemyData GetEnemyByChapter(int chapter)
    {
        if (chapter < 0 || chapter >= _allEnemyData.Count) return null;
        return _allEnemyData.GetEnemyByIndex(chapter);
    }
}