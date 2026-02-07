using System.Collections.Generic;

/// <summary>
/// カードの価値順位を管理するモデル
/// 各カードに1-4の順位を割り当て（重複不可）
/// </summary>
public class ValueRankingModel
{
    /// <summary>
    /// 全順位が設定済みか
    /// </summary>
    public bool IsComplete => _rankings.Count == _maxRank;

    /// <summary>
    /// 設定済みの順位数
    /// </summary>
    public int Count => _rankings.Count;

    private readonly Dictionary<CardModel, int> _rankings = new();
    private readonly int _maxRank;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="maxRank">最大順位（デフォルト4）</param>
    public ValueRankingModel(int maxRank = GameConstants.CARDS_PER_PLAYER)
    {
        _maxRank = maxRank;
    }

    /// <summary>
    /// 順位を取得
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <returns>順位（未設定の場合は0）</returns>
    public int GetRanking(CardModel card) => _rankings.TryGetValue(card, out var rank) ? rank : 0;

    /// <summary>
    /// カードが順位設定済みか
    /// </summary>
    public bool HasRanking(CardModel card) => _rankings.ContainsKey(card);

    /// <summary>
    /// 順位に応じた基準リソース値を取得（報酬計算用）
    /// 順位1が最も高く、順位4が最も低い
    /// </summary>
    /// <param name="rank">順位</param>
    /// <returns>基準リソース値</returns>
    // 順位1 → 基準値4, 順位4 → 基準値1
    public int GetBaseResourceValue(int rank) => GameConstants.VALUE_RANKING_BASE_RESOURCE - rank + 1;

    /// <summary>
    /// 全ての順位設定をクリア
    /// </summary>
    public void Clear() => _rankings.Clear();

    /// <summary>
    /// 順位を設定（重複チェック付き）
    /// </summary>
    /// <param name="card">対象カード</param>
    /// <param name="rank">順位（1-maxRank）</param>
    /// <returns>設定成功時true、順位が既に使用済みの場合false</returns>
    public bool TrySetRanking(CardModel card, int rank)
    {
        if (rank < 1 || rank > _maxRank)
            return false;

        // 既に同じ順位が設定されているカードがあるかチェック
        if (_rankings.ContainsValue(rank) && !_rankings.ContainsKey(card))
            return false;

        _rankings[card] = rank;
        return true;
    }
}
