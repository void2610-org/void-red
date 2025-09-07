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
    private NovelDialogService _novelDialogService;
    private AddressableCharacterImageLoader _characterImageLoader;
    private DialogView _dialogView;
    
    [Inject]
    public void Construct(
        GameProgressService gameProgressService, 
        SceneTransitionManager sceneTransitionManager, 
        NovelDialogService novelDialogService,
        AddressableCharacterImageLoader characterImageLoader)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _novelDialogService = novelDialogService;
        _characterImageLoader = characterImageLoader;
    }
    
    private void Start()
    {
        // DialogViewを取得
        _dialogView = UnityEngine.Object.FindFirstObjectByType<DialogView>();
        if (!_dialogView)
        {
            return;
        }
        
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        // シナリオIDに応じて処理を分岐
        StartScenario(scenarioId).Forget();
    }
    
    /// <summary>
    /// シナリオIDに応じてシナリオを開始
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        try
        {
            List<DialogData> dialogList = null;
            
            // ハードコードシナリオの処理
            switch (scenarioId)
            {
                case "prologue1":
                    dialogList = PrologueProvider.GetPrologueScenario();
                    break;
                case "prologue2":
                    dialogList = PrologueProvider.GetPrologue2Scenario();
                    break;
                case "ending":
                    dialogList = new List<DialogData>
                    {
                        new DialogData("", "アルファ版はここまでです。"),
                        new DialogData("", "プレイしていただきありがとうございます。"),
                        new DialogData("", "製品版リリースをお待ちください。")
                    };
                    break;
                default:
                    // スプレッドシートからシナリオを読み込み
                    dialogList = await _novelDialogService.GetDialogDataAsync(scenarioId);
                    break;
            }
            
            // ダイアログリストが有効かチェック
            if (dialogList == null || dialogList.Count == 0)
            {
                await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
                return;
            }
            
            // キャラクター画像を事前に読み込み
            await PreloadCharacterImages(dialogList);
            
            // DialogViewにキャラクター画像読み込みコールバックを設定
            _dialogView.SetCharacterImageLoader(imageName => _characterImageLoader.LoadCharacterImageAsync(imageName));
            
            // ダイアログを開始
            await _dialogView.StartDialog(dialogList);
        }
        catch (System.Exception ex)
        {
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
        }
    }
    
    /// <summary>
    /// ダイアログリストに含まれるキャラクター画像を事前に読み込み
    /// </summary>
    private async UniTask PreloadCharacterImages(List<DialogData> dialogList)
    {
        var imageNames = new HashSet<string>();
        
        // ダイアログリストから使用される画像名を抽出
        for (int i = 0; i < dialogList.Count; i++)
        {
            var dialog = dialogList[i];
            if (!string.IsNullOrEmpty(dialog.CharacterImageName))
            {
                imageNames.Add(dialog.CharacterImageName);
            }
        }
        
        // 画像を並列で読み込み
        var loadTasks = new List<UniTask>();
        foreach (var imageName in imageNames)
        {
            loadTasks.Add(_characterImageLoader.LoadCharacterImageAsync(imageName).AsUniTask());
        }
        
        if (loadTasks.Count > 0)
        {
            await UniTask.WhenAll(loadTasks);
        }
    }
    
    /// <summary>
    /// ダイアログ完了時の処理
    /// </summary>
    private async UniTaskVoid OnDialogCompleted()
    {
        // 少し待ってからシーンを戻る
        await UniTask.Delay(1000);
        
        // 現在のノードを結果記録前に取得
        var currentNode = _gameProgressService.GetCurrentNode();
        
        // ダイアログ結果を記録
        var choices = new Dictionary<string, string>
        {
            { "dialog_completed", "true" },
            { "scenario_id", currentNode.NodeId }
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
    
    private void OnDestroy()
    {
        // キャラクター画像のメモリを解放
        _characterImageLoader?.UnloadAllCharacterImages();
    }
}