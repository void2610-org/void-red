using System.Collections.Generic;

/// <summary>
/// ノベル進行データを保持するクラス
/// ノベル選択結果を管理
/// </summary>
public class NovelProgressData
{
    /// <summary>
    /// ノベル選択結果のリスト
    /// </summary>
    public List<NovelChoiceResult> ChoiceResults { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public NovelProgressData()
    {
        ChoiceResults = new List<NovelChoiceResult>();
    }

    /// <summary>
    /// 選択結果を記録
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="choiceIndex">選択肢番号</param>
    /// <param name="selectedOptionIndex">選択された選択肢のインデックス</param>
    public void RecordChoice(string scenarioId, int choiceIndex, int selectedOptionIndex)
    {
        var choiceResult = new NovelChoiceResult(scenarioId, choiceIndex, selectedOptionIndex);
        ChoiceResults.Add(choiceResult);
    }

    /// <summary>
    /// 特定のシナリオの選択結果を取得
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <returns>該当する選択結果のリスト</returns>
    public List<NovelChoiceResult> GetChoiceResultsByScenario(string scenarioId)
    {
        return ChoiceResults.FindAll(result => result.ScenarioId == scenarioId);
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        ChoiceResults.Clear();
    }
}
