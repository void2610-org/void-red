using System.Collections.Generic;

/// <summary>
/// 敵AIの全意思決定を抽象化するインターフェース
/// </summary>
public interface IEnemyAIController
{
    List<CardModel> SelectDeck(List<CardModel> availableCards);
    void DecideBids(IReadOnlyList<CardModel> auctionCards);
    void TryCompetitionRaise(CompetitionHandler handler);
    void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck);
    EmotionType DecideEmotionState();
    bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState);
}
