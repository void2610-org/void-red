using System.Collections.Generic;
using System.Linq;

/// <summary>
/// バトルデッキモデル（3枚管理）
/// </summary>
public class BattleDeckModel
{
    /// <summary>デッキ内の全カード</summary>
    public IReadOnlyList<CardModel> Cards => _cards;

    private readonly List<CardModel> _cards = new();
    private readonly Stack<CardModel> _usedHistory = new();

    /// <summary>使用可能なカードを取得</summary>
    public IReadOnlyList<CardModel> GetAvailableCards() => _cards.Where(c => !c.IsUsed).ToList();

    /// <summary>直前に使用したカードを取得（信頼スキル用）</summary>
    public CardModel GetLastUsedCard() => _usedHistory.Count > 0 ? _usedHistory.Peek() : null;

    /// <summary>使用済みカードを未使用に戻す（信頼スキル用）</summary>
    public void RestoreUsedCard(CardModel card) => card.IsUsed = false;

    /// <summary>デッキにカードをセットする</summary>
    public void SetDeck(List<CardModel> cards)
    {
        _cards.Clear();
        _cards.AddRange(cards);
        _usedHistory.Clear();
    }

    /// <summary>カードを使用済みにする</summary>
    public void MarkAsUsed(CardModel card)
    {
        card.IsUsed = true;
        _usedHistory.Push(card);
    }

    /// <summary>
    /// 不足分をデフォルトカード（数字3）で補完する
    /// </summary>
    public static void FillWithDefaults(List<CardModel> cards)
    {
        while (cards.Count < GameConstants.DECK_SIZE)
        {
            cards.Add(new CardModel(GameConstants.DEFAULT_CARD_NUMBER));
        }
    }
}
