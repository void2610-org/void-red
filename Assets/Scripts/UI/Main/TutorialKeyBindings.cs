using R3;

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
        // チュートリアルを進める
        inputActionsProvider.UI.Advance.OnPerformedAsObservable()
            .Subscribe(_ => tutorialView.NotifyAdvance())
            .AddTo(disposables);
    }
}
