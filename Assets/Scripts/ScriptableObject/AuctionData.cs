using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オークションのデータ（敵・テーマ・カード配置）
/// </summary>
[CreateAssetMenu(fileName = "NewAuctionData", menuName = "VoidRed/Auction Data")]
public class AuctionData : ScriptableObject
{
    [SerializeField] private string auctionId;
    [SerializeField] private EnemyData enemy;
    [SerializeField] private ThemeData theme;
    [SerializeField] private List<CardData> auctionCards = new();
    [SerializeField] private VictoryCondition victoryCondition;

    public string AuctionId => auctionId;
    public EnemyData Enemy => enemy;
    public ThemeData Theme => theme;
    public IReadOnlyList<CardData> AuctionCards => auctionCards;
    /// <summary>カードバトルの勝利条件</summary>
    public VictoryCondition VictoryCondition => victoryCondition;
}
