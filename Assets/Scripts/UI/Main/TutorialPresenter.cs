using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
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

    public TutorialPresenter(AllTutorialData allTutorialData, InputActionsProvider inputActionsProvider)
    {
        _allTutorialData = allTutorialData;
        _inputActionsProvider = inputActionsProvider;
        _allTutorialData.RegisterAllTutorials();
        _tutorialView = UnityEngine.Object.FindFirstObjectByType<TutorialView>();
    }

    /// <summary>
    /// 指定されたIDのチュートリアルを開始
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    public async UniTask StartTutorial(string tutorialId)
    {
        var tutorialData = _allTutorialData.GetTutorialById(tutorialId);
        var isBattleTutorial = tutorialId == "Battle";

        // キーバインドを設定
        TutorialKeyBindings.Setup(_inputActionsProvider, _tutorialView, _disposables);
        
        await _tutorialView.Show();

        // すべてのステップを順番に表示
        for (var i = 0; i < tutorialData.StepCount; i++)
        {
            var step = tutorialData.GetStep(i);
            await _tutorialView.ShowStepAndWaitForClick(step, isBattleTutorial);
        }

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