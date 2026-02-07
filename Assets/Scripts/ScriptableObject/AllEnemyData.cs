using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
#endif

/// <summary>
/// 全ての敵データを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllEnemyData", menuName = "VoidRed/All Enemy Data")]
public class AllEnemyData : ScriptableObject
{
    [SerializeField] private List<EnemyData> enemyList = new();

    // プロパティ
    public List<EnemyData> EnemyList => enemyList;
    public int Count => enemyList.Count;

    /// <summary>
    /// 敵IDで敵を取得
    /// </summary>
    /// <param name="enemyId">敵ID</param>
    /// <returns>指定されたIDの敵データ</returns>
    public EnemyData GetEnemyById(string enemyId) => enemyList.FirstOrDefault(enemy => enemy.EnemyId == enemyId);

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
}
