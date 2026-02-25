using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 報酬計算を行うサービス
/// 基本報酬は固定値1。感情マッチで1.5倍、自己記憶で2倍
/// </summary>
public static class RewardCalculator
{
    /// <summary>
    /// 報酬計算結果
    /// </summary>
    public struct RewardResult
    {
        /// <summary>最終報酬（切り上げ: 1, 2）</summary>
        public int TotalReward;

        /// <summary>投入リソース</summary>
        public int BidAmount;

        /// <summary>倍率</summary>
        public float Multiplier;

        /// <summary>感情マッチしたか</summary>
        public bool IsEmotionMatched;

        /// <summary>自己記憶か</summary>
        public bool IsSelfMemory;

        /// <summary>入札した感情タイプ</summary>
        public EmotionType BidEmotion;

        /// <summary>カードの感情タイプ</summary>
        public EmotionType CardEmotion;
    }

    /// <summary>
    /// 落札カードの報酬を計算
    /// </summary>
    public static RewardResult Calculate(CardModel card, BidModel playerBids)
    {
        var bidEmotion = playerBids.GetBidEmotion(card);
        var bidAmount = playerBids.GetTotalBid(card);
        var cardEmotion = card.Data.CardEmotion;
        var isSelfMemory = card.Data.MemoryType == MemoryType.SelfMemory;
        var isEmotionMatched = bidEmotion.HasValue && bidEmotion.Value == cardEmotion;

        // 倍率決定（自己記憶 > 感情マッチ > 通常）
        float multiplier;
        if (isSelfMemory)
            multiplier = GameConstants.SELF_MEMORY_MULTIPLIER;
        else if (isEmotionMatched)
            multiplier = GameConstants.EMOTION_MATCH_MULTIPLIER;
        else
            multiplier = 1.0f;

        // 基本報酬=1 × 倍率（切り上げ）
        var totalReward = Mathf.CeilToInt(1f * multiplier);

        return new RewardResult
        {
            TotalReward = totalReward,
            BidAmount = bidAmount,
            Multiplier = multiplier,
            IsEmotionMatched = isEmotionMatched,
            IsSelfMemory = isSelfMemory,
            BidEmotion = bidEmotion ?? default,
            CardEmotion = cardEmotion,
        };
    }

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
            results[card] = Calculate(card, playerBids);
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
