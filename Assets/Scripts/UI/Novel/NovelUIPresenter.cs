using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;
using VoidRed.Game.Services;
using Void2610.UnityTemplate;

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
    private AddressableCharacterImageLoader _imageLoader;
    private DialogView _dialogView;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService, SceneTransitionManager sceneTransitionManager, NovelDialogService novelDialogService, AddressableCharacterImageLoader imageLoader)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _novelDialogService = novelDialogService;
        _imageLoader = imageLoader;
    }
    
    private async void Start()
    {
        _dialogView = UnityEngine.Object.FindFirstObjectByType<DialogView>();
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();

        if (_dialogView == null)
        {
            await OnDialogCompleted();
            return;
        }

        // 必要なサービスのnullチェック
        if (_gameProgressService == null)
        {
            await OnDialogCompleted();
            return;
        }

        var currentNode = _gameProgressService.GetNextNode();
        if (currentNode == null)
        {
            await OnDialogCompleted();
            return;
        }

        var scenarioId = currentNode.NodeId;
        
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
        await ExecuteDialogs(prologueDialogs);
    }
    
    /// <summary>
    /// デモビルド用のプロローグシナリオ開始2
    /// </summary>
    private async UniTaskVoid StartPrologueTest2()
    {
        var prologueDialogs = new List<DialogData> { new("システム", "これはプロローグシナリオ2です。") };
        await ExecuteDialogs(prologueDialogs);
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
            new("システム", "これはエンディングです。"),
            new("ナレーター", "お疲れ様でした！")
        };
        await ExecuteDialogs(endingDialogs);
    }
    
    /// <summary>
    /// ダイアログリストを実行する共通処理
    /// </summary>
    private async UniTask ExecuteDialogs(List<DialogData> dialogList)
    {
        if (_dialogView == null || dialogList == null || dialogList.Count == 0)
        {
            await OnDialogCompleted();
            return;
        }

        try
        {
            await _dialogView.FadeIn();
            
            for (int i = 0; i < dialogList.Count; i++)
            {
                var dialog = dialogList[i];
                
                _dialogView.SetSpeakerName(dialog.SpeakerName);
                
                if (!string.IsNullOrEmpty(dialog.CharacterImageName))
                {
                    var sprite = await _imageLoader.LoadCharacterImageAsync(dialog.CharacterImageName);
                    _dialogView.SetCharacterImage(sprite);
                }
                else
                {
                    _dialogView.SetCharacterImage(null);
                }
                
                if (dialog.HasSE && dialog.PlaySEOnStart)
                {
                    SeManager.Instance.PlaySe(dialog.SEClipName);
                }
                
                await _dialogView.DisplayText(dialog.DialogText);
                
                if (i < dialogList.Count - 1)
                {
                    await _dialogView.WaitForNextInput();
                }
            }
            
            await OnDialogCompleted();
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は正常終了として扱う
            await OnDialogCompleted();
        }
        catch (Exception ex)
        {
            await OnDialogCompleted();
        }
    }
    
    /// <summary>
    /// 指定されたシナリオIDのシナリオを開始
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        if (_dialogView == null)
        {
            await OnDialogCompleted();
            return;
        }

        try
        {
            var scenarioDialogs = await _novelDialogService.GetDialogsByScenarioIdAsync(scenarioId);
            
            if (scenarioDialogs == null || scenarioDialogs.Count == 0)
            {
                await OnDialogCompleted();
                return;
            }
            
            await _dialogView.FadeIn();
            
            for (int i = 0; i < scenarioDialogs.Count; i++)
            {
                var dialog = scenarioDialogs[i];
                
                _dialogView.SetSpeakerName(dialog.SpeakerName);
                
                if (!string.IsNullOrEmpty(dialog.CharacterImageName))
                {
                    var sprite = await _imageLoader.LoadCharacterImageAsync(dialog.CharacterImageName);
                    _dialogView.SetCharacterImage(sprite);
                }
                else
                {
                    _dialogView.SetCharacterImage(null);
                }
                
                if (dialog.HasSE && dialog.PlaySEOnStart)
                {
                    SeManager.Instance.PlaySe(dialog.SEClipName);
                }
                
                await _dialogView.DisplayText(dialog.DialogText);
                
                if (i < scenarioDialogs.Count - 1)
                {
                    await _dialogView.WaitForNextInput();
                }
            }
            
            await OnDialogCompleted();
        }
        catch (Exception ex)
        {
            await OnDialogCompleted();
        }
    }
    
    private async UniTask OnDialogCompleted()
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
            { "scenario_id", _gameProgressService.GetCurrentNode().NodeId }
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