using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 選択肢演出に使用するデータクラス
/// </summary>
[System.Serializable]
public class ChoiceData
{
    [SerializeField] private string question;
    [SerializeField] private List<string> options;

    /// <summary>
    /// 質問文
    /// </summary>
    public string Question => question;

    /// <summary>
    /// 選択肢のリスト
    /// </summary>
    public List<string> Options => options;

    /// <summary>
    /// 選択肢の数
    /// </summary>
    public int OptionCount => options.Count;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="question">質問文</param>
    /// <param name="options">選択肢のリスト</param>
    public ChoiceData(string question, List<string> options)
    {
        this.question = question;
        this.options = options ?? new List<string>();
    }

    /// <summary>
    /// カンマ区切り文字列からChoiceDataを作成
    /// </summary>
    /// <param name="commaSeparatedValue">カンマ区切り文字列 ("質問文,選択肢1,選択肢2,...")</param>
    /// <returns>選択肢データ、または作成できない場合はnull</returns>
    public static ChoiceData FromCommaSeparatedString(string commaSeparatedValue)
    {
        if (string.IsNullOrEmpty(commaSeparatedValue))
            return null;

        var parts = commaSeparatedValue.Split(',');
        if (parts.Length < 3) // 質問文 + 最低2つの選択肢
        {
            Debug.LogWarning($"[ChoiceData] 不正なChoiceパラメータ形式です。最低でも質問文と2つの選択肢が必要です: '{commaSeparatedValue}'");
            return null;
        }

        var question = parts[0].Trim();
        var options = new List<string>();

        // 2番目以降を選択肢として追加
        for (var i = 1; i < parts.Length; i++)
        {
            var option = parts[i].Trim();
            if (!string.IsNullOrEmpty(option))
            {
                options.Add(option);
            }
        }

        if (options.Count < 2)
        {
            Debug.LogWarning($"[ChoiceData] 選択肢が2個未満です: '{commaSeparatedValue}'");
            return null;
        }

        return new ChoiceData(question, options);
    }

    /// <summary>
    /// 指定されたインデックスの選択肢を取得
    /// </summary>
    /// <param name="index">選択肢のインデックス</param>
    /// <returns>選択肢のテキスト、無効なインデックスの場合は空文字列</returns>
    public string GetOption(int index)
    {
        if (index >= 0 && index < OptionCount)
            return options[index];
        return "";
    }
}
