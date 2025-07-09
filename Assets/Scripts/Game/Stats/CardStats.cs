using System;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// カード毎の統計データ
/// </summary>
[Serializable]
public class CardStats
{
    [SerializeField] private int totalUse;
    [SerializeField] private int totalWin;
    [SerializeField] private int totalLoss;
    [SerializeField] private int collapseCount;
    [SerializeField] private int currentConsecutiveWin;
    [SerializeField] private int maxConsecutiveWin;
    
    // プレイスタイル別勝利回数（拡張性のため辞書で管理）
    [SerializeField] private SerializableDictionary<PlayStyle, int> playStyleWins = new();
    // プレイスタイル別敗北回数
    [SerializeField] private SerializableDictionary<PlayStyle, int> playStyleLosses = new();
    
    public int TotalUse => totalUse;
    public int TotalWin => totalWin;
    public int TotalLoss => totalLoss;
    public int CollapseCount => collapseCount;
    public int CurrentConsecutiveWin => currentConsecutiveWin;
    public int MaxConsecutiveWin => maxConsecutiveWin;
    
    public int GetPlayStyleWins(PlayStyle playStyle)
    {
        return playStyleWins.TryGetValue(playStyle, out var wins) ? wins : 0;
    }
    
    public int GetPlayStyleLosses(PlayStyle playStyle)
    {
        return playStyleLosses.TryGetValue(playStyle, out var losses) ? losses : 0;
    }
    
    /// <summary>
    /// カード使用を記録
    /// </summary>
    public void RecordUse()
    {
        totalUse++;
    }
    
    /// <summary>
    /// 勝利を記録
    /// </summary>
    public void RecordWin(PlayStyle playStyle)
    {
        totalWin++;
        currentConsecutiveWin++;
        if (currentConsecutiveWin > maxConsecutiveWin)
            maxConsecutiveWin = currentConsecutiveWin;
        
        // プレイスタイル別勝利回数を更新
        if (!playStyleWins.ContainsKey(playStyle))
            playStyleWins[playStyle] = 0;
        playStyleWins[playStyle]++;
    }
    
    /// <summary>
    /// 敗北を記録
    /// </summary>
    public void RecordLoss(PlayStyle playStyle)
    {
        totalLoss++;
        currentConsecutiveWin = 0;
        
        // プレイスタイル別敗北回数を更新
        if (!playStyleLosses.ContainsKey(playStyle))
            playStyleLosses[playStyle] = 0;
        playStyleLosses[playStyle]++;
    }
    
    /// <summary>
    /// 崩壊を記録
    /// </summary>
    public void RecordCollapse()
    {
        collapseCount++;
    }
}