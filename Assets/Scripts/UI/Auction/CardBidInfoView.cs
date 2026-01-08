using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

// カード上の入札情報表示View
// プレイヤー/敵の入札額とWIN/LOSEを表示
public class CardBidInfoView : MonoBehaviour
{
    [Header("価値順位表示")]
    [SerializeField] private TextMeshProUGUI playerRankText;
    [SerializeField] private TextMeshProUGUI enemyRankText;

    [Header("入札額表示")]
    [SerializeField] private TextMeshProUGUI playerBidText;
    [SerializeField] private TextMeshProUGUI enemyBidText;

    [Header("結果表示")]
    [SerializeField] private TextMeshProUGUI resultText;

    // 価値順位を表示
    public void ShowRanks(int playerRank, int enemyRank)
    {
        playerRankText.text = $"{playerRank}";
        enemyRankText.text = $"{enemyRank}";
    }

    // プレイヤーの価値順位のみ表示（敵は非公開）
    public void ShowPlayerRankOnly(int playerRank)
    {
        playerRankText.text = $"{playerRank}";
        enemyRankText.text = "?";
    }

    // 価値順位を非表示
    public void HideRanks()
    {
        playerRankText.text = "";
        enemyRankText.text = "";
    }

    // 入札額を表示（両方公開）
    public void ShowBidAmounts(int playerBid, int enemyBid)
    {
        playerBidText.text = $"{playerBid}";
        enemyBidText.text = $"{enemyBid}";
    }

    // プレイヤーの入札額のみ表示（敵は非公開）
    public void ShowPlayerBidOnly(int playerBid)
    {
        playerBidText.text = $"{playerBid}";
        enemyBidText.text = "?";
    }

    // 入札額を非表示
    public void HideBids()
    {
        playerBidText.text = "";
        enemyBidText.text = "";
    }

    // 入札対象公開用（自分の入札額は数値、相手は?）
    public void ShowBidTargetReveal(int playerBid, bool enemyHasBid)
    {
        // 自分が入札していれば数値、していなければ非表示
        playerBidText.text = playerBid > 0 ? $"{playerBid}" : "";
        // 相手が入札していれば?、していなければ非表示
        enemyBidText.text = enemyHasBid ? "?" : "";
    }

    // 感情別入札を表示（主要感情の色で合計値を表示）
    public void ShowPlayerBidsWithEmotion(Dictionary<EmotionType, int> bids)
    {
        var totalBid = bids.Values.Sum();
        if (totalBid == 0)
        {
            playerBidText.text = "";
            return;
        }

        // 最も入札額が多い感情の色を使用
        var primaryEmotion = bids.OrderByDescending(kv => kv.Value).First().Key;
        playerBidText.text = $"{totalBid}";
        playerBidText.color = primaryEmotion.GetColor();
    }

    // 結果を表示（WIN/LOSE）
    public void ShowResult(bool isPlayerWon)
    {
        resultText.gameObject.SetActive(true);
        resultText.text = isPlayerWon ? "WIN" : "LOSE";
        resultText.color = isPlayerWon ? Color.green : Color.red;
    }

    // 結果を非表示
    public void HideResult()
    {
        resultText.gameObject.SetActive(false);
    }
    
    private void Awake()
    {
        playerRankText.color = Color.green;
        enemyRankText.color = Color.red;
    }
}
