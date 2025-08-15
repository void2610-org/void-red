using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// ゲーム結果表示を担当するViewクラス（勝敗・ドロー・進化・共鳴等）
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ResultView : MonoBehaviour
{
    [Header("結果表示要素")]
    [SerializeField] private Image gavelImage;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image textBackgroundImage;
    
    [Header("勝敗背景色設定")]
    [SerializeField] private Color playerWinColor = new Color(0f, 0.6f, 0f, 0.8f);  // プレイヤー勝利時の背景色（緑）
    [SerializeField] private Color enemyWinColor = new Color(0.6f, 0f, 0f, 0.8f);   // 敵勝利時の背景色（赤）
    
    private CanvasGroup _canvasGroup;
    private CanvasGroup _textCanvasGroup;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _textCanvasGroup = textBackgroundImage.GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _textCanvasGroup.alpha = 0f;
        _textCanvasGroup.interactable = false;
        _textCanvasGroup.blocksRaycasts = false;
        
        gavelImage.color = new Color(1f, 1f, 1f, 0f);
    }
    
    /// <summary>
    /// 勝敗結果を専用スタイルで表示
    /// </summary>
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin)
    {
        await _canvasGroup.FadeIn(0.3f).ToUniTask();
        
        // ガベルのアニメーション
        SeManager.Instance.PlaySe("Gavel");
        await gavelImage.FadeIn(0.25f).ToUniTask();
        await UniTask.Delay(1000);
        await gavelImage.FadeOut(0.25f).ToUniTask();
        
        await UniTask.Delay(1000);
        
        // テキストのアニメーション
        resultText.text = result;
        textBackgroundImage.color = isPlayerWin ? playerWinColor : enemyWinColor;
        await _textCanvasGroup.FadeIn(0.3f).ToUniTask();
        await UniTask.Delay(3000); // 結果表示を2秒間維持
        await _textCanvasGroup.FadeOut(0.3f).ToUniTask();
        
        await _canvasGroup.FadeOut(0.3f).ToUniTask();
    }
}