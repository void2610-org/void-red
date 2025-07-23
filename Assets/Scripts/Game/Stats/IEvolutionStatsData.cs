/// <summary>
/// 進化条件判定のための統計データインターフェース（EvolutionStatsData、PlayerSaveData、EnemyStatsで共通化）
/// </summary>
public interface IEvolutionStatsData
{
    // 基本統計プロパティ
    int TotalGames { get; }
    int TotalWins { get; }
    int TotalLosses { get; }
    float WinRate { get; }
    
    /// <summary>
    /// 指定したカードの統計を取得
    /// </summary>
    /// <param name="cardId">カードID</param>
    /// <returns>カード統計</returns>
    CardStats GetCardStats(string cardId);
    
    /// <summary>
    /// ゲーム結果を記録
    /// </summary>
    /// <param name="ownerWon">オーナーが勝利したかどうか</param>
    /// <param name="ownerMove">オーナーの手</param>
    /// <param name="ownerCollapsed">オーナーのカードが崩壊したかどうか</param>
    void RecordGameResult(bool ownerWon, PlayerMove ownerMove, bool ownerCollapsed);
    
    /// <summary>
    /// 全ての進化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    bool CheckAllEvolutionConditions(CardData cardData);
    
    /// <summary>
    /// 全ての劣化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>いずれかのグループの条件を全て満たしているかどうか</returns>
    bool CheckAllDegradationConditions(CardData cardData);
    
    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    string GetStatsString();
}