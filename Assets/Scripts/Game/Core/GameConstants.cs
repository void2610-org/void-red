/// <summary>
/// ゲーム全体で使用される定数を管理するクラス
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// 感情リソースのデフォルト値（8種類×各3枚 = 合計24枚）
    /// </summary>
    public const int DEFAULT_EMOTION_VALUE = 3;

    /// <summary>
    /// オークションに並べるカード枚数
    /// </summary>
    public const int AUCTION_CARD_COUNT = 6;

    /// <summary>
    /// カードゲージの最大値
    /// </summary>
    public const int MAX_GAUGE_VALUE = 10;

    /// <summary>
    /// 競合フェーズのタイムアウト時間（秒）
    /// </summary>
    public const float COMPETITION_TIMEOUT_SECONDS = 10f;
}
