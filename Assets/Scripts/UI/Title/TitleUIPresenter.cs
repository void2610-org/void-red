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
    [SerializeField] private Button settingsButton;
    
    private SettingsPresenter _settingsPresenter;
    private SceneTransitionManager _sceneTransitionManager;
    
    [Inject]
    public void Construct(SettingsPresenter settingsPresenter, SceneTransitionManager sceneTransitionManager)
    {
        _settingsPresenter = settingsPresenter;
        _sceneTransitionManager = sceneTransitionManager;
    }

    private void Start()
    {
        startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClicked()).AddTo(this);
        settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClicked()).AddTo(this);
        
        BgmManager.Instance.PlayRandomBGM(BgmType.Title);
    }

    /// <summary>
    /// スタートボタンがクリックされた時の処理
    /// </summary>
    private void OnStartButtonClicked()
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
}