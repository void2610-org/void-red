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
        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        await _novelDialogService.InitializeAsync();
        
        if (scenarioIdText != null)
        {
            scenarioIdText.text = $"シナリオID: {scenarioId}";
        }
        
        StartScenario(scenarioId).Forget();
    }
    
    /// <summary>
    /// 指定されたシナリオIDのシナリオを開始
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        if (_dialogView == null)
        {
            ReturnAsync().Forget();
            return;
        }

        try
        {
            var scenarioDialogs = await _novelDialogService.GetDialogsByScenarioIdAsync(scenarioId);
            
            if (scenarioDialogs == null || scenarioDialogs.Count == 0)
            {
                ReturnAsync().Forget();
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
            ReturnAsync().Forget();
        }
    }
    
    private async UniTask OnDialogCompleted()
    {
        await UniTask.Delay(1000);
        
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "true" },
            { "scenario_id", _gameProgressService.GetCurrentNode().NodeId }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
    }
    
    private async UniTask ReturnAsync()
    {
        await UniTask.Delay(1000);
        
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "false" }
        };
        
        _gameProgressService.RecordNovelResultAndSave(choices);
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
    }
}