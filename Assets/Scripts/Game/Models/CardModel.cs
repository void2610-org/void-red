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
    /// コンストラクタ
    /// </summary>
    /// <param name="data">カードの定義データ</param>
    public CardModel(CardData data)
    {
        InstanceId = Guid.NewGuid().ToString();
        Data = data;
    }

    /// <summary>
    /// 復元用コンストラクタ（セーブデータから復元する際に使用）
    /// </summary>
    /// <param name="data">カードの定義データ</param>
    /// <param name="instanceId">インスタンスID</param>
    public CardModel(CardData data, string instanceId)
    {
        InstanceId = instanceId;
        Data = data;
    }

    /// <summary>
    /// デバッグ用文字列
    /// </summary>
    public override string ToString() => $"{Data?.CardName ?? "Unknown"} [{InstanceId.Substring(0, 8)}]";
}
