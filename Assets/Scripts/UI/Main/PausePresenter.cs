using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ポーズ機能を管理するPresenterクラス
/// VContainerで依存性注入される
/// </summary>
public class PausePresenter : IStartable, System.IDisposable
{
    private readonly PauseView _pauseView;
    private readonly PauseButtonView _pauseButtonView;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly CompositeDisposable _disposables = new();

    public PausePresenter(SceneTransitionManager sceneTransitionManager)
    {
        _sceneTransitionManager = sceneTransitionManager;
        
        // ビューの取得
        _pauseView = Object.FindFirstObjectByType<PauseView>();
        _pauseButtonView = Object.FindFirstObjectByType<PauseButtonView>();
    }

    public void Start()
    {
        // ポーズボタンのイベント設定
        _pauseButtonView?.OnButtonClicked.Subscribe(
            _ => _pauseView.Show())
            .AddTo(_disposables);
        
        // タイトルボタンのイベント設定
        _pauseView.OnTitleButtonClicked.Subscribe(
            _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget()
        ).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}