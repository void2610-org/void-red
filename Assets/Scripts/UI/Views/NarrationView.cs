using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;

/// <summary>
/// ナレーション表示を担当するViewクラス
/// </summary>
public class NarrationView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrationText;
    
    private const float FADE_IN_DURATION = 0.3f;
    private const float FADE_OUT_DURATION = 0.3f;
    private const float CHARACTER_DISPLAY_INTERVAL = 0.05f; // 1文字あたりの表示間隔
    
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
            await TypewriterAnimation(message, cancellationToken);
            
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
    
    /// <summary>
    /// タイプライターアニメーション（1文字ずつ表示）
    /// </summary>
    private async UniTask TypewriterAnimation(string message, CancellationToken cancellationToken)
    {
        var currentLength = 0;
        var targetLength = message.Length;
        
        // LitMotionで文字数を増やすアニメーション
        await LMotion.Create(0f, targetLength, targetLength * CHARACTER_DISPLAY_INTERVAL)
            .WithEase(Ease.Linear)
            .Bind(value =>
            {
                var newLength = Mathf.FloorToInt(value);
                if (newLength != currentLength && narrationText)
                {
                    currentLength = newLength;
                    narrationText.text = message.Substring(0, currentLength);
                }
            })
            .AddTo(gameObject)
            .ToUniTask(cancellationToken);
        
        // 最終的に全文を表示（念のため）
        narrationText.text = message;
    }
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
    }
}