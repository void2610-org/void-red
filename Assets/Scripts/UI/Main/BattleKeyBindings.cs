using R3;

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
        UIPresenter uiPresenter,
        ReadOnlyReactiveProperty<GameState> currentGameState,
        BattleRootView battleRootView,
        CompositeDisposable disposables)
    {
        // ViewをシーンからFind
        var personalityLogView = UnityEngine.Object.FindFirstObjectByType<PersonalityLogView>();
        var themeView = UnityEngine.Object.FindFirstObjectByType<ThemeView>();
        var playStyleView = UnityEngine.Object.FindFirstObjectByType<PlayStyleView>();

        // 人格ログを開く
        inputActionsProvider.Battle.OpenPersonalityLog.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => personalityLogView.Show())
            .AddTo(disposables);

        // テーマのキーワードをトグル表示
        inputActionsProvider.Battle.FocusOnTheme.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => themeView.ToggleKeywords())
            .AddTo(disposables);

        // プレイスタイルを切り替え
        inputActionsProvider.Battle.ChangePlayStyle.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => playStyleView.RotateWheel())
            .AddTo(disposables);

        // 精神ベットを減らす
        inputActionsProvider.Battle.MinusMentalBet.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => uiPresenter.DecrementMentalBet())
            .AddTo(disposables);

        // 精神ベットを増やす
        inputActionsProvider.Battle.PlusMentalBet.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => uiPresenter.IncrementMentalBet())
            .AddTo(disposables);

        // カード詳細を表示
        inputActionsProvider.Battle.ShowCardDetail.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => uiPresenter.ShowSelectedCardDetail())
            .AddTo(disposables);

        // カードをプレイ
        inputActionsProvider.Battle.PlayCard.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ => uiPresenter.TryPlayCard())
            .AddTo(disposables);

        // カードナビゲーション
        inputActionsProvider.UI.Navigate.OnPerformedAsObservable()
            .Where(_ => currentGameState.CurrentValue == GameState.PlayerCardSelection)
            .Where(_ => battleRootView.IsRootSelected)
            .Subscribe(_ =>
            {
                var direction = inputActionsProvider.UI.Navigate.ReadValue<UnityEngine.Vector2>();
                if (direction.x > 0.5f)
                {
                    // 右方向：次のカード
                    uiPresenter.NavigateToNextCard();
                }
                else if (direction.x < -0.5f)
                {
                    // 左方向：前のカード
                    uiPresenter.NavigateToPreviousCard();
                }
            })
            .AddTo(disposables);
    }
}
