using R3;
using Void2610.UnityTemplate;

/// <summary>
/// チュートリアルのキーバインド設定
/// InputSystemアクションを購読してTutorialViewに処理を委譲
/// </summary>
public static class TutorialKeyBindings
{
    /// <summary>
    /// キーバインドを設定（TutorialPresenterから呼び出される）
    /// </summary>
    public static void Setup(
        InputActionsProvider inputActionsProvider,
        TutorialView tutorialView,
        CompositeDisposable disposables)
    {
        // チュートリアルを進める（clickAreaButton選択時のみ）
        inputActionsProvider.UI.Submit.OnPerformedAsObservable()
            // .Where(_ => tutorialView.IsClickAreaButtonSelected)
            .Subscribe(_ => tutorialView.NotifyAdvance())
            .AddTo(disposables);
    }
}