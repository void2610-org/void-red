using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;
using DelayType = Cysharp.Threading.Tasks.DelayType;

/// <summary>
/// シーン遷移とクロスフェード演出を管理するマネージャー
/// VContainerでSingletonとして登録され、全シーンで共有される
/// </summary>
public class SceneTransitionManager : IDisposable
{
    // フェード設定
    private const float DEFAULT_FADE_DURATION = 0.5f;
    private const float MAX_OPACITY = 1f;
    
    // フェード用UI要素
    private GameObject _fadeCanvas;
    private Image _fadeImage;
    private MotionHandle _currentFadeHandle;
    
    private readonly DiscordService _discordService;
    private readonly InputActionsProvider _inputActionsProvider;
    
    /// <summary>
    /// 現在フェード中かどうか
    /// </summary>
    public bool IsFading { get; private set; }
    
    /// <summary>
    /// コンストラクタでフェード用のUIを初期化
    /// </summary>
    public SceneTransitionManager(DiscordService discordService, InputActionsProvider inputActionsProvider)
    {
        _discordService = discordService;
        _inputActionsProvider = inputActionsProvider;
        InitializeFadeCanvas();
    }
    
    /// <summary>
    /// フェード用のCanvasとImageを作成・初期化
    /// </summary>
    private void InitializeFadeCanvas()
    {
        // フェード用Canvasの作成
        _fadeCanvas = new GameObject("SceneTransitionCanvas");
        UnityEngine.Object.DontDestroyOnLoad(_fadeCanvas);
        
        // Canvas設定
        var canvas = _fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 最前面に表示
        
        var canvasScaler = _fadeCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        _fadeCanvas.AddComponent<GraphicRaycaster>();
        
        // フェード用Image作成
        var imageObject = new GameObject("FadeImage");
        imageObject.tag = "IgnoreHoverSelection";
        imageObject.transform.SetParent(_fadeCanvas.transform, false);
        
        _fadeImage = imageObject.AddComponent<Image>();
        _fadeImage.color = new Color(0, 0, 0, 0); // 初期状態は透明
        
        // RectTransformを全画面サイズに設定
        var rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 初期状態では非表示
        _fadeCanvas.SetActive(false);
    }
    
    /// <summary>
    /// クロスフェード演出付きでシーンを遷移
    /// </summary>
    /// <param name="targetScene">遷移先のシーンタイプ</param>
    /// <param name="fadeDuration">フェードの時間（省略時はデフォルト値）</param>
    /// <returns>遷移完了のUniTask</returns>
    public async UniTask TransitionToSceneWithFade(SceneType targetScene, float fadeDuration = DEFAULT_FADE_DURATION)
    {
        if (IsFading) return;
        
        IsFading = true;
        
        try
        {
            // 念の為timeScaleを1に戻す
            Time.timeScale = 1;
            
            BgmManager.Instance.Stop().Forget();
            await FadeIn(fadeDuration);
            
            // シーンをロード
            var sceneName = targetScene.ToSceneName();
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;
            // ロード完了を待つ（90%まで）
            await UniTask.WaitUntil(() => asyncOperation.progress >= 0.9f);
            asyncOperation.allowSceneActivation = true;
            // シーンの有効化完了を待つ
            await UniTask.WaitUntil(() => asyncOperation.isDone);
            await UniTask.Delay(100, DelayType.UnscaledDeltaTime);
            
            // InputSystemを更新
            _inputActionsProvider.EnableActionMapsForScene(targetScene);
            
            // Discord Rich Presence更新
            _discordService.SetSceneState(targetScene);

            // シーンの初期化完了を待つ
            await WaitForSceneReady();

            await FadeOut(fadeDuration);
        }
        finally
        {
            IsFading = false;
        }
    }
    
    /// <summary>
    /// シーンの初期化完了を待つ
    /// </summary>
    private async UniTask WaitForSceneReady()
    {
        // SceneInitializationBridgeを検索
        var bridge = UnityEngine.Object.FindAnyObjectByType<SceneInitializationBridge>();
        if (bridge != null)
        {
            Debug.Log("[SceneTransitionManager] シーン初期化完了を待機中...");
            await bridge.WaitForInitializationAsync();
            Debug.Log("[SceneTransitionManager] シーン初期化完了");
        }
    }

    /// <summary>
    /// 画面をフェードイン（暗転）
    /// </summary>
    /// <param name="duration">フェード時間</param>
    /// <returns>完了のUniTask</returns>
    private async UniTask FadeIn(float duration)
    {
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        
        _fadeCanvas.SetActive(true);
        _fadeImage.color = new Color(0, 0, 0, 0); // 完全に透明な黒から開始

        _currentFadeHandle = _fadeImage.FadeIn(duration, Ease.OutQuart, ignoreTimeScale: true);
        
        await _currentFadeHandle.ToUniTask();
    }
    
    /// <summary>
    /// 画面をフェードアウト（明転）
    /// </summary>
    /// <param name="duration">フェード時間</param>
    /// <returns>完了のUniTask</returns>
    private async UniTask FadeOut(float duration)
    {
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        
        _currentFadeHandle = _fadeImage.FadeOut(duration, Ease.InQuart, ignoreTimeScale: true);
        
        await _currentFadeHandle.ToUniTask();
        
        _fadeCanvas.SetActive(false);
    }
    
    /// <summary>
    /// リソースの破棄
    /// </summary>
    public void Dispose()
    {
        if (_currentFadeHandle.IsActive())
            _currentFadeHandle.Cancel();
        
        if (_fadeCanvas) UnityEngine.Object.Destroy(_fadeCanvas);
    }
}