using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using System.Collections.Generic;
using Void2610.UnityTemplate;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleTutorialWindowView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Image backgroundImage;

    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _currentNarrationCts;
    private CancellationTokenSource _dialogSeCancellationTokenSource;
    private bool _isTyping;

    /// <summary>
    /// ナレーションを表示
    /// </summary>
    public async UniTask DisplayText(string message, float duration = 2f, bool autoAdvance = true)
    {
        // 現在実行中のナレーションをキャンセル
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
        
        // 新しいキャンセレーショントークンを作成
        _currentNarrationCts = new CancellationTokenSource();
        var cancellationToken = _currentNarrationCts.Token;
        
        _canvasGroup.alpha = 1f;
        
        try
        {
            // メッセージを空で初期化（後で1文字ずつ表示）
            tutorialText.text = "";
            
            // 初期状態を設定
            tutorialText.gameObject.SetActive(true);
            tutorialText.SetAlpha(0f);
            
            // backgroundImageとテキストのフェードインを同時実行
            backgroundImage.FadeIn(FADE_DURATION, Ease.OutQuart).ToUniTask(cancellationToken).Forget();
            await tutorialText.FadeIn(FADE_DURATION, Ease.OutQuart);

            // ダイアログSEループを開始
            _dialogSeCancellationTokenSource = new CancellationTokenSource();
            SeManager.Instance.PlaySeLoop("Dialog", cancellationToken: _dialogSeCancellationTokenSource.Token).Forget();

            // 1文字ずつ表示するアニメーション
            _isTyping = true;
            await tutorialText.TypewriterAnimation(message, cancellationToken: cancellationToken);
            _isTyping = false;

            // dialogSeループを停止
            _dialogSeCancellationTokenSource?.Cancel();
            _dialogSeCancellationTokenSource?.Dispose();
            _dialogSeCancellationTokenSource = null;
            
            // autoAdvanceフラグに基づいた待機処理
            if (autoAdvance)
            {
                // 自動進行の場合は指定された時間を待つ
                await UniTask.Delay((int)(duration * 1000), cancellationToken: cancellationToken);
                
                // backgroundImageとテキストのフェードアウトを同時実行
                backgroundImage.FadeOut(FADE_DURATION, Ease.InQuart).ToUniTask(cancellationToken).Forget();
                await tutorialText.FadeOut(FADE_DURATION, Ease.InQuart);
            }
        }
        catch (System.OperationCanceledException) { }
    }
    
    /// <summary>
    /// タイピングアニメーションをスキップ
    /// </summary>
    public void SkipTyping()
    {
        if (!_isTyping) return;

        // タイピングアニメーションをキャンセル
        _currentNarrationCts?.Cancel();

        // ダイアログSEを停止
        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();
        _dialogSeCancellationTokenSource = null;

        _isTyping = false;
    }

    /// <summary>
    /// ナレーションを非表示にする
    /// </summary>
    public async UniTask HideNarration()
    {
        // 現在実行中のナレーションをキャンセル
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
        
        // 新しいキャンセレーショントークンを作成
        _currentNarrationCts = new CancellationTokenSource();
        
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
            _currentNarrationCts.Token,
            this.GetCancellationTokenOnDestroy(),
            Application.exitCancellationToken
        ).Token;
        
        try
        {
            // backgroundImageとテキストのフェードアウトを同時実行
            backgroundImage.FadeOut(FADE_DURATION, Ease.InQuart).ToUniTask(cancellationToken).Forget();
            await tutorialText.FadeOut(FADE_DURATION, Ease.InQuart);
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            _canvasGroup.alpha = 0f;
            backgroundImage.SetAlpha(0f);
            tutorialText.gameObject.SetActive(false);
        }
    }
    
    private void Awake()
    {
        // 初期状態の設定
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;  // CanvasGroup全体を透明に初期化
        tutorialText.gameObject.SetActive(false);
        
        // backgroundImageの初期状態を透明に設定
        backgroundImage.SetAlpha(0f);
    }
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();

        // ダイアログSEのキャンセレーショントークンをクリーンアップ
        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();
    }
}