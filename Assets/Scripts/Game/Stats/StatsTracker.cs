using UnityEngine;
using VContainer;
using Void2610.UnityTemplate;

/// <summary>
/// プレイヤーの統計データを追跡・管理するサービスクラス
/// </summary>
public class StatsTracker
{
    private PlayerStats _playerStats;
    private readonly string _ownerId;
    
    /// <summary>
    /// 現在の統計データ
    /// </summary>
    public PlayerStats PlayerStats => _playerStats ??= new PlayerStats();
    
    /// <summary>
    /// 統計トラッカーのオーナーID（プレイヤー/敵の識別用）
    /// </summary>
    public string OwnerId => _ownerId;
    
    public StatsTracker(string ownerId = "Player")
    {
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
        PlayerStats.RecordGameResult(ownerWon, ownerMove, ownerCollapsed);
    }
    
    /// <summary>
    /// 指定したカードの統計を取得
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    /// <returns>カード統計</returns>
    public CardStats GetCardStats(CardData cardData)
    {
        if (!cardData) return new CardStats();
        return PlayerStats.GetCardStats(cardData.CardId);
    }
    
    /// <summary>
    /// 進化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>進化可能かどうか</returns>
    public bool CanCardEvolve(CardData cardData)
    {
        return PlayerStats.CheckAllEvolutionConditions(cardData);
    }
    
    /// <summary>
    /// 劣化条件をチェック
    /// </summary>
    /// <param name="cardData">チェック対象のカード</param>
    /// <returns>劣化可能かどうか</returns>
    public bool CanCardDegrade(CardData cardData)
    {
        return PlayerStats.CheckAllDegradationConditions(cardData);
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
    
    /// <summary>
    /// 統計データをリセット
    /// </summary>
    public void ResetStats()
    {
        _playerStats = new PlayerStats();
    }
}