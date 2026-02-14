using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// カード上の入札情報表示View
// 価値順位、入札額、WIN/LOSEを表示
public class CardBidInfoView : MonoBehaviour
{
    [Header("価値順位表示")]
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("入札額表示")]
    [SerializeField] private TextMeshProUGUI playerBidText;
    [SerializeField] private TextMeshProUGUI enemyBidText;

    [Header("結果表示")]
    [SerializeField] private TextMeshProUGUI resultText;

    private static readonly Color PLAYER_COLOR = Color.green;
    private static readonly Color ENEMY_COLOR = Color.red;

    // 価値順位を非表示
    public void HideRank() => rankText.text = "";

    // 結果を非表示
    public void HideResult() => resultText.gameObject.SetActive(false);

    // 価値順位を表示（色でプレイヤー/敵を判別）
    public void ShowRank(int rank, bool isPlayerCard)
    {
        // プレイヤーカードは順位を表示、敵カードは非公開（?）
        rankText.text = isPlayerCard ? $"{rank}" : "?";
        rankText.color = isPlayerCard ? PLAYER_COLOR : ENEMY_COLOR;
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
        playerBidText.text = playerBid > 0 ? $"{playerBid}" : "";
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
        playerBidText.text = playerBid > 0 ? $"{playerBid}" : "";
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
        resultText.color = isPlayerWon ? PLAYER_COLOR : ENEMY_COLOR;
    }

    // 引き分け結果を表示
    public void ShowDraw()
    {
        resultText.gameObject.SetActive(true);
        resultText.text = "DRAW";
        resultText.color = Color.yellow;
    }
}
