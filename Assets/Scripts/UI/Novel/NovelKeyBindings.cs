using R3;

/// <summary>
/// ノベルシーンのキーバインド設定
/// InputSystemアクションを購読してViewやNovelUIPresenterに処理を委譲
/// </summary>
public static class NovelKeyBindings
{
    /// <summary>
    /// キーバインドを設定（NovelUIPresenterから呼び出される）
    /// </summary>
    public static void Setup(
        InputActionsProvider inputActionsProvider,
        NovelPresenter novelPresenter,
        CompositeDisposable disposables)
    {
        var dialogView = UnityEngine.Object.FindAnyObjectByType<DialogView>();

        // オートモードをトグル
        inputActionsProvider.Novel.Auto.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ => dialogView.ToggleAutoMode())
            .AddTo(disposables);

        // スキップ
        inputActionsProvider.Novel.Skip.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ => novelPresenter.RequestSkipAllDialogs())
            .AddTo(disposables);

        // ダイアログを進める
        inputActionsProvider.Novel.Advance.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ => dialogView.OnClick())
            .AddTo(disposables);
    }
}
