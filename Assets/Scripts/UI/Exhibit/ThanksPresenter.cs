using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using Void2610.UnityTemplate;

/// <summary>
/// 展示モード感謝画面のPresenter
/// 一定時間後またはタッチでタイトルに戻る
/// </summary>
public class ThanksPresenter : IStartable, IDisposable
{
    private const float AUTO_RETURN_SECONDS = 10f;

    private readonly ExhibitSettings _settings;
    private readonly ISceneTransitionService _sceneTransitionService;
    private readonly IdleDetector _idleDetector;
    private readonly ExhibitSessionTimerService _sessionTimerService;

    private bool _isTransitioning;

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

    private async UniTaskVoid WaitAndReturnToTitle()
    {
        var startTime = Time.realtimeSinceStartup;

        // 一定時間経過または入力があるまで待機
        await UniTask.WaitUntil(() =>
        {
            var elapsed = Time.realtimeSinceStartup - startTime;
            return elapsed >= AUTO_RETURN_SECONDS || _idleDetector.IdleSeconds < 1f;
        });

        if (_isTransitioning) return;
        _isTransitioning = true;

        // セッションタイマーをリセットしてタイトルへ
        _sessionTimerService.ResetSession();
        _idleDetector.ResetIdleTimer();
        await _sceneTransitionService.TransitionToSceneWithFade(_settings.IdleReturnSceneName);
    }

    public void Start() => WaitAndReturnToTitle().Forget();

    public void Dispose() { }
}
