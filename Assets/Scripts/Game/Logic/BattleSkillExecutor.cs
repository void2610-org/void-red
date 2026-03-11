using UnityEngine;

/// <summary>
/// スキル発動時の即時効果と予約効果の振り分けを担当する
/// </summary>
public static class BattleSkillExecutor
{
    /// <summary>
    /// スキル効果の日本語説明を取得する
    /// </summary>
    public static string GetDescription(EmotionType skill) => skill switch
    {
        EmotionType.Anger => "次のターンのみ勝利条件が逆になる",
        EmotionType.Anticipation => "自分の残りカードの数字を全てランダムに変える",
        EmotionType.Joy => "自分の次に出すカードの数字を2倍にする",
        EmotionType.Trust => "一度使ったカードがもう一度使える",
        EmotionType.Fear => "相手と自分のカードの数字を入れ替える",
        EmotionType.Surprise => "自分の出したカードの数字をランダムに変える",
        EmotionType.Disgust => "自分の次に出すカードの数字を2分の1にする",
        EmotionType.Sadness => "デッキ内の任意のカードの数字を3に変える",
        _ => ""
    };

    /// <summary>
    /// デッキ選択中に使用できるスキルか
    /// </summary>
    public static bool CanUseInDeckSelection(EmotionType skill) => skill == EmotionType.Anticipation;

    /// <summary>
    /// カード未選択でも使用できるスキルか
    /// </summary>
    public static bool CanActivateWithoutSelectedCard(EmotionType skill) => skill switch
    {
        EmotionType.Anger => true,
        EmotionType.Anticipation => true,
        EmotionType.Joy => true,
        EmotionType.Trust => true,
        EmotionType.Disgust => true,
        _ => false
    };

    /// <summary>
    /// 開示直前まで適用を遅らせるべきスキルか
    /// </summary>
    public static bool ShouldDeferUntilReveal(EmotionType skill) => skill == EmotionType.Fear;

    /// <summary>
    /// デッキ選択中のスキルを発動する
    /// </summary>
    /// <param name="skill">発動するスキル種別</param>
    /// <param name="previewDeck">デッキ選択画面で操作中の作業デッキ</param>
    /// <returns>デッキ選択中に発動できた場合はtrue</returns>
    public static bool TryActivateInDeckSelection(EmotionType skill, BattleDeckModel previewDeck)
    {
        if (!CanUseInDeckSelection(skill)) return false;

        Execute(skill, null, null, previewDeck, null, isPlayerSide: true);
        return true;
    }

    /// <summary>
    /// バトル中のスキルを発動または予約する
    /// </summary>
    /// <param name="skill">発動するスキル種別</param>
    /// <param name="myCard">スキル発動側がこのラウンドで出したカード。未選択時はnull</param>
    /// <param name="opponentCard">相手がこのラウンドで出したカード。未配置時はnull</param>
    /// <param name="myDeck">スキル発動側のバトル用デッキ</param>
    /// <param name="handler">予約効果やラウンド状態を保持するバトルハンドラ</param>
    /// <param name="isPlayerSide">発動側がプレイヤーならtrue、敵ならfalse</param>
    /// <param name="targetCardForSadness">悲しみスキルで3に変更する対象カード</param>
    public static void Execute(
        EmotionType skill,
        CardModel myCard,
        CardModel opponentCard,
        BattleDeckModel myDeck,
        CardBattleHandler handler,
        bool isPlayerSide,
        CardModel targetCardForSadness = null)
    {
        switch (skill)
        {
            case EmotionType.Anger:
                handler.QueueConditionReversedNextTurn();
                break;

            case EmotionType.Joy:
                handler.QueueNextCardDoubled(isPlayerSide);
                break;

            case EmotionType.Disgust:
                handler.QueueNextCardHalved(isPlayerSide);
                break;

            case EmotionType.Anticipation:
                // 自分の残りカードの数字を全てランダムに変える（被りあり）
                foreach (var card in myDeck.GetAvailableCards())
                    card.SetBattleNumber(Random.Range(1, 7));
                break;

            case EmotionType.Trust:
                // 直前に使ったカードが自動的に手札に戻る
                var lastUsed = myDeck.GetLastUsedCard();
                if (lastUsed != null)
                    myDeck.RestoreUsedCard(lastUsed);
                break;

            case EmotionType.Fear:
                // 相手と自分のカードの数字を入れ替える
                if (myCard != null && opponentCard != null)
                {
                    var temp = myCard.BattleNumber;
                    myCard.SetBattleNumber(opponentCard.BattleNumber);
                    opponentCard.SetBattleNumber(temp);
                }
                break;

            case EmotionType.Surprise:
                // 自分の出したカードの数字をランダムに変える
                if (myCard != null)
                    myCard.SetBattleNumber(Random.Range(1, 7));
                break;

            case EmotionType.Sadness:
                // 未使用カードから1枚を選んで数字を3に変える
                if (targetCardForSadness != null)
                    targetCardForSadness.SetBattleNumber(GameConstants.DEFAULT_CARD_NUMBER);
                break;
        }
    }
}
