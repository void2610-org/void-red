using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// バトル全体の結果を表示するViewクラス（3勝到達後の最終結果）
/// </summary>
public class BattleResultView : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI scoreText; 
    [SerializeField] private Button homeButton;

    /// <summary>
    /// バトル結果を表示
    /// </summary>
    /// <param name="playerWon">プレイヤーが勝利したかどうか</param>
    /// <param name="playerWins">プレイヤーの勝利数</param>
    /// <param name="enemyWins">敵の勝利数</param>
    public async UniTask ShowAndWaitBattleResult(bool playerWon, int playerWins, int enemyWins)
    {
        // テキストと色を設定
        resultText.text = playerWon ? "勝利！" : "敗北...";
        scoreText.text = $"{playerWins} - {enemyWins}";
        
        panel.gameObject.SetActive(true);
		
	await homeButton.OnClickAsync();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
        // 初期状態は非表示
        panel.SetActive(false);
    }
}
