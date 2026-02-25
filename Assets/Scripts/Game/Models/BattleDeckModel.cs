using System.Collections.Generic;
using System.Linq;

/// <summary>
/// バトルデッキモデル（3枚管理）
/// </summary>
public class BattleDeckModel
{
    /// <summary>デッキ内の全カード</summary>
    public IReadOnlyList<BattleCardModel> Cards => _cards;

    private readonly List<BattleCardModel> _cards = new();
    private readonly Stack<BattleCardModel> _usedHistory = new();

    /// <summary>使用可能なカードを取得</summary>
    public IReadOnlyList<BattleCardModel> GetAvailableCards()
        => _cards.Where(c => !c.IsUsed).ToList();

    /// <summary>直前に使用したカードを取得（信頼スキル用）</summary>
    public BattleCardModel GetLastUsedCard()
        => _usedHistory.Count > 0 ? _usedHistory.Peek() : null;

    /// <summary>使用済みカードを未使用に戻す（信頼スキル用）</summary>
    public void RestoreUsedCard(BattleCardModel card) => card.IsUsed = false;

    /// <summary>デッキにカードをセットする</summary>
    public void SetDeck(List<BattleCardModel> cards)
    {
        _cards.Clear();
        _cards.AddRange(cards);
        _usedHistory.Clear();
    }

    /// <summary>カードを使用済みにする</summary>
    public void MarkAsUsed(BattleCardModel card)
    {
        card.IsUsed = true;
        _usedHistory.Push(card);
    }
}
