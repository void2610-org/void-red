using System.Linq;
using UnityEngine;

/// <summary>
/// 感情状態に応じたスキル効果を適用する
/// </summary>
public static class SkillEffectApplier
{
    /// <summary>
    /// スキル効果の日本語説明を取得
    /// </summary>
    public static string GetDescription(EmotionType emotion) => emotion switch
    {
        EmotionType.Anger => "次のターンのみ勝利条件が逆になる",
        EmotionType.Anticipation => "自分の残りカードの数字を全てランダムに変える",
        EmotionType.Joy => "自分の出したカードの数字を2倍にする",
        EmotionType.Trust => "一度使ったカードがもう一度使える",
        EmotionType.Fear => "相手と自分のカードの数字を入れ替える",
        EmotionType.Surprise => "自分の出したカードの数字をランダムに変える",
        EmotionType.Disgust => "自分の出したカードの数字を2分の1にする",
        EmotionType.Sadness => "デッキ内の任意のカードの数字を3に変える",
        _ => ""
    };

    /// <summary>
    /// スキル効果を適用する
    /// </summary>
    /// <param name="emotion">発動するスキルの感情タイプ</param>
    /// <param name="myCard">自分の出したカード</param>
    /// <param name="opponentCard">相手の出したカード</param>
    /// <param name="myDeck">自分のデッキ</param>
    /// <param name="handler">バトルハンドラ</param>
    /// <param name="targetCardForSadness">悲しみスキル: 数字を3に変える対象カード</param>
    public static void Apply(
        EmotionType emotion,
        CardModel myCard,
        CardModel opponentCard,
        BattleDeckModel myDeck,
        CardBattleHandler handler,
        CardModel targetCardForSadness = null)
    {
        switch (emotion)
        {
            case EmotionType.Anger:
                // 次のターンのみ勝利条件が逆になる
                handler.ReverseConditionNextTurn();
                break;

            case EmotionType.Anticipation:
                // 自分の残りカードの数字を全てランダムに変える（被りあり）
                foreach (var card in myDeck.GetAvailableCards())
                    card.SetBattleNumber(Random.Range(1, 7));
                break;

            case EmotionType.Joy:
                // 自分の出したカードの数字を2倍にする
                if (myCard != null)
                    myCard.SetBattleNumber(myCard.BattleNumber * 2);
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

            case EmotionType.Disgust:
                // 自分の出したカードの数字を2分の1にする（切り捨て、最低1）
                if (myCard != null)
                    myCard.SetBattleNumber(Mathf.Max(1, myCard.BattleNumber / 2));
                break;

            case EmotionType.Sadness:
                // 未使用カードから1枚を選んで数字を3に変える
                if (targetCardForSadness != null)
                    targetCardForSadness.SetBattleNumber(GameConstants.DEFAULT_CARD_NUMBER);
                break;
        }
    }
}
