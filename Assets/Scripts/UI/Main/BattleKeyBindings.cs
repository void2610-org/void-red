using R3;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// バトルシーンのキーバインド設定
/// InputSystemアクションを購読してViewやUIPresenterに処理を委譲
/// </summary>
public static class BattleKeyBindings
{
    /// <summary>
    /// キーバインドを設定（UIPresenterから呼び出される）
    /// </summary>
    public static void Setup(
        InputActionsProvider inputActionsProvider,
        BattleUIPresenter battleUIPresenter,
        ThemeView themeView,
        ReadOnlyReactiveProperty<GameState> currentGameState,
        CompositeDisposable disposables)
    {
        // テーマのキーワードをトグル表示
        // inputActionsProvider.Battle.FocusOnTheme.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => themeView.ToggleKeywords())
        //     .AddTo(disposables);
        
        // 人格ログを開く
        // inputActionsProvider.Battle.OpenPersonalityLog.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => personalityLogView.Show())
        //     .AddTo(disposables);

        // プレイスタイルを切り替え
        // inputActionsProvider.Battle.ChangePlayStyle.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => playStyleView.RotateWheel())
        //     .AddTo(disposables);

        // 精神ベットを減らす
        // inputActionsProvider.Battle.MinusMentalBet.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.DecrementMentalBet())
        //     .AddTo(disposables);

        // 精神ベットを増やす
        // inputActionsProvider.Battle.PlusMentalBet.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.IncrementMentalBet())
        //     .AddTo(disposables);

        // カード詳細を表示
        // inputActionsProvider.Battle.ShowCardDetail.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.ShowSelectedCardDetail())
        //     .AddTo(disposables);

        // カードをプレイ
        // inputActionsProvider.Battle.PlayCard.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.TryPlayCard())
        //     .AddTo(disposables);

        // カードナビゲーション
        // inputActionsProvider.Battle.SelectNextCard.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.NavigateToNextCard())
        //     .AddTo(disposables);
        // inputActionsProvider.Battle.SelectPrevCard.OnPerformedAsObservable()
        //     .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
        //     .Where(_ => !BaseWindowView.HasActiveWindows)
        //     .Subscribe(_ => battleUIPresenter.NavigateToPrevCard())
        //     .AddTo(disposables);
        
        // ナレーションをスキップする（非表示のオブジェクトも含めて検索）
        var narrationViews = Object.FindObjectsByType<NarrationView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        inputActionsProvider.UI.Advance.OnPerformedAsObservable()
            .Where(_ => !BaseWindowView.HasActiveWindows)
            .Subscribe(_ =>
            {
                foreach (var view in narrationViews)
                    view.OnClick();
            })
            .AddTo(disposables);
    }
}
