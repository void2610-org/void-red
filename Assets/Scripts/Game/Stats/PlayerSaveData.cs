using System;
using UnityEngine;

/// <summary>
/// プレイヤー固有のセーブデータ（統計データ + 将来の章クリア状況等）
/// </summary>
[Serializable]
public class PlayerSaveData : IEvolutionStatsData
{
    [Header("進化統計データ")]
    [SerializeField] private EvolutionStatsData evolutionStatsData = new EvolutionStatsData();
    
    [Header("ゲーム進行データ")]
    [SerializeField] private int currentChapter = 0; // 現在のチャプター
    
    [Header("プレイヤー関連データ")]
    [SerializeField] private int currentMentalPower = GameConstants.MAX_MENTAL_POWER; // 現在の精神力
    
    // EvolutionStatsDataの主要プロパティのラッパー
    public int TotalGames => evolutionStatsData.TotalGames;
    public int TotalWins => evolutionStatsData.TotalWins;
    public int TotalLosses => evolutionStatsData.TotalLosses;
    public float WinRate => evolutionStatsData.WinRate;
    public int CurrentChapter => currentChapter;
    public int CurrentMentalPower => currentMentalPower;
    
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
    /// <param name="playerWon">プレイヤーが勝利したかどうか</param>
    /// <param name="playerMove">プレイヤーの手</param>
    /// <param name="playerCollapsed">プレイヤーのカードが崩壊したかどうか</param>
    public void RecordGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        evolutionStatsData.RecordGameResult(playerWon, playerMove, playerCollapsed);
    }
    
    /// <summary>
    /// 現在の精神力を更新
    /// </summary>
    /// <param name="mentalPower">新しい精神力の値</param>
    public void UpdateMentalPower(int mentalPower)
    {
        currentMentalPower = Mathf.Clamp(mentalPower, 0, GameConstants.MAX_MENTAL_POWER);
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
    /// 単一カードの進化チェック（即時進化用）
    /// </summary>
    /// <param name="card">チェックするカード</param>
    /// <returns>進化先カード（進化しない場合は元のカード）</returns>
    public CardData CheckCardEvolution(CardData card)
    {
        if (CheckAllEvolutionConditions(card))
        {
            return card.EvolutionTarget;
        }
        
        // 進化しない場合は劣化チェック
        if (CheckAllDegradationConditions(card))
        {
            return card.DegradationTarget;
        }
        
        // 変化なしの場合は元のカードを返す
        return card;
    }
    
    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    public string GetStatsString()
    {
        return $"Chapter: {currentChapter}, MentalPower: {currentMentalPower}, {evolutionStatsData.GetStatsString()}";
    }
}