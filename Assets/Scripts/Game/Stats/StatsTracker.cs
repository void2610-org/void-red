using UnityEngine;
using VContainer;
using Void2610.UnityTemplate;

/// <summary>
/// 統計データを追跡・管理するサービスクラス
/// </summary>
public class StatsTracker
{
    private readonly IEvolutionStatsData _evolutionStatsData;
    private readonly string _ownerId;
    
    public StatsTracker(IEvolutionStatsData evolutionStatsData, string ownerId)
    {
        _evolutionStatsData = evolutionStatsData;
        _ownerId = ownerId;
    }
    
    /// <summary>
    /// ゲーム結果を記録（オーナーに応じてプレイヤーまたは敵の統計を記録）
    /// </summary>
    /// <param name="ownerMove">このトラッカーのオーナーの手</param>
    /// <param name="opponentMove">相手の手</param>
    /// <param name="ownerWon">このトラッカーのオーナーが勝利したかどうか</param>
    /// <param name="ownerCollapsed">このトラッカーのオーナーのカードが崩壊したかどうか</param>
    public void RecordGameResult(PlayerMove ownerMove, PlayerMove opponentMove, bool ownerWon, bool ownerCollapsed)
    {
        if (!ownerMove?.SelectedCard) return;
        
        // 統計データを更新
        _evolutionStatsData.RecordGameResult(ownerWon, ownerMove, ownerCollapsed);
    }
    
    /// <summary>
    /// 進化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>進化可能かどうか</returns>
    public bool CanCardEvolve(CardData cardData)
    {
        return _evolutionStatsData.CheckAllEvolutionConditions(cardData);
    }
    
    /// <summary>
    /// 劣化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>劣化可能かどうか</returns>
    public bool CanCardDegrade(CardData cardData)
    {
        return _evolutionStatsData.CheckAllDegradationConditions(cardData);
    }
    
    /// <summary>
    /// 単一カードの進化チェック（即時進化用）
    /// </summary>
    /// <param name="card">チェックするカード</param>
    /// <returns>進化先カード（進化しない場合は元のカード）</returns>
    public CardData CheckCardEvolution(CardData card)
    {
        if (CanCardEvolve(card))
        {
            return card.EvolutionTarget;
        }
        
        // 進化しない場合は劣化チェック
        if (CanCardDegrade(card))
        {
            return card.DegradationTarget;
        }
        
        // 変化なしの場合は元のカードを返す
        return card;
    }
    
}