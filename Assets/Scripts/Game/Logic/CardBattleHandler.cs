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

    /// <summary>バトル数字が同数で競合が必要か</summary>
    public bool RequiresCompetition => PlayerCard != null
                                    && EnemyCard != null
                                    && PlayerCard.BattleNumber == EnemyCard.BattleNumber;

    /// <summary>プレイヤーのスキルが使用可能か</summary>
    public bool PlayerSkillAvailable { get; private set; } = true;

    /// <summary>敵のスキルが使用可能か</summary>
    public bool EnemySkillAvailable { get; private set; } = true;

    // 勝利条件の一時反転（怒りスキル用）
    private bool _conditionReversedNextTurn;
    // 次に出すカードへの2倍予約（喜びスキル用）
    private bool _isPlayerNextCardDoubled;
    private bool _isEnemyNextCardDoubled;
    // 次に出すカードへの半減予約（嫌悪スキル用）
    private bool _isPlayerNextCardHalved;
    private bool _isEnemyNextCardHalved;
    private CardModel _previewedPlayerNextCard;
    private int _previewedPlayerNextCardOriginalNumber;

    public CardBattleHandler(VictoryCondition condition, bool isPlayerSkillAvailable = true)
    {
        BaseCondition = condition;
        // デッキ選択中に使ったスキルは、バトル開始時点で使用済みとして引き継ぐ
        PlayerSkillAvailable = isPlayerSkillAvailable;
    }

    /// <summary>コイントスで先攻後攻を決定</summary>
    public void DecideFirstPlayer() => IsPlayerFirst = Random.value > 0.5f;

    /// <summary>先攻後攻を直接設定する</summary>
    public void SetFirstPlayer(bool isPlayerFirst) => IsPlayerFirst = isPlayerFirst;

    /// <summary>怒りスキル: 次ターンの勝利条件を反転する予約を積む</summary>
    public void QueueConditionReversedNextTurn() => _conditionReversedNextTurn = true;

    /// <summary>嫌悪スキル: 次に出すカードの半減予約を積む</summary>
    public void QueueNextCardHalved(bool isPlayerSide)
    {
        if (isPlayerSide)
            _isPlayerNextCardHalved = true;
        else
            _isEnemyNextCardHalved = true;
    }

    /// <summary>喜びスキル: 次に出すカードの2倍予約を積む</summary>
    public void QueueNextCardDoubled(bool isPlayerSide)
    {
        if (isPlayerSide)
            _isPlayerNextCardDoubled = true;
        else
            _isEnemyNextCardDoubled = true;
    }

    /// <summary>プレイヤーがカードを伏せる</summary>
    public void PlacePlayerCard(CardModel card)
    {
        PlayerCard = card;
        ConsumePlayerNextCardPreview(card);
        ApplyPendingCardEffects(isPlayerSide: true, PlayerCard);
    }

    /// <summary>敵がカードを伏せる</summary>
    public void PlaceEnemyCard(CardModel card)
    {
        EnemyCard = card;
        ApplyPendingCardEffects(isPlayerSide: false, EnemyCard);
    }

    /// <summary>スキル使用権のみ消費する（効果は後から適用する場合に使用）</summary>
    public bool TryConsumePlayerSkill()
    {
        if (!PlayerSkillAvailable) return false;
        // 効果の適用タイミングを後ろへずらす場合でも、使用権だけは先に消費する
        PlayerSkillAvailable = false;
        return true;
    }

    /// <summary>敵スキルの使用権を消費する</summary>
    public bool TryConsumeEnemySkill()
    {
        if (!EnemySkillAvailable) return false;
        EnemySkillAvailable = false;
        return true;
    }

    /// <summary>嫌悪スキルの予約結果を仮置きカードへ先に反映する</summary>
    public void PreviewPlayerNextCardEffects(CardModel card)
    {
        if ((!_isPlayerNextCardDoubled && !_isPlayerNextCardHalved) || card == null) return;

        if (_previewedPlayerNextCard != null && _previewedPlayerNextCard != card)
            _previewedPlayerNextCard.SetBattleNumber(_previewedPlayerNextCardOriginalNumber);

        if (_previewedPlayerNextCard != card)
        {
            _previewedPlayerNextCard = card;
            _previewedPlayerNextCardOriginalNumber = card.BattleNumber;
            card.SetBattleNumber(ApplyReservedModifiers(_previewedPlayerNextCardOriginalNumber, isPlayerSide: true));
        }
    }

    /// <summary>
    /// カードオープンして勝敗判定
    /// 同数の場合は競合フェーズ結果で判定し、未決定なら入札リソース総量で比較
    /// </summary>
    /// <param name="competitionWinner">競合フェーズで勝者が決まっていればその結果。完全同数ならnull</param>
    public RoundResult ResolveRound(bool? competitionWinner = null)
    {
        // 勝利条件を決定（怒りスキルによる反転チェック）
        var condition = _conditionReversedNextTurn
            ? GetReversedCondition(BaseCondition)
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

        if (competitionWinner == true)
            return RecordResult(true);

        if (competitionWinner == false)
            return RecordResult(false);

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

    /// <summary>
    /// 勝利条件を逆転した値へ変換する
    /// </summary>
    private static VictoryCondition GetReversedCondition(VictoryCondition condition)
    {
        return condition == VictoryCondition.LowerWins ? VictoryCondition.HigherWins : VictoryCondition.LowerWins;
    }

    /// <summary>
    /// ラウンド結果を勝利数へ反映する
    /// </summary>
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

    /// <summary>
    /// 予約されている次カード補正を、確定カードへ消費する
    /// </summary>
    private void ApplyPendingCardEffects(bool isPlayerSide, CardModel card)
    {
        if (card == null) return;

        if (isPlayerSide && (_isPlayerNextCardDoubled || _isPlayerNextCardHalved))
        {
            card.SetBattleNumber(ApplyReservedModifiers(card.BattleNumber, isPlayerSide: true));
            _isPlayerNextCardDoubled = false;
            _isPlayerNextCardHalved = false;
        }
        else if (!isPlayerSide && (_isEnemyNextCardDoubled || _isEnemyNextCardHalved))
        {
            card.SetBattleNumber(ApplyReservedModifiers(card.BattleNumber, isPlayerSide: false));
            _isEnemyNextCardDoubled = false;
            _isEnemyNextCardHalved = false;
        }
    }

    /// <summary>
    /// 仮置きプレビューが確定した時に、予約状態を消費してプレビュー情報をクリアする
    /// </summary>
    private void ConsumePlayerNextCardPreview(CardModel card)
    {
        if (_previewedPlayerNextCard != card) return;

        _isPlayerNextCardDoubled = false;
        _isPlayerNextCardHalved = false;
        _previewedPlayerNextCard = null;
        _previewedPlayerNextCardOriginalNumber = 0;
    }

    /// <summary>
    /// 予約済みの2倍・半減補正を数字へ適用する
    /// </summary>
    private int ApplyReservedModifiers(int baseNumber, bool isPlayerSide)
    {
        var value = baseNumber;
        var isNextCardDoubled = isPlayerSide ? _isPlayerNextCardDoubled : _isEnemyNextCardDoubled;
        var isNextCardHalved = isPlayerSide ? _isPlayerNextCardHalved : _isEnemyNextCardHalved;

        if (isNextCardDoubled)
            value *= 2;

        if (isNextCardHalved)
            value = Mathf.Max(1, value / 2);

        return value;
    }
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
