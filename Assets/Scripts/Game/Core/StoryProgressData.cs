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
    /// バトル結果を保存する辞書
    /// キー: ノードID、値: 勝利(true)/敗北(false)
    /// </summary>
    public Dictionary<string, bool> BattleResults { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public StoryProgressData()
    {
        CurrentStep = 0;
        CurrentNode = null;
        BattleResults = new Dictionary<string, bool>();
    }

    /// <summary>
    /// 結果を記録
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="isPlayerWon"></param>
    public void RecordBattleResult(string nodeId, bool isPlayerWon)
    {
        BattleResults[nodeId] = isPlayerWon;
    }

    /// <summary>
    /// 結果を取得
    /// </summary>
    /// <param name="nodeId">結果のキー</param>
    /// <returns>結果の値（存在しない場合は空文字列）</returns>
    public bool GetBattleResult(string nodeId)
    {
        return BattleResults.GetValueOrDefault(nodeId, false);
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
        BattleResults.Clear();
    }
}
