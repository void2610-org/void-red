using UnityEngine;

/// <summary>
/// カード風選択肢演出に使用するデータクラス
/// </summary>
[System.Serializable]
public class CardChoiceData
{
    [SerializeField] private string option1;
    [SerializeField] private string imageName1;
    [SerializeField] private string option2;
    [SerializeField] private string imageName2;

    /// <summary>
    /// 選択肢1のテキスト
    /// </summary>
    public string Option1 => option1;

    /// <summary>
    /// 選択肢1の画像名
    /// </summary>
    public string ImageName1 => imageName1;

    /// <summary>
    /// 選択肢2のテキスト
    /// </summary>
    public string Option2 => option2;

    /// <summary>
    /// 選択肢2の画像名
    /// </summary>
    public string ImageName2 => imageName2;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    private CardChoiceData(string option1, string imageName1, string option2, string imageName2)
    {
        this.option1 = option1;
        this.imageName1 = imageName1;
        this.option2 = option2;
        this.imageName2 = imageName2;
    }

    /// <summary>
    /// スラッシュ区切り文字列からCardChoiceDataを作成
    /// </summary>
    /// <param name="slashSeparatedValue">スラッシュ区切り文字列 ("[選択肢1,画像1]/[選択肢2,画像2]")</param>
    /// <returns>カード選択肢データ、または作成できない場合はnull</returns>
    public static CardChoiceData FromSlashSeparatedString(string slashSeparatedValue)
    {
        if (string.IsNullOrEmpty(slashSeparatedValue))
            return null;

        var parts = slashSeparatedValue.Split('/');
        if (parts.Length != 2)
        {
            Debug.LogWarning($"[CardChoiceData] 不正なCardChoiceパラメータ形式です。2つの選択肢が必要です: '{slashSeparatedValue}'");
            return null;
        }

        // 各選択肢をカンマで分割（角括弧を除去）
        var choice1Parts = parts[0].Trim().Trim('[', ']').Split(',');
        var choice2Parts = parts[1].Trim().Trim('[', ']').Split(',');

        if (choice1Parts.Length != 2 || choice2Parts.Length != 2)
        {
            Debug.LogWarning($"[CardChoiceData] 各選択肢は「テキスト,画像名」の形式である必要があります: '{slashSeparatedValue}'");
            return null;
        }

        var option1 = choice1Parts[0].Trim();
        var imageName1 = choice1Parts[1].Trim();
        var option2 = choice2Parts[0].Trim();
        var imageName2 = choice2Parts[1].Trim();

        return new CardChoiceData(option1, imageName1, option2, imageName2);
    }

    /// <summary>
    /// 指定されたインデックスの選択肢テキストを取得
    /// </summary>
    public string GetOption(int index)
    {
        return index switch
        {
            0 => option1,
            1 => option2,
            _ => ""
        };
    }

    /// <summary>
    /// 指定されたインデックスの画像名を取得
    /// </summary>
    public string GetImageName(int index)
    {
        return index switch
        {
            0 => imageName1,
            1 => imageName2,
            _ => ""
        };
    }
}
