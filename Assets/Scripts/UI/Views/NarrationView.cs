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
    [SerializeField] private Image narrationBackground;
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
            // テキストの位置をリセット（前回のアニメーションの影響を除去）
            var textRect = narrationText.rectTransform;
            textRect.anchoredPosition = Vector2.zero;
            
            // メッセージを空で初期化（後で1文字ずつ表示）
            narrationText.text = "";
            
            // 初期状態を設定
            narrationBackground.gameObject.SetActive(true);
            narrationText.gameObject.SetActive(true);
            narrationBackground.color = new Color(narrationBackground.color.r, narrationBackground.color.g, narrationBackground.color.b, 0f);
            narrationText.color = new Color(narrationText.color.r, narrationText.color.g, narrationText.color.b, 0f);
            
            // フェードインアニメーション
            var fadeInTasks = new UniTask[2];
            
            // 背景のフェードイン
            var bgColor = narrationBackground.color;
            fadeInTasks[0] = LMotion.Create(new Color(bgColor.r, bgColor.g, bgColor.b, 0f), new Color(bgColor.r, bgColor.g, bgColor.b, 0.95f), FADE_IN_DURATION)
                .WithEase(Ease.OutQuart)
                .BindToColor(narrationBackground)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            // テキストのフェードイン
            var textColor = narrationText.color;
            fadeInTasks[1] = LMotion.Create(new Color(textColor.r, textColor.g, textColor.b, 0f), new Color(textColor.r, textColor.g, textColor.b, 1f), FADE_IN_DURATION)
                .WithEase(Ease.OutQuart)
                .BindToColor(narrationText)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            await UniTask.WhenAll(fadeInTasks);
            
            // 1文字ずつ表示するアニメーション
            await TypewriterAnimation(message, cancellationToken);
            
            // 表示時間を待つ
            await UniTask.Delay((int)(duration * 1000), cancellationToken: cancellationToken);
            
            // フェードアウトアニメーション
            var fadeOutTasks = new UniTask[2];
            
            // 背景のフェードアウト
            var bgColorOut = narrationBackground.color;
            fadeOutTasks[0] = LMotion.Create(new Color(bgColorOut.r, bgColorOut.g, bgColorOut.b, 0.95f), new Color(bgColorOut.r, bgColorOut.g, bgColorOut.b, 0f), FADE_OUT_DURATION)
                .WithEase(Ease.InQuart)
                .BindToColor(narrationBackground)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            // テキストのフェードアウト
            var textColorOut = narrationText.color;
            fadeOutTasks[1] = LMotion.Create(new Color(textColorOut.r, textColorOut.g, textColorOut.b, 1f), new Color(textColorOut.r, textColorOut.g, textColorOut.b, 0f), FADE_OUT_DURATION)
                .WithEase(Ease.InQuart)
                .BindToColor(narrationText)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken);
            
            await UniTask.WhenAll(fadeOutTasks);
            
            // 最終クリーンアップ
            if (narrationBackground && narrationText)
            {
                narrationBackground.gameObject.SetActive(false);
                narrationText.gameObject.SetActive(false);
                textRect.anchoredPosition = Vector2.zero;
            }
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合のクリーンアップ
            if (narrationBackground && narrationText)
            {
                narrationBackground.gameObject.SetActive(false);
                narrationText.gameObject.SetActive(false);
                var textRect = narrationText.rectTransform;
                if (textRect)
                {
                    textRect.anchoredPosition = Vector2.zero;
                }
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
        narrationBackground.gameObject.SetActive(false);
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
        if (narrationText)
        {
            narrationText.text = message;
        }
    }
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
    }
}