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
    [SerializeField] private EnemyDialogueData dialogueData;
    [SerializeField] private ThemeData theme;
    [SerializeField] private List<CardData> playerCards = new();
    [SerializeField] private List<CardData> enemyCards = new();

    public string AuctionId => auctionId;
    public EnemyData Enemy => enemy;
    public ThemeData Theme => theme;
    public IReadOnlyList<CardData> PlayerCards => playerCards;
    public IReadOnlyList<CardData> EnemyCards => enemyCards;
    public EnemyDialogueData DialogueData => dialogueData;
}
