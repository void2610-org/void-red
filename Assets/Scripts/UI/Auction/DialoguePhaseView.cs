using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialoguePhaseView : BasePhaseView
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private DialogueCutInView cutInView;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    private Sprite _enemyPortraitSprite;
    private Sprite _enemyCutInSprite;

    /// <summary>
    /// オークション中にカード対話を1本だけ再生する
    /// </summary>
    /// <param name="card">対話対象のカード</param>
    /// <param name="enemyData">対話演出に使う敵データ</param>
    /// <param name="forcedChoiceIndex">強制する対話選択肢</param>
    public async UniTask ShowCardDialogueAsync(CardModel card, EnemyData enemyData, int? forcedChoiceIndex = null)
    {
        Initialize(enemyData);
        Show();
        choicesView.Show();
        
        if (forcedChoiceIndex.HasValue) choicesView.SetOnlyAllowed(forcedChoiceIndex.Value);

        try
        {
            _ = await choicesView.WaitForSelectionAsync();
            choicesView.Hide();
            
            await ShowPlayerDialogueAsync("あなたにこの記憶は必要ないのでは？");
            await ShowEnemyDialogueAsync("あくまで、分かりやすい状況を作るためですよ。");
        }
        catch (OperationCanceledException) { }
        finally
        {
            Hide();
        }
    }

    public override void Show()
    {
        portraitView.SetPortraitImmediate(playerPortraitSprite);
        base.Show();
        portraitView.SlideIn();
    }

    public override void Hide()
    {
        portraitView.SlideOut();
        base.Hide();
    }

    private void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
        _enemyCutInSprite = enemyData.CutInSprite;
    }

    private async UniTask ShowPlayerDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayPlayerCutInAsync(text);
    }

    private async UniTask ShowEnemyDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayCutInAsync(_enemyPortraitSprite, _enemyCutInSprite, text);
    }
}
