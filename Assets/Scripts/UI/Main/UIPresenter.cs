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
    private readonly GameOverView _gameOverView;
    private readonly ConfirmationDialogView _confirmationDialogView;
    private readonly EnemyView _enemyView;
    private PlayStyle _selectedPlayStyle = PlayStyle.Hesitation;
    private int _mentalBetValue = 1;
    private readonly CompositeDisposable _disposables = new ();
    private readonly Player _player;

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
    
    public UIPresenter(Player player)
    {
        _player = player;
        
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
        _gameOverView = UnityEngine.Object.FindFirstObjectByType<GameOverView>();
        _confirmationDialogView = UnityEngine.Object.FindFirstObjectByType<ConfirmationDialogView>();
        _enemyView = UnityEngine.Object.FindFirstObjectByType<EnemyView>();
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
        _mentalBetView.UpdateDisplay(_mentalBetValue, currentMentalPower, GameConstants.MAX_MENTAL_POWER, GameConstants.MIN_MENTAL_BET, GameConstants.MAX_MENTAL_BET);
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
        // プレイヤーの精神力変化を監視
        _player.MentalPower.Subscribe(_ => UpdateMentalBetDisplay()).AddTo(_disposables);
        
        // プレイヤーのカード選択を監視して敵のSpriteを更新
        _player.SelectedCard.Subscribe(cardData => 
        {
            if (cardData)
            {
                _enemyView.UpdateSpriteForAttribute(cardData.Attribute).Forget();
            }
        }).AddTo(_disposables);
        
        // Viewイベントの設定
        SetupViewEvents();
        
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