using System.Collections.Generic;
using LitMotion;
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
        var targets = new List<(RectTransform, CanvasGroup)>();
        for (var i = 0; i < buttonsLayoutGroup.transform.childCount; i++)
        {
            var child = buttonsLayoutGroup.transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;
            targets.Add((child.GetComponent<RectTransform>(), null));
        }

        targets.StaggeredSlideIn(new Vector2(buttonSlideOffset, 0), slideDuration, buttonStaggerDelay,
            _buttonAnimHandles, moveEase: Ease.OutQuad, fadeEase: null, ignoreTimeScale: true);
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
