using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 展示モードのアイドル時オーバーレイUI
/// DontDestroyOnLoadで常駐し、フェード付きで表示/非表示を切り替える
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ExhibitOverlayView : MonoBehaviour, IExhibitOverlay
{
    /// <summary>オーバーレイが表示中か</summary>
    public bool IsVisible { get; private set; }

    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private MotionHandle _fadeHandle;

    public void Show()
    {
        if (IsVisible) return;
        IsVisible = true;

        CancelFade();
        _fadeHandle = _canvasGroup.FadeIn(FADE_DURATION, Ease.OutQuart, ignoreTimeScale: true);
    }

    public void Hide()
    {
        if (!IsVisible) return;
        IsVisible = false;

        CancelFade();
        _fadeHandle = _canvasGroup.FadeOut(FADE_DURATION, Ease.InQuart, ignoreTimeScale: true);
    }

    private void CancelFade() => _fadeHandle.TryCancel();

    /// <summary>
    /// オーバーレイ用のCanvasと背景Imageをプログラムで生成
    /// </summary>
    private void InitializeCanvas()
    {
        // Canvas設定
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;

        var canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        // 背景Image作成
        var background = new GameObject("Background");
        background.transform.SetParent(transform, false);

        var image = background.AddComponent<Image>();
        image.color = Color.black;

        // RectTransformを全画面サイズに設定
        var rectTransform = background.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        InitializeCanvas();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        CancelFade();
    }
}
