using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// ノベルシーンでの選択結果に基づいてカードを選択するサービス
/// シナリオごとのカード選択ロジックを管理
/// </summary>
public static class NovelCardSelectionService
{
    private static readonly Dictionary<string, MethodInfo> _scenarioMethods = new();

    /// <summary>
    /// 静的コンストラクタ ScenarioCardSelectionAttributeが付与されたメソッドを収集
    /// </summary>
    static NovelCardSelectionService()
    {
        InitializeScenarioMethods();
    }

    /// <summary>
    /// シナリオIDと選択結果に基づいてカードIDを決定
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="choiceResults">該当シナリオの選択結果リスト</param>
    /// <returns>獲得するカードのID、選択できない場合はnull</returns>
    public static string SelectCardByChoices(string scenarioId, List<NovelChoiceResult> choiceResults)
    {
        // キャッシュされたメソッドを検索
        if (_scenarioMethods.TryGetValue(scenarioId, out var method)) return (string)method.Invoke(null, new object[] { choiceResults });

        return null;
    }

    /// <summary>
    /// アトリビュートが付与されたメソッドを収集してキャッシュ
    /// </summary>
    private static void InitializeScenarioMethods()
    {
        var methods = typeof(NovelCardSelectionService).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ScenarioCardSelectionAttribute>();
            if (attribute != null)
            {
                _scenarioMethods[attribute.ScenarioId] = method;
            }
        }
    }

    /// <summary>
    /// プロローグ1のカード選択ロジック
    /// 最初の選択肢の結果に基づいてカードを決定
    /// </summary>
    /// <param name="choiceResults">選択結果リスト</param>
    /// <returns>選択されたカードID</returns>
    [ScenarioCardSelection("prologue1")]
    private static string SelectCardForPrologue1(List<NovelChoiceResult> choiceResults)
    {
        // 選択結果が存在しない場合
        if (choiceResults == null || choiceResults.Count == 0) return null;

        // 最初の選択肢（choiceIndex = 0）の結果を最後から検索
        NovelChoiceResult firstChoice = null;
        for (var i = choiceResults.Count - 1; i >= 0; i--)
        {
            if (choiceResults[i].ChoiceIndex == 0)
            {
                firstChoice = choiceResults[i];
                break;
            }
        }

        if (firstChoice == null) return null;

        // 選択された選択肢のインデックスに基づいてカードIDを決定
        return firstChoice.SelectedOptionIndex switch
        {
            0 => "CARD_001A", // 最初の選択肢を選んだ場合
            1 => "CARD_002A",  // 二番目の選択肢を選んだ場合
            _ => null // 想定外の選択肢インデックス
        };
    }
}
