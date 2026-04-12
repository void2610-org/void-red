/// <summary>
/// ゲーム全体で使用される定数を管理するクラス
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// 感情リソースのデフォルト値（8種類×各5枚 = 合計40枚）
    /// </summary>
    public const int DEFAULT_EMOTION_VALUE = 5;

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
    public const float COMPETITION_TIMEOUT_SECONDS = 5f;

    /// <summary>
    /// バトルのラウンド数（3本勝負）
    /// </summary>
    public const int BATTLE_ROUND_COUNT = 3;

    /// <summary>
    /// バトルの勝利に必要な本数
    /// </summary>
    public const int BATTLE_WINS_REQUIRED = 2;

    /// <summary>
    /// デッキの枚数
    /// </summary>
    public const int DECK_SIZE = 3;

    /// <summary>
    /// 不足カードのデフォルト数字
    /// </summary>
    public const int DEFAULT_CARD_NUMBER = 3;

    /// <summary>
    /// 感情マッチ倍率
    /// </summary>
    public const float EMOTION_MATCH_MULTIPLIER = 1.5f;

    /// <summary>
    /// 自己記憶倍率
    /// </summary>
    public const float SELF_MEMORY_MULTIPLIER = 2.0f;
}
