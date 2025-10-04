using System.Collections.Generic;

/// <summary>
/// ストーリー進行データを保持するクラス
/// ストーリーの現在ステップ、ノード、結果辞書を管理
/// </summary>
public class StoryProgressData
{
    /// <summary>
    /// 現在のストーリーステップ
    /// </summary>
    public int CurrentStep { get; set; }

    /// <summary>
    /// 現在のストーリーノード
    /// </summary>
    public StoryNode CurrentNode { get; set; }

    /// <summary>
    /// バトル/ノベル結果を保存する辞書
    /// キー: ステップ番号 or 識別子、値: 結果（"win"/"lose" など）
    /// </summary>
    public Dictionary<string, string> Results { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public StoryProgressData()
    {
        CurrentStep = 0;
        CurrentNode = null;
        Results = new Dictionary<string, string>();
    }

    /// <summary>
    /// 結果を記録
    /// </summary>
    /// <param name="key">結果のキー</param>
    /// <param name="value">結果の値</param>
    public void RecordResult(string key, string value)
    {
        Results[key] = value;
    }

    /// <summary>
    /// 結果を取得
    /// </summary>
    /// <param name="key">結果のキー</param>
    /// <returns>結果の値（存在しない場合は空文字列）</returns>
    public string GetResult(string key)
    {
        return Results.GetValueOrDefault(key, "");
    }

    /// <summary>
    /// ステップを進める
    /// </summary>
    public void AdvanceStep()
    {
        CurrentStep++;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        CurrentStep = 0;
        CurrentNode = null;
        Results.Clear();
    }
}
