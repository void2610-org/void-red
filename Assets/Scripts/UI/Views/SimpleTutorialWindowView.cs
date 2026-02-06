using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using Void2610.UnityTemplate;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleTutorialWindowView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private CancellationTokenSource _currentNarrationCts;
    private TextProgressController _textProgressController;

    /// <summary>
    /// ナレーションを表示
    /// </summary>
    public async UniTask DisplayText(string message, float duration = 2f, bool autoAdvance = true)
    {
        // 現在実行中のナレーションをキャンセル
        if (_currentNarrationCts != null && !_currentNarrationCts.IsCancellationRequested)
            _currentNarrationCts.Cancel();
        _currentNarrationCts?.Dispose();

        // 新しいキャンセレーショントークンを作成
        _currentNarrationCts = new CancellationTokenSource();
        var cancellationToken = _currentNarrationCts.Token;

        try
        {
            // メッセージを空で初期化（後で1文字ずつ表示）
            tutorialText.text = "";

            // 初期状態を設定
            tutorialText.gameObject.SetActive(true);

            // CanvasGroupをフェードイン
            if (_canvasGroup.alpha < 1f)
                await _canvasGroup.FadeIn(FADE_DURATION, Ease.OutQuart).ToUniTask(cancellationToken);

            // 1文字ずつ表示するアニメーション（BeginTyping()でSEループ用トークンも作成される）
            var typingToken = _textProgressController.BeginTyping();

            // ダイアログSEループを開始
            SeManager.Instance.PlaySeLoop("Dialog", cancellationToken: _textProgressController.DialogSeToken).Forget();

            try
            {
                await tutorialText.TypewriterAnimation(message, cancellationToken: typingToken);
            }
            catch (System.OperationCanceledException)
            {
                // キャンセルされた場合は全文を即座に表示
                tutorialText.text = message;
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
        }
        catch (System.OperationCanceledException)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// クリック時の処理（キーボード入力でも使用）
    /// </summary>
    // 進行処理（SEループの停止も含めてTextProgressControllerが管理）
    public void OnClick() => _textProgressController.AdvanceToNext();

    /// <summary>
    /// ナレーションを非表示にする
    /// </summary>
    public async UniTask HideNarration()
    {
        // 現在実行中のナレーションをキャンセル
        if (_currentNarrationCts != null && !_currentNarrationCts.IsCancellationRequested)
            _currentNarrationCts.Cancel();
        _currentNarrationCts?.Dispose();
        _currentNarrationCts = null;

        // CanvasGroupをフェードアウト
        await _canvasGroup.FadeOut(FADE_DURATION, Ease.InQuart);

        tutorialText.gameObject.SetActive(false);
    }

    private void Awake()
    {
        // 初期状態の設定
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        tutorialText.gameObject.SetActive(false);

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
