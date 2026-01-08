using System.Collections.Generic;
using System.Linq;

/// <summary>
/// NPCクラス
/// AIによって制御される対戦相手を表す
/// </summary>
public class Enemy : PlayerPresenter
{
    private EnemyData _data;

    public Enemy(GameProgressService gameProgressService = null) : base(gameProgressService) { }

    public void SetEnemyData(EnemyData data) => _data = data;

    // 敵AIで価値順位をランダム設定（自身のCardsに対してValueRankingを設定）
    public void DecideValueRankings()
    {
        ValueRanking.Clear();
        if (Cards.Count == 0) return;

        var maxRank = GameConstants.CARDS_PER_PLAYER;
        var ranks = Enumerable.Range(1, System.Math.Min(Cards.Count, maxRank)).ToList();
        ShuffleList(ranks);

        for (var i = 0; i < System.Math.Min(Cards.Count, ranks.Count); i++)
        {
            ValueRanking.TrySetRanking(Cards[i], ranks[i]);
        }
    }

    // 敵AIで入札を決定（自身のValueRankingに基づいてBidsを設定）
    public void DecideBids(IReadOnlyList<CardModel> auctionCards, EmotionType emotion)
    {
        Bids.Clear();

        var totalResource = GetEmotionAmount(emotion);
        if (auctionCards.Count == 0 || totalResource <= 0) return;

        // 価値順位に応じて配分（順位1に最も多く、順位4に最も少なく）
        var totalWeight = 0;
        var weights = new Dictionary<CardModel, int>();

        foreach (var card in auctionCards)
        {
            var rank = ValueRanking.GetRanking(card);
            if (rank == 0) continue;

            var weight = GameConstants.CARDS_PER_PLAYER - rank + 1;
            weights[card] = weight;
            totalWeight += weight;
        }

        if (totalWeight == 0) return;

        // 重みに応じてリソースを配分
        var remaining = totalResource;
        var cardList = weights.Keys.ToList();

        for (var i = 0; i < cardList.Count; i++)
        {
            var card = cardList[i];
            var weight = weights[card];

            int amount;
            if (i == cardList.Count - 1)
            {
                amount = remaining;
            }
            else
            {
                amount = (int)((float)totalResource * weight / totalWeight);
                amount = System.Math.Min(amount, remaining);
            }

            if (amount > 0)
            {
                Bids.SetBid(card, emotion, amount);
                remaining -= amount;
            }
        }
    }

    private static void ShuffleList<T>(IList<T> list)
    {
        var random = new System.Random();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
