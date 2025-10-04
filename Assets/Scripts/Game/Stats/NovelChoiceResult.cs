using System;
using UnityEngine;

/// <summary>
/// ノベルシーンでの選択結果を保存するデータクラス
/// </summary>
[Serializable]
public class NovelChoiceResult
{
    [SerializeField] private string scenarioId;
    [SerializeField] private int choiceIndex;
    [SerializeField] private int selectedOptionIndex;
    
    /// <summary>
    /// シナリオID（どのシナリオでの選択か）
    /// </summary>
    public string ScenarioId => scenarioId;
    
    /// <summary>
    /// 選択肢番号（そのシナリオ内での何番目の選択肢か）
    /// </summary>
    public int ChoiceIndex => choiceIndex;
    
    /// <summary>
    /// 選択された選択肢のインデックス（0から始まる）
    /// </summary>
    public int SelectedOptionIndex => selectedOptionIndex;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="choiceIndex">選択肢番号</param>
    /// <param name="selectedOptionIndex">選択された選択肢のインデックス</param>
    public NovelChoiceResult(string scenarioId, int choiceIndex, int selectedOptionIndex)
    {
        this.scenarioId = scenarioId;
        this.choiceIndex = choiceIndex;
        this.selectedOptionIndex = selectedOptionIndex;
    }
    
    /// <summary>
    /// デバッグ用の文字列表現
    /// </summary>
    public override string ToString()
    {
        return $"[{scenarioId}] Choice{choiceIndex}: Index{selectedOptionIndex}";
    }
}