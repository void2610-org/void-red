using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感情リソースのセーブデータ（シリアライズ用）
/// </summary>
[Serializable]
public class EmotionResourceData
{
    public EmotionType emotionType;
    public int amount;

    public EmotionResourceData(EmotionType type, int value)
    {
        emotionType = type;
        amount = value;
    }
}

/// <summary>
/// プレイヤー固有のセーブデータ（感情リソース + 将来の章クリア状況等）
/// </summary>
[Serializable]
public class PlayerSaveData
{
    [Header("ゲーム進行データ")]
    [SerializeField] private int currentChapter;

    [Header("感情リソースデータ")]
    [SerializeField] private List<EmotionResourceData> emotionResources = new();

    public int CurrentChapter => currentChapter;

    public PlayerSaveData()
    {
        InitializeEmotionResources();
    }

    private void InitializeEmotionResources()
    {
        emotionResources.Clear();
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            emotionResources.Add(new EmotionResourceData(emotion, GameConstants.DEFAULT_EMOTION_VALUE));
        }
    }

    /// <summary>
    /// 感情リソースを更新
    /// </summary>
    public void UpdateEmotionResources(IReadOnlyDictionary<EmotionType, int> resources)
    {
        emotionResources.Clear();
        foreach (var kvp in resources)
        {
            emotionResources.Add(new EmotionResourceData(kvp.Key, kvp.Value));
        }
    }

    /// <summary>
    /// 感情リソースをDictionary形式で取得
    /// </summary>
    public Dictionary<EmotionType, int> GetEmotionResources()
    {
        var result = new Dictionary<EmotionType, int>();
        foreach (var data in emotionResources)
        {
            result[data.emotionType] = data.amount;
        }

        // 不足している感情タイプがあればデフォルト値で補完
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            if (!result.ContainsKey(emotion))
            {
                result[emotion] = GameConstants.DEFAULT_EMOTION_VALUE;
            }
        }

        return result;
    }

    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    public string GetStatsString()
    {
        var emotionStr = string.Join(", ", emotionResources.ConvertAll(e => $"{e.emotionType}:{e.amount}"));
        return $"Chapter: {currentChapter}, Emotions: [{emotionStr}]";
    }
}
