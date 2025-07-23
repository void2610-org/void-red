using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using R3;
using VContainer;
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
    
    [Inject]
    public void Construct(SettingsPresenter settingsPresenter)
    {
        _settingsPresenter = settingsPresenter;
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
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// 設定ボタンがクリックされた時の処理
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        _settingsPresenter.ShowSettings();
    }
}