using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 獲得テーマのシリアライズ用データクラス
/// セーブデータに保存される形式
/// </summary>
[Serializable]
public class SavedAcquiredTheme
{
    /// <summary>
    /// テーマのID（ThemeData.ThemeIdに対応）
    /// </summary>
    [SerializeField] private string themeId;

    /// <summary>
    /// 獲得カードの詳細情報リスト
    /// </summary>
    [SerializeField] private List<SavedCardAcquisitionInfo> cardInfoList = new();

    /// <summary>
    /// プレイヤーの感情リソース使用量
    /// Key: EmotionType (int), Value: 使用量
    /// </summary>
    [SerializeField] private List<int> usedEmotionTypes = new();
    [SerializeField] private List<int> usedEmotionAmounts = new();

    /// <summary>
    /// オークションでの総獲得カード数（プレイヤー勝利数）
    /// </summary>
    [SerializeField] private int wonCardCount;

    /// <summary>
    /// オークションでの総敗北数
    /// </summary>
    [SerializeField] private int lostCardCount;

    // プロパティ
    public string ThemeId => themeId;
    public IReadOnlyList<SavedCardAcquisitionInfo> CardInfoList => cardInfoList;
    public int WonCardCount => wonCardCount;
    public int LostCardCount => lostCardCount;

    /// <summary>
    /// 支配的な感情タイプ（勝利カードの入札から計算）
    /// </summary>
    public EmotionType DominantEmotion =>
        MemoryEmotionCalculator.CalculateFromSavedCardInfoList(cardInfoList);

    /// <summary>
    /// 支配的感情の計算結果（複合感情も考慮）
    /// </summary>
    public DominantEmotionResult DominantEmotionResult =>
        MemoryEmotionCalculator.CalculateWithCompoundFromSavedCardInfoList(cardInfoList);

    /// <summary>
    /// 獲得したカードIDのリスト（互換性用）
    /// </summary>
    public IReadOnlyList<string> CardIds => cardInfoList
        .Where(info => info.PlayerWon)
        .Select(info => info.CardId)
        .ToList();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SavedAcquiredTheme(
        string themeId,
        IEnumerable<SavedCardAcquisitionInfo> cardInfoList,
        Dictionary<EmotionType, int> usedEmotions)
    {
        this.themeId = themeId;
        this.cardInfoList = new List<SavedCardAcquisitionInfo>(cardInfoList);

        // 感情リソース使用量を保存
        foreach (var kvp in usedEmotions)
        {
            usedEmotionTypes.Add((int)kvp.Key);
            usedEmotionAmounts.Add(kvp.Value);
        }

        // 勝敗カウントを計算
        wonCardCount = this.cardInfoList.Count(info => info.PlayerWon);
        lostCardCount = this.cardInfoList.Count(info => !info.PlayerWon);
    }

    /// <summary>
    /// 使用した感情リソースを取得
    /// </summary>
    public Dictionary<EmotionType, int> GetUsedEmotions()
    {
        var result = new Dictionary<EmotionType, int>();
        for (var i = 0; i < usedEmotionTypes.Count && i < usedEmotionAmounts.Count; i++)
        {
            result[(EmotionType)usedEmotionTypes[i]] = usedEmotionAmounts[i];
        }
        return result;
    }

    /// <summary>
    /// 全カードの感情別入札合計を取得（プレイヤー側）
    /// </summary>
    public Dictionary<EmotionType, int> GetTotalPlayerBidsByEmotion()
    {
        var result = new Dictionary<EmotionType, int>();
        foreach (var cardInfo in cardInfoList)
        {
            foreach (var kvp in cardInfo.GetPlayerBidsByEmotion())
            {
                result.TryAdd(kvp.Key, 0);
                result[kvp.Key] += kvp.Value;
            }
        }
        return result;
    }
}
