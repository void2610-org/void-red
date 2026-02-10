using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// チュートリアル表示を管理するViewクラス
/// UnmaskForUGUIライブラリと連携してマスク表示とメッセージ表示を行う
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TutorialView : MonoBehaviour
{
    [SerializeField] private RectTransform maskArea;
    [SerializeField] private SimpleTutorialWindowView simpleTutorialWindow;
    [SerializeField] private Button clickAreaButton;

    private const float FADE_DURATION = 0.3f;
    private const float MASK_TRANSITION_DURATION = 0.5f;

    private CanvasGroup _canvasGroup;
    private MotionHandle _currentFadeHandle;
    private MotionHandle _currentMaskPositionHandle;
    private MotionHandle _currentMaskSizeHandle;
    private Vector2 _currentMaskSize = Vector2.zero;

    // 各Viewに対してクリックを通知（各Viewが自分の状態に応じて処理）
    public void NotifyAdvance() => simpleTutorialWindow.OnClick();

    /// <summary>
    /// チュートリアルステップを表示してクリック待機
    /// </summary>
    public async UniTask ShowStepAndWaitForClick(TutorialStep step, string messageOverride = null)
    {
        _currentMaskPositionHandle.TryCancel();
        _currentMaskSizeHandle.TryCancel();

        var isCurrentSizeZero = _currentMaskSize == Vector2.zero;
        var isNewSizeZero = step.MaskSize == Vector2.zero;

        if (!isCurrentSizeZero && isNewSizeZero)
        {
            // 非(0,0) → (0,0) - その場で縮小（位置は動かさない）
        }
        else if (isCurrentSizeZero && !isNewSizeZero)
        {
            // (0,0) → 非(0,0) - 位置を瞬間移動してサイズアニメーションのみ
            maskArea.anchoredPosition = step.MaskPosition;
        }
        else
        {
            _currentMaskPositionHandle = maskArea.MoveToAnchored(step.MaskPosition, MASK_TRANSITION_DURATION, Ease.OutQuart);
        }

        _currentMaskSizeHandle = maskArea.SizeTo(step.MaskSize, MASK_TRANSITION_DURATION, Ease.OutQuart);
        _currentMaskSize = step.MaskSize;

        await UpdateMessageText(messageOverride ?? step.Message, step.IsProtagonist);
        await UniTask.Delay(TimeSpan.FromSeconds(MASK_TRANSITION_DURATION));

        SafeNavigationManager.SelectRootForceSelectable().Forget();
    }

    /// <summary>
    /// チュートリアルビューを表示
    /// </summary>
    public async UniTask Show()
    {
        _currentFadeHandle.TryCancel();
        _currentFadeHandle = _canvasGroup.FadeIn(FADE_DURATION);

        await _currentFadeHandle.ToUniTask();
        SafeNavigationManager.SetSelectedGameObjectSafe(clickAreaButton.gameObject);
    }

    /// <summary>
    /// チュートリアルビューを非表示
    /// </summary>
    public async UniTask Hide()
    {
        // マスクをその場で縮小
        _currentMaskSizeHandle.TryCancel();
        _currentMaskSizeHandle = maskArea.SizeTo(Vector2.zero, MASK_TRANSITION_DURATION, Ease.OutQuart);
        await UniTask.Delay(TimeSpan.FromSeconds(MASK_TRANSITION_DURATION));

        _currentFadeHandle.TryCancel();
        _currentFadeHandle = _canvasGroup.FadeOut(FADE_DURATION);

        await _currentFadeHandle.ToUniTask();
        _currentMaskSize = Vector2.zero;
    }

    private async UniTask UpdateMessageText(string message, bool isProtagonist)
    {
        await simpleTutorialWindow.DisplayText(message, isProtagonist, autoAdvance: false);
    }

    private void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        _canvasGroup.Hide();
        maskArea.anchoredPosition = Vector2.zero;
        maskArea.sizeDelta = Vector2.zero;
    }

    private void OnDestroy()
    {
        _currentFadeHandle.TryCancel();
        _currentMaskPositionHandle.TryCancel();
        _currentMaskSizeHandle.TryCancel();
    }
}
