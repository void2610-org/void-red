using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;
using Void2610.SettingsSystem;
using Void2610.UnityTemplate;

/// <summary>
/// ホーム画面のPresenter
/// ビジネスロジックとイベント処理を担当
/// </summary>
public class HomePresenter : IStartable, IDisposable
{
    private readonly HomeView _homeView;
    private readonly GameProgressService _gameProgressService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllCardData _allCardData;
    private readonly IConfirmationDialog _confirmationDialogService;

    private StoryNode _currentNode;
    private readonly CompositeDisposable _disposables = new();

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public HomePresenter(
        HomeView homeView,
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllCardData allCardData,
        IConfirmationDialog confirmationDialogService)
    {
        _homeView = homeView;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allCardData = allCardData;
        _confirmationDialogService = confirmationDialogService;
    }

    public void Start()
    {
        // Viewを初期化
        _homeView.Initialize();

        _homeView.TitleButtonClicked
            .Subscribe(_ => OnTitleButtonClicked())
            .AddTo(_disposables);

        _homeView.StoryButtonClicked
            .Subscribe(_ => StartCurrentNodeAsync().Forget())
            .AddTo(_disposables);

        _homeView.LibraryButtonClicked
            .Subscribe(_ => ShowCardLibrary())
            .AddTo(_disposables);

        // カードクリックイベントの購読
        _homeView.DeckCardClicked
            .Subscribe(cardData => _homeView.ShowCardDetail(cardData))
            .AddTo(_disposables);

        _homeView.LibraryCardClicked
            .Subscribe(cardData => _homeView.ShowCardDetail(cardData))
            .AddTo(_disposables);

        // ホームBGMを再生
        BgmManager.Instance.PlayBGM("Home");

        SafeNavigationManager.SelectRootForceSelectable().Forget();
    }

    /// <summary>
    /// タイトルボタンがクリックされた時の処理
    /// </summary>
    private void OnTitleButtonClicked()
    {
        _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Title).Forget();
    }

    /// <summary>
    /// 現在のノードを開始
    /// </summary>
    private async UniTask StartCurrentNodeAsync()
    {
        _currentNode = _gameProgressService.GetNextNode();

        switch (_currentNode)
        {
            case BattleNode battleNode:
                await StartBattleNode(battleNode);
                break;
            case NovelNode novelNode:
                await StartNovelNode(novelNode);
                break;
        }
    }

    /// <summary>
    /// バトルノード開始
    /// </summary>
    private async UniTask StartBattleNode(BattleNode battleNode)
    {
        Debug.Log($"[HomePresenter] バトル開始: オークションID {battleNode.AuctionId}");

        // 単純にBattleSceneに遷移（敵情報はGameProgressServiceから取得）
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Battle);
    }

    /// <summary>
    /// ノベルノード開始
    /// </summary>
    private async UniTask StartNovelNode(NovelNode novelNode)
    {
        Debug.Log($"[HomePresenter] ノベル開始: {novelNode.ScenarioId}");

        // ノベルシーンに遷移（シナリオ情報はGameProgressServiceから取得）
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Novel);
    }

    /// <summary>
    /// カード図鑑を表示
    /// </summary>
    private void ShowCardLibrary()
    {
        var viewedCardIds = _gameProgressService.GetViewedCardIds();
        _homeView.ShowCardLibrary(_allCardData, viewedCardIds);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
