using UnityEngine;
using R3;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// ホーム画面のPresenter
/// ビジネスロジックとイベント処理を担当
/// </summary>
public class HomePresenter : IStartable
{
    private readonly HomeView _homeView;
    private readonly GameProgressService _gameProgressService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllCardData _allCardData;
    private readonly ConfirmationDialogService _confirmationDialogService;
    private readonly SettingsPresenter _settingsPresenter;

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
        ConfirmationDialogService confirmationDialogService,
        SettingsPresenter settingsPresenter)
    {
        _homeView = homeView;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allCardData = allCardData;
        _confirmationDialogService = confirmationDialogService;
        _settingsPresenter = settingsPresenter;
    }

    public void Start()
    {
        // Viewを初期化
        _homeView.Initialize();

        // ボタンイベントの購読
        _homeView.SettingsButtonClicked
            .Subscribe(_ => _settingsPresenter.ShowSettings())
            .AddTo(_disposables);

        _homeView.TitleButtonClicked
            .Subscribe(_ => OnTitleButtonClicked())
            .AddTo(_disposables);

        _homeView.StoryButtonClicked
            .Subscribe(_ => StartCurrentNodeAsync().Forget())
            .AddTo(_disposables);

        _homeView.DeckButtonClicked
            .Subscribe(_ => ShowDeckData())
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
        BgmManager.Instance.PlayRandomBGM(BgmType.Home);

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
            case EndingNode:
                Debug.Log("[HomePresenter] ゲームが完了しています");
                break;
        }
    }

    /// <summary>
    /// バトルノード開始
    /// </summary>
    private async UniTask StartBattleNode(BattleNode battleNode)
    {
        Debug.Log($"[HomePresenter] バトル開始: 敵ID {battleNode.EnemyId}");

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
    /// デッキデータを表示
    /// </summary>
    private void ShowDeckData()
    {
        var cardModels = _gameProgressService.GetDeckCardModels();
        _homeView.ShowDeckData(cardModels);
    }

    /// <summary>
    /// カード図鑑を表示
    /// </summary>
    private void ShowCardLibrary()
    {
        var viewedCardIds = _gameProgressService.GetViewedCardIds();
        _homeView.ShowCardLibrary(_allCardData, viewedCardIds);
    }
}
