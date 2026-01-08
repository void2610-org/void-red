/// <summary>
/// ゲーム全体で使用される定数を管理するクラス
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// 感情リソースのデフォルト値
    /// </summary>
    public const int DEFAULT_EMOTION_VALUE = 10;

    /// <summary>
    /// 各プレイヤーの手札枚数
    /// </summary>
    public const int CARDS_PER_PLAYER = 4;

    /// <summary>
    /// 価値順位の基準リソース値（順位1の値、順位が下がるごとに-1）
    /// 順位1=4, 順位2=3, 順位3=2, 順位4=1
    /// </summary>
    public const int VALUE_RANKING_BASE_RESOURCE = 4;

    /// <summary>
    /// 基本報酬の最大値（順位1の報酬）
    /// 順位1=6, 順位2=5, 順位3=4, 順位4=3
    /// </summary>
    public const int BASE_REWARD_MAX = 6;

    /// <summary>
    /// 自分のカードを落札した場合のボーナス
    /// </summary>
    public const int OWN_CARD_BONUS = 2;
}