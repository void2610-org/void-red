using UnityEngine;
using System.Collections.Generic;
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
    /// キーワード一致数を取得
    /// </summary>
    /// <param name="card">カードデータ</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>一致したキーワード数</returns>
    public static int GetKeywordMatchCount(CardData card, ThemeData theme)
    {
        if (card.Keywords == null || theme.Keywords == null)
            return 0;

        return card.Keywords.Intersect(theme.Keywords).Count();
    }

    /// <summary>
    /// 一致したキーワードのリストを取得
    /// </summary>
    /// <param name="card">カードデータ</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>一致したキーワードのリスト</returns>
    public static List<KeywordType> GetMatchedKeywords(CardData card, ThemeData theme)
    {
        if (card.Keywords == null || theme.Keywords == null)
            return new List<KeywordType>();

        return card.Keywords.Intersect(theme.Keywords).ToList();
    }

    /// <summary>
    /// キーワード一致によるボーナス倍率を取得
    /// </summary>
    /// <param name="card">カードデータ</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>キーワードボーナス倍率</returns>
    private static float GetKeywordMatchBonus(CardData card, ThemeData theme)
    {
        var matchCount = GetKeywordMatchCount(card, theme);

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
    /// プレイヤーの手のスコアを計算（PlayStyle相性を考慮）
    /// </summary>
    /// <param name="playerMove">プレイヤーの手（カード選択、プレイスタイル、精神ベット）</param>
    /// <param name="opponentMove">相手の手（PlayStyle相性判定用）</param>
    /// <param name="theme">テーマデータ</param>
    /// <returns>PlayStyle相性を考慮した計算スコア</returns>
    public static float CalculateScore(PlayerMove playerMove, PlayerMove opponentMove, ThemeData theme)
    {
        // テーマから該当属性の倍率を取得
        var attributeMultiplier = theme.GetMultiplier(playerMove.SelectedCard.Attribute);

        // プレイスタイルによるスコア倍率を取得
        var playStyleMultiplier = playerMove.PlayStyle.GetScoreMultiplier();

        // ベット額によるスコア補正倍率を取得
        var betScoreMultiplier = GetBetScoreMultiplier(playerMove.MentalBet);

        // キーワード一致によるボーナス倍率を取得
        var keywordBonus = GetKeywordMatchBonus(playerMove.SelectedCard, theme);

        // スコア = 属性倍率 × 精神ベット × カード固有の倍率 × プレイスタイル倍率 × ベット補正倍率 × キーワードボーナス
        var baseScore = attributeMultiplier * playerMove.MentalBet * playerMove.SelectedCard.ScoreMultiplier * playStyleMultiplier * betScoreMultiplier * keywordBonus;

        // PlayStyle相性倍率を取得（有利: 1.2倍, 不利: 0.9倍, 同じ: 1.0倍）
        var advantageMultiplier = playerMove.PlayStyle.GetAdvantageMultiplier(opponentMove.PlayStyle);

        // 相性倍率を適用して最終スコアを返す
        return baseScore * advantageMultiplier;
    }
}