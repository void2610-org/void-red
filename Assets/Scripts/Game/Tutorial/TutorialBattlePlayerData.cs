using System;
using System.Collections.Generic;

/// <summary>
/// チュートリアルバトル中のプレイヤー制約を管理するデータ
/// </summary>
public sealed class TutorialBattlePlayerData
{
    public int BidForcedCardIndex => 0;
    public EmotionType BidForcedEmotion => EmotionType.Joy;
    public int BidRequiredAmount => 3;

    public int AuctionCompetitionRequiredRaises => 2;
    public EmotionType AuctionCompetitionForcedEmotion => EmotionType.Trust;
    public int BattleCompetitionRequiredRaises => 1;
    public EmotionType BattleCompetitionForcedEmotion => EmotionType.Trust;

    public IReadOnlyList<int> DeckAllowedCardIndices => _deckAllowedCardIndices;

    public VictoryCondition BattleVictoryCondition => VictoryCondition.LowerWins;
    public IReadOnlyList<bool> CoinFlipPerRound => _coinFlipPerRound;
    public IReadOnlyList<int?> ForcedCardPerRound => _forcedCardPerRound;
    public int SkillRoundIndex => 1;
    public EmotionType BattleForcedSkillEmotion => EmotionType.Joy;
    private static readonly IReadOnlyList<int> _deckAllowedCardIndices = new[] { 0, 1, 2 };
    private static readonly IReadOnlyList<bool> _coinFlipPerRound = new[] { true, false, true };
    private static readonly IReadOnlyList<int?> _forcedCardPerRound = new int?[] { 0, null, null };
}
