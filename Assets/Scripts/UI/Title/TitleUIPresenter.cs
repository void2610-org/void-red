using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using VContainer;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// タイトル画面のUI管理を担当するプレゼンター
/// </summary>
public class TitleUIPresenter : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button reviewFormButton;
    
    [SerializeField] private TMPro.TextMeshProUGUI continueButtonText;
    
    private SettingsPresenter _settingsPresenter;
    private SceneTransitionManager _sceneTransitionManager;
    private GameProgressService _gameProgressService;
    private ConfirmationDialogService _confirmationDialogService;
    private SteamService _steamService;
    
    [Inject]
    public void Construct(SettingsPresenter settingsPresenter, SceneTransitionManager sceneTransitionManager, GameProgressService gameProgressService, ConfirmationDialogService confirmationDialogService, SteamService steamService)
    {
        _settingsPresenter = settingsPresenter;
        _sceneTransitionManager = sceneTransitionManager;
        _gameProgressService = gameProgressService;
        _confirmationDialogService = confirmationDialogService;
        _steamService = steamService;
    }

    private void Start()
    {
        startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClicked().Forget()).AddTo(this);
        continueButton.OnClickAsObservable().Subscribe(_ => OnContinueButtonClicked()).AddTo(this);
        quitButton.OnClickAsObservable().Subscribe(_ => OnQuitButtonClicked()).AddTo(this);
        settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClicked()).AddTo(this);
        reviewFormButton.OnClickAsObservable().Subscribe(_ => OnReviewFormButtonClicked()).AddTo(this);
        
        // セーブデータの有無によるボタン状態管理
        var hasSaveData = _gameProgressService.HasSaveData();
        SetContinueButtonState(hasSaveData);
        
        _gameProgressService.OnDataSaved
            .Select(_ => _gameProgressService.HasSaveData())
            .Subscribe(SetContinueButtonState)
            .AddTo(this);
        
        BgmManager.Instance.PlayRandomBGM(BgmType.Title);
        
        _steamService.UnlockAchievement(SteamAchieveType.FIRST_BOOT);
        
        SelectFirstButton().Forget();
    }

    private async UniTask SelectFirstButton()
    {
        await UniTask.Yield();
        SafeNavigationManager.SelectRootForceSelectable();
    }

    /// <summary>
    /// スタートボタンがクリックされた時の処理（セーブデータリセット）
    /// </summary>
    private async UniTask OnStartButtonClicked()
    {
        // セーブデータが存在する場合は確認ダイアログを表示
        if (_gameProgressService.HasSaveData())
        {
            var confirmed = await _confirmationDialogService.ShowDialog(
                "既存のセーブデータが削除されます。よろしいですか？",
                "はい", 
                "いいえ"
            );
            
            if (!confirmed) return;
        }
        
        _gameProgressService.ResetToDefaultData();
        _steamService.AddStat(SteamStatType.START_GAME_COUNT, 1);
        
        // 新規開始時は次のノードに直接遷移
        var nextScene = _gameProgressService.GetNextSceneType();
        _sceneTransitionManager.TransitionToSceneWithFade(nextScene).Forget();
        BgmManager.Instance.Stop().Forget();
    }
    
    private void OnContinueButtonClicked()
    {
        // 続きから開始時は一旦ホームに遷移
        _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget();
        BgmManager.Instance.Stop().Forget();
    }
    
    private void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
        // エディタ上では再生停止
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    private void OnSettingsButtonClicked()
    {
        _settingsPresenter.ShowSettings();
    }
    
    private void OnReviewFormButtonClicked()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfNMqCyXFzWijWAv__wTpDVRN6AtEfFXpdPxyFcIkMbiq2UKw/viewform");
    }
    
    /// <summary>
    /// つづきからボタンの状態を設定（テキストカラーも含む）
    /// </summary>
    private void SetContinueButtonState(bool e)
    {
        continueButton.interactable = e;
        continueButtonText.color = e ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
}