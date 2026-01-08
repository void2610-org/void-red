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

    // 感情別入札を表示（TextMeshProのリッチテキストで色分け表示）
    public void ShowPlayerBidsWithEmotion(Dictionary<EmotionType, int> bids)
    {
        var activeBids = bids.Where(kv => kv.Value > 0).ToList();

        if (activeBids.Count == 0)
        {
            playerBidText.text = "";
            return;
        }

        // 入札量の多い順にソート
        activeBids = activeBids.OrderByDescending(kv => kv.Value).ToList();

        // リッチテキストで各感情の入札を色分け表示
        var textParts = new List<string>();
        foreach (var (emotion, amount) in activeBids)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(emotion.GetColor());
            textParts.Add($"<color=#{colorHex}>{amount}</color>");
        }

        playerBidText.text = string.Join("+", textParts);
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
