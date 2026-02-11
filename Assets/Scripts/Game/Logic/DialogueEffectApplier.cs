using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DialogueEffectApplier
{
    public static string ApplyEffect(DialogueEffect effect, PlayerPresenter target, IReadOnlyList<CardModel> auctionCards)
    {
        if (effect == null) return "効果なし";

        return effect.ActionType switch
        {
            DialogueActionType.None => "効果なし",
            DialogueActionType.TargetChange => ApplyTargetChange(target, auctionCards),
            DialogueActionType.ResourceChange => ApplyResourceChange(effect, target),
            DialogueActionType.BluffStrengthen => ApplyBluffStrengthen(effect, target),
            _ => "効果なし"
        };
    }

    private static string ApplyTargetChange(PlayerPresenter target, IReadOnlyList<CardModel> auctionCards)
    {
        var bidTargets = target.Bids.GetBidTargets();
        if (bidTargets.Count == 0) return "入札対象なし";

        var randomOldTarget = bidTargets[Random.Range(0, bidTargets.Count)];
        var availableNewTargets = auctionCards.Where(c => !target.Bids.HasBid(c)).ToList();
        if (availableNewTargets.Count == 0) return "変更可能な対象なし";

        var randomNewTarget = availableNewTargets[Random.Range(0, availableNewTargets.Count)];

        var emotionBids = target.Bids.GetBidsByEmotion(randomOldTarget);
        foreach (var (emotion, amount) in emotionBids)
        {
            target.Bids.SetBid(randomOldTarget, emotion, 0);
            target.Bids.AddBid(randomNewTarget, emotion, amount);
        }

        var oldCardName = randomOldTarget.Data.CardName;
        var newCardName = randomNewTarget.Data.CardName;

        return $"入札対象が「{oldCardName}」から「{newCardName}」に変更！";
    }

    private static string ApplyResourceChange(DialogueEffect effect, PlayerPresenter target)
    {
        var bidTargets = target.Bids.GetBidTargets();
        if (bidTargets.Count == 0) return "入札対象なし";

        var randomCard = bidTargets[Random.Range(0, bidTargets.Count)];
        var cardName = randomCard.Data.CardName;
        var currentBid = target.Bids.GetTotalBid(randomCard);
        var newBid = Mathf.Max(0, currentBid + effect.ResourceChangeAmount);

        var emotionBids = target.Bids.GetBidsByEmotion(randomCard);
        var emotion = emotionBids.Keys.FirstOrDefault();

        target.Bids.SetBid(randomCard, emotion, newBid);

        var changeText = effect.ResourceChangeAmount > 0 ? "増加" : "減少";
        return $"「{cardName}」への入札が {currentBid} → {newBid} に{changeText}！";
    }

    private static string ApplyBluffStrengthen(DialogueEffect effect, PlayerPresenter target)
    {
        return ApplyResourceChangeWithPenalty(effect, target);
    }

    private static string ApplyResourceChangeWithPenalty(DialogueEffect effect, PlayerPresenter target)
    {
        var bidTargets = target.Bids.GetBidTargets();
        if (bidTargets.Count == 0) return "入札対象なし（ブラフ強化）";

        var randomCard = bidTargets[Random.Range(0, bidTargets.Count)];
        var cardName = randomCard.Data.CardName;
        var currentBid = target.Bids.GetTotalBid(randomCard);

        var penalty = Mathf.Abs(effect.ResourceChangeAmount);
        var newBid = Mathf.Max(0, currentBid - penalty);

        var emotionBids = target.Bids.GetBidsByEmotion(randomCard);
        var emotion = emotionBids.Keys.FirstOrDefault();

        target.Bids.SetBid(randomCard, emotion, newBid);

        return $"ブラフ失敗！「{cardName}」への入札が {currentBid} → {newBid} に減少...";
    }
}
