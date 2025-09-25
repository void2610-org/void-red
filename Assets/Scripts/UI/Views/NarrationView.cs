using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using System.Collections.Generic;
using Void2610.UnityTemplate;

/// <summary>
/// ナレーション表示を担当するViewクラス
/// </summary>
public class NarrationView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrationText;
    [SerializeField] private Image backgroundImage;
    
    private const float FADE_IN_DURATION = 0.3f;
    private const float FADE_OUT_DURATION = 0.3f;
    
    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _currentNarrationCts;

    /// <summary>
    /// ナレーションを表示
    /// </summary>
    public async UniTask DisplayNarration(string message, float duration = 2f, bool autoAdvance = true)
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
            narrationText.SetAlpha(0f);
            
            // backgroundImageとテキストのフェードインを同時実行
            backgroundImage.FadeIn(FADE_IN_DURATION, Ease.OutQuart).ToUniTask(cancellationToken).Forget();
            await narrationText.FadeIn(FADE_IN_DURATION, Ease.OutQuart);
            
            // 1文字ずつ表示するアニメーション
            await narrationText.TypewriterAnimation(message, cancellationToken: cancellationToken);
            
            // autoAdvanceフラグに基づいた待機処理
            if (autoAdvance)
            {
                // 自動進行の場合は指定された時間を待つ
                await UniTask.Delay((int)(duration * 1000), cancellationToken: cancellationToken);
                
                // backgroundImageとテキストのフェードアウトを同時実行
                backgroundImage.FadeOut(FADE_OUT_DURATION, Ease.InQuart).ToUniTask(cancellationToken).Forget();
                await narrationText.FadeOut(FADE_OUT_DURATION, Ease.InQuart);
            }
            else
            {
                // 手動進行の場合は表示を維持（呼び出し側で制御）
                // フェードアウトは行わない
            }
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            // autoAdvanceがfalseの場合は表示を維持
            if (autoAdvance)
            {
                _canvasGroup.alpha = 0f;
                backgroundImage.SetAlpha(0f);
                narrationText.gameObject.SetActive(false);
            }
        }
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
            backgroundImage.FadeOut(FADE_OUT_DURATION, Ease.InQuart).ToUniTask(cancellationToken).Forget();
            await narrationText.FadeOut(FADE_OUT_DURATION, Ease.InQuart);
        }
        catch (System.OperationCanceledException) { }
        finally
        {
            _canvasGroup.alpha = 0f;
            backgroundImage.SetAlpha(0f);
            narrationText.gameObject.SetActive(false);
        }
    }
    
    private void Awake()
    {
        // 初期状態の設定
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;  // CanvasGroup全体を透明に初期化
        narrationText.gameObject.SetActive(false);
        
        // backgroundImageの初期状態を透明に設定
        backgroundImage.SetAlpha(0f);
    }
    
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();
    }
}