using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター
/// ダイアログの進行制御とViewの管理を行う
/// </summary>
public class NovelUIPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scenarioIdText;
    [SerializeField] private NovelSeManager novelSeManager;
    
    [Header("データソース設定")]
    [SerializeField] private bool useAlphaHardcode; // trueでアルファ版ハードコード、falseでサービス経由（Excel/スプレッドシート）
    [SerializeField] private bool useLocalExcel = true; // trueでExcel、falseでスプレッドシート（useAlphaHardcode=falseの時に有効）
    
    private GameProgressService _gameProgressService;
    private SceneTransitionManager _sceneTransitionManager;
    private NovelDialogService _novelDialogService;
    private AddressableImageLoader _characterImageLoader;
    private ConfirmationDialogService _confirmationDialogService;
    private SettingsManager _settingsManager;
    private DialogView _dialogView;
    private ItemGetEffectView _itemGetEffectView;
    
    // ダイアログ制御用
    private List<DialogData> _currentDialogList;
    private int _currentDialogIndex;
    private bool _isShowingItemGetEffect; // アイテム取得演出表示中フラグ
    
    [Inject]
    public void Construct(
        GameProgressService gameProgressService, 
        SceneTransitionManager sceneTransitionManager, 
        ConfirmationDialogService confirmationDialogService,
        SettingsManager settingsManager)
    {
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _confirmationDialogService = confirmationDialogService;
        _settingsManager = settingsManager;
        _characterImageLoader = new AddressableImageLoader();
    }
    
    private async UniTask Start()
    {
        // DialogViewを取得
        _dialogView = FindFirstObjectByType<DialogView>();
        
        // ItemGetEffectViewを取得
        _itemGetEffectView = FindFirstObjectByType<ItemGetEffectView>();
        
        // Viewイベントを購読
        _dialogView.OnDialogCompleted += () => OnDialogCompleted().Forget();
        _dialogView.OnUserClickDetected += () => HandleUserClick().Forget();
        _dialogView.OnSkipRequested += () => SkipAllDialogs().Forget();
        
        // ビルドでは必ずローカルExcelを使用
        #if !UNITY_EDITOR
        useLocalExcel = true;
        #endif
        
        _novelDialogService = new NovelDialogService(useLocalExcel);
        
        // SE音量設定を適用
        var seSetting = _settingsManager.GetSetting<SliderSetting>("SE音量");
        novelSeManager.SeVolume = seSetting.CurrentValue;

        var scenarioId = _gameProgressService.GetCurrentNode().NodeId;

        // データソース設定により処理を分岐
        List<DialogData> dialogList;
        if (useAlphaHardcode)
        {
            // アルファ版はハードコードでシナリオを提供
            if (scenarioId == "prologue1") dialogList = PrologueProvider.GetPrologueScenario();
            else if (scenarioId == "prologue2") dialogList = PrologueProvider.GetPrologue2Scenario();
            else if (scenarioId == "ending") dialogList = PrologueProvider.GetEndingScenario();
            else
            {
                Debug.LogWarning($"ハードコード未対応のシナリオID: {scenarioId}");
                return;
            }
            
            await PreloadCharacterImages(dialogList);
            await StartDialogSequence(dialogList);
        }
        else
        {
            // Excel/スプレッドシートからシナリオを読み込み
            StartScenario(scenarioId).Forget();
        }
    }
    
    /// <summary>
    /// シナリオIDに応じてシナリオを開始（Excel/スプレッドシート読み込み）
    /// </summary>
    private async UniTaskVoid StartScenario(string scenarioId)
    {
        // Excel/スプレッドシートからシナリオを読み込み
        var dialogList = await _novelDialogService.GetDialogDataAsync(scenarioId);

        // ダイアログリストが有効かチェック
        if (dialogList == null || dialogList.Count == 0)
        {
            Debug.LogError($"シナリオ '{scenarioId}' の読み込みに失敗しました");
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
        
        // 背景画像を読み込み
        Sprite backgroundSprite = null;
        if (!string.IsNullOrEmpty(currentDialog.BackgroundImageName))
        {
            backgroundSprite = await _characterImageLoader.LoadBackgroundImageAsync(currentDialog.BackgroundImageName);
        }
        
        // SE再生と再生時間の取得
        novelSeManager.StopSe();
        var seWaitTime = 0f;
        if (currentDialog.HasSe)
        {
            // SEのクリップ長を取得（オートモード時のため）
            seWaitTime = novelSeManager.PlaySe(currentDialog.SeClipName);
        }
        
        // 読み込み完了後にViewに渡してダイアログを表示
        await _dialogView.ShowSingleDialog(currentDialog, characterSprite, backgroundSprite, seWaitTime);
    }
    
    /// <summary>
    /// アイテム取得演出を表示
    /// </summary>
    /// <param name="dialogData">アイテム取得情報を含むダイアログデータ</param>
    private async UniTask ShowItemGetEffect(DialogData dialogData)
    {
        // アイテムデータを作成
        var itemGetData = ItemGetData.FromDialogData(dialogData);
        if (itemGetData == null)
        {
            Debug.LogWarning("[NovelUIPresenter] アイテム取得演出データの作成に失敗しました");
            return;
        }
        
        // アイテム画像を読み込み
        Sprite itemSprite = null;
        if (!string.IsNullOrEmpty(itemGetData.ItemImageName))
        {
            // アイテム画像はキャラクター画像ローダーを使用
            itemSprite = await _characterImageLoader.LoadCharacterImageAsync(itemGetData.ItemImageName);
        }
        
        // ItemGetEffectViewが存在する場合は実際の演出を表示
        if (_itemGetEffectView != null)
        {
            await _itemGetEffectView.ShowItemGetEffect(itemGetData, itemSprite);
        }
        else
        {
            // フォールバック：ログ出力
            Debug.LogWarning($"[NovelUIPresenter] ItemGetEffectViewが見つかりません。アイテム取得演出をスキップします。");
            Debug.Log($"[アイテム取得演出] アイテム名: '{itemGetData.ItemName}', 説明: '{itemGetData.ItemDescription}', 画像: '{itemGetData.ItemImageName}'");
        }
    }
    
    /// <summary>
    /// ユーザークリック処理
    /// </summary>
    private async UniTaskVoid HandleUserClick()
    {
        // アイテム取得演出中はクリックを無視
        if (_isShowingItemGetEffect)
        {
            return;
        }
        
        // 前のダイアログ（現在のインデックス-1）にアイテム取得演出があるかチェック
        var previousDialogIndex = _currentDialogIndex - 1;
        if (previousDialogIndex >= 0 && previousDialogIndex < _currentDialogList.Count)
        {
            var previousDialog = _currentDialogList[previousDialogIndex];
            if (previousDialog.HasGetItem)
            {
                _isShowingItemGetEffect = true;
                
                // DialogViewのクリックを無効化
                _dialogView.SetInteractable(false);
                
                await ShowItemGetEffect(previousDialog);
                
                // DialogViewのクリックを有効化
                _dialogView.SetInteractable(true);
                
                _isShowingItemGetEffect = false;
                
                // 演出完了後、次のダイアログへ進む
                await ShowNextDialog();
                return;
            }
        }
        
        // 通常の次のダイアログへ進む
        await ShowNextDialog();
    }
    
    /// <summary>
    /// 全ダイアログをスキップして即座に完了
    /// </summary>
    private async UniTaskVoid SkipAllDialogs()
    {
        // アイテム取得演出中はスキップを無視
        if (_isShowingItemGetEffect)
        {
            return;
        }
        
        var confirmed = await _confirmationDialogService.ShowDialog(
            "現在のシナリオをスキップしますか？",
            "スキップ",
            "キャンセル"
        );
        
        if (!confirmed) return;
        
        // 残りのダイアログを全てスキップ
        _currentDialogIndex = _currentDialogList.Count;
        // ダイアログ完了を表示
        await _dialogView.ShowDialogComplete();
    }
    
    /// <summary>
    /// ダイアログリストに含まれるキャラクター画像と背景画像を事前に読み込み
    /// </summary>
    private async UniTask PreloadCharacterImages(List<DialogData> dialogList)
    {
        var characterImageNames = new HashSet<string>();
        var backgroundImageNames = new HashSet<string>();
        
        // ダイアログリストから使用される画像名を抽出
        foreach (var dialog in dialogList)
        {
            if (!string.IsNullOrEmpty(dialog.CharacterImageName))
            {
                characterImageNames.Add(dialog.CharacterImageName);
            }
            
            if (!string.IsNullOrEmpty(dialog.BackgroundImageName))
            {
                backgroundImageNames.Add(dialog.BackgroundImageName);
            }
        }
        
        // 画像を並列で読み込み
        var loadTasks = new List<UniTask>();
        
        // キャラクター画像の読み込み
        foreach (var imageName in characterImageNames)
        {
            loadTasks.Add(_characterImageLoader.LoadCharacterImageAsync(imageName).AsUniTask());
        }
        
        // 背景画像の読み込み
        foreach (var imageName in backgroundImageNames)
        {
            loadTasks.Add(_characterImageLoader.LoadBackgroundImageAsync(imageName).AsUniTask());
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