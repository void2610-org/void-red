using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using R3;

/// <summary>
/// 確認ダイアログの表示を担当するViewクラス
/// </summary>
public class ConfirmationDialogView : BaseWindowView
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    
    private CancellationTokenSource _currentDialogCts;
    private UniTaskCompletionSource<bool> _dialogResult;

    /// <summary>
    /// 確認ダイアログを表示し、ユーザーの選択を待つ
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="confirmText">確認ボタンのテキスト（デフォルト: "OK"）</param>
    /// <param name="cancelText">キャンセルボタンのテキスト（デフォルト: "キャンセル"）</param>
    /// <returns>true: 確認, false: キャンセル</returns>
    public async UniTask<bool> ShowDialog(string message, string confirmText = "OK", string cancelText = "キャンセル")
    {
        // 現在実行中のダイアログをキャンセル
        _currentDialogCts?.Cancel();
        _currentDialogCts?.Dispose();
        
        // 新しいキャンセレーショントークンを作成
        _currentDialogCts = new CancellationTokenSource();
        
        // アプリケーション終了時にもキャンセルされるようにする
        var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
            _currentDialogCts.Token,
            this.GetCancellationTokenOnDestroy(),
            Application.exitCancellationToken
        ).Token;

        try
        {
            // ダイアログの結果を管理するCompletionSourceを作成
            _dialogResult = new UniTaskCompletionSource<bool>();

            // メッセージとボタンテキストを設定
            messageText.text = message;
            confirmButtonText.text = confirmText;
            cancelButtonText.text = cancelText;

            // ボタンのインタラクションを有効化
            confirmButton.interactable = true;

            // ダイアログを表示
            Show();

            // ユーザーの選択を待つ
            var result = await _dialogResult.Task;

            await UniTask.Yield(cancellationToken);

            // ダイアログを非表示
            Hide();

            return result;
        }
        catch (System.OperationCanceledException)
        {
            Hide();
            return false;
        }
    }
    
    protected override void Awake()
    {
        base.Awake();

        // ボタンイベントの設定
        confirmButton.OnClickAsObservable()
            .Subscribe(_ => _dialogResult?.TrySetResult(true))
            .AddTo(Disposables);
        closeButton.OnClickAsObservable()
            .Subscribe(_ => _dialogResult?.TrySetResult(false))
            .AddTo(Disposables);

        // ボタンを無効化
        confirmButton.interactable = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // ダイアログのキャンセレーショントークンをクリーンアップ
        _currentDialogCts?.Cancel();
        _currentDialogCts?.Dispose();

        // 未完了のCompletionSourceをキャンセル
        _dialogResult?.TrySetCanceled();
    }
}