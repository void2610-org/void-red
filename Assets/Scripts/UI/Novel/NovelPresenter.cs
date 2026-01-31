using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;
using Void2610.SettingsSystem;
using Void2610.UnityTemplate;

/// <summary>
/// ノベルシーンのUI管理を担当するプレゼンター
/// ダイアログの進行制御とViewの管理を行う
/// </summary>
public class NovelPresenter : IStartable, ISceneInitializable, System.IDisposable
{
    private GameProgressService _gameProgressService;
    private SceneTransitionManager _sceneTransitionManager;
    private NovelDialogService _novelDialogService;
    private AddressableImageLoader _addressableImageLoader;
    private IConfirmationDialog _confirmationDialogService;
    private SettingsManager _settingsManager;
    private CardPoolService _cardPoolService;
    private InputActionsProvider _inputActionsProvider;
    private DialogView _dialogView;
    private DialogCharacterView _dialogCharacterView;
    private DialogBackgroundView _dialogBackgroundView;
    private ItemGetEffectView _itemGetEffectView;
    private ChoiceView _choiceView;
    private CardChoiceView _cardChoiceView;
    private NovelSeManager _novelSeManager;

    private readonly UniTaskCompletionSource _initializationComplete = new();

    // ダイアログ制御用
    private List<DialogData> _currentDialogList;
    private int _currentDialogIndex;
    private int _choiceCounter;
    private string _currentScenarioId;
    private readonly CompositeDisposable _disposables = new();

    /// <summary>
    /// シーンの初期化完了を待つ（ISceneInitializable実装）
    /// </summary>
    public UniTask WaitForInitializationAsync() => _initializationComplete.Task;

    public NovelPresenter(bool useLocalExcel)
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
        IConfirmationDialog confirmationDialogService,
        SettingsManager settingsManager,
        CardPoolService cardPoolService,
        InputActionsProvider inputActionsProvider)
    {
        _novelSeManager = novelSeManager;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _confirmationDialogService = confirmationDialogService;
        _settingsManager = settingsManager;
        _cardPoolService = cardPoolService;
        _inputActionsProvider = inputActionsProvider;
        _addressableImageLoader = new AddressableImageLoader();
    }
    
    public void Start()
    {
        Initialize().Forget();
        SafeNavigationManager.SelectRootForceSelectable().Forget();
        BgmManager.Instance.PlayBGM("Novel");
    }
    
    private async UniTaskVoid Initialize()
    {
        _dialogView = Object.FindAnyObjectByType<DialogView>();
        _dialogCharacterView = Object.FindAnyObjectByType<DialogCharacterView>();
        _dialogBackgroundView = Object.FindAnyObjectByType<DialogBackgroundView>();
        _itemGetEffectView = Object.FindAnyObjectByType<ItemGetEffectView>();
        _choiceView = Object.FindAnyObjectByType<ChoiceView>();
        _cardChoiceView = Object.FindAnyObjectByType<CardChoiceView>();

        // キーバインドを初期化
        NovelKeyBindings.Setup(_inputActionsProvider, this, _disposables);

        // Viewイベントを購読
        _dialogView.OnSkipRequested
            .Subscribe(_ => SkipAllDialogsInternal().Forget())
            .AddTo(_disposables);

        // SE音量設定を適用
        var seSetting = _settingsManager.GetSetting<SliderSetting>("SE音量");
        _novelSeManager.SeVolume = seSetting.CurrentValue;

        _currentScenarioId = _gameProgressService.GetCurrentNode().NodeId;
        Debug.Log($"[NovelUIPresenter] シナリオ開始: {_currentScenarioId}");

        // Excel/スプレッドシートからシナリオを読み込み
        await StartScenario(_currentScenarioId);

        await OnDialogCompleted();
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

        // シーン初期化完了を通知
        _initializationComplete.TrySetResult();

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
        
        // 全てのダイアログを順番に処理
        while (_currentDialogIndex < _currentDialogList.Count)
        {
            var currentDialog = _currentDialogList[_currentDialogIndex];
            
            // ダイアログを表示（完了まで待機）
            await ShowSingleDialog(currentDialog);
            
            if (currentDialog.HasGetItem) await ShowItemGetEffect(currentDialog.GetItemData);
            if (currentDialog.HasChoice) await ShowChoiceEffect(currentDialog.ChoiceData);
            if (currentDialog.HasCardChoice) await ShowCardChoiceEffect(currentDialog.CardChoiceData);
            if (currentDialog.HasGetCard) await ShowCardGetEffect();
            
            _currentDialogIndex++;
        }
    }
    
    /// <summary>
    /// 単一のダイアログを表示（完了まで待機）
    /// </summary>
    private async UniTask ShowSingleDialog(DialogData dialogData)
    {
        // 背景画像を読み込み
        if (!string.IsNullOrEmpty(dialogData.BackgroundImageName))
        {
            var backgroundSprite = await _addressableImageLoader.LoadBackgroundImageAsync(dialogData.BackgroundImageName);
            await _dialogBackgroundView.SetBackground(backgroundSprite);
        }
        
        // キャラクター画像を読み込み（事前読み込み済みなのでキャッシュから取得）
        if (!string.IsNullOrEmpty(dialogData.CharacterImageName))
        {
            var characterSprite = await _addressableImageLoader.LoadCharacterImageAsync(dialogData.CharacterImageName);
            _dialogCharacterView.SetCharacterImage(characterSprite, dialogData.CharacterImageName.Contains("Alv"));
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
        await _dialogView.ShowSingleDialog(dialogData, seWaitTime);
    }
    
    /// <summary>
    /// アイテム取得演出を表示
    /// </summary>
    private async UniTask ShowItemGetEffect(ItemGetData itemGetData)
    {
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
    }
    
    /// <summary>
    /// 選択肢表示を実行
    /// </summary>
    private async UniTask ShowChoiceEffect(ChoiceData choiceData)
    {
        // 選択肢を表示して結果を取得
        var selectedIndex = await _choiceView.ShowChoice(choiceData);
        
        // 選択結果をGameProgressServiceに記録
        var novelRes = new NovelChoiceResult(_currentScenarioId, _choiceCounter, selectedIndex);
        _gameProgressService.RecordNovelChoice(novelRes);
        
        // 選択肢番号をインクリメント
        _choiceCounter++;
        
        // 選択結果をログ出力
        Debug.Log($"[NovelUIPresenter] ユーザーが選択した選択肢: {selectedIndex} - {choiceData.GetOption(selectedIndex)}");
    }
    
    /// <summary>
    /// カード風選択肢表示を実行
    /// </summary>
    private async UniTask ShowCardChoiceEffect(CardChoiceData cardChoiceData)
    {
        // ダイアログパネルと立ち絵を非表示
        _dialogCharacterView.FadeOut().Forget();
        await _dialogView.SetDialogPanelVisible(false);
        
        // カード画像を並列で読み込み（待機時間短縮）
        var (cardImage1, cardImage2) = await UniTask.WhenAll(
            _addressableImageLoader.LoadItemImageAsync(cardChoiceData.ImageName1),
            _addressableImageLoader.LoadItemImageAsync(cardChoiceData.ImageName2)
        );
        
        // カード風選択肢を表示して結果を取得
        var selectedIndex = await _cardChoiceView.ShowCardChoice(cardChoiceData, cardImage1, cardImage2);
        
        // 選択結果をGameProgressServiceに記録
        var novelRes = new NovelChoiceResult(_currentScenarioId, _choiceCounter, selectedIndex);
        _gameProgressService.RecordNovelChoice(novelRes);
        
        // 選択肢番号をインクリメント
        _choiceCounter++;
        
        // 選択結果をログ出力
        Debug.Log($"[NovelPresenter] ユーザーが選択したカード選択肢: {selectedIndex} - {cardChoiceData.GetOption(selectedIndex)}");
        
        // ダイアログパネルと立ち絵を再表示
        _dialogCharacterView.FadeIn().Forget();
        await _dialogView.SetDialogPanelVisible(true);
    }
    
    /// <summary>
    /// カード獲得演出を実行
    /// 選択結果に基づいてカードを決定し、DeckCardViewを使用してカード表示
    /// </summary>
    private async UniTask ShowCardGetEffect()
    {
        // 現在のシナリオの選択結果を取得
        var choiceResults = _gameProgressService.GetChoiceResultsByScenario(_currentScenarioId);
        
        // 選択結果に基づいてカードIDを決定
        var selectedCardId = NovelCardSelectionService.SelectCardByChoices(_currentScenarioId, choiceResults);
        
        if (string.IsNullOrEmpty(selectedCardId)) return;
        
        // CardPoolServiceから実際のCardDataを取得
        var cardData = _cardPoolService.GetCardById(selectedCardId);
        if (cardData == null)
        {
            Debug.LogWarning($"[NovelUIPresenter] カードID '{selectedCardId}' が見つかりません");
            return;
        }
        
        var cardModel =  new CardModel(cardData);

        // ItemGetDataとしてカード獲得演出データを作成
        var cardGetData = new ItemGetData("", cardData.CardName, "");
        
        // カード専用演出を実行（DeckCardViewを使用）
        _novelSeManager.WaitAndPlaySe("ItemGet", delayTime: 1f, pitch: 1f);
        await _itemGetEffectView.ShowCardGetEffect(cardGetData, cardModel);
    }

    /// <summary>
    /// 全ダイアログをスキップして即座に完了（InputSystem用の公開メソッド）
    /// </summary>
    public void RequestSkipAllDialogs()
    {
        SkipAllDialogsInternal().Forget();
    }

    /// <summary>
    /// 全ダイアログをスキップして即座に完了（内部実装）
    /// </summary>
    private async UniTaskVoid SkipAllDialogsInternal()
    {
        var confirmed = await _confirmationDialogService.ShowDialog(
            "現在のシナリオをスキップしますか？",
            "スキップ",
            "キャンセル"
        );

        if (!confirmed) return;

        // 現在のダイアログ表示を強制終了
        _dialogView.ForceComplete();

        // 選択肢またはカード獲得が見つかるまでインデックスを進める
        _currentDialogIndex = FindNextInteractionPoint(_currentDialogIndex);
    }
    
    /// <summary>
    /// 次の選択肢またはカード獲得までのインデックスを探索
    /// </summary>
    private int FindNextInteractionPoint(int startIndex)
    {        
        // その先の選択肢/カード獲得を探索
        for (var i = startIndex + 1; i < _currentDialogList.Count; i++)
        {
            var dialog = _currentDialogList[i];
            if (dialog.HasChoice || dialog.HasCardChoice || dialog.HasGetCard)
            {
                return i - 1;
            }
        }
        
        // 見つからない場合はシナリオ完了
        return _currentDialogList.Count;
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
    private async UniTask OnDialogCompleted()
    {
        // プロローグ終了後に初期デッキを受け取る
        // エンディング終了後にSteamストアページを開く
        if (_currentScenarioId == "ending")
            Application.OpenURL("https://store.steampowered.com/app/3997140/");
        
        await UniTask.Delay(1000);
        
        // 現在のノードを結果記録前に取得
        var currentNode = _gameProgressService.GetCurrentNode();

        _gameProgressService.RecordNovelResultAndSave();
        
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
    
    public void Dispose()
    {
        // 購読を一括解除
        _disposables?.Dispose();

        // 画像のメモリを解放
        _addressableImageLoader?.UnloadAllImages();
    }
}