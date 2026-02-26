using UnityEngine;

/// <summary>
/// 3本勝負のカードバトルを管理する
/// </summary>
public class CardBattleHandler
{
    /// <summary>基本勝利条件</summary>
    public VictoryCondition BaseCondition { get; }

    /// <summary>プレイヤーの勝利数</summary>
    public int PlayerWins { get; private set; }

    /// <summary>敵の勝利数</summary>
    public int EnemyWins { get; private set; }

    /// <summary>現在のラウンド（0始まり）</summary>
    public int CurrentRound { get; private set; }

    /// <summary>プレイヤーが先攻か</summary>
    public bool IsPlayerFirst { get; private set; }

    /// <summary>バトル終了か</summary>
    public bool IsFinished => PlayerWins >= GameConstants.BATTLE_WINS_REQUIRED
                           || EnemyWins >= GameConstants.BATTLE_WINS_REQUIRED;

    /// <summary>プレイヤーが勝利したか（バトル終了後に使用）</summary>
    public bool IsPlayerWon => PlayerWins >= GameConstants.BATTLE_WINS_REQUIRED;

    /// <summary>現ラウンドでプレイヤーが伏せたカード</summary>
    public CardModel PlayerCard { get; private set; }

    /// <summary>現ラウンドで敵が伏せたカード</summary>
    public CardModel EnemyCard { get; private set; }

    /// <summary>プレイヤーのスキルが使用可能か</summary>
    public bool PlayerSkillAvailable { get; private set; } = true;

    /// <summary>敵のスキルが使用可能か</summary>
    public bool EnemySkillAvailable { get; private set; } = true;

    // 勝利条件の一時反転（怒りスキル用）
    private bool _conditionReversedNextTurn;

    public CardBattleHandler(VictoryCondition condition)
    {
        BaseCondition = condition;
    }

    /// <summary>コイントスで先攻後攻を決定</summary>
    public void DecideFirstPlayer() => IsPlayerFirst = Random.value > 0.5f;

    /// <summary>プレイヤーがカードを伏せる</summary>
    public void PlacePlayerCard(CardModel card) => PlayerCard = card;

    /// <summary>敵がカードを伏せる</summary>
    public void PlaceEnemyCard(CardModel card) => EnemyCard = card;

    /// <summary>怒りスキル: 次ターンの勝利条件を反転させる</summary>
    public void ReverseConditionNextTurn() => _conditionReversedNextTurn = true;

    /// <summary>プレイヤーがスキルを発動</summary>
    public bool TryActivatePlayerSkill(
        EmotionType skill,
        BattleDeckModel playerDeck,
        CardModel targetCardForSadness = null)
    {
        if (!PlayerSkillAvailable) return false;
        PlayerSkillAvailable = false;
        SkillEffectApplier.Apply(skill, PlayerCard, EnemyCard, playerDeck, this, targetCardForSadness);
        return true;
    }

    /// <summary>敵がスキルを発動</summary>
    public bool TryActivateEnemySkill(
        EmotionType skill,
        BattleDeckModel enemyDeck)
    {
        if (!EnemySkillAvailable) return false;
        EnemySkillAvailable = false;
        // 敵視点: 自分のカード=EnemyCard, 相手=PlayerCard
        SkillEffectApplier.Apply(skill, EnemyCard, PlayerCard, enemyDeck, this);
        return true;
    }

    /// <summary>
    /// カードオープンして勝敗判定
    /// 同数の場合はオークション入札リソース総量で比較
    /// </summary>
    public RoundResult ResolveRound()
    {
        // 勝利条件を決定（怒りスキルによる反転チェック）
        var condition = _conditionReversedNextTurn
            ? ReverseCondition(BaseCondition)
            : BaseCondition;
        _conditionReversedNextTurn = false;

        // 数字が異なる場合
        if (PlayerCard.BattleNumber != EnemyCard.BattleNumber)
        {
            var playerWins = condition == VictoryCondition.LowerWins
                ? PlayerCard.BattleNumber < EnemyCard.BattleNumber
                : PlayerCard.BattleNumber > EnemyCard.BattleNumber;

            return RecordResult(playerWins);
        }

        // 同数の場合: オークション入札リソース総量で比較
        if (PlayerCard.AuctionBidTotal != EnemyCard.AuctionBidTotal)
        {
            return RecordResult(PlayerCard.AuctionBidTotal > EnemyCard.AuctionBidTotal);
        }

        // 入札量も同じ場合はランダム
        return RecordResult(Random.value > 0.5f);
    }

    /// <summary>次ラウンドへ進む</summary>
    public void NextRound()
    {
        CurrentRound++;
        PlayerCard = null;
        EnemyCard = null;
    }

    private RoundResult RecordResult(bool playerWins)
    {
        if (playerWins)
        {
            PlayerWins++;
            return RoundResult.PlayerWin;
        }

        EnemyWins++;
        return RoundResult.EnemyWin;
    }

    private static VictoryCondition ReverseCondition(VictoryCondition condition) =>
        condition == VictoryCondition.LowerWins
            ? VictoryCondition.HigherWins
            : VictoryCondition.LowerWins;
}

/// <summary>
/// ラウンド結果
/// </summary>
public enum RoundResult
{
    /// <summary>プレイヤー勝利</summary>
    PlayerWin,

    /// <summary>敵勝利</summary>
    EnemyWin,
}
