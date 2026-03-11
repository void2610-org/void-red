using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵AIの全意思決定を一元管理するコントローラ
/// </summary>
public class EnemyAIController
{
    private readonly Enemy _enemy;

    public EnemyAIController(Enemy enemy)
    {
        _enemy = enemy;
    }

    /// <summary>
    /// デッキ選択（ランダム3枚）
    /// </summary>
    public List<CardModel> SelectDeck(List<CardModel> availableCards) =>
        availableCards
            .OrderBy(_ => Random.value)
            .Take(GameConstants.DECK_SIZE)
            .ToList();

    /// <summary>
    /// 入札決定（ランダム感情・ランダム額）
    /// </summary>
    public void DecideBids(IReadOnlyList<CardModel> auctionCards)
    {
        _enemy.Bids.Clear();
        if (auctionCards.Count == 0) return;

        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        var remainingResources = new Dictionary<EmotionType, int>();
        foreach (var emotion in emotions)
            remainingResources[emotion] = _enemy.GetEmotionAmount(emotion);

        // カードをシャッフルしてランダムに入札
        var shuffledCards = auctionCards.OrderBy(_ => Random.value).ToList();

        foreach (var card in shuffledCards)
        {
            // ランダムに感情を選択（リソースが残っているものから）
            var availableEmotions = emotions.Where(e => remainingResources[e] > 0).ToList();
            if (availableEmotions.Count == 0) break;

            var emotion = availableEmotions[Random.Range(0, availableEmotions.Count)];
            var available = remainingResources[emotion];
            var amount = Random.Range(1, available + 1);

            _enemy.Bids.SetBid(card, emotion, amount);
            remainingResources[emotion] -= amount;
        }
    }

    /// <summary>
    /// 競合時の上乗せ判定（50%確率）
    /// </summary>
    public void TryCompetitionRaise(CompetitionHandler handler)
    {
        // 既にプレイヤーより多い場合は無駄に消費しない
        if (handler.EnemyTotal > handler.PlayerTotal) return;

        // 50%の確率で上乗せしない
        if (Random.value < 0.5f) return;

        // リソースが残っている感情からランダムに選択
        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        var available = new List<EmotionType>();
        foreach (var emotion in emotions)
        {
            if (_enemy.GetEmotionAmount(emotion) > 0)
                available.Add(emotion);
        }

        if (available.Count == 0) return;

        var chosen = available[Random.Range(0, available.Count)];
        handler.EnemyRaise(chosen, _enemy);
    }

    /// <summary>
    /// バトル中のカード配置（ランダム）
    /// </summary>
    public void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck)
    {
        var availableCards = enemyDeck.GetAvailableCards();
        if (availableCards.Count == 0) return;

        var card = availableCards[Random.Range(0, availableCards.Count)];
        handler.PlaceEnemyCard(card);
        enemyDeck.MarkAsUsed(card);
    }

    /// <summary>
    /// バトル用感情状態のランダム決定
    /// </summary>
    public EmotionType DecideEmotionState()
    {
        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        return emotions[Random.Range(0, emotions.Length)];
    }

    /// <summary>
    /// スキル発動判定（50%確率）
    /// </summary>
    /// <param name="handler">現在ラウンドの状態とスキル予約を保持するハンドラ</param>
    /// <param name="enemyDeck">敵が現在使用しているバトル用デッキ</param>
    /// <param name="emotionState">今回のバトルで敵に割り当てられた感情状態</param>
    /// <returns>実際にスキルを発動した場合はtrue</returns>
    public bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState)
    {
        if (!handler.EnemySkillAvailable) return false;
        if (Random.value <= 0.5f) return false;

        if (!handler.TryConsumeEnemySkill()) return false;

        var targetCardForSadness = emotionState == EmotionType.Sadness
            ? SelectSadnessTarget(enemyDeck)
            : null;
        BattleSkillExecutor.Execute(
            emotionState,
            handler.EnemyCard,
            handler.PlayerCard,
            enemyDeck,
            handler,
            isPlayerSide: false,
            targetCardForSadness);
        return true;
    }

    /// <summary>悲しみスキルで数字を3に変える対象カードをランダム選択する</summary>
    private static CardModel SelectSadnessTarget(BattleDeckModel enemyDeck)
    {
        var availableCards = enemyDeck.GetAvailableCards();
        if (availableCards.Count == 0) return null;

        return availableCards[Random.Range(0, availableCards.Count)];
    }
}
