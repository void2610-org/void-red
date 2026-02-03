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
    private IPauseView _pauseView;
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
        // ビューの取得（BattlePauseViewを優先、なければPauseViewを使用）
        _pauseView = (IPauseView)Object.FindFirstObjectByType<BattlePauseView>(FindObjectsInactive.Include)
                     ?? Object.FindFirstObjectByType<PauseView>(FindObjectsInactive.Include);
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

        // ホームボタンのイベント設定
        _pauseView.OnHomeButtonClicked.Subscribe(
            _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget()
        ).AddTo(_disposables);

        // BattlePauseView固有のボタン処理
        if (_pauseView is BattlePauseView battlePauseView)
        {
            battlePauseView.OnHelpButtonClicked
                .Subscribe(_ => ShowHelp())
                .AddTo(_disposables);

            battlePauseView.OnOptionButtonClicked
                .Subscribe(_ => ShowOptions())
                .AddTo(_disposables);
        }
    }

    public void Dispose() => _disposables.Dispose();

    private void ShowHelp()
    {
        var helpView = Object.FindFirstObjectByType<HelpView>();
        helpView.Show();
    }

    private void ShowOptions()
    {
        var settingsView = Object.FindFirstObjectByType<SettingsWindowView>();
        settingsView.Show();
    }

    private void TogglePause()
    {
        if (_pauseView.IsShowing)
            _pauseView.Hide();
        else
            _pauseView.Show();
    }
}