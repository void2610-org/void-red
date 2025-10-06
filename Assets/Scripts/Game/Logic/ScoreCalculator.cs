using UnityEngine;
using System.Linq;

/// <summary>
/// スコア計算を担当する静的クラス
/// ゲームルールに関するスコア計算ロジックを集約
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// ベット額によるスコア補正倍率を取得
    /// </summary>
    /// <param name="mentalBet">精神ベット値</param>
    /// <returns>スコア補正倍率</returns>
    private static float GetBetScoreMultiplier(int mentalBet)
    {
        return mentalBet switch
        {
            >= 1 and <= 5 => 0.8f,
            >= 6 and <= 10 => 1.0f,
            >= 11 and <= 15 => 1.3f,
            >= 16 and <= 20 => 1.6f,
            _ => 1.0f // デフォルト値
        };
    }

    /// <summary>
    /// キーワード一致によるボーナス倍率を取得
    /// </summary>
    /// <param name="card">カードデータ</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>キーワードボーナス倍率</returns>
    private static float GetKeywordMatchBonus(CardData card, ThemeData theme)
    {
        if (card.Keywords == null || theme.Keywords == null)
            return 1.0f;

        // カードとテーマで一致するキーワード数をカウント
        var matchCount = card.Keywords.Intersect(theme.Keywords).Count();

        return matchCount switch
        {
            0 => 1.0f,   // ボーナスなし
            1 => 1.1f,   // +10%
            2 => 1.25f,  // +25%
            3 => 1.5f,   // +50%
            _ => 1.5f + (matchCount - 3) * 0.2f // 4つ以上: +50% + 追加20%/キーワード
        };
    }

    /// <summary>
    /// プレイヤーの手のスコアを計算
    /// </summary>
    /// <param name="move">プレイヤーの手（カード選択、プレイスタイル、精神ベット）</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>計算されたスコア</returns>
    public static float CalculateScore(PlayerMove move, ThemeData theme)
    {
        if (move == null || !theme) return 0f;

        // テーマから該当属性の倍率を取得
        var attributeMultiplier = theme.GetMultiplier(move.SelectedCard.Attribute);

        // プレイスタイルによるスコア倍率を取得
        var playStyleMultiplier = move.PlayStyle.GetScoreMultiplier();

        // ベット額によるスコア補正倍率を取得
        var betScoreMultiplier = GetBetScoreMultiplier(move.MentalBet);

        // キーワード一致によるボーナス倍率を取得
        var keywordBonus = GetKeywordMatchBonus(move.SelectedCard, theme);

        // スコア = 属性倍率 × 精神ベット × カード固有の倍率 × プレイスタイル倍率 × ベット補正倍率 × キーワードボーナス
        return attributeMultiplier * move.MentalBet * move.SelectedCard.ScoreMultiplier * playStyleMultiplier * betScoreMultiplier * keywordBonus;
    }
}