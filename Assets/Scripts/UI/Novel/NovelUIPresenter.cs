using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター
/// DialogViewを使用してダイアログ表示を管理し、完了後にシーンを戻る
/// </summary>
public class NovelUIPresenter : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private TextMeshProUGUI scenarioIdText;
    [SerializeField] private DialogView dialogView;
    
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
        if (scenarioIdText != null)
        {
            scenarioIdText.text = $"シナリオID: {novelData.ScenarioId}";
        }
        
        // DialogViewが設定されている場合はダイアログテストを開始
        if (dialogView != null)
        {
            StartDialogTest(novelData.ReturnScene).Forget();
        }
        else
        {
            // DialogViewがない場合は従来の処理
            Debug.LogWarning("[NovelUIPresenter] DialogViewが設定されていません。3秒後に戻ります。");
            ReturnAsync(novelData.ReturnScene).Forget();
        }
    }
    
    /// <summary>
    /// ダイアログテストを開始
    /// </summary>
    private async UniTaskVoid StartDialogTest(SceneType returnScene)
    {
        // DialogViewの完了イベントを購読
        dialogView.OnDialogCompleted += () => OnDialogCompleted(returnScene).Forget();
        
        Debug.Log("[NovelUIPresenter] テストダイアログを開始します");
        
        // テストダイアログを開始
        await dialogView.StartTestDialog();
        
        Debug.Log("[NovelUIPresenter] ダイアログが完了しました");
    }
    
    /// <summary>
    /// ダイアログ完了時の処理
    /// </summary>
    private async UniTaskVoid OnDialogCompleted(SceneType returnScene)
    {
        Debug.Log("[NovelUIPresenter] 全てのダイアログが完了しました。シーンを戻ります。");
        
        // 少し待ってからシーンを戻る
        await UniTask.Delay(1000);
        
        // 遷移データをクリアしてシーンに戻る
        _sceneTransitionService.ClearTransitionData();
        await _sceneTransitionService.TransitionToScene(returnScene);
    }
    
    /// <summary>
    /// フォールバック用の戻り処理（DialogViewがない場合）
    /// </summary>
    private async UniTask ReturnAsync(SceneType returnScene)
    {
        await UniTask.Delay(3000);
        
        // 遷移データをクリアしてシーンに戻る
        _sceneTransitionService.ClearTransitionData();
        await _sceneTransitionService.TransitionToScene(returnScene);
    }
}