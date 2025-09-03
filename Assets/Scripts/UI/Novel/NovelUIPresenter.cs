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
    private SceneTransitionManager _sceneTransitionManager;
    private DialogView _dialogView;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService, SceneTransitionManager sceneTransitionManager)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
    }
    
    private void Start()
    {
        // DialogViewを取得
        _dialogView = UnityEngine.Object.FindFirstObjectByType<DialogView>();
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        if (scenarioId == "prologue1")
            StartPrologueTest().Forget();
        else if (scenarioId == "prologue2")
        {
            StartPrologueTest2().Forget();
        }
        else if (scenarioId == "ending")
            StartEndingTest().Forget();
        else
        {
            Debug.LogWarning($"[NovelUIPresenter] 未知のシナリオID: {scenarioId}。フォールバックで3秒後にシーンを戻ります。");
            _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget();
        }
    }
    
    /// <summary>
    /// デモビルド用のプロローグシナリオ開始
    /// </summary>
    private async UniTaskVoid StartPrologueTest()
    {
        // プロローグシナリオを取得して開始
        var prologueDialogs = PrologueProvider.GetPrologueScenario();
        await _dialogView.StartDialog(prologueDialogs);
    }
    
    /// <summary>
    /// デモビルド用のプロローグシナリオ開始2
    /// </summary>
    private async UniTaskVoid StartPrologueTest2()
    {
        var prologueDialogs = new List<DialogData> { new("システム", "これはプロローグシナリオ2です。") };
        await _dialogView.StartDialog(prologueDialogs);
    }
    
    /// <summary>
    /// デモビルド用のエンディングシナリオ開始
    /// </summary>
    private async UniTaskVoid StartEndingTest()
    {
        var endingDialogs = new List<DialogData>
        {
            new DialogData("", "アルファ版はここまでです。"),
            new DialogData("", "プレイしていただきありがとうございます。"),
            new DialogData("", "製品版リリースをお待ちください。")
        };
        await _dialogView.StartDialog(endingDialogs);
    }
    
    /// <summary>
    /// ダイアログ完了時の処理
    /// </summary>
    private async UniTaskVoid OnDialogCompleted()
    {
        Debug.Log("[NovelUIPresenter] 全てのダイアログが完了しました。シーンを戻ります。");
        
        // 少し待ってからシーンを戻る
        await UniTask.Delay(1000);
        
        // 現在のノードを結果記録前に取得
        var currentNode = _gameProgressService.GetCurrentNode();
        
        // ダイアログ結果を記録（将来的にはDialogViewから取得）
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "true" },
            { "scenario_id", "test_scenario_001" }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        
        // 記録前に取得したノードの設定を確認
        if (currentNode.ReturnToHome)
        {
            // ホームに戻る
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
        }
        else
        {
            // 次のノードへ直接遷移
            var nextScene = _gameProgressService.GetNextSceneType();
            await _sceneTransitionManager.TransitionToSceneWithFade(nextScene);
        }
    }
}