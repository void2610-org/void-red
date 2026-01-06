using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
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
    /// <param name="theme">テーマデータ</param>
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin, float playerScore, float enemyScore, ThemeData theme)
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
        // var breakdown = BuildScoreBreakdown(playerMove, enemyMove, theme);
        // breakdownText.text = breakdown;

        await textsCanvasGroup.FadeIn(0.5f);
        await UniTask.Delay(5000);

        await _canvasGroup.FadeOut(0.3f);

        // 結果表示後の状態をリセット
        textsCanvasGroup.alpha = 0f;
        scoreText.text = "";
        breakdownText.text = "";
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