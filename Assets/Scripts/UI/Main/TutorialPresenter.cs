using System;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// チュートリアル機能の制御を担当するPresenterクラス
/// AllTutorialDataの管理とTutorialViewへの指示を行う
/// </summary>
public class TutorialPresenter : IDisposable
{
    private readonly AllTutorialData _allTutorialData;
    private readonly TutorialView _tutorialView;
    private readonly InputActionsProvider _inputActionsProvider;
    private readonly CompositeDisposable _disposables = new();
    private readonly CardDetailView _cardDetailView;
    private readonly ThemeView _themeView;
    private readonly SimpleTutorialWindowView _simpleTutorialWindowView;

    public TutorialPresenter(AllTutorialData allTutorialData, InputActionsProvider inputActionsProvider)
    {
        _allTutorialData = allTutorialData;
        _inputActionsProvider = inputActionsProvider;
        _cardDetailView = UnityEngine.Object.FindFirstObjectByType<CardDetailView>();
        _tutorialView = UnityEngine.Object.FindFirstObjectByType<TutorialView>();
        _simpleTutorialWindowView = UnityEngine.Object.FindFirstObjectByType<SimpleTutorialWindowView>();
        _themeView = UnityEngine.Object.FindFirstObjectByType<ThemeView>();
    }

    /// <summary>
    /// バトルチュートリアルを4つに分割して実行
    /// Battle1 → カード自動選択 → カード詳細ウィンドウ表示 → Battle2 → ウィンドウ閉じる → Battle3 → Battle4
    /// </summary>
    public async UniTask StartBattleTutorial()
    {
        await StartTutorial("Battle1");

        // テーマのキーワード表示
        _themeView.OnPointerEnter(null);
        await UniTask.Delay(2000);
        _themeView.OnPointerExit(null);
        await UniTask.Delay(500);

        await StartTutorial("Battle2");

        // 手札の最初のカード（インデックス0）を自動選択
        await UniTask.Delay(500);
        // _player.SelectCardAt(0);
        await UniTask.Delay(1000);

        // カード詳細ウィンドウを開く
        // _cardDetailView.ShowCardDetail(_player.SelectedCard.CurrentValue.Data, true);
        SafeNavigationManager.SelectRootForceSelectable().Forget();
        await UniTask.Delay(500);

        // Battle2を表示(簡易ウィンドウで表示)
        await StartTutorial("Battle3");

        // カード詳細ウィンドウを閉じる
        _cardDetailView.Hide();
        await UniTask.Delay(500);

        await StartTutorial("Battle4");
        await StartTutorial("Battle5");
        await StartTutorial("Battle6");
    }

    public async UniTask StartResultTutorial()
    {
        await StartTutorial("BattleResult");
        var b = BaseWindowView.GetTopActiveWindowCloseButton();
        SafeNavigationManager.SetSelectedGameObjectSafe(b);
    }

    /// <summary>
    /// 指定されたIDのチュートリアルを開始
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    private async UniTask StartTutorial(string tutorialId)
    {
        var tutorialData = _allTutorialData.GetTutorialById(tutorialId);

        // キーバインドを設定
        TutorialKeyBindings.Setup(_inputActionsProvider, _tutorialView, _disposables);

        await _tutorialView.Show();

        // すべてのステップを順番に表示
        for (var i = 0; i < tutorialData.StepCount; i++)
        {
            var step = tutorialData.GetStep(i);
            await _tutorialView.ShowStepAndWaitForClick(step);
        }

        await _simpleTutorialWindowView.HideNarration();

        await _tutorialView.Hide();
        await UniTask.Delay(500);

        // キーバインドをクリア
        _disposables.Clear();
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
