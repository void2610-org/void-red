using System.Collections.Generic;

/// <summary>
/// 報酬計算を行うサービス
/// TODO: 新オークションシステムに合わせて報酬計算ロジックを再設計する
/// </summary>
public static class RewardCalculator
{
    /// <summary>
    /// 報酬計算結果
    /// </summary>
    public struct RewardResult
    {
        /// <summary>合計報酬</summary>
        public int TotalReward;

        /// <summary>投入リソース</summary>
        public int BidAmount;
    }

    /// <summary>
    /// 落札カードの報酬を計算（スタブ実装）
    /// </summary>
    // TODO: 報酬計算ロジックを再設計
    public static RewardResult Calculate(int bidAmount) =>
        new()
        {
            BidAmount = bidAmount,
            TotalReward = 1
        };

    /// <summary>
    /// 複数カードの報酬を一括計算
    /// </summary>
    public static Dictionary<CardModel, RewardResult> CalculateAll(
        IReadOnlyList<CardModel> wonCards,
        BidModel playerBids)
    {
        var results = new Dictionary<CardModel, RewardResult>();

        foreach (var card in wonCards)
        {
            var bidAmount = playerBids.GetTotalBid(card);
            results[card] = Calculate(bidAmount);
        }

        return results;
    }

    /// <summary>
    /// 合計報酬を計算
    /// </summary>
    public static int CalculateTotalReward(Dictionary<CardModel, RewardResult> results)
    {
        var total = 0;
        foreach (var result in results.Values)
        {
            total += result.TotalReward;
        }
        return total;
    }
}
