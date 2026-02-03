using R3;

/// <summary>
/// ポーズビューの共通インターフェース
/// </summary>
public interface IPauseView
{
    bool IsShowing { get; }
    Observable<Unit> OnResumeButtonClicked { get; }
    Observable<Unit> OnHomeButtonClicked { get; }
    void Show();
    void Hide();
}
