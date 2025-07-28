using UnityEngine;

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

        // スコア = 属性倍率 × 精神ベット × カード固有の倍率 × プレイスタイル倍率 × ベット補正倍率
        return attributeMultiplier * move.MentalBet * move.SelectedCard.ScoreMultiplier * playStyleMultiplier * betScoreMultiplier;
    }
}