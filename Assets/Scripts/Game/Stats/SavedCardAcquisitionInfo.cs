using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 獲得カードの詳細情報（シリアライズ用）
/// オークションでの入札状況、勝敗を保存
/// </summary>
[Serializable]
public class SavedCardAcquisitionInfo
{
    /// <summary>
    /// カードID（CardData.CardIdに対応）
    /// </summary>
    [SerializeField] private string cardId;

    /// <summary>
    /// プレイヤーの感情別入札量
    /// Key: EmotionType (int), Value: 入札量
    /// </summary>
    [SerializeField] private List<int> playerEmotionTypes = new();
    [SerializeField] private List<int> playerBidAmounts = new();

    /// <summary>
    /// 敵の感情別入札量
    /// </summary>
    [SerializeField] private List<int> enemyEmotionTypes = new();
    [SerializeField] private List<int> enemyBidAmounts = new();

    /// <summary>
    /// オークションでの勝敗（true: プレイヤー勝利、false: 敵勝利）
    /// </summary>
    [SerializeField] private bool playerWon;

    /// <summary>
    /// プレイヤーの入札合計
    /// </summary>
    [SerializeField] private int playerTotalBid;

    /// <summary>
    /// 敵の入札合計
    /// </summary>
    [SerializeField] private int enemyTotalBid;

    // プロパティ
    public string CardId => cardId;
    public bool PlayerWon => playerWon;
    public int PlayerTotalBid => playerTotalBid;
    public int EnemyTotalBid => enemyTotalBid;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SavedCardAcquisitionInfo(
        string cardId,
        Dictionary<EmotionType, int> playerBids,
        Dictionary<EmotionType, int> enemyBids,
        bool playerWon)
    {
        this.cardId = cardId;
        this.playerWon = playerWon;

        // プレイヤー入札を保存
        playerTotalBid = 0;
        foreach (var kvp in playerBids)
        {
            playerEmotionTypes.Add((int)kvp.Key);
            playerBidAmounts.Add(kvp.Value);
            playerTotalBid += kvp.Value;
        }

        // 敵入札を保存
        enemyTotalBid = 0;
        foreach (var kvp in enemyBids)
        {
            enemyEmotionTypes.Add((int)kvp.Key);
            enemyBidAmounts.Add(kvp.Value);
            enemyTotalBid += kvp.Value;
        }
    }

    /// <summary>
    /// プレイヤーの感情別入札を取得
    /// </summary>
    public Dictionary<EmotionType, int> GetPlayerBidsByEmotion()
    {
        var result = new Dictionary<EmotionType, int>();
        for (var i = 0; i < playerEmotionTypes.Count && i < playerBidAmounts.Count; i++)
        {
            result[(EmotionType)playerEmotionTypes[i]] = playerBidAmounts[i];
        }
        return result;
    }

    /// <summary>
    /// 敵の感情別入札を取得
    /// </summary>
    public Dictionary<EmotionType, int> GetEnemyBidsByEmotion()
    {
        var result = new Dictionary<EmotionType, int>();
        for (var i = 0; i < enemyEmotionTypes.Count && i < enemyBidAmounts.Count; i++)
        {
            result[(EmotionType)enemyEmotionTypes[i]] = enemyBidAmounts[i];
        }
        return result;
    }
}
