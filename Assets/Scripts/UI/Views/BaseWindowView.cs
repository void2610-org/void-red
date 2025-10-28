using System.Collections.Generic;
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
    private readonly Subject<Unit> _onClosed = new();

    // アクティブなウィンドウをリストで管理（任意の順序で閉じられるように）
    private static readonly List<BaseWindowView> _activeWindows = new();

    /// <summary>
    /// アクティブなウィンドウが存在するか
    /// </summary>
    public static bool HasActiveWindows => _activeWindows.Count > 0;

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

    /// <summary>
    /// ウィンドウが閉じられるまで待機
    /// </summary>
    public async UniTask WaitForClose()
    {
        await _onClosed.FirstAsync();
    }

    private async UniTaskVoid ShowWithAnimation()
    {
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // このウィンドウをアクティブなウィンドウリストに追加（重複チェック）
        if (!_activeWindows.Contains(this))
            _activeWindows.Add(this);

        _currentFadeHandle = _canvasGroup.FadeIn(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();
        SafeNavigationManager.SetSelectedGameObjectSafe(closeButton.gameObject);
    }

    private async UniTaskVoid HideWithAnimation()
    {
        _currentFadeHandle.TryCancel();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        // リストから自身を削除（順序に関係なく確実に削除）
        _activeWindows.Remove(this);

        _currentFadeHandle = _canvasGroup.FadeOut(FADE_ANIMATION_DURATION, ignoreTimeScale: true);
        await _currentFadeHandle.ToUniTask();

        // 閉じたことを通知
        _onClosed.OnNext(Unit.Default);

        // リストに他のウィンドウがあればそのcloseButtonを選択、なければルートを選択
        if (_activeWindows.Count > 0)
        {
            var topWindow = _activeWindows[^1]; // 最後の要素（最新のウィンドウ）
            if (topWindow)
            {
                SafeNavigationManager.SetSelectedGameObjectSafe(topWindow.closeButton.gameObject);
            }
        }
        else
        {
            SafeNavigationManager.SelectRootForceSelectable().Forget();
        }
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
        _onClosed?.Dispose();

        // リストから自身を削除（シーン遷移時のクリーンアップ）
        _activeWindows.Remove(this);
    }
}
