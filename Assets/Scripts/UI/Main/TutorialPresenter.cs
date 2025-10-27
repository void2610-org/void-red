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
    private readonly Player _player;
    private readonly CardDetailView _cardDetailView;

    public TutorialPresenter(AllTutorialData allTutorialData, InputActionsProvider inputActionsProvider, Player player, CardDetailView cardDetailView)
    {
        _allTutorialData = allTutorialData;
        _inputActionsProvider = inputActionsProvider;
        _player = player;
        _cardDetailView = cardDetailView;
        _allTutorialData.RegisterAllTutorials();
        _tutorialView = UnityEngine.Object.FindFirstObjectByType<TutorialView>();
    }

    /// <summary>
    /// バトルチュートリアルを4つに分割して実行
    /// Battle1 → カード自動選択 → カード詳細ウィンドウ表示 → Battle2 → ウィンドウ閉じる → Battle3 → Battle4
    /// </summary>
    public async UniTask StartBattleTutorial()
    {
        // Battle1を表示
        await StartTutorial("Battle1", true);
        
        await UniTask.Delay(500);

        // 手札の最初のカード（インデックス0）を自動選択
        _player.SelectCardAt(0);
        await UniTask.Delay(500);

        // カード詳細ウィンドウを開く
        _cardDetailView.ShowCardDetail(_player.SelectedCard.CurrentValue.Data, true);
        await UniTask.Delay(500); // ウィンドウ表示のアニメーション待機

        // Battle2を表示(簡易ウィンドウで表示)
        await StartTutorial("Battle2");

        // カード詳細ウィンドウを閉じる
        _cardDetailView.Hide();
        await UniTask.Delay(500);

        // Battle3を表示
        await StartTutorial("Battle3", true);

        // Battle4を表示
        await StartTutorial("Battle4", true);
    }
    
    public async UniTask StartResultTutorial()
    {
        await StartTutorial("Result");
    }

    /// <summary>
    /// 指定されたIDのチュートリアルを開始
    /// </summary>
    /// <param name="tutorialId">チュートリアルID</param>
    /// <param name="isBattleTutorial">バトル画面の吹き出しを使った表示か？</param>
    private async UniTask StartTutorial(string tutorialId, bool isBattleTutorial = false)
    {
        var tutorialData = _allTutorialData.GetTutorialById(tutorialId);

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