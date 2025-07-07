using System;
using System.Collections.Generic;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// プレイヤーの統計データを管理するクラス
/// </summary>
[Serializable]
public class PlayerStats
{
    [Header("全体統計")]
    [SerializeField] private int totalGames;
    [SerializeField] private int totalWins;
    [SerializeField] private int totalLosses;
    
    [Header("カード別統計")]
    [SerializeField] private SerializableDictionary<string, CardStats> cardStatsDict = new ();
    
    public int TotalGames => totalGames;
    public int TotalWins => totalWins;
    public int TotalLosses => totalLosses;
    public float WinRate => totalGames > 0 ? (float)totalWins / totalGames : 0f;
    
    /// <summary>
    /// 指定したカードの統計を取得（存在しなければ新規作成）
    /// </summary>
    /// <param name="cardId">カードID</param>
    /// <returns>カード統計</returns>
    public CardStats GetCardStats(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return new CardStats();
        
        if (!cardStatsDict.ContainsKey(cardId))
        {
            cardStatsDict[cardId] = new CardStats();
        }
        
        return cardStatsDict[cardId];
    }
    
    /// <summary>
    /// ゲーム結果を記録
    /// </summary>
    /// <param name="playerWon">プレイヤーが勝利したかどうか</param>
    /// <param name="playerMove">プレイヤーの手</param>
    /// <param name="playerCollapsed">プレイヤーのカードが崩壊したかどうか</param>
    public void RecordGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        if (!playerMove?.SelectedCard) return;
        
        // 全体統計を更新
        totalGames++;
        if (playerWon) totalWins++;
        else totalLosses++;
        
        // カード別統計を更新
        var cardStats = GetCardStats(playerMove.SelectedCard.CardId);
        cardStats.RecordUse();
        
        if (playerWon) cardStats.RecordWin(playerMove.PlayStyle);
        else cardStats.RecordLoss(playerMove.PlayStyle);
        
        if (playerCollapsed) cardStats.RecordCollapse();
    }
    
    /// <summary>
    /// 全ての進化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    public bool CheckAllEvolutionConditions(CardData cardData)
    {
        if (!cardData || !cardData.CanEvolve) return false;
        
        var cardStats = GetCardStats(cardData.CardId);
        
        // いずれかのグループの条件を全て満たしていればOK（OR条件）
        foreach (var group in cardData.EvolutionConditionGroups)
        {
            if (group.IsSatisfied(cardStats, this))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 全ての劣化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    public bool CheckAllDegradationConditions(CardData cardData)
    {
        if (!cardData || !cardData.CanDegrade) return false;
        
        var cardStats = GetCardStats(cardData.CardId);
        
        // いずれかのグループの条件を全て満たしていればOK（OR条件）
        foreach (var group in cardData.DegradationConditionGroups)
        {
            if (group.IsSatisfied(cardStats, this))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    public string GetStatsString()
    {
        return $"総ゲーム数: {totalGames}, 勝利: {totalWins}, 敗北: {totalLosses}, 勝率: {WinRate:P1}";
    }
}