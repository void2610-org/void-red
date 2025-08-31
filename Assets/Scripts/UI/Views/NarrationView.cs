using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using Void2610.UnityTemplate;

/// <summary>
/// ナレーション表示を担当するViewクラス
/// </summary>
public class NarrationView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrationText;
    
    private const float FADE_IN_DURATION = 0.3f;
    private const float FADE_OUT_DURATION = 0.3f;
    
    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _currentNarrationCts;

    /// <summary>
    /// ナレーションを表示
    /// </summary>
    public async UniTask DisplayNarration(string message, float duration = 2f)
    {
        // 現在実行中のナレーションをキャンセル
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
        
        // 新しいキャンセレーショントークンを作成
        _currentNarrationCts = new CancellationTokenSource();
        
        // アプリケーション終了時にもキャンセルされるようにする  
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
            _currentNarrationCts.Token,
            this.GetCancellationTokenOnDestroy(), 
            Application.exitCancellationToken
        ).Token;
        
        _canvasGroup.alpha = 1f;
        
        try
        {
            // メッセージを空で初期化（後で1文字ずつ表示）
            narrationText.text = "";
            
            // 初期状態を設定
            narrationText.gameObject.SetActive(true);
            narrationText.color = new Color(narrationText.color.r, narrationText.color.g, narrationText.color.b, 0f);
            
            // テキストのフェードインアニメーション
            var textColor = narrationText.color;
            await LMotion.Create(new Color(textColor.r, textColor.g, textColor.b, 0f), new Color(textColor.r, textColor.g, textColor.b, 1f), FADE_IN_DURATION)
                .WithEase(Ease.OutQuart)
                .BindToColor(narrationText)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            // 1文字ずつ表示するアニメーション
            await narrationText.TypewriterAnimation(message, cancellationToken: cancellationToken);
            
            // 表示時間を待つ
            await UniTask.Delay((int)(duration * 1000), cancellationToken: cancellationToken);
            
            // テキストのフェードアウトアニメーション
            var textColorOut = narrationText.color;
            await LMotion.Create(new Color(textColorOut.r, textColorOut.g, textColorOut.b, 1f), new Color(textColorOut.r, textColorOut.g, textColorOut.b, 0f), FADE_OUT_DURATION)
                .WithEase(Ease.InQuart)
                .BindToColor(narrationText)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            // 最終クリーンアップ
            if (narrationText)
            {
                narrationText.gameObject.SetActive(false);
            }
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合のクリーンアップ
            if (narrationText)
            {
                narrationText.gameObject.SetActive(false);
            }
        }
        finally
        {
            _canvasGroup.alpha = 0f;
        }
    }
    
    private void Awake()
    {
        // 初期状態の設定
        _canvasGroup = GetComponent<CanvasGroup>();
        narrationText.gameObject.SetActive(false);
    }
    
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
    }
}