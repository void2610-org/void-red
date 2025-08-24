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
    [SerializeField] private Button battleButton;
    [SerializeField] private Button novelButton;
    
    [Header("テスト用敵データ")]
    [SerializeField] private EnemyData testEnemyData;
    
    private SceneTransitionService _sceneTransitionService;
    
    [Inject]
    public void Construct(SceneTransitionService sceneTransitionService)
    {
        _sceneTransitionService = sceneTransitionService;
    }

    private void Start()
    {
        // ボタンイベントの設定
        titleButton.OnClickAsObservable().Subscribe(_ => OnTitleButtonClicked()).AddTo(this);
        battleButton.OnClickAsObservable().Subscribe(_ => OnBattleButtonClicked()).AddTo(this);
        novelButton.OnClickAsObservable().Subscribe(_ => OnNovelButtonClicked()).AddTo(this);
        
        // ホームBGMを再生
        BgmManager.Instance.PlayRandomBGM(BgmType.Home);
    }

    /// <summary>
    /// タイトルボタンがクリックされた時の処理
    /// </summary>
    private void OnTitleButtonClicked()
    {
        _sceneTransitionService.TransitionToScene(SceneType.Title).Forget();
    }

    /// <summary>
    /// バトルボタンがクリックされた時の処理
    /// </summary>
    private void OnBattleButtonClicked()
    {
        StartBattleAsync().Forget();
    }

    /// <summary>
    /// ノベルボタンがクリックされた時の処理
    /// </summary>
    private void OnNovelButtonClicked()
    {
        StartNovelAsync().Forget();
    }

    /// <summary>
    /// バトル開始処理
    /// </summary>
    private async UniTask StartBattleAsync()
    {
        // 指定された敵データでバトル開始
        var battleData = new BattleTransitionData
        {
            TargetEnemy = testEnemyData,
            ReturnScene = SceneType.Home
        };
        // バトルシーンに遷移
        await _sceneTransitionService.TransitionToScene(battleData);
    }
    
    /// <summary>
    /// ノベル開始処理
    /// </summary>
    private async UniTask StartNovelAsync()
    {
        // テスト用ノベルデータ
        var novelData = new NovelTransitionData
        {
            ScenarioId = "test_scenario_001",
            ReturnScene = SceneType.Home
        };
        
        Debug.Log($"[HomeUI] ノベル開始: {novelData.ScenarioId}");
        
        // ノベルシーンに遷移
        await _sceneTransitionService.TransitionToScene(novelData);
    }
}