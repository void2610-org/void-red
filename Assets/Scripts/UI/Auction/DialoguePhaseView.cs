using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private NarrationView playerNarration;
    [SerializeField] private NarrationView enemyNarration;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    public void SetupChoices(List<string> labels) => choicesView.Setup(labels);
    public void HideChoices() => choicesView.Hide();
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public UniTask ShowPlayerDialogueAsync(string text) => playerNarration.DisplayNarration(text, autoAdvance: false);
    public UniTask HidePlayerDialogueAsync() => playerNarration.HideNarration();
    public UniTask ShowEnemyDialogueAsync(string text) => enemyNarration.DisplayNarration(text, autoAdvance: false);
    public UniTask HideEnemyDialogueAsync() => enemyNarration.HideNarration();
    public UniTask ShowResultAsync(string message) => playerNarration.DisplayNarration(message, autoAdvance: false);
    public UniTask HideResultAsync() => playerNarration.HideNarration();

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
