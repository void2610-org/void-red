using UnityEngine;
using UnityEngine.UI;

/// <summary>3本先取の勝敗状況を表示するダイヤモンドインジケータ。プレイヤー勝利は右端から、敵勝利は左端から色が変わる。</summary>
public class DiamondIndicatorView : MonoBehaviour
{
    [Header("ダイヤモンド画像（左→右の順で割り当て）")]
    [SerializeField] private Image[] diamondImages;

    [Header("色設定")]
    [SerializeField] private Color playerColor = Color.white;
    [SerializeField] private Color enemyColor = new(0.8f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color undecidedColor = new(0.3f, 0.3f, 0.3f, 0.5f);

    /// <summary>勝敗カウントに応じて色を更新する</summary>
    public void UpdateIndicators(int playerWins, int enemyWins)
    {
        var length = diamondImages.Length;
        for (var i = 0; i < length; i++)
        {
            // プレイヤーは右端 (length-1) から左に向かって埋める
            var fromRight = length - 1 - i;
            if (fromRight < playerWins)
                diamondImages[i].color = playerColor;
            else if (i < enemyWins)
                diamondImages[i].color = enemyColor;
            else
                diamondImages[i].color = undecidedColor;
        }
    }
}
