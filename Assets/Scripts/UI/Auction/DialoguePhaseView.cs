using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private NarrationView playerNarration;
    [SerializeField] private NarrationView enemyNarration;

    [Header("立ち絵")]
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private Sprite playerPortraitSprite;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private Sprite _enemyPortraitSprite;

    public void HideChoices() => choicesView.Hide();

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    public void Initialize(EnemyData enemyData) => _enemyPortraitSprite = enemyData.DefaultSprite;

    public UniTask HidePlayerDialogueAsync() => playerNarration.HideNarration();
    public UniTask HideEnemyDialogueAsync() => enemyNarration.HideNarration();
    public UniTask ShowResultAsync(string message) => playerNarration.DisplayNarration(message, autoAdvance: false);
    public UniTask HideResultAsync() => playerNarration.HideNarration();

    public async UniTask SetupChoices(List<string> labels)
    {
        // 選択肢表示中はプレイヤー立ち絵
        await portraitView.ChangePortrait(playerPortraitSprite);
        choicesView.Setup(labels);
    }

    public void Show()
    {
        // プレイヤー立ち絵で開始
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
        await playerNarration.DisplayNarration(text, autoAdvance: false);
    }

    public async UniTask ShowEnemyDialogueAsync(string text)
    {
        await portraitView.ChangePortrait(_enemyPortraitSprite);
        await enemyNarration.DisplayNarration(text, autoAdvance: false);
    }

    public async UniTask HideAllAsync()
    {
        HideChoices();
        await UniTask.WhenAll(
            HidePlayerDialogueAsync(),
            HideEnemyDialogueAsync(),
            HideResultAsync()
        );
    }
}
