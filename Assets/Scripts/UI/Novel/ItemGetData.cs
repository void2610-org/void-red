using UnityEngine;

/// <summary>
/// アイテム取得演出に使用するデータクラス
/// </summary>
[System.Serializable]
public class ItemGetData
{
    [SerializeField] private string itemImageName;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;

    /// <summary>
    /// アイテムの画像名
    /// </summary>
    public string ItemImageName => itemImageName;

    /// <summary>
    /// アイテム名
    /// </summary>
    public string ItemName => itemName;

    /// <summary>
    /// アイテムの説明文
    /// </summary>
    public string ItemDescription => itemDescription;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="itemImageName">アイテム画像名</param>
    /// <param name="itemName">アイテム名</param>
    /// <param name="itemDescription">アイテム説明</param>
    public ItemGetData(string itemImageName, string itemName, string itemDescription)
    {
        this.itemImageName = itemImageName;
        this.itemName = itemName;
        this.itemDescription = itemDescription;
    }

    /// <summary>
    /// カンマ区切り文字列からItemGetDataを作成
    /// </summary>
    /// <param name="commaSeparatedValue">カンマ区切り文字列 ("アイテム画像名,アイテム名,アイテム説明")</param>
    /// <returns>アイテム取得データ、または作成できない場合はnull</returns>
    public static ItemGetData FromCommaSeparatedString(string commaSeparatedValue)
    {
        if (string.IsNullOrEmpty(commaSeparatedValue))
            return null;

        var parts = commaSeparatedValue.Split(',');
        if (parts.Length < 3)
        {
            Debug.LogWarning($"[ItemGetData] 不正なGetItemパラメータ形式です。: '{commaSeparatedValue}'");
            return null;
        }

        var imageName = parts[0].Trim();
        var name = parts[1].Trim();
        var description = parts[2].Trim();

        return new ItemGetData(imageName, name, description);
    }
}
