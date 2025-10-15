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
public class UIPresenter : IStartable, System.IDisposable
{
    [Inject] private readonly CardPoolService _cardPoolService;
    [Inject] private readonly GameProgressService _gameProgressService;
    
    public Observable<Unit> PlayButtonClicked => Observable.Merge(
        _playButtonView.PlayButtonClicked,
        _cardDetailView.PlayButtonClicked.Do(_ => _cardDetailView.Hide())
    );
    
    private readonly ThemeView _themeView;
    private readonly AnnouncementView _announcementView;
    private readonly NarrationView _narrationView;
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
    private readonly BattleResultView _battleResultView;
    private PlayStyle _selectedPlayStyle = PlayStyle.Hesitation;
    private int _mentalBetValue = 1;
    private readonly CompositeDisposable _disposables = new ();
    private readonly Player _player;
    private readonly Enemy _enemy;
    private bool _isEnemySpriteManualMode;
    private readonly TutorialPresenter _tutorialPresenter;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private ThemeData _currentTheme;
    private readonly HandView _playerHandView;

    public void SetTheme(ThemeData theme)
    {
        _currentTheme = theme;
        _themeView.DisplayThemeWithKeywords(theme);
    }
    
    public async UniTask ShowAnnouncement(string message, float duration = 2f) => await _announcementView.DisplayAnnouncement(message, duration);
    public async UniTask ShowNarration(string message, float duration = 2f) => await _narrationView.DisplayNarration(message, duration);
    public async UniTask ShowEnemyNarration(string message, float duration = 2f) => await _enemyNarrationView.DisplayNarration(message, duration);
    public void ShowPlayButton() => _playButtonView.Show();
    public void HidePlayButton() => _playButtonView.Hide();
    public async UniTask ShowGameOverScreen(string reason)  => await _gameOverView.ShowGameOverScreen(reason);
    public PlayStyle GetSelectedPlayStyle() => _selectedPlayStyle;
    public int GetMentalBetValue() => _mentalBetValue;

    public void InitializeEnemy(EnemyData enemyData)
    {
        _enemyView　= UnityEngine.Object.FindFirstObjectByType<EnemyView>();
        _enemyView.Initialize(enemyData);
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
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin, float playerScore, float enemyScore, PlayerMove playerMove, PlayerMove enemyMove, ThemeData theme) => await _scoreResultView.ShowWinLoseResult(result, isPlayerWin, playerScore, enemyScore, playerMove, enemyMove, theme);
    public void ShowBattleResult(bool playerWon, int playerWins, int enemyWins, List<ThemeData> wonThemes) => _battleResultView.ShowBattleResult(playerWon, playerWins, enemyWins, wonThemes);
    public async UniTask WaitForBattleResultClose() => await _battleResultView.WaitForUntilClose();
    public async UniTask ShowBlackOverlay() => await _blackOverlayView.FadeIn();
    public async UniTask HideBlackOverlay() => await _blackOverlayView.FadeOut();
    public async UniTask StartTutorial(string tutorialId) => await _tutorialPresenter.StartTutorial(tutorialId);
    
    /// <summary>
    /// 選択されたカードの詳細を表示
    /// </summary>
    private void ShowCardDetail()
    {
        var selectedCard = _player.SelectedCard.CurrentValue;
        if (selectedCard?.Data != null)
        {
            _cardDetailView.ShowCardDetail(selectedCard.Data, true);
        }
    }
    
    /// <summary>
    /// 詳細ボタンの表示状態を現在の選択状態に基づいて更新
    /// </summary>
    private void UpdateDetailButtonVisibility()
    {
        if (_player.SelectedCard.CurrentValue != null)
            _cardDetailButtonView?.Show();
        else
            _cardDetailButtonView?.Hide();
    }
    
    public UIPresenter(Player player, Enemy enemy, AllTutorialData allTutorialData, SceneTransitionManager sceneTransitionManager)
    {
        _player = player;
        _enemy = enemy;

        // 初期化
        _themeView = UnityEngine.Object.FindFirstObjectByType<ThemeView>();
        _announcementView = UnityEngine.Object.FindFirstObjectByType<AnnouncementView>();

        // 複数のNarrationViewを取得して、プレイヤー用と敵用を区別
        var narrationViews = UnityEngine.Object.FindObjectsByType<NarrationView>(UnityEngine.FindObjectsSortMode.None);
        if (narrationViews.Length != 2) throw new System.Exception("Expected exactly two NarrationViews in the scene.");
        _narrationView = narrationViews[0].transform.position.y > narrationViews[1].transform.position.y ? narrationViews[1] : narrationViews[0];
        _enemyNarrationView = narrationViews[0].transform.position.y > narrationViews[1].transform.position.y ? narrationViews[0] : narrationViews[1];

        _playButtonView = UnityEngine.Object.FindFirstObjectByType<PlayButtonView>();
        _playStyleView = UnityEngine.Object.FindFirstObjectByType<PlayStyleView>();
        _mentalBetView = UnityEngine.Object.FindFirstObjectByType<MentalBetView>();

        var mentalPowerViews = UnityEngine.Object.FindObjectsByType<MentalPowerView>(UnityEngine.FindObjectsSortMode.None);
        // Y座標が低い方をプレイヤー、高い方を敵とする
        _playerMentalPowerView = mentalPowerViews[0].transform.position.y < mentalPowerViews[1].transform.position.y ? mentalPowerViews[0] : mentalPowerViews[1];
        _enemyMentalPowerView = mentalPowerViews[0].transform.position.y > mentalPowerViews[1].transform.position.y ? mentalPowerViews[0] : mentalPowerViews[1];

        _gameOverView = UnityEngine.Object.FindFirstObjectByType<GameOverView>();
        _enemyView = UnityEngine.Object.FindFirstObjectByType<EnemyView>();
        _personalityLogView = UnityEngine.Object.FindFirstObjectByType<PersonalityLogView>();
        _personalityLogButtonView = UnityEngine.Object.FindFirstObjectByType<PersonalityLogButtonView>();
        _scoreView = UnityEngine.Object.FindFirstObjectByType<ScoreView>();
        _scoreResultView = UnityEngine.Object.FindFirstObjectByType<ScoreResultView>();
        _blackOverlayView = UnityEngine.Object.FindFirstObjectByType<BlackOverlayView>();
        _cardDetailButtonView = UnityEngine.Object.FindFirstObjectByType<CardDetailButtonView>();
        _cardDetailView = UnityEngine.Object.FindFirstObjectByType<CardDetailView>();
        _battleResultView = UnityEngine.Object.FindFirstObjectByType<BattleResultView>();
        _tutorialPresenter = new TutorialPresenter(allTutorialData);
        _sceneTransitionManager = sceneTransitionManager;
        
        // プレイヤーのHandViewを取得（Y座標が低い方がプレイヤー）
        var handViews = Object.FindObjectsByType<HandView>(FindObjectsSortMode.None);
        _playerHandView = handViews[0].transform.position.y < handViews[1].transform.position.y ? handViews[0] : handViews[1];
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
    /// 選択されたカードの崩壊ビジュアルを更新
    /// </summary>
    private void UpdateCardCollapseVisual(CardModel cardModel, int selectedIndex, float score)
    {
        if (_currentTheme == null) return;
        
        // 崩壊確率を計算
        var move = new PlayerMove(cardModel.Data, _selectedPlayStyle, _mentalBetValue);
        var collapseChance = CollapseJudge.CalculateCollapseChance(move);
        
        // HandViewのメソッドを使用して色を更新
        _playerHandView.UpdateCardVisual(selectedIndex, collapseChance, score);
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
        // プレイヤーのカード選択を監視して敵のSpriteを更新
        _player.SelectedCard.Subscribe(cardModel => 
        {
            // 手動制御モード中は自動更新をスキップ
            if (_isEnemySpriteManualMode) return;
            if (cardModel != null && cardModel.Data) _enemyView.UpdateSpriteForAttribute(cardModel.Data.Attribute).Forget();
            else _enemyView.ResetToDefaultSprite().Forget();
        }).AddTo(_disposables);
        
        // プレイヤーのカード選択を監視して詳細ボタンの表示制御
        _player.SelectedCard.Subscribe(_ => UpdateDetailButtonVisibility()).AddTo(_disposables);
        
        // 崩壊ビジュアル更新に関する全てのイベントを統合
        var cardSelectionChange = _player.SelectedCard.Select(_ => Unit.Default);
        var cardIndexChange = _player.SelectedIndex.Select(_ => Unit.Default);
        var playStyleChange = _playStyleView.PlayStyleSelected.Select(_ => Unit.Default);
        var mentalBetChange = _mentalBetView.MentalBetChanged.Select(_ => Unit.Default);
        
        // 全ての変更イベントをマージして崩壊ビジュアルを更新
        Observable.Merge(cardSelectionChange, cardIndexChange, playStyleChange, mentalBetChange)
            .Subscribe(_ =>
            {
                ResetAllCardCollapseVisuals();
                var card = _player.SelectedCard.CurrentValue;
                var index = _player.SelectedIndex.CurrentValue;
                if (card != null && index >= 0 && _currentTheme != null)
                {
                    // PlayerMoveを作成してスコアを計算
                    var move = new PlayerMove(card.Data, _selectedPlayStyle, _mentalBetValue);
                    var score = ScoreCalculator.CalculateScoreWithoutEnemy(move, _currentTheme);
                    UpdateCardCollapseVisual(card, index, score);
                }
            }).AddTo(_disposables);
    }
    
    private void SetUpButtonEvents()
    {
        _personalityLogButtonView?.OnButtonClicked.Subscribe(
            _ => _personalityLogView.ShowLog())
            .AddTo(_disposables);
        _gameOverView.OnRetryClicked.Subscribe(
                _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Battle).Forget())
            .AddTo(_disposables);
        _gameOverView.OnTitleClicked.Subscribe(
                _ => _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget())
            .AddTo(_disposables);
        _cardDetailButtonView?.DetailButtonClicked.Subscribe(
                _ => ShowCardDetail())
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
        
        // 詳細ボタンの初期状態を設定
        UpdateDetailButtonVisibility();
    }

    public void Dispose()
    {
        // すべてのViewのイベントを解除
        _disposables.Dispose();
    }
}