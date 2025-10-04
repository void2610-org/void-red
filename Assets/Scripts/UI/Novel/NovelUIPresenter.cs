using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター
/// ダイアログの進行制御とViewの管理を行う
/// </summary>
public class NovelUIPresenter : IStartable
{
    private GameProgressService _gameProgressService;
    private SceneTransitionManager _sceneTransitionManager;
    private NovelDialogService _novelDialogService;
    private AddressableImageLoader _addressableImageLoader;
    private ConfirmationDialogService _confirmationDialogService;
    private SettingsManager _settingsManager;
    private DialogView _dialogView;
    private ItemGetEffectView _itemGetEffectView;
    private ChoiceView _choiceView;
    private NovelSeManager _novelSeManager;
    
    // ダイアログ制御用
    private List<DialogData> _currentDialogList;
    private int _currentDialogIndex;
    private int _choiceCounter;
    private string _currentScenarioId;
    private readonly CompositeDisposable _disposables = new();
    
    public NovelUIPresenter(bool useLocalExcel)
    {
        // ビルドでは必ずローカルExcelを使用
        #if !UNITY_EDITOR
        useLocalExcel = true;
        #endif
        
        _novelDialogService = new NovelDialogService(useLocalExcel);
    }
    
    [Inject]
    public void Construct(
        NovelSeManager novelSeManager,
        GameProgressService gameProgressService, 
        SceneTransitionManager sceneTransitionManager, 
        ConfirmationDialogService confirmationDialogService,
        SettingsManager settingsManager)
    {
        _novelSeManager = novelSeManager;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _confirmationDialogService = confirmationDialogService;
        _settingsManager = settingsManager;
        _addressableImageLoader = new AddressableImageLoader();
    }
    
    public void Start()
    {
        Initialize().Forget();
    }
    
    private async UniTaskVoid Initialize()
    {
        _dialogView = UnityEngine.Object.FindAnyObjectByType<DialogView>();
        _itemGetEffectView = UnityEngine.Object.FindAnyObjectByType<ItemGetEffectView>();
        _choiceView = UnityEngine.Object.FindAnyObjectByType<ChoiceView>();
        
        // Viewイベントを購読
        _dialogView.OnDialogCompleted
            .Subscribe(_ => OnDialogCompleted().Forget())
            .AddTo(_disposables);
        _dialogView.OnSkipRequested
            .Subscribe(_ => SkipAllDialogs().Forget())
            .AddTo(_disposables);
        
        // SE音量設定を適用
        var seSetting = _settingsManager.GetSetting<SliderSetting>("SE音量");
        _novelSeManager.SeVolume = seSetting.CurrentValue;

        _currentScenarioId = _gameProgressService.GetCurrentNode().NodeId;
        Debug.Log($"[NovelUIPresenter] シナリオ開始: {_currentScenarioId}");

        // Excel/スプレッドシートからシナリオを読み込み
        await StartScenario(_currentScenarioId);
    }
    
    /// <summary>
    /// シナリオIDに応じてシナリオを開始（Excel/スプレッドシート読み込み）
    /// </summary>
    private async UniTask StartScenario(string scenarioId)
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
        Debug.Log("[NovelUIPresenter] キャラクター画像を事前読み込み中...");
        await PreloadCharacterImages(dialogList);
        // ダイアログシーケンスを開始
        Debug.Log("[NovelUIPresenter] ダイアログシーケンスを開始");
        await StartDialogSequence(dialogList);
    }
    
    /// <summary>
    /// ダイアログシーケンスを開始（Presenterが制御）
    /// </summary>
    private async UniTask StartDialogSequence(List<DialogData> dialogList)
    {
        _currentDialogList = dialogList;
        _currentDialogIndex = 0;
        _choiceCounter = 0;
        
        // フェードイン
        await _dialogView.FadeIn();
        
        // 全てのダイアログを順番に処理
        while (_currentDialogIndex < _currentDialogList.Count)
        {
            var currentDialog = _currentDialogList[_currentDialogIndex];
            
            // ダイアログを表示（完了まで待機）
            await ShowSingleDialog(currentDialog);
            
            // アイテム取得演出がある場合は実行
            if (currentDialog.HasGetItem)
            {
                await ShowItemGetEffect(currentDialog);
            }
            
            // 選択肢表示がある場合は実行
            if (currentDialog.HasChoice)
            {
                await ShowChoiceEffect(currentDialog);
            }
            
            _currentDialogIndex++;
        }
        
        // 全てのダイアログが完了
        await _dialogView.ShowDialogComplete();
    }
    
    /// <summary>
    /// 単一のダイアログを表示（完了まで待機）
    /// </summary>
    private async UniTask ShowSingleDialog(DialogData dialogData)
    {
        // キャラクター画像を読み込み（事前読み込み済みなのでキャッシュから取得）
        Sprite characterSprite = null;
        if (!string.IsNullOrEmpty(dialogData.CharacterImageName))
        {
            characterSprite = await _addressableImageLoader.LoadCharacterImageAsync(dialogData.CharacterImageName);
        }
        
        // 背景画像を読み込み
        Sprite backgroundSprite = null;
        if (!string.IsNullOrEmpty(dialogData.BackgroundImageName))
        {
            backgroundSprite = await _addressableImageLoader.LoadBackgroundImageAsync(dialogData.BackgroundImageName);
        }
        
        // SE再生と再生時間の取得
        _novelSeManager.StopSe();
        var seWaitTime = 0f;
        if (dialogData.HasSe)
        {
            // SEのクリップ長を取得（オートモード時のため）
            seWaitTime = _novelSeManager.PlaySe(dialogData.SeClipName);
        }
        
        // ダイアログを表示（完了まで待機）
        await _dialogView.ShowSingleDialog(dialogData, characterSprite, backgroundSprite, seWaitTime);
    }
    
    /// <summary>
    /// アイテム取得演出を表示
    /// </summary>
    /// <param name="dialogData">アイテム取得情報を含むダイアログデータ</param>
    private async UniTask ShowItemGetEffect(DialogData dialogData)
    {
        // アイテムデータを取得
        var itemGetData = dialogData.GetItemData;
        if (itemGetData == null)
        {
            Debug.LogWarning("[NovelUIPresenter] アイテム取得データが存在しません");
            return;
        }
        
        _dialogView.SetInteractable(false);
        
        // アイテム画像を読み込み
        Sprite itemSprite = null;
        if (!string.IsNullOrEmpty(itemGetData.ItemImageName))
        {
            // アイテム画像を読み込み
            itemSprite = await _addressableImageLoader.LoadItemImageAsync(itemGetData.ItemImageName);
        }
        
        // アイテム取得演出を実行
        _novelSeManager.WaitAndPlaySe("ItemGet", delayTime:1f, pitch: 1f);
        await _itemGetEffectView.ShowItemGetEffect(itemGetData, itemSprite);
        
        _dialogView.SetInteractable(true);
    }
    
    /// <summary>
    /// 選択肢表示を実行
    /// </summary>
    /// <param name="dialogData">選択肢情報を含むダイアログデータ</param>
    private async UniTask ShowChoiceEffect(DialogData dialogData)
    {
        // 選択肢データを取得
        var choiceData = dialogData.ChoiceData;
        if (choiceData == null)
        {
            Debug.LogWarning("[NovelUIPresenter] 選択肢データが存在しません");
            return;
        }
        
        _dialogView.SetInteractable(false);
        
        // 選択肢を表示して結果を取得
        var selectedIndex = await _choiceView.ShowChoice(choiceData);
        
        // 選択結果をGameProgressServiceに記録
        _gameProgressService.RecordNovelChoiceAndSave(_currentScenarioId, _choiceCounter, selectedIndex);
        
        // 選択肢番号をインクリメント
        _choiceCounter++;
        
        // 選択結果をログ出力
        Debug.Log($"[NovelUIPresenter] ユーザーが選択した選択肢: {selectedIndex} - {choiceData.GetOption(selectedIndex)}");
        
        _dialogView.SetInteractable(true);
    }
    
    /// <summary>
    /// 全ダイアログをスキップして即座に完了
    /// </summary>
    private async UniTaskVoid SkipAllDialogs()
    {
        var confirmed = await _confirmationDialogService.ShowDialog(
            "現在のシナリオをスキップしますか？",
            "スキップ",
            "キャンセル"
        );
        
        if (!confirmed) return;
        
        // ダイアログループを強制終了してダイアログ完了を表示
        _currentDialogIndex = _currentDialogList.Count;
        await _dialogView.ShowDialogComplete();
    }
    
    /// <summary>
    /// ダイアログリストに含まれるキャラクター画像、背景画像、アイテム画像を事前に読み込み
    /// </summary>
    private async UniTask PreloadCharacterImages(List<DialogData> dialogList)
    {
        var characterImageNames = new HashSet<string>();
        var backgroundImageNames = new HashSet<string>();
        var itemImageNames = new HashSet<string>();
        
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
            
            // アイテム画像名を抽出
            if (dialog.HasGetItem && !string.IsNullOrEmpty(dialog.GetItemData.ItemImageName))
            {
                itemImageNames.Add(dialog.GetItemData.ItemImageName);
            }
        }
        
        // 画像を並列で読み込み
        var loadTasks = new List<UniTask>();
        
        // キャラクター画像の読み込み
        foreach (var imageName in characterImageNames)
        {
            loadTasks.Add(_addressableImageLoader.LoadCharacterImageAsync(imageName).AsUniTask());
        }
        
        // 背景画像の読み込み
        foreach (var imageName in backgroundImageNames)
        {
            loadTasks.Add(_addressableImageLoader.LoadBackgroundImageAsync(imageName).AsUniTask());
        }
        
        // アイテム画像の読み込み
        foreach (var imageName in itemImageNames)
        {
            loadTasks.Add(_addressableImageLoader.LoadItemImageAsync(imageName).AsUniTask());
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
        // 購読を一括解除
        _disposables?.Dispose();
        
        // 画像のメモリを解放
        _addressableImageLoader?.UnloadAllImages();
    }
}