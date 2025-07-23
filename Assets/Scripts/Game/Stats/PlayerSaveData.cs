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
    
    // TODO: 将来的に追加するプレイヤー固有データ
    // [Header("ゲーム進行データ")]
    // [SerializeField] private ChapterProgressData chapterProgress;
    // [SerializeField] private PlayerSettingsData playerSettings;
    
    /// <summary>
    /// 進化統計データへのアクセス
    /// </summary>
    public EvolutionStatsData EvolutionStatsData => evolutionStatsData;
    
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
    /// <param name="playerWon">プレイヤーが勝利したかどうか</param>
    /// <param name="playerMove">プレイヤーの手</param>
    /// <param name="playerCollapsed">プレイヤーのカードが崩壊したかどうか</param>
    public void RecordGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        evolutionStatsData.RecordGameResult(playerWon, playerMove, playerCollapsed);
        
        // TODO: 将来的にここで章進行状況の更新等を行う
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