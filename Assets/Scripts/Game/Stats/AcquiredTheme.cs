using System.Collections.Generic;
using System.Linq;

/// <summary>
/// カード獲得情報（実行時データ）
/// オークションでの各カードの入札・勝敗情報を保持
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
        bool playerWon)
    {
        Card = card;
        PlayerBids = new Dictionary<EmotionType, int>(playerBids);
        EnemyBids = new Dictionary<EmotionType, int>(enemyBids);
        PlayerWon = playerWon;
    }

    /// <summary>
    /// シリアライズ用データに変換
    /// </summary>
    public SavedCardAcquisitionInfo ToSavedData() => new SavedCardAcquisitionInfo(
        Card.Data.CardId,
        new Dictionary<EmotionType, int>(PlayerBids),
        new Dictionary<EmotionType, int>(EnemyBids),
        PlayerWon
    );
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
    /// 支配的感情の計算結果（複合感情も考慮）
    /// </summary>
    public DominantEmotionResult DominantEmotionResult => MemoryEmotionCalculator.CalculateWithCompoundFromCardInfoList(AllCardInfoList);

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
    /// テーマ名
    /// </summary>
    public string ThemeName => Theme.Title;

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
}
