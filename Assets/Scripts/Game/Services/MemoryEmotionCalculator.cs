using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 支配的感情の計算結果（基本感情または複合感情）
/// </summary>
public readonly struct DominantEmotionResult
{
    /// <summary>
    /// 基本感情（複合感情の場合は最も強い基本感情）
    /// </summary>
    public EmotionType PrimaryEmotion { get; }

    /// <summary>
    /// 複合感情（該当する場合のみ値が入る）
    /// </summary>
    public CompoundEmotionType? CompoundEmotion { get; }

    /// <summary>
    /// 複合感情かどうか
    /// </summary>
    public bool IsCompound => CompoundEmotion.HasValue;

    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public string JapaneseName => IsCompound
        ? CompoundEmotion.Value.ToJapaneseName()
        : PrimaryEmotion.ToJapaneseName();

    public DominantEmotionResult(EmotionType primary, CompoundEmotionType? compound = null)
    {
        PrimaryEmotion = primary;
        CompoundEmotion = compound;
    }

    /// <summary>
    /// 色を取得
    /// </summary>
    public Color GetColor() => IsCompound
        ? CompoundEmotion.Value.GetColor()
        : PrimaryEmotion.GetColor();

    /// <summary>
    /// 薄い色調を取得（キャラクター染色用）
    /// </summary>
    public Color GetTintColor() => IsCompound
        ? CompoundEmotion.Value.GetTintColor()
        : PrimaryEmotion.GetTintColor();
}

/// <summary>
/// 記憶育成フェーズで使用する感情計算ロジック
/// </summary>
public static class MemoryEmotionCalculator
{
    /// <summary>
    /// CardAcquisitionInfoリストから支配的感情を計算（勝利カードのみ対象）
    /// </summary>
    /// <param name="cardInfoList">カード獲得情報リスト</param>
    /// <returns>支配的な感情タイプ（デフォルトはJoy）</returns>
    public static EmotionType CalculateFromCardInfoList(IReadOnlyList<CardAcquisitionInfo> cardInfoList) => CalculateWithCompoundFromCardInfoList(cardInfoList).PrimaryEmotion;

    /// <summary>
    /// SavedCardAcquisitionInfoリストから支配的感情を計算（勝利カードのみ対象）
    /// </summary>
    /// <param name="cardInfoList">保存されたカード獲得情報リスト</param>
    /// <returns>支配的な感情タイプ（デフォルトはJoy）</returns>
    public static EmotionType CalculateFromSavedCardInfoList(IReadOnlyList<SavedCardAcquisitionInfo> cardInfoList) => CalculateWithCompoundFromSavedCardInfoList(cardInfoList).PrimaryEmotion;

    /// <summary>
    /// 落札したカードに対する入札から支配的感情を計算
    /// 各感情タイプの入札合計を集計し、最も多い感情を返す
    /// </summary>
    /// <param name="wonCards">落札したカードリスト</param>
    /// <param name="bids">入札データ</param>
    /// <returns>支配的な感情タイプ（デフォルトはJoy）</returns>
    public static EmotionType CalculateDominantEmotion(IReadOnlyList<CardModel> wonCards, BidModel bids)
    {
        if (wonCards == null || wonCards.Count == 0)
        {
            return EmotionType.Joy;
        }

        // 各感情タイプの合計を集計
        var emotionTotals = new Dictionary<EmotionType, int>();

        foreach (var card in wonCards)
        {
            var cardBids = bids.GetBidsByEmotion(card);
            foreach (var (emotion, amount) in cardBids)
            {
                emotionTotals.TryAdd(emotion, 0);
                emotionTotals[emotion] += amount;
            }
        }

        return CalculateFromEmotionTotals(emotionTotals);
    }

    /// <summary>
    /// 感情別入札合計から支配的感情を計算
    /// </summary>
    /// <param name="emotionTotals">感情タイプ別の入札合計</param>
    /// <returns>支配的な感情タイプ（デフォルトはJoy）</returns>
    public static EmotionType CalculateFromEmotionTotals(IReadOnlyDictionary<EmotionType, int> emotionTotals)
    {
        if (emotionTotals == null || emotionTotals.Count == 0)
        {
            return EmotionType.Joy;
        }

        // 最大値の感情を返す（同点時は先に見つかった方）
        return emotionTotals.OrderByDescending(e => e.Value).First().Key;
    }

    /// <summary>
    /// 感情別入札合計から複合感情も考慮した支配的感情を計算
    /// 上位2つの感情が隣接している場合、複合感情として解釈する
    /// </summary>
    /// <param name="emotionTotals">感情タイプ別の入札合計</param>
    /// <returns>支配的感情の計算結果（基本感情または複合感情）</returns>
    public static DominantEmotionResult CalculateWithCompound(IReadOnlyDictionary<EmotionType, int> emotionTotals)
    {
        if (emotionTotals == null || emotionTotals.Count == 0)
        {
            return new DominantEmotionResult(EmotionType.Joy);
        }

        // 値でソートして上位を取得
        var sorted = emotionTotals
            .Where(e => e.Value > 0)
            .OrderByDescending(e => e.Value)
            .ToList();

        if (sorted.Count == 0)
        {
            return new DominantEmotionResult(EmotionType.Joy);
        }

        var primary = sorted[0].Key;

        // 1つしかない場合は基本感情のみ
        if (sorted.Count == 1)
        {
            return new DominantEmotionResult(primary);
        }

        var secondary = sorted[1].Key;
        var primaryValue = sorted[0].Value;
        var secondaryValue = sorted[1].Value;

        // 2番目の感情が1番目の50%以上ある場合のみ複合感情を考慮
        if (secondaryValue >= primaryValue * 0.5f)
        {
            var compound = CompoundEmotionTypeExtensions.GetCompoundEmotion(primary, secondary);
            if (compound.HasValue)
            {
                return new DominantEmotionResult(primary, compound);
            }
        }

        return new DominantEmotionResult(primary);
    }

    /// <summary>
    /// CardAcquisitionInfoリストから複合感情も考慮した支配的感情を計算（勝利カードのみ対象）
    /// </summary>
    /// <param name="cardInfoList">カード獲得情報リスト</param>
    /// <returns>支配的感情の計算結果（基本感情または複合感情）</returns>
    public static DominantEmotionResult CalculateWithCompoundFromCardInfoList(IReadOnlyList<CardAcquisitionInfo> cardInfoList)
    {
        if (cardInfoList == null || cardInfoList.Count == 0)
        {
            return new DominantEmotionResult(EmotionType.Joy);
        }

        var emotionTotals = new Dictionary<EmotionType, int>();

        // 勝利したカードの入札のみ集計
        foreach (var cardInfo in cardInfoList.Where(c => c.PlayerWon))
        {
            foreach (var kvp in cardInfo.PlayerBids)
            {
                emotionTotals.TryAdd(kvp.Key, 0);
                emotionTotals[kvp.Key] += kvp.Value;
            }
        }

        return CalculateWithCompound(emotionTotals);
    }

    /// <summary>
    /// SavedCardAcquisitionInfoリストから複合感情も考慮した支配的感情を計算（勝利カードのみ対象）
    /// </summary>
    /// <param name="cardInfoList">保存されたカード獲得情報リスト</param>
    /// <returns>支配的感情の計算結果（基本感情または複合感情）</returns>
    public static DominantEmotionResult CalculateWithCompoundFromSavedCardInfoList(IReadOnlyList<SavedCardAcquisitionInfo> cardInfoList)
    {
        if (cardInfoList == null || cardInfoList.Count == 0)
        {
            return new DominantEmotionResult(EmotionType.Joy);
        }

        var emotionTotals = new Dictionary<EmotionType, int>();

        // 勝利したカードの入札のみ集計
        foreach (var cardInfo in cardInfoList.Where(c => c.PlayerWon))
        {
            foreach (var kvp in cardInfo.GetPlayerBidsByEmotion())
            {
                emotionTotals.TryAdd(kvp.Key, 0);
                emotionTotals[kvp.Key] += kvp.Value;
            }
        }

        return CalculateWithCompound(emotionTotals);
    }
}
