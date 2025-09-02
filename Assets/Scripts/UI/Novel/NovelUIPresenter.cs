using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;
using VoidRed.Game.Services;

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
    private NovelDialogService _novelDialogService;
    private DialogView _dialogView;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService, SceneTransitionManager sceneTransitionManager, NovelDialogService novelDialogService)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _novelDialogService = novelDialogService;
    }
    
    private async void Start()
    {
        // DialogViewを取得
        _dialogView = UnityEngine.Object.FindFirstObjectByType<DialogView>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // デバッグ用：プロローグから開始するためにリセット
        Debug.Log("[NovelUIPresenter] デバッグモード: ゲーム進行をリセットしてプロローグから開始");
        _gameProgressService.ResetToDefaultData();
#endif

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        // NovelDialogServiceを初期化
        await _novelDialogService.InitializeAsync();
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // デバッグ用：スプレッドシート接続テスト
        await _novelDialogService.TestSpreadsheetConnection();
        
        // デバッグ用：利用可能なシナリオIDを表示（開発時のみ）
        _novelDialogService.LogAvailableScenarios();
#endif
        
        // シナリオIDを画面に表示
        if (scenarioIdText != null)
        {
            scenarioIdText.text = $"シナリオID: {scenarioId}";
        }
        
        // シナリオを開始
        StartScenario(scenarioId).Forget();
    }
    
    /// <summary>
    /// 指定されたシナリオIDのシナリオを開始
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        if (_dialogView == null)
        {
            Debug.LogError("[NovelUIPresenter] DialogViewが見つかりません。");
            ReturnAsync().Forget();
            return;
        }

        try
        {
            // DialogViewの完了イベントを購読
            _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
            
            Debug.Log($"[NovelUIPresenter] シナリオ '{scenarioId}' をスプレッドシートから取得中...");
            
            // スプレッドシートからシナリオデータを取得（遅延ロード対応）
            var scenarioDialogs = await _novelDialogService.GetDialogsByScenarioIdAsync(scenarioId);
            
            if (scenarioDialogs == null || scenarioDialogs.Count == 0)
            {
                Debug.LogWarning($"[NovelUIPresenter] シナリオ '{scenarioId}' のデータが空です。");
                ReturnAsync().Forget();
                return;
            }
            
            Debug.Log($"[NovelUIPresenter] シナリオ '{scenarioId}' を開始 ({scenarioDialogs.Count}行)");
            
            // ダイアログを開始
            await _dialogView.StartDialog(scenarioDialogs);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NovelUIPresenter] シナリオ開始エラー: {ex.Message}");
            ReturnAsync().Forget();
        }
    }
    
    /// <summary>
    /// ダイアログ完了時の処理
    /// </summary>
    private async UniTaskVoid OnDialogCompleted()
    {
        Debug.Log("[NovelUIPresenter] 全てのダイアログが完了しました。シーンを戻ります。");
        
        // 少し待ってからシーンを戻る
        await UniTask.Delay(1000);
        
        // ダイアログ結果を記録
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "true" },
            { "scenario_id", _gameProgressService.GetCurrentNode().NodeId }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        
        // ホームシーンに戻る
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
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
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
    }
}