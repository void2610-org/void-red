using UnityEngine;
using UnityEngine.UI;
using R3;
using VContainer;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// ホーム画面のUI管理を担当するプレゼンター
/// タイトルへの戻りとバトル開始機能を提供
/// </summary>
public class HomeUIPresenter : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button titleButton;
    [SerializeField] private Button storyButton;
    
    private GameProgressService _gameProgressService;
    private StoryNode _currentNode;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }

    private void Start()
    {
        // ボタンイベントの設定
        titleButton.OnClickAsObservable().Subscribe(_ => OnTitleButtonClicked()).AddTo(this);
        storyButton.OnClickAsObservable().Subscribe(_ => OnStoryButtonClicked()).AddTo(this);
        
        // ホームBGMを再生
        BgmManager.Instance.PlayRandomBGM(BgmType.Home);
    }

    /// <summary>
    /// タイトルボタンがクリックされた時の処理
    /// </summary>
    private void OnTitleButtonClicked()
    {
        _gameProgressService.TransitionToScene(SceneType.Title).Forget();
    }

    /// <summary>
    /// ストーリーボタンがクリックされた時の処理
    /// </summary>
    private void OnStoryButtonClicked()
    {
        StartCurrentNodeAsync().Forget();
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
        await _gameProgressService.TransitionToScene(SceneType.Battle);
    }
    
    /// <summary>
    /// ノベルノード開始
    /// </summary>
    private async UniTask StartNovelNode(NovelNode novelNode)
    {
        Debug.Log($"[ホームUI] ノベル開始: {novelNode.ScenarioId}");
        
        // ノベルシーンに遷移（シナリオ情報はGameProgressServiceから取得）
        await _gameProgressService.TransitionToScene(SceneType.Novel);
    }
}