
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConfirmationDialogService
{
    private ConfirmationDialogView _confirmationDialogViewPrefab;
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