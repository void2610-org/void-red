using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using R3;
using VContainer;
using Cysharp.Threading.Tasks;
using TMPro;
using Void2610.UnityTemplate;

/// <summary>
/// ホーム画面のUI管理を担当するプレゼンター
/// タイトルへの戻りとバトル開始機能を提供
/// </summary>
public class HomeUIPresenter : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private Button libraryButton;
    [SerializeField] private Button storyButton;
    [SerializeField] private Button personButton;
    [SerializeField] private Button dreamButton;
    [SerializeField] private DeckView deckView;
    [SerializeField] private CardLibraryView cardLibraryView;
    [SerializeField] private TextMeshProUGUI speakingText;
    
    private GameProgressService _gameProgressService;
    private SceneTransitionManager _sceneTransitionManager;
    private StoryNode _currentNode;
    private AllCardData _allCardData;
    private ConfirmationDialogService _confirmationDialogService;
    private SettingsPresenter _settingsPresenter;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService, SceneTransitionManager sceneTransitionManager, AllCardData allCardData, ConfirmationDialogService confirmationDialogService, SettingsPresenter settingsPresenter)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allCardData = allCardData;
        _confirmationDialogService = confirmationDialogService;
        _settingsPresenter = settingsPresenter;
    }

    private void Start()
    {
        // 未実装のボタンを無効化
        personButton.interactable = false;
        dreamButton.interactable = false;
        
        // ボタンイベントの設定
        settingsButton.OnClickAsObservable().Subscribe(_ => _settingsPresenter.ShowSettings()).AddTo(this);
        titleButton.OnClickAsObservable().Subscribe(_ => OnTitleButtonClicked()).AddTo(this);
        storyButton.OnClickAsObservable().Subscribe(_ => StartCurrentNodeAsync().Forget()).AddTo(this);
        libraryButton.OnClickAsObservable().Subscribe(_ => ShowCardLibrary()).AddTo(this);
        
        // ホームBGMを再生
        BgmManager.Instance.PlayRandomBGM(BgmType.Home);
        
        InitSpeaking().Forget();
    }

    private async UniTask InitSpeaking()
    {
        await UniTask.Delay(1000);
        speakingText.TypewriterAnimation("...").Forget();
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
                Debug.Log("[ホームUI] ゲームが完了しています");
                break;
        }
    }
    
    /// <summary>
    /// バトルノード開始
    /// </summary>
    private async UniTask StartBattleNode(BattleNode battleNode)
    {
        Debug.Log($"[ホームUI] バトル開始: 敵ID {battleNode.EnemyId}");
        
        // 単純にBattleSceneに遷移（敵情報はGameProgressServiceから取得）
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Battle);
    }
    
    /// <summary>
    /// ノベルノード開始
    /// </summary>
    private async UniTask StartNovelNode(NovelNode novelNode)
    {
        Debug.Log($"[ホームUI] ノベル開始: {novelNode.ScenarioId}");
        
        // ノベルシーンに遷移（シナリオ情報はGameProgressServiceから取得）
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Novel);
    }
    
    
    /// <summary>
    /// デッキデータを更新
    /// </summary>
    private void RefreshDeckData()
    {
        var cardModels = _gameProgressService.GetDeckCardModels();
        var cardDataList = cardModels.Select(cm => cm.Data).ToList();
        
        deckView.ShowDeck(cardModels);
    }
    
    /// <summary>
    /// カード図鑑を表示
    /// </summary>
    private void ShowCardLibrary()
    {
        var viewedCardIds = _gameProgressService.GetViewedCardIds();
        cardLibraryView.Show(_allCardData, viewedCardIds);
    }
}