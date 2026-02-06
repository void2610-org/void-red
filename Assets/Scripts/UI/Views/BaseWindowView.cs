using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.UI;
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
    private readonly Subject<Unit> _onClosed = new();
    private bool _isInitialized;

    private void EnsureInitialized()
    {
        if (_isInitialized) return;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _isInitialized = true;
    }

    // アクティブなウィンドウをリストで管理（任意の順序で閉じられるように）
    private static readonly List<BaseWindowView> _activeWindows = new();

    public static GameObject GetTopActiveWindowCloseButton()
    {
        if (_activeWindows.Count == 0) return null;
        var topWindow = _activeWindows[^1]; // 最後の要素（最新のウィンドウ）
        return topWindow ? topWindow.GetPreferredNavigationTarget() : null;
    }

    /// <summary>
    /// アクティブなウィンドウが存在するか
    /// </summary>
    public static bool HasActiveWindows => _activeWindows.Count > 0;

    /// <summary>
    /// ウィンドウの表示状態
    /// </summary>
    public bool IsShowing
    {
        get
        {
            EnsureInitialized();
            return _canvasGroup.interactable;
        }
    }

    /// <summary>
    /// このウィンドウが再アクティブになったときに選択すべきGameObject
    /// デフォルトはcloseButton、継承先でオーバーライド可能
    /// </summary>
    protected virtual GameObject GetPreferredNavigationTarget()
    {
        return closeButton ? closeButton.gameObject : null;
    }

    /// <summary>
    /// ウィンドウを表示
    /// </summary>
    public virtual void Show() => ShowWithAnimation().Forget();

    /// <summary>
    /// ウィンドウを非表示
    /// </summary>
    public virtual void Hide() => HideWithAnimation().Forget();

    /// <summary>
    /// ウィンドウの表示/非表示を切り替え
    /// </summary>
    public void Toggle()
    {
        if (IsShowing) Hide();
        else Show();
    }

    /// <summary>
    /// ウィンドウが閉じられるまで待機
    /// </summary>
    public async UniTask WaitForClose() => await _onClosed.FirstAsync();

    private async UniTaskVoid ShowWithAnimation()
    {
        EnsureInitialized();
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // このウィンドウをアクティブなウィンドウリストに追加（重複チェック）
        if (!_activeWindows.Contains(this))
            _activeWindows.Add(this);

        var navigationTarget = GetPreferredNavigationTarget();
        if (navigationTarget)
            SafeNavigationManager.SetSelectedGameObjectSafe(navigationTarget);

        _currentFadeHandle = _canvasGroup.FadeIn(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();
    }

    private async UniTaskVoid HideWithAnimation()
    {
        EnsureInitialized();
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // リストから自身を削除（順序に関係なく確実に削除）
        _activeWindows.Remove(this);

        _currentFadeHandle = _canvasGroup.FadeOut(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();

        // 閉じたことを通知
        _onClosed.OnNext(Unit.Default);

        // リストに他のウィンドウがあればその優先ナビゲーション対象を選択、なければルートを選択
        if (_activeWindows.Count > 0)
        {
            var topWindow = _activeWindows[^1]; // 最後の要素（最新のウィンドウ）
            if (topWindow)
            {
                var navigationTarget = topWindow.GetPreferredNavigationTarget();
                if (navigationTarget)
                {
                    SafeNavigationManager.SetSelectedGameObjectSafe(navigationTarget);
                }
            }
        }
        else
        {
            SafeNavigationManager.SelectRootForceSelectable().Forget();
        }
    }

    protected virtual void Awake()
    {
        EnsureInitialized();

        if (closeButton)
        {
            closeButton.OnClickAsObservable()
                .Subscribe(_ => Hide())
                .AddTo(Disposables);
        }
    }

    protected virtual void OnDestroy()
    {
        Disposables.Dispose();
        _currentFadeHandle.TryCancel();
        _onClosed?.Dispose();

        // リストから自身を削除（シーン遷移時のクリーンアップ）
        _activeWindows.Remove(this);
    }
}
