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
    public List<EvolutionConditionBase> Conditions = new();
    
    /// <summary>
    /// このグループの全ての条件を満たしているかチェック
    /// </summary>
    public bool IsSatisfied(CardStats cardStats, PlayerStats playerStats)
    {
        if (Conditions == null || Conditions.Count == 0) return true;
        
        foreach (var condition in Conditions)
        {
            if (condition == null || !condition.IsSatisfied(cardStats, playerStats))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// グループの説明文を取得
    /// </summary>
    public string GetDescription()
    {
        if (Conditions == null || Conditions.Count == 0) return "条件なし";
        
        if (Conditions.Count == 1)
            return Conditions[0]?.GetDescription() ?? "不明";
        
        var descriptions = new List<string>();
        foreach (var condition in Conditions)
        {
            if (condition != null)
                descriptions.Add(condition.GetDescription());
        }
        
        return string.Join(" かつ ", descriptions);
    }
    
    /// <summary>
    /// 条件数を取得
    /// </summary>
    public int ConditionCount => Conditions?.Count ?? 0;
}