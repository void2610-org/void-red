using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// UIのビジネスロジックとイベント処理を担当するPresenterクラス
/// VContainerで依存性注入される
/// </summary>
public class BattleUIPresenter : IStartable, System.IDisposable
{
    [Inject] private readonly CardPoolService _cardPoolService;
    [Inject] private readonly GameProgressService _gameProgressService;
    [Inject] private readonly InputActionsProvider _inputActionsProvider;
    
    public Observable<Unit> PlayButtonClicked => Observable.Merge(
        _playButtonView.PlayButtonClicked,
        _cardDetailView.PlayButtonClicked.Do(_ => _cardDetailView.Hide())
    );
    
    private readonly ThemeView _themeView;
    private readonly AnnouncementView _announcementView;
    private readonly NarrationView _playerNarrationView;
    private readonly NarrationView _enemyNarrationView;
    private readonly PlayButtonView _playButtonView;
    private readonly PlayStyleView _playStyleView;
    private readonly MentalBetView _mentalBetView;
    private readonly MentalPowerView _playerMentalPowerView;
    private readonly MentalPowerView _enemyMentalPowerView;
    private readonly GameOverView _gameOverView;
    private EnemyView _enemyView;
    private readonly PersonalityLogView _personalityLogView;
    private readonly PersonalityLogButtonView _personalityLogButtonView;
    private readonly ScoreView _scoreView;
    private readonly ScoreResultView _scoreResultView;
    private readonly BlackOverlayView _blackOverlayView;
    private readonly CardDetailButtonView _cardDetailButtonView;
    private readonly CardDetailView _cardDetailView;
    private readonly ThemeDetailView _themeDetailView;
    private readonly BattleResultView _battleResultView;
    private PlayStyle _selectedPlayStyle = PlayStyle.Impulse;
    private int _mentalBetValue = 1;
    private readonly CompositeDisposable _disposables = new ();
    private readonly Player _player;
    private readonly Enemy _enemy;
    private bool _isEnemySpriteManualMode;
    private readonly TutorialPresenter _tutorialPresenter;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private ThemeData _currentTheme;
    private readonly HandView _playerHandView;
    private BattlePresenter _battlePresenter;

    /// <summary>
    /// BattlePresenterを設定（循環依存を避けるため後から設定）
    /// </summary>
    public void SetBattlePresenter(BattlePresenter battlePresenter)
    {
        _battlePresenter = battlePresenter;
        // BattlePresenterが設定されたらキーバインドをセットアップ
        BattleKeyBindings.Setup(_inputActionsProvider, this, _themeView, _battlePresenter.CurrentGameState, _disposables);
    }

    public async UniTask SetTheme(ThemeData theme, bool isMainTheme)
    {
        _currentTheme = theme;
        await _themeView.DisplayThemeWithKeywords(theme, isMainTheme);
    }
    
    public async UniTask ShowThemeDetailAndWait()
    {
        _themeDetailView.ShowThemeDetail(_currentTheme);
        await _themeDetailView.WaitForClose();
    }

    public async UniTask ShowAnnouncement(string message, float duration = 2f) => await _announcementView.DisplayAnnouncement(message, duration);
    public async UniTask ShowPlayerNarration(string message, bool autoAdvance) => await _playerNarrationView.DisplayNarration(message, 2f, autoAdvance);
    public async UniTask ShowEnemyNarration(string message, bool autoAdvance) => await _enemyNarrationView.DisplayNarration(message, 2f, autoAdvance);
    public void SetPlayButtonInteractable(bool interactable) => _playButtonView.SetInteractable(interactable);
    public void SetCardDetailButtonInteractable(bool interactable) => _cardDetailButtonView.SetInteractable(interactable);
    public void ShowGameOverScreen(string reason) => _gameOverView.ShowGameOverScreen(reason);
    public PlayStyle GetSelectedPlayStyle() => _selectedPlayStyle;
    public int GetMentalBetValue() => _mentalBetValue;

    public void InitializeEnemy(EnemyData enemyData)
    {
        _enemyView = Object.FindFirstObjectByType<EnemyView>();
        _enemyView.Initialize(enemyData);
        _enemyMentalPowerView.SetCharacterIcon(enemyData.IconSprite, enemyData.FrameSprite);
    }

    public async UniTask ShowEnemy() => await _enemyView.Show();
    public async UniTask HideEnemy() => await _enemyView.Hide();
    public async UniTask ResetEnemyToDefault() 
    {
        _isEnemySpriteManualMode = false; // 自動監視モードに戻す
        await _enemyView.ResetToDefaultSprite();
    }
    
    public async UniTask UpdateEnemySprite(CardAttribute attribute) 
    {
        _isEnemySpriteManualMode = true; // 手動制御モードに切り替え
        await _enemyView.UpdateSpriteForAttribute(attribute);
    }
    
    public async UniTask ShowScores(float playerScore, float enemyScore) => await _scoreView.ShowScores(playerScore, enemyScore);
    public void ShowBattleResult(bool playerWon, int playerWins, int enemyWins, List<ThemeData> wonThemes) => _battleResultView.ShowBattleResult(playerWon, playerWins, enemyWins, wonThemes);
    public async UniTask WaitForBattleResultClose() => await _battleResultView.WaitForUntilClose();
    public async UniTask ShowBlackOverlay() => await _blackOverlayView.FadeIn();
    public async UniTask HideBlackOverlay() => await _blackOverlayView.FadeOut();
    public async UniTask StartBattleTutorial() => await _tutorialPresenter.StartBattleTutorial();
    public async UniTask StartResultTutorial() => await _tutorialPresenter.StartResultTutorial();
    
    /// <summary>
    /// 精神ベットを増やす（InputSystem用の公開メソッド）
    /// </summary>
    public void IncrementMentalBet() => OnMentalBetChanged(1);

    /// <summary>
    /// 精神ベットを減らす（InputSystem用の公開メソッド）
    /// </summary>
    public void DecrementMentalBet() => OnMentalBetChanged(-1);

    public BattleUIPresenter(Player player, Enemy enemy, AllTutorialData allTutorialData, SceneTransitionManager sceneTransitionManager, InputActionsProvider inputActionsProvider)
    {
        _player = player;
        _enemy = enemy;
        _sceneTransitionManager = sceneTransitionManager;

        // 初期化
        _themeView = Object.FindFirstObjectByType<ThemeView>();
        _announcementView = Object.FindFirstObjectByType<AnnouncementView>();
        _playButtonView = Object.FindFirstObjectByType<PlayButtonView>();
        _playStyleView = Object.FindFirstObjectByType<PlayStyleView>();
        _mentalBetView = Object.FindFirstObjectByType<MentalBetView>();
        _gameOverView = Object.FindFirstObjectByType<GameOverView>();
        _enemyView = Object.FindFirstObjectByType<EnemyView>();
        _personalityLogView = Object.FindFirstObjectByType<PersonalityLogView>();
        _personalityLogButtonView = Object.FindFirstObjectByType<PersonalityLogButtonView>();
        _scoreView = Object.FindFirstObjectByType<ScoreView>();
        _scoreResultView = Object.FindFirstObjectByType<ScoreResultView>();
        _blackOverlayView = Object.FindFirstObjectByType<BlackOverlayView>();
        _cardDetailButtonView = Object.FindFirstObjectByType<CardDetailButtonView>();
        _cardDetailView = Object.FindFirstObjectByType<CardDetailView>();
        _themeDetailView = Object.FindFirstObjectByType<ThemeDetailView>();
        _battleResultView = Object.FindFirstObjectByType<BattleResultView>();

        // 複数のViewを取得して、プレイヤー用と敵用を区別
        // Y座標が低い方をプレイヤー、高い方を敵とする
        var narrationViews = Object.FindObjectsByType<NarrationView>(FindObjectsSortMode.None);
        _playerNarrationView = narrationViews[0].transform.position.y > narrationViews[1].transform.position.y ? narrationViews[1] : narrationViews[0];
        _enemyNarrationView = narrationViews[0].transform.position.y > narrationViews[1].transform.position.y ? narrationViews[0] : narrationViews[1];

        var mentalPowerViews = Object.FindObjectsByType<MentalPowerView>(FindObjectsSortMode.None);
        _playerMentalPowerView = mentalPowerViews[0].transform.position.y < mentalPowerViews[1].transform.position.y ? mentalPowerViews[0] : mentalPowerViews[1];
        _enemyMentalPowerView = mentalPowerViews[0].transform.position.y > mentalPowerViews[1].transform.position.y ? mentalPowerViews[0] : mentalPowerViews[1];
        
        var handViews = Object.FindObjectsByType<HandView>(FindObjectsSortMode.None);
        _playerHandView = handViews[0].transform.position.y < handViews[1].transform.position.y ? handViews[0] : handViews[1];

        _tutorialPresenter = new TutorialPresenter(allTutorialData, inputActionsProvider, _player);
    }
    
    private void OnPlayStyleSelected(PlayStyle playStyle)
    {
        _selectedPlayStyle = playStyle;
    }
    
    private void OnMentalBetChanged(int delta)
    {
        var newValue = _mentalBetValue + delta;
        // 範囲チェック
        if (newValue < GameConstants.MIN_MENTAL_BET || newValue > GameConstants.MAX_MENTAL_BET) return;
        _mentalBetValue = newValue;
        UpdateMentalBetDisplay();
    }
    
    private void UpdateMentalBetDisplay()
    {
        var currentMentalPower = _player.MentalPower.CurrentValue;

        // 現在のベット値が精神力を超えている場合や最小値を下回る場合は調整
        if (_mentalBetValue > currentMentalPower)
            _mentalBetValue = currentMentalPower;
        if (_mentalBetValue < GameConstants.MIN_MENTAL_BET)
            _mentalBetValue = GameConstants.MIN_MENTAL_BET;

        // MentalBetViewに表示を委譲
        _mentalBetView.UpdateDisplay(_mentalBetValue, currentMentalPower, GameConstants.MIN_MENTAL_BET, GameConstants.MAX_MENTAL_BET);
        // MentalPowerViewに精神力表示を委譲
        _playerMentalPowerView.UpdateDisplay(currentMentalPower, GameConstants.MAX_MENTAL_POWER);
    }

    /// <summary>
    /// 選択されたカードのUIEffectsを更新
    /// </summary>
    private void UpdateCardVisual(CardModel cardModel, int selectedIndex, float score)
    {
        if (_currentTheme == null) return;
        
        // HandViewのメソッドを使用して色を更新
        // _playerHandView.UpdateCardVisual(selectedIndex, collapseChance, score);
    }
    
    /// <summary>
    /// 全カードの崩壊ビジュアルをリセット
    /// </summary>
    private void ResetAllCardCollapseVisuals()
    {
        _playerHandView.ResetAllCardVisuals();
    }
    
    private void SetupViewEvents()
    {
        // プレイスタイル選択イベント
        _playStyleView.PlayStyleSelected.Subscribe(OnPlayStyleSelected).AddTo(_disposables);
        // 精神ベット変更イベント
        _mentalBetView.MentalBetChanged.Subscribe(OnMentalBetChanged).AddTo(_disposables);
        // プレイヤーの精神力変化を監視
        _player.MentalPower.Subscribe(_ => UpdateMentalBetDisplay()).AddTo(_disposables);
        // 敵の精神力変化を監視
        _enemy.MentalPower.Subscribe(mentalPower =>
            _enemyMentalPowerView.UpdateDisplay(mentalPower, GameConstants.MAX_MENTAL_POWER)
        ).AddTo(_disposables);
    }
    
    private void SetUpButtonEvents()
    {
        _personalityLogButtonView?.OnButtonClicked.Subscribe(
            _ => _personalityLogView.Show())
            .AddTo(_disposables);
        _gameOverView.OnRetryClicked.Subscribe(
                _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Battle).Forget())
            .AddTo(_disposables);
        _gameOverView.OnTitleClicked.Subscribe(
                _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget())
            .AddTo(_disposables);
    }
    
    public void Start()
    {
        // PersonalityLogViewを初期化
        _personalityLogView.Initialize(_gameProgressService);

        // Viewイベントの設定
        SetupViewEvents();
        // ボタンのイベント設定
        SetUpButtonEvents();

        // 初期表示の更新
        OnPlayStyleSelected(_selectedPlayStyle);
        UpdateMentalBetDisplay();

        // ルートボタンを初期選択
        SafeNavigationManager.SelectRootForceSelectable().Forget();
    }

    public void Dispose()
    {
        // すべてのViewのイベントを解除
        _disposables.Dispose();
    }
}