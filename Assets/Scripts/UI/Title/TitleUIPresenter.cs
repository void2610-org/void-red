using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button reviewFormButton;
    [SerializeField] private ConfirmationDialogView confirmationDialog;
    
    private SettingsPresenter _settingsPresenter;
    private SceneTransitionManager _sceneTransitionManager;
    private GameProgressService _gameProgressService;
    
    [Inject]
    public void Construct(SettingsPresenter settingsPresenter, SceneTransitionManager sceneTransitionManager, GameProgressService gameProgressService)
    {
        _settingsPresenter = settingsPresenter;
        _sceneTransitionManager = sceneTransitionManager;
        _gameProgressService = gameProgressService;
    }

    private void Start()
    {
        startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClicked().Forget()).AddTo(this);
        continueButton.OnClickAsObservable().Subscribe(_ => OnContinueButtonClicked()).AddTo(this);
        settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClicked()).AddTo(this);
        reviewFormButton.OnClickAsObservable().Subscribe(_ => OnReviewFormButtonClicked()).AddTo(this);
        
        // セーブデータの有無によるボタン状態管理
        continueButton.interactable = _gameProgressService.HasSaveData();
        _gameProgressService.OnDataSaved
            .Select(_ => _gameProgressService.HasSaveData())
            .Subscribe(b => continueButton.interactable = b)
            .AddTo(this);
        
        BgmManager.Instance.PlayRandomBGM(BgmType.Title);
    }

    /// <summary>
    /// スタートボタンがクリックされた時の処理（セーブデータリセット）
    /// </summary>
    private async UniTask OnStartButtonClicked()
    {
        // セーブデータが存在する場合は確認ダイアログを表示
        if (_gameProgressService.HasSaveData())
        {
            var confirmed = await confirmationDialog.ShowDialog(
                "既存のセーブデータが削除されます。よろしいですか？",
                "はい", 
                "いいえ"
            );
            
            if (!confirmed) return;
        }
        
        _gameProgressService.ResetToDefaultData();
        _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget();
    }
    
    /// <summary>
    /// 続きからボタンがクリックされた時の処理（既存データで続行）
    /// </summary>
    private void OnContinueButtonClicked()
    {
        _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home).Forget();
    }

    /// <summary>
    /// 設定ボタンがクリックされた時の処理
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        _settingsPresenter.ShowSettings();
    }
    
    /// <summary>
    /// 感想フォームボタンがクリックされた時の処理
    /// </summary>
    private void OnReviewFormButtonClicked()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfNMqCyXFzWijWAv__wTpDVRN6AtEfFXpdPxyFcIkMbiq2UKw/viewform");
    }
}