using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private DialogueCutInView cutInView;
    [SerializeField] private NarrationView playerNarration;
    [SerializeField] private NarrationView enemyNarration;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private Sprite _enemyPortraitSprite;
    private Sprite _enemyCutInSprite;

    public void HideChoices() => choicesView.Hide();
    public UniTask HidePlayerDialogueAsync() => UniTask.CompletedTask;
    public UniTask HideEnemyDialogueAsync() => UniTask.CompletedTask;
    public UniTask ShowPlayerNarrationAsync(string message) => playerNarration.DisplayNarration(message, autoAdvance: false);
    public UniTask HidePlayerNarrationAsync() => playerNarration.HideNarration();
    public UniTask ShowEnemyNarrationAsync(string message) => enemyNarration.DisplayNarration(message, autoAdvance: false);
    public UniTask HideEnemyNarrationAsync() => enemyNarration.HideNarration();

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    public void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
        _enemyCutInSprite = enemyData.CutInSprite;
    }

    public void SetupChoices(List<string> labels)
    {
        portraitView.SetPortraitImmediate(playerPortraitSprite);
        portraitView.SlideIn();
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
        portraitView.SlideOut();
        await cutInView.PlayPlayerCutInAsync(text);
    }

    public async UniTask ShowEnemyDialogueAsync(string text)
    {
        portraitView.SlideOut();
        await cutInView.PlayCutInAsync(_enemyPortraitSprite, _enemyCutInSprite, text);
    }

    public async UniTask HideAllAsync()
    {
        HideChoices();
        await HidePlayerNarrationAsync();
        await HideEnemyNarrationAsync();
    }
}
