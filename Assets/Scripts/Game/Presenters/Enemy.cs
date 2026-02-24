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

    /// <summary>
    /// 敵AIで入札を決定（1カード1感情制約を遵守）
    /// </summary>
    public void DecideBids(IReadOnlyList<CardModel> auctionCards)
    {
        Bids.Clear();
        if (auctionCards.Count == 0) return;

        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        var remainingResources = new Dictionary<EmotionType, int>();
        foreach (var emotion in emotions)
            remainingResources[emotion] = GetEmotionAmount(emotion);

        // カードをシャッフルしてランダムに入札
        var shuffledCards = auctionCards.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var card in shuffledCards)
        {
            // ランダムに感情を選択（リソースが残っているものから）
            var availableEmotions = emotions.Where(e => remainingResources[e] > 0).ToList();
            if (availableEmotions.Count == 0) break;

            var emotion = availableEmotions[UnityEngine.Random.Range(0, availableEmotions.Count)];
            var available = remainingResources[emotion];
            var amount = UnityEngine.Random.Range(1, available + 1);

            Bids.SetBid(card, emotion, amount);
            remainingResources[emotion] -= amount;
        }
    }
}
