using System.Collections.Generic;

/// <summary>
/// メモリ進捗データコンテナ
/// 獲得済みテーマリストを保持する実行時データ
/// </summary>
public class MemoryProgressData
{
    /// <summary>
    /// 獲得済みテーマリスト
    /// </summary>
    public List<AcquiredTheme> AcquiredThemes { get; } = new();

    /// <summary>
    /// テーマを追加
    /// </summary>
    /// <param name="theme">獲得したテーマ</param>
    public void AddAcquiredTheme(AcquiredTheme theme) => AcquiredThemes.Add(theme);

    /// <summary>
    /// データをリセット
    /// </summary>
    public void Reset() => AcquiredThemes.Clear();
}
