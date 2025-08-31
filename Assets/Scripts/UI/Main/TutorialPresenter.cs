using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// チュートリアル機能の制御を担当するPresenterクラス
/// TutorialDataの管理とTutorialViewへの指示を行う
/// </summary>
public class TutorialPresenter
{
    private readonly TutorialData _tutorialData;
    private readonly TutorialView _tutorialView;
    
    public TutorialPresenter(TutorialData tutorialData)
    {
        _tutorialData = tutorialData;
        _tutorialView = UnityEngine.Object.FindFirstObjectByType<TutorialView>();
    }
    
    /// <summary>
    /// チュートリアルを開始
    /// </summary>
    public async UniTask StartTutorial()
    {
        // すべてのステップを順番に表示
        for (var i = 0; i < _tutorialData.StepCount; i++)
        {
            var step = _tutorialData.GetStep(i);
            await _tutorialView.ShowStepAndWaitForClick(step);
        }
        
        await _tutorialView.Hide();
        await UniTask.Delay(500);
    }
}