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
    /// DialogDataからItemGetDataを作成
    /// </summary>
    /// <param name="dialogData">ダイアログデータ</param>
    /// <returns>アイテム取得データ、または作成できない場合はnull</returns>
    public static ItemGetData FromDialogData(DialogData dialogData)
    {
        if (!dialogData.HasGetItem)
            return null;
        
        return new ItemGetData(
            dialogData.GetItemImageName,
            dialogData.GetItemName,
            dialogData.GetItemDescription
        );
    }
}