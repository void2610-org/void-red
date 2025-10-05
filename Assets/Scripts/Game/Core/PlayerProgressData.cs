using System.Collections.Generic;

/// <summary>
/// プレイヤー進行データを保持するクラス
/// デッキ、進化統計、閲覧済みカードを管理
/// </summary>
public class PlayerProgressData
{
    /// <summary>
    /// プレイヤーのデッキ（セーブ用カードリスト）
    /// </summary>
    public List<SavedCard> Deck { get; set; }

    /// <summary>
    /// プレイヤーの進化統計データ
    /// </summary>
    public EvolutionStatsData EvolutionStats { get; set; }

    /// <summary>
    /// 閲覧済みカードID
    /// </summary>
    public HashSet<string> ViewedCardIds { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayerProgressData()
    {
        Deck = new List<SavedCard>();
        EvolutionStats = new EvolutionStatsData();
        ViewedCardIds = new HashSet<string>();
    }

    /// <summary>
    /// デッキを更新
    /// </summary>
    /// <param name="deck">新しいデッキデータ</param>
    public void UpdateDeck(List<SavedCard> deck)
    {
        Deck.Clear();
        Deck.AddRange(deck);
    }

    /// <summary>
    /// カード閲覧を記録
    /// </summary>
    /// <param name="cardId">カードID</param>
    public void RecordCardView(string cardId)
    {
        if (!string.IsNullOrEmpty(cardId))
        {
            ViewedCardIds.Add(cardId);
        }
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        Deck.Clear();
        EvolutionStats = new EvolutionStatsData();
        ViewedCardIds.Clear();
    }
}
