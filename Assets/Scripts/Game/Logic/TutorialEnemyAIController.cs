using System.Collections.Generic;
using System.Linq;

/// <summary>
/// チュートリアル専用の敵AI。全判断をスクリプト通りに行う。
/// </summary>
public class TutorialEnemyAIController : IEnemyAIController
{
    // 入札: (カードインデックス, 感情, 量)
    private static readonly (int cardIdx, EmotionType emotion, int amount)[] _enemyBids = { (0, EmotionType.Fear, 2) };
    // デッキ選択: enemy.Cardsリスト内のインデックス
    private static readonly int[] _enemyDeckCardIndices = { 0, 1, 2 };
    // バトル感情（ラウンド毎）
    private static readonly EmotionType[] _enemyEmotionPerRound = { EmotionType.Fear, EmotionType.Joy, EmotionType.Sadness };
    // カード配置（ラウンド毎、enemyDeck.Cards 内の固定インデックス）
    private static readonly int[] _enemyCardIndexPerRound = { 0, 1, 2 };
    // スキル発動（ラウンド毎）
    private static readonly bool[] _enemySkillPerRound = { false, true, false };

    private readonly Enemy _enemy;
    private int _roundIndex;

    public TutorialEnemyAIController(Enemy enemy) => _enemy = enemy;

    public EmotionType DecideEmotionState() => _enemyEmotionPerRound[_roundIndex];

    public void TryCompetitionRaise(CompetitionHandler handler) => handler.EnemyRaise(EmotionType.Fear, _enemy);

    public List<CardModel> SelectDeck(List<CardModel> availableCards)
    {
        var selectedCards = new List<CardModel>();
        foreach (var index in _enemyDeckCardIndices)
        {
            if (index < 0 || index >= availableCards.Count)
            {
                UnityEngine.Debug.LogError($"[TutorialEnemyAIController] 不正なデッキカードインデックス: {index}");
                continue;
            }

            selectedCards.Add(availableCards[index]);
        }

        return selectedCards;
    }

    public void DecideBids(IReadOnlyList<CardModel> auctionCards)
    {
        _enemy.Bids.Clear();
        foreach (var (cardIdx, emotion, amount) in _enemyBids)
        {
            if (cardIdx < 0 || cardIdx >= auctionCards.Count)
            {
                UnityEngine.Debug.LogError($"[TutorialEnemyAIController] 不正な入札カードインデックス: {cardIdx}");
                continue;
            }

            _enemy.Bids.SetBid(auctionCards[cardIdx], emotion, amount);
        }
    }

    public void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck)
    {
        var deckCardIndex = _enemyCardIndexPerRound[_roundIndex];
        if (deckCardIndex < 0 || deckCardIndex >= enemyDeck.Cards.Count)
        {
            UnityEngine.Debug.LogError($"[TutorialEnemyAIController] 不正なカードインデックス: {deckCardIndex}");
            return;
        }

        var card = enemyDeck.Cards[deckCardIndex];
        if (card.IsUsed)
        {
            UnityEngine.Debug.LogError($"[TutorialEnemyAIController] 使用済みカードが指定されました: {card}");
            return;
        }

        handler.PlaceEnemyCard(card);
        enemyDeck.MarkAsUsed(card);
    }

    public bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState)
    {
        var result = _enemySkillPerRound[_roundIndex];
        _roundIndex++;  // スキル判定がラウンド最後の呼び出し
        return result;
    }
}
