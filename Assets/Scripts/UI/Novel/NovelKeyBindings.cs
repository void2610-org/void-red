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
        NovelUIPresenter novelUIPresenter,
        CompositeDisposable disposables)
    {
        var dialogView = UnityEngine.Object.FindAnyObjectByType<DialogView>();

        // オートモードをトグル（clickAreaButton選択時のみ）
        inputActionsProvider.Novel.Auto.OnPerformedAsObservable()
            .Where(_ => dialogView.IsClickAreaButtonSelected)
            .Subscribe(_ => dialogView.ToggleAutoMode())
            .AddTo(disposables);

        // スキップ（clickAreaButton選択時のみ）
        inputActionsProvider.Novel.Skip.OnPerformedAsObservable()
            .Where(_ => dialogView.IsClickAreaButtonSelected)
            .Subscribe(_ => novelUIPresenter.RequestSkipAllDialogs())
            .AddTo(disposables);

        // ダイアログを進める（clickAreaButton選択時のみ）
        inputActionsProvider.Novel.Advance.OnPerformedAsObservable()
            .Where(_ => dialogView.IsClickAreaButtonSelected)
            .Subscribe(_ => dialogView.OnClick())
            .AddTo(disposables);
    }
}
