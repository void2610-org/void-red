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

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        if (scenarioId == "prologue")
            StartPrologueTest().Forget();
        else if (scenarioId == "ending")
            StartEndingTest().Forget();
        else
        {
            Debug.LogWarning($"[NovelUIPresenter] 未知のシナリオID: {scenarioId}。フォールバックで3秒後にシーンを戻ります。");
            ReturnAsync().Forget();
        }
    }
    
    /// <summary>
    /// デモビルド用のプロローグシナリオ開始
    /// </summary>
    private async UniTaskVoid StartPrologueTest()
    {
        // DialogViewの完了イベントを購読
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
        
        // プロローグシナリオを取得して開始
        var prologueDialogs = PrologueProvider.GetPrologueScenario();
        await _dialogView.StartDialog(prologueDialogs);
    }
    
    /// <summary>
    /// デモビルド用のエンディングシナリオ開始
    /// </summary>
    private async UniTaskVoid StartEndingTest()
    {
        // DialogViewの完了イベントを購読
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
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