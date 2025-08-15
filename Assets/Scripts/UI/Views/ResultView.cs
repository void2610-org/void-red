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
    [SerializeField] private Color playerWinColor;
    [SerializeField] private Color enemyWinColor;
    
    private CanvasGroup _canvasGroup;
    private readonly Vector3 _textMoveStartPosition = new(-1000f, 0f, 0f); // テキストの初期位置
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        
        gavelImage.color = new Color(1f, 1f, 1f, 0f);
        textBackgroundImage.transform.localPosition = _textMoveStartPosition;
    }
    
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
        await textBackgroundImage.transform.MoveTo(Vector3.zero, 0.75f, Ease.InOutBack);
        await UniTask.Delay(3000); // 結果表示を2秒間維持
        
        await _canvasGroup.FadeOut(0.3f);
        
        // 結果表示後の状態をリセット
        textBackgroundImage.transform.localPosition = _textMoveStartPosition;
    }
}