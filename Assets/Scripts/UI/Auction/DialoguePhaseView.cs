using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class DialoguePhaseView : BasePhaseView
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private DialogueCutInView cutInView;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private Sprite _enemyPortraitSprite;
    private Sprite _enemyCutInSprite;

    public void HideChoices() => choicesView.Hide();
    public UniTask HidePlayerDialogueAsync() => UniTask.CompletedTask;
    public UniTask HideEnemyDialogueAsync() => UniTask.CompletedTask;

    public UniTask HideAllAsync()
    {
        HideChoices();
        return UniTask.CompletedTask;
    }

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    /// <param name="enemyData">立ち絵とカットインに使う敵データ</param>
    public void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
        _enemyCutInSprite = enemyData.CutInSprite;
    }

    /// <summary>
    /// オークション中にカード対話を1本だけ再生する
    /// </summary>
    /// <param name="card">対話対象のカード</param>
    /// <param name="enemyData">対話演出に使う敵データ</param>
    public async UniTask ShowCardDialogueAsync(CardModel card, EnemyData enemyData)
    {
        Initialize(enemyData);
        Show();
        await HideAllAsync();
        await ShowEnemyDialogueAsync(card.Data.Description);
        Hide();
    }

    public void SetupChoices(List<string> labels)
    {
        portraitView.SetPortraitImmediate(playerPortraitSprite);
        portraitView.SlideIn();
        choicesView.Setup(labels);
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

    public async UniTask ShowPlayerDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayPlayerCutInAsync(text);
    }

    public async UniTask ShowEnemyDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayCutInAsync(_enemyPortraitSprite, _enemyCutInSprite, text);
    }
}
