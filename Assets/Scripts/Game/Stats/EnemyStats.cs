using System;
using UnityEngine;

/// <summary>
/// 敵用の簡略統計データ（進化機能に必要な最小限のデータのみ）
/// </summary>
[Serializable]
public class EnemyStats : IEvolutionStatsData
{
    [Header("進化統計データ")]
    [SerializeField] private EvolutionStatsData evolutionStatsData = new EvolutionStatsData();
    
    // EvolutionStatsDataの主要プロパティのラッパー
    public int TotalGames => evolutionStatsData.TotalGames;
    public int TotalWins => evolutionStatsData.TotalWins;
    public int TotalLosses => evolutionStatsData.TotalLosses;
    public float WinRate => evolutionStatsData.WinRate;
    
    /// <summary>
    /// 指定したカードの統計を取得
    /// </summary>
    /// <param name="cardId">カードID</param>
    /// <returns>カード統計</returns>
    public CardStats GetCardStats(string cardId)
    {
        return evolutionStatsData.GetCardStats(cardId);
    }
    
    /// <summary>
    /// ゲーム結果を記録
    /// </summary>
    /// <param name="enemyWon">敵が勝利したかどうか</param>
    /// <param name="enemyMove">敵の手</param>
    /// <param name="enemyCollapsed">敵のカードが崩壊したかどうか</param>
    public void RecordGameResult(bool enemyWon, PlayerMove enemyMove, bool enemyCollapsed)
    {
        evolutionStatsData.RecordGameResult(enemyWon, enemyMove, enemyCollapsed);
    }
    
    /// <summary>
    /// 全ての進化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    public bool CheckAllEvolutionConditions(CardData cardData)
    {
        return evolutionStatsData.CheckAllEvolutionConditions(cardData);
    }
    
    /// <summary>
    /// 全ての劣化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    public bool CheckAllDegradationConditions(CardData cardData)
    {
        return evolutionStatsData.CheckAllDegradationConditions(cardData);
    }
    
    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    public string GetStatsString()
    {
        return evolutionStatsData.GetStatsString();
    }
}