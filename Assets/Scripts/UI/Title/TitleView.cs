using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Void2610.UnityTemplate;

/// <summary>
/// タイトル画面のView
/// UI要素の参照とイベントの公開を担当
/// </summary>
public class TitleView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button reviewFormButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    // ボタンクリックイベントをObservableとして公開
    public Observable<Unit> StartButtonClicked => startButton.OnClickAsObservable();
    public Observable<Unit> ContinueButtonClicked => continueButton.OnClickAsObservable();
    public Observable<Unit> QuitButtonClicked => quitButton.OnClickAsObservable();
    public Observable<Unit> SettingsButtonClicked => settingsButton.OnClickAsObservable();
    public Observable<Unit> ReviewFormButtonClicked => reviewFormButton.OnClickAsObservable();

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        // 初期化処理が必要な場合はここに記述
    }

    /// <summary>
    /// つづきからボタンの状態を設定（テキストカラーも含む）
    /// </summary>
    public void SetContinueButtonState(bool enabled)
    {
        if (!this) return;
        
        continueButton.interactable = enabled;
        continueButtonText.color = enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
}
