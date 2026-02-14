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
    [SerializeField] private StaggeredSlideInGroup buttonStagger;

    [Header("アニメーション設定")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float hiddenX = -400f;
    [SerializeField] private float shownX;

    public bool IsShowing { get; private set; }
    public Observable<Unit> OnResumeButtonClicked => resumeButton.OnClickAsObservable();
    public Observable<Unit> OnHomeButtonClicked => homeButton.OnClickAsObservable();
    public Observable<Unit> OnHelpButtonClicked => helpButton.OnClickAsObservable();
    public Observable<Unit> OnOptionButtonClicked => optionButton.OnClickAsObservable();

    private CanvasGroup _canvasGroup;
    private MotionHandle _slideHandle;

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

        _slideHandle = panelRect.MoveToX(shownX, slideDuration, Ease.OutQuad, ignoreTimeScale: true);
        _canvasGroup.FadeIn(0.1f, ignoreTimeScale: true);

        buttonStagger.Play();
    }

    public void Hide()
    {
        Time.timeScale = 1;
        IsShowing = false;
        _slideHandle.TryCancel();
        buttonStagger.Cancel();

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
    }
}
