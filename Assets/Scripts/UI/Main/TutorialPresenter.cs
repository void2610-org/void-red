using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// チュートリアル機能の制御を担当するPresenterクラス
/// AllTutorialDataの管理とTutorialViewへの指示を行う
/// </summary>
public class TutorialPresenter
{
    private readonly AllTutorialData _allTutorialData;
    private readonly TutorialView _tutorialView;

    public TutorialPresenter(AllTutorialData allTutorialData)
    {
        _allTutorialData = allTutorialData;
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

        // すべてのステップを順番に表示
        for (var i = 0; i < tutorialData.StepCount; i++)
        {
            var step = tutorialData.GetStep(i);
            await _tutorialView.ShowStepAndWaitForClick(step);
        }

        await _tutorialView.Hide();
        await UniTask.Delay(500);
    }
}