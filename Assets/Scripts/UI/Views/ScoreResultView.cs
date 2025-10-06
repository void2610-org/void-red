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
public class ScoreResultView : MonoBehaviour
{
    [Header("結果表示要素")]
    [SerializeField] private Image gavelImage;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image textBackgroundImage;
    
    [Header("勝敗背景色設定")]
    [SerializeField] private Color playerWinColor;
    [SerializeField] private Color enemyWinColor;
    
    private CanvasGroup _canvasGroup;
    private CanvasGroup _textBackgroundCanvasGroup;
    
    /// <summary>
    /// 勝敗結果を専用スタイルで表示
    /// </summary>
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin)
    {
        await _canvasGroup.FadeIn(0.3f);
        
        // ガベルのアニメーション
        SeManager.Instance.PlaySe("Gavel");
        await gavelImage.FadeIn(0.25f);
        await UniTask.Delay(1000);
        await gavelImage.FadeOut(0.25f);
        
        await UniTask.Delay(1000);
        
        // テキストのアニメーション
        resultText.text = result;
        textBackgroundImage.color = isPlayerWin ? playerWinColor : enemyWinColor;
        await _textBackgroundCanvasGroup.FadeIn( 0.5f);
        await UniTask.Delay(3000); // 結果表示を2秒間維持
        
        await _canvasGroup.FadeOut(0.3f);
        
        // 結果表示後の状態をリセット
        _textBackgroundCanvasGroup.alpha = 0f;
    }
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _textBackgroundCanvasGroup = textBackgroundImage.GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        
        gavelImage.color = new Color(1f, 1f, 1f, 0f);
        _textBackgroundCanvasGroup.alpha = 0f;
    }
}