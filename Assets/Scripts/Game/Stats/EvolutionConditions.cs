using System;
using UnityEngine;

/// <summary>
/// 進化・劣化条件の基底クラス
/// </summary>
[Serializable]
public abstract class EvolutionConditionBase
{
    /// <summary>
    /// 条件を満たしているかチェック
    /// </summary>
    public abstract bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData);
    
    /// <summary>
    /// 条件の説明文を取得(エディタ表示用)
    /// </summary>
    public abstract string GetDescription();
    
    /// <summary>
    /// エディタ表示用の条件タイプ名(エディタ表示用)
    /// </summary>
    public abstract string GetConditionTypeName();
}

/// <summary>
/// プレイスタイル勝利条件
/// </summary>
[Serializable]
public class PlayStyleWinCondition : EvolutionConditionBase
{
    [Header("必要なプレイスタイル")]
    public PlayStyle requiredPlayStyle = PlayStyle.Impulse;
    
    [Header("必要勝利数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.GetPlayStyleWins(requiredPlayStyle) >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"{requiredPlayStyle.ToJapaneseString()}で{requiredCount.GetDisplayString()}回勝利";
    }
    
    public override string GetConditionTypeName() => "プレイスタイル勝利";
}

/// <summary>
/// プレイスタイル敗北条件
/// </summary>
[Serializable]
public class PlayStyleLoseCondition : EvolutionConditionBase
{
    [Header("必要なプレイスタイル")]
    public PlayStyle requiredPlayStyle = PlayStyle.Impulse;
    
    [Header("必要敗北数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.GetPlayStyleLosses(requiredPlayStyle) >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"{requiredPlayStyle.ToJapaneseString()}で{requiredCount.GetDisplayString()}回敗北";
    }
    
    public override string GetConditionTypeName() => "プレイスタイル敗北";
}

/// <summary>
/// 総勝利数条件
/// </summary>
[Serializable]
public class TotalWinCondition : EvolutionConditionBase
{
    [Header("必要勝利数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.TotalWin >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"総勝利回数{requiredCount.GetDisplayString()}回";
    }
    
    public override string GetConditionTypeName() => "総勝利数";
}

/// <summary>
/// 崩壊回数条件
/// </summary>
[Serializable]
public class CollapseCountCondition : EvolutionConditionBase
{
    [Header("必要崩壊数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.CollapseCount >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"崩壊回数{requiredCount.GetDisplayString()}回";
    }
    
    public override string GetConditionTypeName() => "崩壊回数";
}

/// <summary>
/// 連続勝利条件
/// </summary>
[Serializable]
public class ConsecutiveWinCondition : EvolutionConditionBase
{
    [Header("必要連続勝利数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.MaxConsecutiveWin >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"連続勝利{requiredCount.GetDisplayString()}回";
    }
    
    public override string GetConditionTypeName() => "連続勝利";
}

/// <summary>
/// 総使用回数条件
/// </summary>
[Serializable]
public class TotalUseCondition : EvolutionConditionBase
{
    [Header("必要使用回数")]
    public RandomRangeValue requiredCount = new(1);
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        return cardStats.TotalUse >= requiredCount.Value;
    }
    
    public override string GetDescription()
    {
        return $"総使用回数{requiredCount.GetDisplayString()}回";
    }
    
    public override string GetConditionTypeName() => "総使用回数";
}



/// <summary>
/// 勝率条件
/// </summary>
[Serializable]
public class WinRateCondition : EvolutionConditionBase
{
    [Header("必要勝率 (%)")]
    public RandomRangeFloat requiredWinRate = new(60f);
    
    [Header("最低試合数")]
    public int minimumGames = 5;
    
    public override bool IsSatisfied(CardStats cardStats, IEvolutionStatsData evolutionStatsData)
    {
        if (cardStats.TotalUse < minimumGames) return false;
        
        var winRate = cardStats.TotalWin / (float)cardStats.TotalUse * 100f;
        return winRate >= requiredWinRate.Value;
    }
    
    public override string GetDescription()
    {
        return $"勝率{requiredWinRate.GetDisplayString()}%以上（最低{minimumGames}試合）";
    }
    
    public override string GetConditionTypeName() => "勝率";
}