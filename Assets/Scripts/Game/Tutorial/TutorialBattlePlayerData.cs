using System;

/// <summary>
/// チュートリアルバトル中のプレイヤー制約を管理するデータ
/// </summary>
public sealed class TutorialBattlePlayerData
{
    public int BidForcedCardIndex => 0;
    public EmotionType BidForcedEmotion => EmotionType.Joy;
    public int BidRequiredAmount => 3;

    public int CompetitionRequiredRaises => 2;
    public EmotionType CompetitionForcedEmotion => EmotionType.Trust;

    public int[] DeckAllowedCardIndices => new[] { 0, 1, 2 };

    public VictoryCondition BattleVictoryCondition => VictoryCondition.LowerWins;
    public bool[] CoinFlipPerRound => new[] { true, false, true };
    public int?[] ForcedCardPerRound => new int?[] { 0, null, null };
    public int SkillRoundIndex => 1;
    public EmotionType ForcedSkillEmotion => EmotionType.Joy;
}
