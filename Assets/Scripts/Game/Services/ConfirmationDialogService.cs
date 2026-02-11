using Cysharp.Threading.Tasks;
using UnityEngine;
using Void2610.SettingsSystem;

/// <summary>
/// 確認ダイアログサービスの実装
/// IConfirmationDialogインターフェースを実装
/// </summary>
public class ConfirmationDialogService : IConfirmationDialog
{
    private readonly ConfirmationDialogView _confirmationDialogViewPrefab;
    private ConfirmationDialogView _dialogInstance;

    public ConfirmationDialogService(ConfirmationDialogView confirmationDialogView)
    {
        _confirmationDialogViewPrefab = confirmationDialogView;
    }

    /// <summary>
    /// 確認ダイアログを表示し、ユーザーの選択を待つ
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="confirmText">確認ボタンのテキスト</param>
    /// <param name="cancelText">キャンセルボタンのテキスト</param>
    /// <returns>ユーザーが確認した場合はtrue、キャンセルした場合はfalse</returns>
    public async UniTask<bool> ShowDialog(string message, string confirmText = "OK", string cancelText = "キャンセル")
    {
        if (!_dialogInstance)
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            _dialogInstance = Object.Instantiate(_confirmationDialogViewPrefab, canvas.transform);
        }

        return await _dialogInstance.ShowDialog(message, confirmText, cancelText);
    }
}
