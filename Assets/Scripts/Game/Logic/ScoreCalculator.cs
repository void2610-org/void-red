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

    public static float CalculateScoreWithoutEnemy(PlayerMove playerMove, ThemeData theme)
    {
        // テーマから該当属性の倍率を取得
        var attributeMultiplier = theme.GetMultiplier(playerMove.SelectedCard.Attribute);

        // プレイスタイルによるスコア倍率を取得
        var playStyleMultiplier = playerMove.PlayStyle.GetScoreMultiplier();

        // ベット額によるスコア補正倍率を取得
        var betScoreMultiplier = GetBetScoreMultiplier(playerMove.MentalBet);

        // スコア = 属性倍率 × 精神ベット × 固有の倍率 × プレイスタイル倍率 × ベット補正倍率
        return attributeMultiplier * playerMove.MentalBet * playStyleMultiplier * betScoreMultiplier;
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
        var baseScore = CalculateScoreWithoutEnemy(playerMove, theme);
        // PlayStyle相性倍率を取得（有利: 1.2倍, 不利: 0.9倍, 同じ: 1.0倍）
        var advantageMultiplier = playerMove.PlayStyle.GetAdvantageMultiplier(opponentMove.PlayStyle);

        // 相性倍率を適用して最終スコアを返す
        return baseScore * advantageMultiplier;
    }
}