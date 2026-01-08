using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 入札情報を管理するモデル
/// カードごとに感情タイプ別の入札量を保持
/// </summary>
public class BidModel
{
    private readonly Dictionary<CardModel, Dictionary<EmotionType, int>> _bids = new();

    /// <summary>
    /// 入札を追加
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <param name="emotion">感情タイプ</param>
    /// <param name="amount">入札量</param>
    public void AddBid(CardModel card, EmotionType emotion, int amount)
    {
        if (amount <= 0)
            return;

        if (!_bids.ContainsKey(card))
            _bids[card] = new Dictionary<EmotionType, int>();

        if (!_bids[card].ContainsKey(emotion))
            _bids[card][emotion] = 0;

        _bids[card][emotion] += amount;
    }

    /// <summary>
    /// 入札を設定（上書き）
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <param name="emotion">感情タイプ</param>
    /// <param name="amount">入札量</param>
    public void SetBid(CardModel card, EmotionType emotion, int amount)
    {
        if (!_bids.ContainsKey(card))
            _bids[card] = new Dictionary<EmotionType, int>();

        if (amount <= 0)
        {
            _bids[card].Remove(emotion);
            if (_bids[card].Count == 0)
                _bids.Remove(card);
        }
        else
        {
            _bids[card][emotion] = amount;
        }
    }

    /// <summary>
    /// 特定カードの入札合計を取得
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <returns>入札合計値</returns>
    public int GetTotalBid(CardModel card)
    {
        if (!_bids.TryGetValue(card, out var emotions))
            return 0;

        return emotions.Values.Sum();
    }

    /// <summary>
    /// 特定カードの感情別入札量を取得
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <returns>感情タイプ別入札量（入札がない場合は空の辞書）</returns>
    public Dictionary<EmotionType, int> GetBidsByEmotion(CardModel card)
    {
        if (!_bids.TryGetValue(card, out var emotions))
            return new Dictionary<EmotionType, int>();

        return new Dictionary<EmotionType, int>(emotions);
    }

    /// <summary>
    /// 入札対象カード一覧を取得
    /// </summary>
    /// <returns>入札があるカードのリスト</returns>
    public IReadOnlyList<CardModel> GetBidTargets()
    {
        return _bids.Keys.ToList();
    }

    /// <summary>
    /// 特定カードに入札があるか
    /// </summary>
    public bool HasBid(CardModel card)
    {
        return _bids.ContainsKey(card) && _bids[card].Count > 0;
    }

    /// <summary>
    /// 入札合計を取得（全カード）
    /// </summary>
    public int GetTotalBidAmount()
    {
        return _bids.Values.SelectMany(e => e.Values).Sum();
    }

    /// <summary>
    /// 全ての入札をクリア
    /// </summary>
    public void Clear()
    {
        _bids.Clear();
    }
}
