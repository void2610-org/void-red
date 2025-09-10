using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター
/// ダイアログの進行制御とViewの管理を行う
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
    
    // ダイアログ制御用
    private List<DialogData> _currentDialogList;
    private int _currentDialogIndex;
    
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
        _dialogView = FindFirstObjectByType<DialogView>();
        
        // Viewイベントを購読
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
        _dialogView.OnUserClickDetected += () => HandleUserClick().Forget();

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;
        
        // シナリオIDに応じて処理を分岐
        StartScenario(scenarioId).Forget();
    }
    
    /// <summary>
    /// シナリオIDに応じてシナリオを開始
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        List<DialogData> dialogList;
        
        // ハードコードシナリオの処理
        switch (scenarioId)
        {
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

                if (dialogList == null)
                {
                    Debug.LogError($"シナリオ '{scenarioId}' の読み込みに失敗しました");
                    await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
                    return;
                }
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
        
        // ダイアログシーケンスを開始
        await StartDialogSequence(dialogList);
    }
    
    /// <summary>
    /// ダイアログシーケンスを開始（Presenterが制御）
    /// </summary>
    private async UniTask StartDialogSequence(List<DialogData> dialogList)
    {
        _currentDialogList = dialogList;
        _currentDialogIndex = 0;
        
        // フェードイン
        await _dialogView.FadeIn();
        
        // 最初のダイアログを表示
        await ShowNextDialog();
    }
    
    /// <summary>
    /// 次のダイアログを表示
    /// </summary>
    private async UniTask ShowNextDialog()
    {
        if (_currentDialogIndex >= _currentDialogList.Count)
        {
            // すべてのダイアログを表示完了
            await _dialogView.ShowDialogComplete();
            return;
        }
        
        var currentDialog = _currentDialogList[_currentDialogIndex];
        _currentDialogIndex++;
        
        // キャラクター画像を読み込み（事前読み込み済みなのでキャッシュから取得）
        Sprite characterSprite = null;
        if (!string.IsNullOrEmpty(currentDialog.CharacterImageName))
        {
            characterSprite = await _characterImageLoader.LoadCharacterImageAsync(currentDialog.CharacterImageName);
        }
        
        // 読み込み完了後にViewに渡してダイアログを表示
        await _dialogView.ShowSingleDialog(currentDialog, characterSprite);
    }
    
    /// <summary>
    /// ユーザークリック処理
    /// </summary>
    private async UniTaskVoid HandleUserClick()
    {
        // 次のダイアログへ進む
        await ShowNextDialog();
    }
    
    /// <summary>
    /// ダイアログリストに含まれるキャラクター画像を事前に読み込み
    /// </summary>
    private async UniTask PreloadCharacterImages(List<DialogData> dialogList)
    {
        var imageNames = new HashSet<string>();
        
        // ダイアログリストから使用される画像名を抽出
        foreach (var dialog in dialogList)
        {
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