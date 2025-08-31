using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// チュートリアル機能の制御を担当するPresenterクラス
/// TutorialDataの管理とTutorialViewへの指示を行う
/// </summary>
public class TutorialPresenter : IDisposable
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
        if (_tutorialData == null || _tutorialView == null) return;
        
        // すべてのステップを順番に表示
        for (int i = 0; i < _tutorialData.StepCount; i++)
        {
            var step = _tutorialData.GetStep(i);
            if (step != null)
            {
                await _tutorialView.ShowStepAndWaitForClick(step);
            }
        }
        
        await CompleteTutorial();
    }
    
    /// <summary>
    /// チュートリアル完了処理
    /// </summary>
    private async UniTask CompleteTutorial()
    {
        await _tutorialView.Hide();
    }
    
    public void Dispose()
    {
        // 現時点では特にクリーンアップ処理なし
    }
}