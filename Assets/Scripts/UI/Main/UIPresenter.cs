using Cysharp.Threading.Tasks;
using R3;
using VContainer;
using VContainer.Unity;

/// <summary>
/// UIのビジネスロジックとイベント処理を担当するPresenterクラス
/// VContainerで依存性注入される
/// </summary>
public class UIPresenter : IStartable, System.IDisposable
{
    [Inject] private readonly CardPoolService _cardPoolService;
    [Inject] private readonly ThemeService _themeService;
    [Inject] private readonly GameProgressService _gameProgressService;
    
    public Observable<Unit> PlayButtonClicked => _playButtonView.PlayButtonClicked;
    public Observable<Unit> RetryButtonClicked => _gameOverView.OnRetryClicked;
    public Observable<Unit> TitleButtonClicked => _gameOverView.OnTitleClicked;
    
    
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
    private readonly ConfirmationDialogView _confirmationDialogView;
    private readonly EnemyView _enemyView;
    private readonly PersonalityLogView _personalityLogView;
    private readonly PersonalityLogButtonView _personalityLogButtonView;
    private readonly ScoreView _scoreView;
    private readonly ResultView _resultView;
    private readonly BlackOverlayView _blackOverlayView;
    private PlayStyle _selectedPlayStyle = PlayStyle.Hesitation;
    private int _mentalBetValue = 1;
    private readonly CompositeDisposable _disposables = new ();
    private readonly Player _player;
    private readonly Enemy _enemy;
    private bool _isEnemySpriteManualMode;

    public void SetTheme(ThemeData theme) => _themeView.DisplayTheme(theme.Title);
    public async UniTask ShowAnnouncement(string message, float duration = 2f) => await _announcementView.DisplayAnnouncement(message, duration);
    public async UniTask ShowNarration(string message, float duration = 2f) => await _narrationView.DisplayNarration(message, duration);
    public async UniTask ShowEnemyNarration(string message, float duration = 2f) => await _enemyNarrationView.DisplayNarration(message, duration);
    public void ShowPlayButton() => _playButtonView.Show();
    public void HidePlayButton() => _playButtonView.Hide();
    public async UniTask ShowGameOverScreen(string reason)  => await _gameOverView.ShowGameOverScreen(reason);
    public async UniTask<bool> ShowConfirmationDialog(string message, string confirmText = "OK", string cancelText = "キャンセル") 
        => await _confirmationDialogView.ShowDialog(message, confirmText, cancelText);
    public PlayStyle GetSelectedPlayStyle() => _selectedPlayStyle;
    public int GetMentalBetValue() => _mentalBetValue;
    public void InitializeEnemy(EnemyData enemyData) => _enemyView.Initialize(enemyData);
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
    public async UniTask ShowWinLoseResult(string result, bool isPlayerWin) => await _resultView.ShowWinLoseResult(result, isPlayerWin);
    
    public async UniTask ShowBlackOverlay() => await _blackOverlayView.FadeIn();
    public async UniTask HideBlackOverlay() => await _blackOverlayView.FadeOut();
    
    public UIPresenter(Player player, Enemy enemy)
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
        _confirmationDialogView = UnityEngine.Object.FindFirstObjectByType<ConfirmationDialogView>();
        _enemyView = UnityEngine.Object.FindFirstObjectByType<EnemyView>();
        _personalityLogView = UnityEngine.Object.FindFirstObjectByType<PersonalityLogView>();
        _personalityLogButtonView = UnityEngine.Object.FindFirstObjectByType<PersonalityLogButtonView>();
        _scoreView = UnityEngine.Object.FindFirstObjectByType<ScoreView>();
        _resultView = UnityEngine.Object.FindFirstObjectByType<ResultView>();
        _blackOverlayView = UnityEngine.Object.FindFirstObjectByType<BlackOverlayView>();
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
    
    private void SetupViewEvents()
    {
        // プレイスタイル選択イベント
        _playStyleView.PlayStyleSelected.Subscribe(OnPlayStyleSelected).AddTo(_disposables);
        // 精神ベット変更イベント
        _mentalBetView.MentalBetChanged.Subscribe(OnMentalBetChanged).AddTo(_disposables);
    }
    
    public void Start()
    {
        // PersonalityLogViewを初期化
        _personalityLogView?.Initialize(_gameProgressService);
        
        // プレイヤーの精神力変化を監視
        _player.MentalPower.Subscribe(_ => UpdateMentalBetDisplay()).AddTo(_disposables);
        
        // 敵の精神力変化を監視
        _enemy.MentalPower.Subscribe(mentalPower => 
        {
            _enemyMentalPowerView.UpdateDisplay(mentalPower, GameConstants.MAX_MENTAL_POWER);
        }).AddTo(_disposables);
        
        // プレイヤーのカード選択を監視して敵のSpriteを更新
        _player.SelectedCard.Subscribe(cardModel => 
        {
            // 手動制御モード中は自動更新をスキップ
            if (_isEnemySpriteManualMode) return;
            if (cardModel != null && cardModel.Data) _enemyView.UpdateSpriteForAttribute(cardModel.Data.Attribute).Forget();
            else _enemyView.ResetToDefaultSprite().Forget();
        }).AddTo(_disposables);
        
        // Viewイベントの設定
        SetupViewEvents();
        
        // PersonalityLogButtonのイベント設定
        _personalityLogButtonView?.OnButtonClicked.Subscribe(_ => _personalityLogView.ShowLog()).AddTo(_disposables);
        
        // 初期表示の更新
        OnPlayStyleSelected(_selectedPlayStyle);
        UpdateMentalBetDisplay();
    }

    public void Dispose()
    {
        // すべてのViewのイベントを解除
        _disposables.Dispose();
    }
}