using System;

/// <summary>
/// カードインスタンスを表すモデルクラス
/// CardDataの実体として、個別の状態を保持
/// </summary>
[Serializable]
public class CardModel
{
    /// <summary>
    /// カードインスタンスの一意ID
    /// </summary>
    public string InstanceId { get; }
    
    /// <summary>
    /// カードの定義データ（ScriptableObject）
    /// </summary>
    public CardData Data { get; }
    
    /// <summary>
    /// 崩壊しているかどうか
    /// </summary>
    public bool IsCollapsed { get; set; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="data">カードの定義データ</param>
    public CardModel(CardData data)
    {
        InstanceId = Guid.NewGuid().ToString();
        Data = data;
        IsCollapsed = false;
    }
    
    /// <summary>
    /// 復元用コンストラクタ（セーブデータから復元する際に使用）
    /// </summary>
    /// <param name="data">カードの定義データ</param>
    /// <param name="instanceId">インスタンスID</param>
    /// <param name="isCollapsed">崩壊状態</param>
    public CardModel(CardData data, string instanceId, bool isCollapsed)
    {
        InstanceId = instanceId;
        Data = data;
        IsCollapsed = isCollapsed;
    }
    
    /// <summary>
    /// デバッグ用文字列
    /// </summary>
    public override string ToString()
    {
        return $"{Data?.CardName ?? "Unknown"} [{InstanceId.Substring(0, 8)}] {(IsCollapsed ? "(崩壊)" : "")}";
    }
}