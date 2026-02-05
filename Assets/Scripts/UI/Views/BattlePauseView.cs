using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// バトルシーン用ポーズメニュー
/// 画面左側からスライドインするUI
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public sealed class BattlePauseView : MonoBehaviour, IPauseView
{
    [Header("UI要素")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private LayoutGroup buttonsLayoutGroup;

    [Header("アニメーション設定")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float hiddenX = -400f;
    [SerializeField] private float shownX;
    [SerializeField] private float buttonSlideOffset = -50f;
    [SerializeField] private float buttonStaggerDelay = 0.05f;

    public bool IsShowing { get; private set; }
    public Observable<Unit> OnResumeButtonClicked => resumeButton.OnClickAsObservable();
    public Observable<Unit> OnHomeButtonClicked => homeButton.OnClickAsObservable();
    public Observable<Unit> OnHelpButtonClicked => helpButton.OnClickAsObservable();
    public Observable<Unit> OnOptionButtonClicked => optionButton.OnClickAsObservable();

    private CanvasGroup _canvasGroup;
    private MotionHandle _slideHandle;
    private List<MotionHandle> _buttonAnimHandles = new();
    private Dictionary<Transform, Vector2> _buttonOriginalPositions = new();

    public void Toggle()
    {
        if (IsShowing) Hide();
        else Show();
    }

    public void Show()
    {
        Time.timeScale = 0;
        IsShowing = true;
        _slideHandle.TryCancel();
        _buttonAnimHandles.CancelAll();

        _slideHandle = panelRect.MoveToX(shownX, slideDuration, Ease.OutQuad, ignoreTimeScale: true);
        _canvasGroup.FadeIn(0.1f, ignoreTimeScale: true);

        Canvas.ForceUpdateCanvases();

        // ボタンの順次スライドアニメーション
        buttonsLayoutGroup.ForEachChildWithDelay((child, _, delay) =>
        {
            var rect = child.GetComponent<RectTransform>();

            // 初回は現在位置を保存、2回目以降は保存済みの位置を使用
            if (!_buttonOriginalPositions.TryGetValue(child, out var targetPos))
            {
                targetPos = rect.anchoredPosition;
                _buttonOriginalPositions[child] = targetPos;
            }

            var startPos = new Vector2(targetPos.x + buttonSlideOffset, targetPos.y);
            rect.anchoredPosition = startPos;

            var handle = LMotion.Create(startPos, targetPos, slideDuration)
                .WithDelay(delay)
                .WithEase(Ease.OutQuad)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .BindToAnchoredPosition(rect)
                .AddTo(rect.gameObject);

            _buttonAnimHandles.Add(handle);
        }, buttonStaggerDelay);
    }

    public void Hide()
    {
        Time.timeScale = 1;
        IsShowing = false;
        _slideHandle.TryCancel();
        _buttonAnimHandles.CancelAll();

        _slideHandle = panelRect.MoveToX(hiddenX, slideDuration, Ease.InQuad, ignoreTimeScale: true);
        _canvasGroup.FadeOut(0.1f, ignoreTimeScale: true);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        panelRect.anchoredPosition = new Vector2(hiddenX, panelRect.anchoredPosition.y);

    }

    private void OnDestroy()
    {
        _slideHandle.TryCancel();
        _buttonAnimHandles.CancelAll();
    }
}
