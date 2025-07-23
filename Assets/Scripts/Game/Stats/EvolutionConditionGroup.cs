using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 進化条件グループ（グループ内の条件は全てAND条件）
/// </summary>
[Serializable]
public class EvolutionConditionGroup
{
    [Header("条件リスト（全て満たす必要がある）")]
    [SerializeReference, SubclassSelector]
    public List<EvolutionConditionBase> conditions = new();
    
    /// <summary>
    /// このグループの全ての条件を満たしているかチェック
    /// </summary>
    public bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        if (conditions == null || conditions.Count == 0) return true;
        
        foreach (var condition in conditions)
        {
            if (condition == null || !condition.IsSatisfied(cardStats, evolutionStatsData))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// グループの説明文を取得
    /// </summary>
    public string GetDescription()
    {
        if (conditions == null || conditions.Count == 0) return "条件なし";
        
        if (conditions.Count == 1)
            return conditions[0]?.GetDescription() ?? "不明";
        
        var descriptions = new List<string>();
        foreach (var condition in conditions)
        {
            if (condition != null)
                descriptions.Add(condition.GetDescription());
        }
        
        return string.Join(" かつ ", descriptions);
    }
    
    /// <summary>
    /// 条件数を取得
    /// </summary>
    public int ConditionCount => conditions?.Count ?? 0;
}