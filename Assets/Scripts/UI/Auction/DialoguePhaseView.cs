using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using LitMotion;
using Void2610.UnityTemplate;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private NarrationView playerNarration;
    [SerializeField] private NarrationView enemyNarration;

    [Header("プレイヤー立ち絵")]
    [SerializeField] private RectTransform playerPortrait;
    [SerializeField] private float portraitHiddenX = 600f;
    [SerializeField] private float portraitShownX = 300f;
    [SerializeField] private float slideDuration = 0.3f;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private MotionHandle _slideHandle;

    public void SetupChoices(List<string> labels)
    {
        choicesView.Setup(labels);
        SlideInPortrait();
    }

    public void HideChoices()
    {
        choicesView.Hide();
        SlideOutPortrait();
    }
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

    private void SlideInPortrait()
    {
        _slideHandle.TryCancel();
        playerPortrait.anchoredPosition = new Vector2(portraitHiddenX, playerPortrait.anchoredPosition.y);
        _slideHandle = playerPortrait.MoveToX(portraitShownX, slideDuration, Ease.OutQuad);
    }

    private void SlideOutPortrait()
    {
        _slideHandle.TryCancel();
        _slideHandle = playerPortrait.MoveToX(portraitHiddenX, slideDuration, Ease.InQuad);
    }

    private void OnDestroy() => _slideHandle.TryCancel();
}
