using UnityEngine;
using UnityEngine.UI;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// ウィンドウのようなUI要素の基底クラス
/// 開閉処理とナビゲーション管理を共通化
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseWindowView : MonoBehaviour
{
    [Header("基本コンポーネント")]
    [SerializeField] protected Button closeButton;

    private const float FADE_ANIMATION_DURATION = 0.15f;

    protected readonly CompositeDisposable Disposables = new();
    
    private CanvasGroup _canvasGroup;
    private MotionHandle _currentFadeHandle;

    /// <summary>
    /// ウィンドウの表示状態
    /// </summary>
    public bool IsShowing => _canvasGroup.interactable;

    /// <summary>
    /// ウィンドウを表示
    /// </summary>
    public virtual void Show()
    {
        ShowWithAnimation().Forget();
    }

    /// <summary>
    /// ウィンドウを非表示
    /// </summary>
    public virtual void Hide()
    {
        HideWithAnimation().Forget();
    }

    private async UniTaskVoid ShowWithAnimation()
    {
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _currentFadeHandle = _canvasGroup.FadeIn(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();
        SafeNavigationManager.SetSelectedGameObjectSafe(closeButton.gameObject);
    }

    private async UniTaskVoid HideWithAnimation()
    {
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _currentFadeHandle = _canvasGroup.FadeOut(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();
        SafeNavigationManager.SelectRootForceSelectable();
    }

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        closeButton.OnClickAsObservable()
            .Subscribe(_ => Hide())
            .AddTo(Disposables);
    }

    protected virtual void OnDestroy()
    {
        Disposables.Dispose();
        _currentFadeHandle.TryCancel();
    }
}
