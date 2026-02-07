using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// 全オークションデータを管理
/// </summary>
[CreateAssetMenu(fileName = "AllAuctionData", menuName = "VoidRed/All Auction Data")]
public class AllAuctionData : ScriptableObject
{
    [SerializeField] private List<AuctionData> auctionList = new();

    public List<AuctionData> AuctionList => auctionList;
    public int Count => auctionList.Count;

    /// <summary>
    /// オークションIDでオークションデータを取得
    /// </summary>
    /// <param name="auctionId">オークションID</param>
    /// <returns>指定されたIDのオークションデータ</returns>
    public AuctionData GetAuctionById(string auctionId) => auctionList.FirstOrDefault(auction => auction.AuctionId == auctionId);

    /// <summary>
    /// 同じディレクトリ内の全てのオークションデータを自動的に登録
    /// </summary>
    public void RegisterAllAuctions()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(auctionList, x => x.AuctionId);
#endif
    }
}
