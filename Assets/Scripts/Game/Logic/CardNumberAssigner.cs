using System.Collections.Generic;
using System.Linq;

/// <summary>
/// オークション入札量に基づいてカードに数字を割り当てる
/// 両者の入札合計が少ない順に1から割り当て（同値は同じ数字）
/// 例: [5, 3, 3, 2, 0, 0] → [6, 4, 4, 3, 1, 1]
/// </summary>
public static class CardNumberAssigner
{
    /// <summary>
    /// カード数字の割り当て結果
    /// </summary>
    public struct CardNumberInfo
    {
        /// <summary>割り当てられた数字</summary>
        public int Number;
        /// <summary>両者の入札合計</summary>
        public int TotalBid;
    }

    /// <summary>
    /// 全カードに数字を割り当てる
    /// </summary>
    public static Dictionary<CardModel, CardNumberInfo> AssignNumbers(
        IReadOnlyList<CardModel> auctionCards,
        BidModel playerBids,
        BidModel enemyBids)
    {
        // 各カードの合計入札量（プレイヤー＋敵）を計算し昇順ソート
        var cardTotals = auctionCards
            .Select(card => (card, total: playerBids.GetTotalBid(card) + enemyBids.GetTotalBid(card)))
            .OrderBy(x => x.total)
            .ToList();

        // ランク方式: 同じ入札量は同じ数字
        var result = new Dictionary<CardModel, CardNumberInfo>();
        var rank = 1;
        for (var i = 0; i < cardTotals.Count; i++)
        {
            if (i > 0 && cardTotals[i].total > cardTotals[i - 1].total)
                rank = i + 1;

            result[cardTotals[i].card] = new CardNumberInfo
            {
                Number = rank,
                TotalBid = cardTotals[i].total,
            };
        }

        return result;
    }
}
