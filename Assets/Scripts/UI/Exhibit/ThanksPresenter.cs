using System;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Void2610.UnityTemplate;

/// <summary>
/// 展示モード感謝画面のPresenter
/// クリック/タッチでタイトルに戻る
/// </summary>
public class ThanksPresenter : IStartable, IDisposable
{
    private readonly ExhibitSettings _settings;
    private readonly ISceneTransitionService _sceneTransitionService;
    private readonly IdleDetector _idleDetector;
    private readonly ExhibitSessionTimerService _sessionTimerService;

    public ThanksPresenter(
        ExhibitSettings settings,
        ISceneTransitionService sceneTransitionService,
        IdleDetector idleDetector,
        ExhibitSessionTimerService sessionTimerService)
    {
        _settings = settings;
        _sceneTransitionService = sceneTransitionService;
        _idleDetector = idleDetector;
        _sessionTimerService = sessionTimerService;
    }

    private async UniTaskVoid WaitForClickAndReturn()
    {
        // シーン遷移直後の入力を無視するため待機
        await UniTask.Delay(TimeSpan.FromSeconds(1));

        // クリック/タッチ入力を待機
        await UniTask.WaitUntil(() =>
            Pointer.current != null && Pointer.current.press.wasPressedThisFrame);

        // セッションをリセットしてタイトルへ
        _sessionTimerService.ResetSession();
        _idleDetector.ResetIdleTimer();
        await _sceneTransitionService.TransitionToSceneWithFade(_settings.IdleReturnSceneName);
    }

    public void Start()
    {
        WaitForClickAndReturn().Forget();
    }

    public void Dispose() { }
}
