using System.Collections.Generic;
using System.Linq;

/// <summary>
/// カード獲得情報（実行時データ）
/// オークションでの各カードの入札・順位・勝敗情報を保持
/// </summary>
public class CardAcquisitionInfo
{
    /// <summary>
    /// 対象カード
    /// </summary>
    public CardModel Card { get; }

    /// <summary>
    /// プレイヤーの感情別入札量
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> PlayerBids { get; }

    /// <summary>
    /// 敵の感情別入札量
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> EnemyBids { get; }

    /// <summary>
    /// プレイヤーの価値順位（1-4、未設定は0）
    /// </summary>
    public int PlayerValueRank { get; }

    /// <summary>
    /// 敵の価値順位（1-4、未設定は0）
    /// </summary>
    public int EnemyValueRank { get; }

    /// <summary>
    /// プレイヤーが勝利したか
    /// </summary>
    public bool PlayerWon { get; }

    /// <summary>
    /// プレイヤーの入札合計
    /// </summary>
    public int PlayerTotalBid => PlayerBids.Values.Sum();

    /// <summary>
    /// 敵の入札合計
    /// </summary>
    public int EnemyTotalBid => EnemyBids.Values.Sum();

    public CardAcquisitionInfo(
        CardModel card,
        Dictionary<EmotionType, int> playerBids,
        Dictionary<EmotionType, int> enemyBids,
        int playerValueRank,
        int enemyValueRank,
        bool playerWon)
    {
        Card = card;
        PlayerBids = new Dictionary<EmotionType, int>(playerBids);
        EnemyBids = new Dictionary<EmotionType, int>(enemyBids);
        PlayerValueRank = playerValueRank;
        EnemyValueRank = enemyValueRank;
        PlayerWon = playerWon;
    }

    /// <summary>
    /// シリアライズ用データに変換
    /// </summary>
    public SavedCardAcquisitionInfo ToSavedData()
    {
        return new SavedCardAcquisitionInfo(
            Card.Data.CardId,
            new Dictionary<EmotionType, int>(PlayerBids),
            new Dictionary<EmotionType, int>(EnemyBids),
            PlayerValueRank,
            EnemyValueRank,
            PlayerWon
        );
    }
}

/// <summary>
/// 獲得テーマの実行時データクラス
/// オークションで獲得したテーマとカードの詳細情報を保持
/// </summary>
public class AcquiredTheme
{
    /// <summary>
    /// テーマデータ参照
    /// </summary>
    public ThemeData Theme { get; }

    /// <summary>
    /// 全カードの獲得情報リスト（勝敗両方含む）
    /// </summary>
    public IReadOnlyList<CardAcquisitionInfo> AllCardInfoList { get; }

    /// <summary>
    /// 獲得したカードリスト（プレイヤー勝利のみ）
    /// </summary>
    public IReadOnlyList<CardModel> AcquiredCards { get; }

    /// <summary>
    /// 支配的な感情タイプ（勝利カードの入札から計算）
    /// </summary>
    public EmotionType DominantEmotion =>
        MemoryEmotionCalculator.CalculateFromCardInfoList(AllCardInfoList);

    /// <summary>
    /// 使用した感情リソース
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> UsedEmotions { get; }

    /// <summary>
    /// 獲得カード数
    /// </summary>
    public int WonCount => AcquiredCards.Count;

    /// <summary>
    /// 敗北カード数
    /// </summary>
    public int LostCount => AllCardInfoList.Count - WonCount;

    /// <summary>
    /// 獲得カード枚数に基づくテーマ名
    /// カード枚数に応じて表示が変化する
    /// </summary>
    public string ThemeName => GetThemeNameByCardCount();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public AcquiredTheme(
        ThemeData theme,
        IReadOnlyList<CardAcquisitionInfo> allCardInfoList,
        Dictionary<EmotionType, int> usedEmotions)
    {
        Theme = theme;
        AllCardInfoList = allCardInfoList;
        UsedEmotions = new Dictionary<EmotionType, int>(usedEmotions);

        // 勝利したカードのみ抽出
        AcquiredCards = allCardInfoList
            .Where(info => info.PlayerWon)
            .Select(info => info.Card)
            .ToList();
    }

    /// <summary>
    /// セーブデータ用に変換
    /// </summary>
    public SavedAcquiredTheme ToSavedData()
    {
        var cardInfoList = AllCardInfoList.Select(info => info.ToSavedData());
        return new SavedAcquiredTheme(
            Theme.ThemeId,
            cardInfoList,
            new Dictionary<EmotionType, int>(UsedEmotions)
        );
    }

    /// <summary>
    /// 全カードの感情別入札合計を取得（プレイヤー側）
    /// </summary>
    public Dictionary<EmotionType, int> GetTotalPlayerBidsByEmotion()
    {
        var result = new Dictionary<EmotionType, int>();
        foreach (var cardInfo in AllCardInfoList)
        {
            foreach (var kvp in cardInfo.PlayerBids)
            {
                result.TryAdd(kvp.Key, 0);
                result[kvp.Key] += kvp.Value;
            }
        }
        return result;
    }

    /// <summary>
    /// カード枚数に応じたテーマ名を取得
    /// 0枚: 「???」
    /// 1枚: 「断片的な〇〇」
    /// 2枚: 「曖昧な〇〇」
    /// 3枚: 「鮮明な〇〇」
    /// 4枚: 「完全な〇〇」
    /// </summary>
    private string GetThemeNameByCardCount()
    {
        var cardCount = AcquiredCards.Count;
        var baseTitle = Theme?.Title ?? "記憶";

        return cardCount switch
        {
            0 => "???",
            1 => $"断片的な{baseTitle}",
            2 => $"曖昧な{baseTitle}",
            3 => $"鮮明な{baseTitle}",
            _ => $"完全な{baseTitle}"
        };
    }
}
