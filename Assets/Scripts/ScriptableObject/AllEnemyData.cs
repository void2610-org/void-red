using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
#endif

/// <summary>
/// 全ての敵データを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllEnemyData", menuName = "VoidRed/All Enemy Data")]
public class AllEnemyData : ScriptableObject
{
    [SerializeField] private List<EnemyData> enemyList = new ();
    
    // プロパティ
    public List<EnemyData> EnemyList => enemyList;
    public int Count => enemyList.Count;
    
    /// <summary>
    /// 同じディレクトリ内の全ての敵データを自動的に登録
    /// </summary>
    public void RegisterAllEnemies()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(enemyList, x => x.EnemyId);
#endif
    }
    
    /// <summary>
    /// インデックスで敵を取得（ストーリー進行用）
    /// </summary>
    /// <param name="index">敵のインデックス</param>
    /// <returns>指定されたインデックスの敵データ</returns>
    public EnemyData GetEnemyByIndex(int index)
    {
        if (index < 0 || index >= enemyList.Count) return null;
        return enemyList[index];
    }
    
    /// <summary>
    /// 敵IDで敵を取得
    /// </summary>
    /// <param name="enemyId">敵ID</param>
    /// <returns>指定されたIDの敵データ</returns>
    public EnemyData GetEnemyById(string enemyId)
    {
        return enemyList.FirstOrDefault(enemy => enemy.EnemyId == enemyId);
    }
    
    /// <summary>
    /// 敵名で敵を取得
    /// </summary>
    /// <param name="enemyName">敵名</param>
    /// <returns>指定された名前の敵データ</returns>
    public EnemyData GetEnemyByName(string enemyName)
    {
        return enemyList.FirstOrDefault(enemy => enemy.EnemyName == enemyName);
    }
    
    /// <summary>
    /// 次の敵を取得（ストーリー進行用）
    /// </summary>
    /// <param name="currentEnemy">現在の敵</param>
    /// <returns>次の敵データ（最後の敵の場合はnull）</returns>
    public EnemyData GetNextEnemy(EnemyData currentEnemy)
    {
        if (!currentEnemy) return GetEnemyByIndex(0);
        
        var currentIndex = enemyList.IndexOf(currentEnemy);
        if (currentIndex < 0 || currentIndex >= enemyList.Count - 1) return null;
        
        return enemyList[currentIndex + 1];
    }
    
    /// <summary>
    /// 前の敵を取得
    /// </summary>
    /// <param name="currentEnemy">現在の敵</param>
    /// <returns>前の敵データ（最初の敵の場合はnull）</returns>
    public EnemyData GetPreviousEnemy(EnemyData currentEnemy)
    {
        if (!currentEnemy) return null;
        
        var currentIndex = enemyList.IndexOf(currentEnemy);
        if (currentIndex <= 0) return null;
        
        return enemyList[currentIndex - 1];
    }
    
    /// <summary>
    /// 指定した敵が最後の敵かどうかを確認
    /// </summary>
    /// <param name="enemy">敵データ</param>
    /// <returns>最後の敵かどうか</returns>
    public bool IsLastEnemy(EnemyData enemy)
    {
        if (!enemy || enemyList.Count == 0) return false;
        return enemyList.LastOrDefault() == enemy;
    }
    
    /// <summary>
    /// 指定した敵が最初の敵かどうかを確認
    /// </summary>
    /// <param name="enemy">敵データ</param>
    /// <returns>最初の敵かどうか</returns>
    public bool IsFirstEnemy(EnemyData enemy)
    {
        if (!enemy || enemyList.Count == 0) return false;
        return enemyList.FirstOrDefault() == enemy;
    }
}