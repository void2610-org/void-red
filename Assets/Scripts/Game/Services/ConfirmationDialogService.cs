using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Void2610.SettingsSystem;

/// <summary>
/// 確認ダイアログサービスの実装
/// IConfirmationDialogインターフェースを実装
/// </summary>
public class ConfirmationDialogService : IConfirmationDialog
{
    private readonly ConfirmationDialogView _confirmationDialogViewPrefab;
    private ConfirmationDialogView _dialogInstance;
    private GameObject _dialogCanvas;

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
            // 専用Canvasを作成（DontDestroyOnLoadでシーン遷移しても破棄されない）
            _dialogCanvas = new GameObject("ConfirmationDialogCanvas");
            Object.DontDestroyOnLoad(_dialogCanvas);

            var canvas = _dialogCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = _dialogCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _dialogCanvas.AddComponent<GraphicRaycaster>();

            _dialogInstance = Object.Instantiate(_confirmationDialogViewPrefab, _dialogCanvas.transform);
        }

        return await _dialogInstance.ShowDialog(message, confirmText, cancelText);
    }
}
