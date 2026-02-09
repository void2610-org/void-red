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
    /// 指定されたIDのチュートリアルを開始
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    public async UniTask StartTutorial(string tutorialId, params string[] args)
    {
        var tutorialData = _allTutorialData.GetTutorialById(tutorialId);

        // キーバインドを設定
        TutorialKeyBindings.Setup(_inputActionsProvider, _tutorialView, _disposables);

        await _tutorialView.Show();

        // すべてのステップを順番に表示
        for (var i = 0; i < tutorialData.StepCount; i++)
        {
            var step = tutorialData.GetStep(i);
            var message = args.Length > 0 ? string.Format(step.Message, args) : step.Message;
            await _tutorialView.ShowStepAndWaitForClick(step, message);
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
