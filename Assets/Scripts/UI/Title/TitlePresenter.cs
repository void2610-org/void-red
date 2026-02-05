using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;
using Void2610.SettingsSystem;
using Void2610.UnityTemplate;
using Void2610.UnityTemplate.Steam;

/// <summary>
/// タイトル画面のPresenter
/// ビジネスロジックとイベント処理を担当
/// </summary>
public class TitlePresenter : IStartable, IDisposable
{
    private readonly TitleView _titleView;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly GameProgressService _gameProgressService;
    private readonly IConfirmationDialog _confirmationDialogService;
    private readonly SteamService _steamService;

    private readonly CompositeDisposable _disposables = new();

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public TitlePresenter(
        TitleView titleView,
        SceneTransitionManager sceneTransitionManager,
        GameProgressService gameProgressService,
        IConfirmationDialog confirmationDialogService,
        SteamService steamService)
    {
        _titleView = titleView;
        _sceneTransitionManager = sceneTransitionManager;
        _gameProgressService = gameProgressService;
        _confirmationDialogService = confirmationDialogService;
        _steamService = steamService;
    }

    public void Start()
    {
        // Viewを初期化
        _titleView.Initialize();

        // ボタンイベントの購読
        _titleView.StartButtonClicked
            .Subscribe(_ => OnStartButtonClicked().Forget())
            .AddTo(_disposables);

        _titleView.ContinueButtonClicked
            .Subscribe(_ => OnContinueButtonClicked())
            .AddTo(_disposables);

        _titleView.QuitButtonClicked
            .Subscribe(_ => OnQuitButtonClicked())
            .AddTo(_disposables);

        _titleView.ReviewFormButtonClicked
            .Subscribe(_ => OnReviewFormButtonClicked())
            .AddTo(_disposables);

        // セーブデータの有無によるボタン状態管理
        var hasSaveData = _gameProgressService.HasSaveData();
        _titleView.SetContinueButtonState(hasSaveData);

        _gameProgressService.OnDataSaved
            .Select(_ => _gameProgressService.HasSaveData())
            .Subscribe(enabled => _titleView.SetContinueButtonState(enabled))
            .AddTo(_disposables);

        // タイトルBGMを再生
        BgmManager.Instance.PlayBGM("Title");

        // Steam実績解除
        _steamService.UnlockAchievement(nameof(SteamAchieveType.FIRST_BOOT));

        SafeNavigationManager.SelectRootForceSelectable().Forget();
    }

    /// <summary>
    /// スタートボタンがクリックされた時の処理（セーブデータリセット）
    /// </summary>
    private async UniTask OnStartButtonClicked()
    {
        // セーブデータが存在する場合は確認ダイアログを表示
        if (_gameProgressService.HasSaveData())
        {
            var confirmed = await _confirmationDialogService.ShowDialog(
                "既存のセーブデータが削除されます。よろしいですか？",
                "はい",
                "いいえ"
            );

            if (!confirmed) return;
        }

        _gameProgressService.ResetToDefaultData();
        _steamService.AddStat(nameof(SteamStatType.START_GAME_COUNT), 1);

        // 新規開始時は次のノードに直接遷移
        var nextScene = _gameProgressService.GetNextSceneType();
        _sceneTransitionManager.TransitionToSceneWithFade(nextScene).Forget();
    }

    /// <summary>
    /// つづきからボタンがクリックされた時の処理
    /// </summary>
    private void OnContinueButtonClicked()
    {
        // 続きから開始時は一旦ホームに遷移
        _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget();
    }

    /// <summary>
    /// 終了ボタンがクリックされた時の処理
    /// </summary>
    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        // エディタ上では再生停止
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    /// <summary>
    /// レビューフォームボタンがクリックされた時の処理
    /// </summary>
    private void OnReviewFormButtonClicked()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfNMqCyXFzWijWAv__wTpDVRN6AtEfFXpdPxyFcIkMbiq2UKw/viewform");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
