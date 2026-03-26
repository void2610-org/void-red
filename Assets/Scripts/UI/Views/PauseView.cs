using R3;
using UnityEngine;
using UnityEngine.UI;

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

#if UNITY_EDITOR
    private float _savedTimeScale = 1f;
#endif

    public override void Show()
    {
#if UNITY_EDITOR
        _savedTimeScale = Time.timeScale;
#endif
        Time.timeScale = 0;
        base.Show();
    }

    public override void Hide()
    {
#if UNITY_EDITOR
        // ポーズ中にタイムスケールが外部から変更されていた場合はその値を維持
        if (Time.timeScale == 0)
            Time.timeScale = _savedTimeScale;
#else
        Time.timeScale = 1;
#endif
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
