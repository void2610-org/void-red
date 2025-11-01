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
        var itemGetEffectView = UnityEngine.Object.FindAnyObjectByType<ItemGetEffectView>();

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

        // ダイアログ/アイテム取得演出を進める
        inputActionsProvider.Novel.Advance.OnPerformedAsObservable()
            .Subscribe(_ =>
            {
                // アイテム取得演出が表示中ならそちらを優先
                if (itemGetEffectView && itemGetEffectView.IsShowing)
                {
                    itemGetEffectView.OnClick();
                }
                // ウィンドウが開いていない場合はダイアログを進める
                else if (!BaseWindowView.HasActiveWindows)
                {
                    dialogView.OnClick();
                }
            })
            .AddTo(disposables);
    }
}
