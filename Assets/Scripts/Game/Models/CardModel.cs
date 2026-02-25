using System;

/// <summary>
/// カードインスタンスを表すモデルクラス
/// CardDataの実体として、個別の状態を保持
/// バトル時の割り当て数字・入札情報・使用済みフラグも管理する
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

    /// <summary>割り当てられた数字（バトル用）</summary>
    public int BattleNumber { get; private set; }

    /// <summary>オークション入札リソース総量（タイブレーク用）</summary>
    public int AuctionBidTotal { get; private set; }

    /// <summary>使用済みかどうか（バトル用）</summary>
    public bool IsUsed { get; set; }

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
    /// ダミーカード用コンストラクタ（Data=null）
    /// </summary>
    /// <param name="battleNumber">割り当て数字</param>
    public CardModel(int battleNumber)
    {
        InstanceId = Guid.NewGuid().ToString();
        Data = null;
        BattleNumber = battleNumber;
    }

    /// <summary>バトルデータを初期化する</summary>
    public void InitializeBattleData(int number, int auctionBidTotal)
    {
        BattleNumber = number;
        AuctionBidTotal = auctionBidTotal;
        IsUsed = false;
    }

    /// <summary>数字を変更する（スキル効果用）</summary>
    public void SetBattleNumber(int number) => BattleNumber = number;

    /// <summary>
    /// デバッグ用文字列
    /// </summary>
    public override string ToString() => $"{Data?.CardName ?? "Unknown"} [{InstanceId.Substring(0, 8)}]";
}
