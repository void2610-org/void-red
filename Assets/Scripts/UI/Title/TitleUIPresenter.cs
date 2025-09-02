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
        startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClicked()).AddTo(this);
        continueButton.OnClickAsObservable().Subscribe(_ => OnContinueButtonClicked()).AddTo(this);
        settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClicked()).AddTo(this);
        reviewFormButton.OnClickAsObservable().Subscribe(_ => OnReviewFormButtonClicked()).AddTo(this);
        
        BgmManager.Instance.PlayRandomBGM(BgmType.Title);
    }

    /// <summary>
    /// スタートボタンがクリックされた時の処理（セーブデータリセット）
    /// </summary>
    private void OnStartButtonClicked()
    {
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