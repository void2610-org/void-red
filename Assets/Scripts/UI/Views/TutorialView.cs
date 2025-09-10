using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// チュートリアル表示を管理するViewクラス
/// UnmaskForUGUIライブラリと連携してマスク表示とメッセージ表示を行う
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TutorialView : MonoBehaviour
{
    [SerializeField] private RectTransform maskArea;
    [SerializeField] private NarrationView narrationView;
    
    private const float FADE_DURATION = 0.3f;
    private const float MASK_TRANSITION_DURATION = 0.5f;
    
    private CanvasGroup _canvasGroup;
    private MotionHandle _currentFadeHandle;
    private MotionHandle _currentMaskPositionHandle;
    private MotionHandle _currentMaskSizeHandle;
    private bool _isFirstStep = true;
    private Vector2 _currentMaskSize = Vector2.zero;
    
    /// <summary>
    /// チュートリアルステップを表示してクリック待機
    /// </summary>
    public async UniTask ShowStepAndWaitForClick(TutorialStep step)
    {
        if (step == null) return;
        
        // 初回のみShow()を呼ぶ
        if (_isFirstStep)
        {
            await Show();
            _isFirstStep = false;
        }
        
        // マスク領域の更新（アニメーション付き）
        if (_currentMaskPositionHandle.IsActive())
            _currentMaskPositionHandle.Cancel();
        if (_currentMaskSizeHandle.IsActive())
            _currentMaskSizeHandle.Cancel();

        // 現在のマスクサイズと新しいマスクサイズをチェック
        var isCurrentSizeZero = _currentMaskSize == Vector2.zero;
        var isNewSizeZero = step.MaskSize == Vector2.zero;
        
        if (isCurrentSizeZero && !isNewSizeZero)
        {
            // (0,0) → 非(0,0) - 位置を瞬間移動してサイズアニメーションのみ
            maskArea.anchoredPosition = step.MaskPosition;
        }
        else
        {
            // （非(0,0) → 非(0,0)、同じ値など）- 通常のアニメーション
            _currentMaskPositionHandle = maskArea.MoveToAnchored(step.MaskPosition, MASK_TRANSITION_DURATION, Ease.OutQuart);
        }

        _currentMaskSizeHandle = maskArea.SizeTo(step.MaskSize, MASK_TRANSITION_DURATION, Ease.OutQuart);

        // 現在のサイズを更新
        _currentMaskSize = step.MaskSize;
        
        // メッセージテキストの更新
        await narrationView.DisplayNarration(step.Message, autoAdvance: false);
        
        // アニメーション完了を待つ
        await UniTask.Delay(TimeSpan.FromSeconds(MASK_TRANSITION_DURATION));
        
        // クリック待機
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
    }
    
    /// <summary>
    /// チュートリアルビューを表示
    /// </summary>
    private async UniTask Show()
    {
        // 現在のアニメーションをキャンセル
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        
        // フェードイン
        _currentFadeHandle = _canvasGroup.FadeIn(FADE_DURATION);
        
        await _currentFadeHandle.ToUniTask();
    }
    
    /// <summary>
    /// チュートリアルビューを非表示
    /// </summary>
    public async UniTask Hide()
    {
        // 現在のアニメーションをキャンセル
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        
        // フェードアウト
        _currentFadeHandle = _canvasGroup.FadeOut(FADE_DURATION);
        
        await _currentFadeHandle.ToUniTask();
        
        // 次回のために初期化
        _isFirstStep = true;
        _currentMaskSize = Vector2.zero;
    }
    
    private void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        maskArea.anchoredPosition = Vector2.zero;
        maskArea.sizeDelta = Vector2.zero;
    }
    
    private void OnDestroy()
    {
        // アニメーションのクリーンアップ
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        if (_currentMaskPositionHandle.IsActive())
            _currentMaskPositionHandle.Cancel();
        if (_currentMaskSizeHandle.IsActive())
            _currentMaskSizeHandle.Cancel();
    }
}