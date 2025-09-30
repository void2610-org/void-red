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
    private PauseView _pauseView;
    private PauseButtonView _pauseButtonView;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly CompositeDisposable _disposables = new();

    public PausePresenter(SceneTransitionManager sceneTransitionManager)
    {
        _sceneTransitionManager = sceneTransitionManager;
    }

    public void Start()
    {
        // ビューの取得
        _pauseView = Object.FindFirstObjectByType<PauseView>();
        _pauseButtonView = Object.FindFirstObjectByType<PauseButtonView>();
        
        // ポーズボタンのイベント設定
        _pauseButtonView.OnButtonClicked.Subscribe(
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