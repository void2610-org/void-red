using UnityEngine;
using UnityEngine.UI;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
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

    [Header("アニメーション設定")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float hiddenX = -400f;
    [SerializeField] private float shownX = 0f;

    public bool IsShowing { get; private set; }
    public Observable<Unit> OnResumeButtonClicked => resumeButton.OnClickAsObservable();
    public Observable<Unit> OnHomeButtonClicked => homeButton.OnClickAsObservable();
    public Observable<Unit> OnHelpButtonClicked => helpButton.OnClickAsObservable();
    public Observable<Unit> OnOptionButtonClicked => optionButton.OnClickAsObservable();

    private CanvasGroup _canvasGroup;
    private MotionHandle _slideHandle;

    public void Show()
    {
        if (IsShowing) return;
        IsShowing = true;

        Time.timeScale = 0;
        gameObject.SetActive(true);

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = true;

        _slideHandle.TryCancel();
        _slideHandle = panelRect.MoveToX(shownX, slideDuration, Ease.OutQuad, ignoreTimeScale: true);
        _slideHandle.ToUniTask().ContinueWith(() => _canvasGroup.interactable = true).Forget();
    }

    public void Hide()
    {
        if (!IsShowing) return;
        IsShowing = false;

        Time.timeScale = 1;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _slideHandle.TryCancel();
        _slideHandle = panelRect.MoveToX(hiddenX, slideDuration, Ease.InQuad, ignoreTimeScale: true);
        _slideHandle.ToUniTask().ContinueWith(() => gameObject.SetActive(false)).Forget();
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        panelRect.anchoredPosition = new Vector2(hiddenX, panelRect.anchoredPosition.y);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _slideHandle.TryCancel();
    }
}
