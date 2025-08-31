using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using R3;
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
    [SerializeField] private TextMeshProUGUI messageText;
    
    private const float FADE_DURATION = 0.3f;
    private const float MASK_TRANSITION_DURATION = 0.5f;
    
    private CanvasGroup _canvasGroup;
    private MotionHandle _currentFadeHandle;
    private MotionHandle _currentMaskHandle;
    private bool _isFirstStep = true;
    
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
        if (_currentMaskHandle.IsActive())
            _currentMaskHandle.Cancel();
        
        _currentMaskHandle = LMotion.Create(maskArea.anchoredPosition, step.MaskPosition, MASK_TRANSITION_DURATION)
            .WithEase(Ease.OutQuart)
            .BindToAnchoredPosition(maskArea)
            .AddTo(gameObject);
        
        // サイズもアニメーション
        LMotion.Create(maskArea.sizeDelta, step.MaskSize, MASK_TRANSITION_DURATION)
            .WithEase(Ease.OutQuart)
            .BindToSizeDelta(maskArea)
            .AddTo(gameObject);
        
        // メッセージテキストの更新
        messageText.text = step.Message;
        
        // アニメーション完了を待つ
        await UniTask.Delay(TimeSpan.FromSeconds(MASK_TRANSITION_DURATION));
        
        // クリック待機
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
    }
    
    /// <summary>
    /// チュートリアルビューを表示
    /// </summary>
    public async UniTask Show()
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
    }
    
    private void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        messageText.text = "";
        maskArea.anchoredPosition = Vector2.zero;
        maskArea.sizeDelta = Vector2.zero;
    }
    
    private void OnDestroy()
    {
        // アニメーションのクリーンアップ
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        if (_currentMaskHandle.IsActive())
            _currentMaskHandle.Cancel();
    }
}