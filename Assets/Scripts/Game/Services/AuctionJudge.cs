using System.Collections.Generic;

/// <summary>
/// オークションの落札者を判定するサービス
/// </summary>
public static class AuctionJudge
{
    /// <summary>
    /// 落札結果のエントリ
    /// </summary>
    public struct AuctionResultEntry
    {
        /// <summary>対象カード</summary>
        public CardModel Card;

        /// <summary>プレイヤーが落札したか</summary>
        public bool IsPlayerWon;

        /// <summary>プレイヤーの入札合計</summary>
        public int PlayerBid;

        /// <summary>敵の入札合計</summary>
        public int EnemyBid;

        /// <summary>どちらも入札していない場合true</summary>
        public bool NoBids;
    }

    /// <summary>
    /// 全カードの落札者を判定
    /// </summary>
    /// <param name="auctionCards">オークション対象の全カード</param>
    /// <param name="playerBids">プレイヤーの入札情報</param>
    /// <param name="enemyBids">敵の入札情報</param>
    /// <returns>各カードの落札結果リスト</returns>
    public static List<AuctionResultEntry> JudgeAll(
        IReadOnlyList<CardModel> auctionCards,
        BidModel playerBids,
        BidModel enemyBids)
    {
        var results = new List<AuctionResultEntry>();

        foreach (var card in auctionCards)
        {
            var playerBid = playerBids.GetTotalBid(card);
            var enemyBid = enemyBids.GetTotalBid(card);

            var entry = new AuctionResultEntry
            {
                Card = card,
                PlayerBid = playerBid,
                EnemyBid = enemyBid,
                NoBids = playerBid == 0 && enemyBid == 0
            };

            // 判定ルール: 入札合計が多い方が落札、同点時はプレイヤー優先
            if (entry.NoBids)
            {
                // どちらも入札していない場合は誰も落札しない
                entry.IsPlayerWon = false;
            }
            else if (playerBid >= enemyBid)
            {
                // プレイヤーの入札が同じか多い場合はプレイヤーが落札
                entry.IsPlayerWon = true;
            }
            else
            {
                // 敵の入札が多い場合は敵が落札
                entry.IsPlayerWon = false;
            }

            results.Add(entry);
        }

        return results;
    }

    /// <summary>
    /// 単一カードの落札者を判定
    /// </summary>
    public static AuctionResultEntry Judge(CardModel card, BidModel playerBids, BidModel enemyBids)
    {
        var playerBid = playerBids.GetTotalBid(card);
        var enemyBid = enemyBids.GetTotalBid(card);

        return new AuctionResultEntry
        {
            Card = card,
            PlayerBid = playerBid,
            EnemyBid = enemyBid,
            NoBids = playerBid == 0 && enemyBid == 0,
            IsPlayerWon = playerBid > 0 && playerBid >= enemyBid
        };
    }
}
