using System.Collections.Generic;
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
    
    private GameProgressService _gameProgressService;
    private DialogView _dialogView;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }
    
    private void Start()
    {
        // DialogViewを取得
        _dialogView = UnityEngine.Object.FindFirstObjectByType<DialogView>();
        
        // 現在のノベルノードから情報を取得
        var currentNode = _gameProgressService.GetCurrentNode();
        string scenarioId = "test_scenario_001"; // デフォルト値
        
        if (currentNode is NovelNode novelNode)
        {
            scenarioId = novelNode.ScenarioId;
            scenarioIdText.text = $"シナリオID: {novelNode.ScenarioId}";
        }
        else
        {
            scenarioIdText.text = $"シナリオID: {scenarioId}";
        }
        
        // DialogViewが見つかった場合はダイアログテストを開始
        if (_dialogView != null)
        {
            Debug.Log("[NovelUIPresenter] DialogViewを発見しました。テストダイアログを開始します。");
            StartDialogTest().Forget();
        }
        else
        {
            // DialogViewがない場合は従来の処理
            Debug.LogWarning("[NovelUIPresenter] DialogViewが見つかりません。3秒後に戻ります。");
            ReturnAsync().Forget();
        }
    }
    
    /// <summary>
    /// ダイアログテストを開始
    /// </summary>
    private async UniTaskVoid StartDialogTest()
    {
        // DialogViewの完了イベントを購読
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
        
        Debug.Log("[NovelUIPresenter] テストダイアログを開始します");
        
        // テストダイアログを開始
        await _dialogView.StartTestDialog();
        
        Debug.Log("[NovelUIPresenter] ダイアログが完了しました");
    }
    
    /// <summary>
    /// ダイアログ完了時の処理
    /// </summary>
    private async UniTaskVoid OnDialogCompleted()
    {
        Debug.Log("[NovelUIPresenter] 全てのダイアログが完了しました。シーンを戻ります。");
        
        // 少し待ってからシーンを戻る
        await UniTask.Delay(1000);
        
        // ダイアログ結果を記録（将来的にはDialogViewから取得）
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "true" },
            { "scenario_id", "test_scenario_001" }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        
        // ホームシーンに戻る
        await _gameProgressService.TransitionToScene(SceneType.Home);
    }
    
    /// <summary>
    /// フォールバック用の戻り処理（DialogViewがない場合）
    /// </summary>
    private async UniTask ReturnAsync()
    {
        await UniTask.Delay(3000);
        
        // ハードコード: 複数選択結果
        var choices = new Dictionary<string, string>
        {
            { "fork0", "option1" },
            { "fork1", "option2" }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        
        // ホームシーンに戻る
        await _gameProgressService.TransitionToScene(SceneType.Home);
    }
}