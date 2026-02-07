using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private DialogueCutInView cutInView;
    [SerializeField] private NarrationView resultNarration;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private Sprite _enemyPortraitSprite;
    private Sprite _enemyCutInSprite;

    public void HideChoices() => choicesView.Hide();
    public UniTask HidePlayerDialogueAsync() => UniTask.CompletedTask;
    public UniTask HideEnemyDialogueAsync() => UniTask.CompletedTask;
    public UniTask ShowResultAsync(string message) => resultNarration.DisplayNarration(message, autoAdvance: false);
    public UniTask HideResultAsync() => resultNarration.HideNarration();

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    public void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
        _enemyCutInSprite = enemyData.CutInSprite;
    }

    public async UniTask SetupChoices(List<string> labels)
    {
        await portraitView.ChangePortrait(playerPortraitSprite);
        choicesView.Setup(labels);
    }

    public void Show()
    {
        portraitView.SetPortraitImmediate(playerPortraitSprite);
        gameObject.SetActive(true);
        portraitView.SlideIn();
    }

    public void Hide()
    {
        portraitView.SlideOut();
        gameObject.SetActive(false);
    }

    public async UniTask ShowPlayerDialogueAsync(string text)
    {
        await portraitView.ChangePortrait(playerPortraitSprite);
        await cutInView.PlayPlayerCutInAsync(text);
    }

    public async UniTask ShowEnemyDialogueAsync(string text)
    {
        await portraitView.ChangePortrait(_enemyPortraitSprite);
        await cutInView.PlayCutInAsync(_enemyPortraitSprite, _enemyCutInSprite, text);
    }

    public async UniTask HideAllAsync()
    {
        HideChoices();
        await HideResultAsync();
    }
}
