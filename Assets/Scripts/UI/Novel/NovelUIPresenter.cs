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
    
    private SceneTransitionService _sceneTransitionService;
    
    [Inject]
    public void Construct(SceneTransitionService sceneTransitionService)
    {
        _sceneTransitionService = sceneTransitionService;
    }
    
    private void Start()
    {
        // 遷移データを取得
        var novelData = _sceneTransitionService.GetTransitionData<NovelTransitionData>();
        
        // 遷移データの情報を表示
        scenarioIdText.text = $"シナリオID: {novelData.ScenarioId}";
        
        ReturnAsync(novelData.ReturnScene).Forget();
    }
    
    private async UniTask ReturnAsync(SceneType returnScene)
    {
        await UniTask.Delay(3000);
        
        // 遷移データをクリアしてシーンに戻る
        _sceneTransitionService.ClearTransitionData();
        await _sceneTransitionService.TransitionToScene(returnScene);
    }
}