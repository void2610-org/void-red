/// <summary>
/// バトル用カードモデル（CardModel + 割り当て数字）
/// </summary>
public class BattleCardModel
{
    /// <summary>元のカードモデル</summary>
    public CardModel Card { get; }

    /// <summary>割り当てられた数字</summary>
    public int Number { get; private set; }

    /// <summary>カードの司る感情</summary>
    public EmotionType Emotion => Card.Data.CardEmotion;

    /// <summary>オークション入札リソース総量（タイブレーク用）</summary>
    public int AuctionBidTotal { get; }

    /// <summary>使用済みかどうか</summary>
    public bool IsUsed { get; set; }

    public BattleCardModel(CardModel card, int number, int auctionBidTotal)
    {
        Card = card;
        Number = number;
        AuctionBidTotal = auctionBidTotal;
    }

    /// <summary>数字を変更する（スキル効果用）</summary>
    public void SetNumber(int number) => Number = number;
}
