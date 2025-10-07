using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ノベルシーンでの選択結果に基づいてカードを選択するサービス
/// シナリオごとのカード選択ロジックを管理
/// </summary>
public static class NovelCardSelectionService
{
    /// <summary>
    /// シナリオIDと選択結果に基づいてカードIDを決定
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="choiceResults">該当シナリオの選択結果リスト</param>
    /// <returns>獲得するカードのID、選択できない場合はnull</returns>
    public static string SelectCardByChoices(string scenarioId, List<NovelChoiceResult> choiceResults)
    {
        return scenarioId switch
        {
            "prologue1" => SelectCardForPrologue1(choiceResults),
            // 他のシナリオのカード選択ロジックは今後ここに追加
            _ => null
        };
    }
    
    /// <summary>
    /// プロローグ1のカード選択ロジック
    /// 最初の選択肢の結果に基づいてカードを決定
    /// </summary>
    /// <param name="choiceResults">選択結果リスト</param>
    /// <returns>選択されたカードID</returns>
    private static string SelectCardForPrologue1(List<NovelChoiceResult> choiceResults)
    {
        // 選択結果が存在しない場合
        if (choiceResults == null || choiceResults.Count == 0)
        {
            return null;
        }
        
        // 最初の選択肢（choiceIndex = 0）の結果を取得（最後に追加されたものを取得）
        var choiceIndex0Results = choiceResults.Where(result => result.ChoiceIndex == 0).ToList();
        
        if (choiceIndex0Results.Count == 0)
        {
            return null;
        }
        
        // 複数ある場合は最後のものを取得
        var firstChoice = choiceIndex0Results.Last();
        
        // 選択された選択肢のインデックスに基づいてカードIDを決定
        return firstChoice.SelectedOptionIndex switch
        {
            0 => "card_aggressive_001", // 最初の選択肢を選んだ場合
            1 => "card_defensive_001",  // 二番目の選択肢を選んだ場合
            _ => null // 想定外の選択肢インデックス
        };
    }
}