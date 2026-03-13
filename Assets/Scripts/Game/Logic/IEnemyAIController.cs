using System.Collections.Generic;

/// <summary>
/// 敵AIの全意思決定を抽象化するインターフェース
/// </summary>
public interface IEnemyAIController
{
    public List<CardModel> SelectDeck(List<CardModel> availableCards);
    public void DecideBids(IReadOnlyList<CardModel> auctionCards);
    public void TryCompetitionRaise(CompetitionHandler handler);
    public void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck);
    public EmotionType DecideEmotionState();
    public bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState);
}
