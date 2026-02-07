using UnityEngine;

/// <summary>
/// カードの記憶種類を表すenum
/// </summary>
public enum MemoryType
{
    /// <summary>自己記憶</summary>
    SelfMemory,
    /// <summary>他者記憶</summary>
    OtherMemory,
    /// <summary>特定他者記憶</summary>
    SpecificOtherMemory,
    /// <summary>曖昧記憶</summary>
    AmbiguousMemory
}

/// <summary>
/// MemoryType enumの拡張メソッド
/// </summary>
public static class MemoryTypeExtensions
{
    /// <summary>
    /// 記憶種類の日本語名を取得
    /// </summary>
    public static string ToJapaneseName(this MemoryType memoryType) => memoryType switch
    {
        MemoryType.SelfMemory => "自己記憶",
        MemoryType.OtherMemory => "他者記憶",
        MemoryType.SpecificOtherMemory => "特定他者記憶",
        MemoryType.AmbiguousMemory => "曖昧記憶",
        _ => "不明"
    };

    /// <summary>
    /// 記憶種類に対応するゲージ色を取得
    /// </summary>
    public static Color ToGaugeColor(this MemoryType memoryType) => memoryType switch
    {
        MemoryType.SelfMemory => new Color(0.2f, 0.4f, 0.9f, 1f),
        MemoryType.OtherMemory => new Color(0.2f, 0.8f, 0.3f, 1f),
        MemoryType.SpecificOtherMemory => new Color(0.9f, 0.2f, 0.2f, 1f),
        MemoryType.AmbiguousMemory => new Color(0.6f, 0.2f, 0.8f, 1f),
        _ => Color.gray
    };
}
