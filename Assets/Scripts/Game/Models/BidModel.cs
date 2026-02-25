using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 入札情報を管理するモデル
/// 1カードにつき1種類の感情のみベット可能（1カード1感情制約）
/// </summary>
public class BidModel
{
    private readonly Dictionary<CardModel, Dictionary<EmotionType, int>> _bids = new();

    /// <summary>
    /// 入札対象カード一覧を取得
    /// </summary>
    public IReadOnlyList<CardModel> GetBidTargets() => _bids.Keys.ToList();

    /// <summary>
    /// 特定カードに入札があるか
    /// </summary>
    public bool HasBid(CardModel card) => _bids.ContainsKey(card) && _bids[card].Count > 0;

    /// <summary>
    /// 入札合計を取得（全カード）
    /// </summary>
    public int GetTotalBidAmount() => _bids.Values.SelectMany(e => e.Values).Sum();

    /// <summary>
    /// 全ての入札をクリア
    /// </summary>
    public void Clear() => _bids.Clear();

    /// <summary>
    /// 入札を設定（1カード1感情制約）
    /// 既に別の感情がセットされている場合はクリアしてから設定
    /// </summary>
    public void SetBid(CardModel card, EmotionType emotion, int amount)
    {
        if (!_bids.ContainsKey(card))
            _bids[card] = new Dictionary<EmotionType, int>();

        // 1カード1感情制約: 既存の他感情をクリア
        _bids[card].Clear();

        if (amount > 0)
        {
            _bids[card][emotion] = amount;
        }
        else
        {
            _bids.Remove(card);
        }
    }

    /// <summary>
    /// カードに設定されている感情タイプを取得（1カード1感情制約）
    /// </summary>
    public EmotionType? GetBidEmotion(CardModel card)
    {
        if (!_bids.TryGetValue(card, out var emotions) || emotions.Count == 0)
            return null;

        foreach (var kvp in emotions)
            return kvp.Key;

        return null;
    }

    /// <summary>
    /// 特定カードの入札合計を取得
    /// </summary>
    public int GetTotalBid(CardModel card)
    {
        if (!_bids.TryGetValue(card, out var emotions))
            return 0;

        return emotions.Values.Sum();
    }

    /// <summary>
    /// 特定カードの感情別入札量を取得
    /// </summary>
    public Dictionary<EmotionType, int> GetBidsByEmotion(CardModel card)
    {
        if (!_bids.TryGetValue(card, out var emotions))
            return new Dictionary<EmotionType, int>();

        return new Dictionary<EmotionType, int>(emotions);
    }

    /// <summary>
    /// 感情タイプ別の入札合計を取得（全カードの合計）
    /// </summary>
    public Dictionary<EmotionType, int> GetTotalBidsByEmotion()
    {
        var result = new Dictionary<EmotionType, int>();
        foreach (var cardBids in _bids.Values)
        {
            foreach (var (emotion, amount) in cardBids)
            {
                result.TryAdd(emotion, 0);
                result[emotion] += amount;
            }
        }
        return result;
    }
}
