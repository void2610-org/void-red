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
[RequireComponent(typeof(CanvasGroup))]
public class NarrationView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrationText;
    [SerializeField] private Image backgroundImage;
    
    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _currentNarrationCts;
    private TextProgressController _textProgressController;

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
            backgroundImage.FadeIn(FADE_DURATION, Ease.OutQuart).ToUniTask(cancellationToken).Forget();
            await narrationText.FadeIn(FADE_DURATION, Ease.OutQuart);

            // 1文字ずつ表示するアニメーション（BeginTyping()でSEループ用トークンも作成される）
            var typingToken = _textProgressController.BeginTyping();

            // ダイアログSEループを開始
            SeManager.Instance.PlaySeLoop("Dialog", cancellationToken: _textProgressController.DialogSeToken).Forget();

            try
            {
                await narrationText.TypewriterAnimation(message, cancellationToken: typingToken);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた場合は全文を即座に表示
                narrationText.text = message;
            }

            // 文字送り完了（SEループも自動停止される）
            _textProgressController.CompleteTyping();

            // autoAdvanceフラグに基づいた待機処理
            if (autoAdvance)
            {
                // 自動進行の場合はタイムアウト付きで待つ（ユーザー入力でもスキップ可能）
                await _textProgressController.WaitForNextWithTimeout(duration);
            }
            else
            {
                // 手動進行の場合はユーザー入力を待つ
                await _textProgressController.WaitForNext();
            }

            // backgroundImageとテキストのフェードアウトを同時実行
            backgroundImage.FadeOut(FADE_DURATION, Ease.InQuart).ToUniTask(cancellationToken).Forget();
            await narrationText.FadeOut(FADE_DURATION, Ease.InQuart);
        }
        catch (System.OperationCanceledException) { }
    }
    
    /// <summary>
    /// クリック時の処理（キーボード入力でも使用）
    /// </summary>
    public void OnClick()
    {
        // 進行処理（SEループの停止も含めてTextProgressControllerが管理）
        _textProgressController.AdvanceToNext();
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
            await _canvasGroup.FadeOut(FADE_DURATION).ToUniTask(cancellationToken);
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

        // TextProgressControllerの初期化
        _textProgressController = new TextProgressController();
    }
    
    private void OnDestroy()
    {
        // ナレーションのキャンセレーショントークンをクリーンアップ
        _currentNarrationCts?.Cancel();
        _currentNarrationCts?.Dispose();

        // TextProgressControllerのクリーンアップ（SEループも含む）
        _textProgressController?.Dispose();
    }
}