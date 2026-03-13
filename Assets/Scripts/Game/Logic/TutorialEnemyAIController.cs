using System.Collections.Generic;
using System.Linq;

/// <summary>
/// チュートリアル専用の敵AI。全判断をスクリプト通りに行う。
/// </summary>
public class TutorialEnemyAIController : IEnemyAIController
{
    // 入札: (カードインデックス, 感情, 量)
    private static readonly (int cardIdx, EmotionType emotion, int amount)[] _enemyBids = { (0, EmotionType.Fear, 3) };
    // デッキ選択: enemy.Cardsリスト内のインデックス
    private static readonly int[] _enemyDeckCardIndices = { 0, 1, 2 };
    // バトル感情（ラウンド毎）
    private static readonly EmotionType[] _enemyEmotionPerRound = { EmotionType.Fear, EmotionType.Joy, EmotionType.Sadness };
    // カード配置（ラウンド毎、enemyDeck.Cards 内の固定インデックス）
    private static readonly int[] _enemyCardIndexPerRound = { 0, 1, 2 };
    // スキル発動（ラウンド毎）
    private static readonly bool[] _enemySkillPerRound = { false, true, false };

    // 競合上乗せを行うか
    private const bool ENEMY_COMPETITION_DO_RAISE = true;

    private readonly Enemy _enemy;
    private int _roundIndex;

    public TutorialEnemyAIController(Enemy enemy) => _enemy = enemy;
    public List<CardModel> SelectDeck(List<CardModel> availableCards) => _enemyDeckCardIndices.Select(i => availableCards[i]).ToList();
    public EmotionType DecideEmotionState() => _enemyEmotionPerRound[_roundIndex];

    public void DecideBids(IReadOnlyList<CardModel> auctionCards)
    {
        _enemy.Bids.Clear();
        foreach (var (cardIdx, emotion, amount) in _enemyBids)
            _enemy.Bids.SetBid(auctionCards[cardIdx], emotion, amount);
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

    public void TryCompetitionRaise(CompetitionHandler handler)
    {
        if (!ENEMY_COMPETITION_DO_RAISE) return;
        handler.EnemyRaise(EmotionType.Fear, _enemy);
    }
}
