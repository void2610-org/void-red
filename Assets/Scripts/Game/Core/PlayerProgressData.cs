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
    /// 閲覧済みカードID
    /// </summary>
    public HashSet<string> ViewedCardIds { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayerProgressData()
    {
        Deck = new List<SavedCard>();
        ViewedCardIds = new HashSet<string>();
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
        ViewedCardIds.Clear();
    }
}
