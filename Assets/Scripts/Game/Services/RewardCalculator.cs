using System.Collections.Generic;

/// <summary>
/// 報酬計算を行うサービス
/// </summary>
public static class RewardCalculator
{
    /// <summary>
    /// 報酬計算結果
    /// </summary>
    public struct RewardResult
    {
        /// <summary>基本報酬（価値順位による）</summary>
        public int BaseReward;

        /// <summary>相対報酬（リソース効率）</summary>
        public int RelativeReward;

        /// <summary>自分のカードボーナス</summary>
        public int OwnCardBonus;

        /// <summary>合計報酬</summary>
        public int TotalReward;

        /// <summary>自分のカードかどうか</summary>
        public bool IsOwnCard;

        /// <summary>価値順位</summary>
        public int ValueRank;

        /// <summary>投入リソース</summary>
        public int BidAmount;

        /// <summary>基準リソース値</summary>
        public int BaseResourceValue;
    }

    /// <summary>
    /// 落札カードの報酬を計算
    /// </summary>
    /// <param name="valueRank">価値順位（1-4）</param>
    /// <param name="bidAmount">投入リソース量</param>
    /// <param name="baseResourceValue">基準リソース値</param>
    /// <param name="isOwnCard">自分のカードかどうか</param>
    /// <returns>報酬計算結果</returns>
    public static RewardResult Calculate(int valueRank, int bidAmount, int baseResourceValue, bool isOwnCard)
    {
        var result = new RewardResult
        {
            ValueRank = valueRank,
            BidAmount = bidAmount,
            BaseResourceValue = baseResourceValue,
            IsOwnCard = isOwnCard
        };

        // 基本報酬: 順位1=6, 順位2=5, 順位3=4, 順位4=3
        result.BaseReward = GameConstants.BASE_REWARD_MAX - valueRank + 1;

        // 相対報酬: 基準リソース値 - 投入リソース値
        result.RelativeReward = baseResourceValue - bidAmount;

        // 自分のカードボーナス
        result.OwnCardBonus = isOwnCard ? GameConstants.OWN_CARD_BONUS : 0;

        // 合計報酬（最低0）
        result.TotalReward = System.Math.Max(0, result.BaseReward + result.RelativeReward + result.OwnCardBonus);

        return result;
    }

    /// <summary>
    /// 複数カードの報酬を一括計算
    /// </summary>
    /// <param name="wonCards">落札したカードリスト</param>
    /// <param name="playerRankings">プレイヤーの価値順位</param>
    /// <param name="enemyRankings">敵の価値順位</param>
    /// <param name="playerBids">プレイヤーの入札情報</param>
    /// <param name="playerOwnCards">プレイヤー自身のカードリスト</param>
    /// <returns>カードごとの報酬結果</returns>
    public static Dictionary<CardModel, RewardResult> CalculateAll(
        IReadOnlyList<CardModel> wonCards,
        ValueRankingModel playerRankings,
        ValueRankingModel enemyRankings,
        BidModel playerBids,
        IReadOnlyList<CardModel> playerOwnCards)
    {
        var results = new Dictionary<CardModel, RewardResult>();
        var playerOwnCardSet = new HashSet<CardModel>(playerOwnCards);

        foreach (var card in wonCards)
        {
            var isOwnCard = playerOwnCardSet.Contains(card);

            // 自分のカードならプレイヤーの順位、敵のカードなら敵の順位を使用
            var rank = isOwnCard
                ? playerRankings.GetRanking(card)
                : enemyRankings.GetRanking(card);

            if (rank == 0) rank = 2; // フォールバック

            var bidAmount = playerBids.GetTotalBid(card);
            var baseResourceValue = GetBaseResourceValue(rank);

            results[card] = Calculate(rank, bidAmount, baseResourceValue, isOwnCard);
        }

        return results;
    }

    /// <summary>
    /// 順位に応じた基準リソース値を取得
    /// 順位1=4, 順位2=3, 順位3=2, 順位4=1
    /// </summary>
    public static int GetBaseResourceValue(int rank) =>
        GameConstants.VALUE_RANKING_BASE_RESOURCE - rank + 1;

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
