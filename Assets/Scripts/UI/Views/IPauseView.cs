using R3;

/// <summary>
/// ポーズビューの共通インターフェース
/// </summary>
public interface IPauseView
{
    public bool IsShowing { get; }
    public Observable<Unit> OnResumeButtonClicked { get; }
    public Observable<Unit> OnHomeButtonClicked { get; }
    public void Show();
    public void Hide();
    public void Toggle();
}
