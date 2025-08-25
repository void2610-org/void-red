using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター（モック実装）
/// 遷移データを表示して1秒後に自動的に戻る
/// </summary>
public class NovelUIPresenter : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private TextMeshProUGUI scenarioIdText;
    
    private GameProgressService _gameProgressService;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }
    
    private void Start()
    {
        // 現在のノベルノードから情報を取得
        var currentNode = _gameProgressService.GetCurrentNode();
        if (currentNode is NovelNode novelNode)
        {
            scenarioIdText.text = $"シナリオID: {novelNode.ScenarioId}";
        }
        else
        {
            scenarioIdText.text = "シナリオIDが取得できませんでした";
        }
        
        ReturnAsync().Forget();
    }
    
    private async UniTask ReturnAsync()
    {
        await UniTask.Delay(3000);
        
        // ストーリーを進行させてセーブ
        _gameProgressService.AdvanceStory();
        _gameProgressService.SaveAndPersist();
        
        // ホームシーンに戻る
        await _gameProgressService.TransitionToScene(SceneType.Home);
    }
}