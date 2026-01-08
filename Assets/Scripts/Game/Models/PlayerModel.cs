using System;
using System.Collections.Generic;

/// <summary>
/// プレイヤーの属性データモデル（Model Layer）
/// 感情リソース（8属性）を管理
/// </summary>
public class PlayerModel : IDisposable
{
    // 公開プロパティ（読み取り専用）
    public IReadOnlyDictionary<EmotionType, int> EmotionResources => _emotionResources;

    // プライベートフィールド
    private readonly Dictionary<EmotionType, int> _emotionResources = new();

    public PlayerModel()
    {
        InitializeEmotionResources();
    }

    private void InitializeEmotionResources()
    {
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            _emotionResources[emotion] = GameConstants.DEFAULT_EMOTION_VALUE;
        }
    }

    /// <summary>
    /// 感情リソースを消費
    /// </summary>
    public bool TryConsumeEmotion(EmotionType emotion, int amount)
    {
        if (_emotionResources[emotion] < amount) return false;
        _emotionResources[emotion] -= amount;
        return true;
    }

    /// <summary>
    /// 感情リソースを追加
    /// </summary>
    public void AddEmotion(EmotionType emotion, int amount)
    {
        _emotionResources[emotion] += amount;
    }

    /// <summary>
    /// 感情リソースをリセット
    /// </summary>
    public void ResetEmotionResources()
    {
        InitializeEmotionResources();
    }

    /// <summary>
    /// 特定の感情リソース量を取得
    /// </summary>
    public int GetEmotionAmount(EmotionType emotion) => _emotionResources[emotion];

    public void Dispose() { }
}
