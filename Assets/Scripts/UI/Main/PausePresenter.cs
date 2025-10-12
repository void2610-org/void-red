using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
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
    private readonly InputActionsProvider _inputActionsProvider;
    private readonly CompositeDisposable _disposables = new();

    public PausePresenter(SceneTransitionManager sceneTransitionManager, InputActionsProvider inputActionsProvider)
    {
        _sceneTransitionManager = sceneTransitionManager;
        _inputActionsProvider = inputActionsProvider;
    }

    public void Start()
    {
        // ビューの取得
        _pauseView = Object.FindFirstObjectByType<PauseView>();
        _pauseButtonView = Object.FindFirstObjectByType<PauseButtonView>();

        // Pauseアクションの購読
        _inputActionsProvider.UI.Pause.OnPerformedAsObservable()
            .Subscribe(_ => TogglePause())
            .AddTo(_disposables);

        // ポーズボタンのイベント設定
        _pauseButtonView.OnButtonClicked.Subscribe(
            _ => _pauseView.Show())
            .AddTo(_disposables);

        // 再開ボタンのイベント設定
        _pauseView.OnResumeButtonClicked.Subscribe(
            _ => _pauseView.Hide())
            .AddTo(_disposables);

        // タイトルボタンのイベント設定
        _pauseView.OnTitleButtonClicked.Subscribe(
            _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget()
        ).AddTo(_disposables);
    }

    private void TogglePause()
    {
        if (_pauseView.IsShowing)
        {
            _pauseView.Hide();
        }
        else
        {
            _pauseView.Show();
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}