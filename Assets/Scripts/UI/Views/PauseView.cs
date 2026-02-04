using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// ポーズ画面を表示するViewコンポーネント
/// </summary>
public class PauseView : BaseWindowView, IPauseView
{
    [Header("ポーズ固有UIコンポーネント")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button homeButton;

    public Observable<Unit> OnHomeButtonClicked { get; private set; }
    public Observable<Unit> OnResumeButtonClicked { get; private set; }

    public override void Show()
    {
        Time.timeScale = 0;
        base.Show();
    }

    public override void Hide()
    {
        Time.timeScale = 1;
        base.Hide();
    }

    protected override void Awake()
    {
        closeButton = resumeButton;
        base.Awake();

        OnHomeButtonClicked = homeButton.OnClickAsObservable();
        OnResumeButtonClicked = resumeButton.OnClickAsObservable();
    }
}