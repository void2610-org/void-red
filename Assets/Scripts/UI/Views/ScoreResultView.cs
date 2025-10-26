using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Linq;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// ゲーム結果表示を担当するViewクラス（勝敗・ドロー・進化・共鳴等）
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScoreResultView : MonoBehaviour
{
    [Header("結果表示要素")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image textBackgroundImage;
    [SerializeField] private CanvasGroup textsCanvasGroup;

    [Header("スコア・内訳表示（オプション）")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI breakdownText;

    [Header("勝敗背景色設定")]
    [SerializeField] private Color playerWinColor;
    [SerializeField] private Color enemyWinColor;

    private CanvasGroup _canvasGroup;

    /// <summary>
    /// 勝敗結果をスコアと内訳付きで表示
    /// </summary>
    /// <param name="result">勝敗結果テキスト</param>
    /// <param name="isPlayerWin">プレイヤーの勝利か</param>
    /// <param name="playerScore">プレイヤーのスコア</param>
    /// <param name="enemyScore">敵のスコア</param>
    /// <param name="playerMove">プレイヤーの手</param>
    /// <param name="enemyMove">敵の手</param>
    /// <param name="theme">テーマデータ</param>
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin, float playerScore, float enemyScore, PlayerMove playerMove, PlayerMove enemyMove, ThemeData theme)
    {
        await _canvasGroup.FadeIn(0.3f);

        await UniTask.Delay(1000);

        // ガベルのアニメーション
        SeManager.Instance.PlaySe("Gavel");

        // テキストのアニメーション
        resultText.text = result;
        textBackgroundImage.color = isPlayerWin ? playerWinColor : enemyWinColor;
        // スコア表示
        scoreText.text = $"{playerScore:F1} vs {enemyScore:F1}";

        // 内訳を生成して表示
        var breakdown = BuildScoreBreakdown(playerMove, enemyMove, theme);
        breakdownText.text = breakdown;

        await textsCanvasGroup.FadeIn(0.5f);
        await UniTask.Delay(5000);

        await _canvasGroup.FadeOut(0.3f);

        // 結果表示後の状態をリセット
        textsCanvasGroup.alpha = 0f;
        scoreText.text = "";
        breakdownText.text = "";
    }

    /// <summary>
    /// スコアの内訳文字列を生成
    /// </summary>
    private string BuildScoreBreakdown(PlayerMove playerMove, PlayerMove enemyMove, ThemeData theme)
    {
        var breakdown = "";

        // キーワード一致情報（プレイヤー）
        var matchedKeywords = ScoreCalculator.GetMatchedKeywords(playerMove.SelectedCard, theme);
        if (matchedKeywords.Count > 0)
        {
            var keywordNames = string.Join("、", matchedKeywords.Select(k => k.GetJapaneseName()));
            breakdown += $"・一致キーワード: {keywordNames}\n";
        }

        // PlayStyle相性情報
        var playerPlayStyle = playerMove.PlayStyle;
        var enemyPlayStyle = enemyMove.PlayStyle;

        if (playerPlayStyle.IsStrongAgainst(enemyPlayStyle))
        {
            breakdown += "・PlayStyle相性: 勝利\n";
        }
        else if (enemyPlayStyle.IsStrongAgainst(playerPlayStyle))
        {
            breakdown += "・PlayStyle相性: 敗北\n";
        }
        else
        {
            breakdown += "・PlayStyle相性: 引き分け\n";
        }

        return breakdown.TrimEnd('\n');
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        
        textsCanvasGroup.alpha = 0f;
    }
}