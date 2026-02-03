using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using Void2610.UnityTemplate;

public class DialoguePhaseView : MonoBehaviour
{
    [SerializeField] private DialogueChoicesView choicesView;
    [SerializeField] private NarrationView playerNarration;
    [SerializeField] private NarrationView enemyNarration;

    [Header("立ち絵")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite playerPortraitSprite;

    [Header("プレイヤー立ち絵アニメーション")]
    [SerializeField] private RectTransform playerPortrait;
    [SerializeField] private float portraitHiddenX = 600f;
    [SerializeField] private float portraitShownX = 300f;
    [SerializeField] private float slideDuration = 0.3f;

    public Observable<int> OnChoiceSelected => choicesView.OnChoiceSelected;

    private MotionHandle _slideHandle;
    private Sprite _enemyPortraitSprite;

    public void SetupChoices(List<string> labels)
    {
        // 選択肢表示中はプレイヤー立ち絵
        portraitImage.sprite = playerPortraitSprite;
        choicesView.Setup(labels);
    }

    public void HideChoices() => choicesView.Hide();

    public void Show()
    {
        // プレイヤー立ち絵で開始
        portraitImage.sprite = playerPortraitSprite;
        gameObject.SetActive(true);
        SlideInPortrait();
    }

    public void Hide()
    {
        SlideOutPortrait();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 敵データで初期化する
    /// </summary>
    public void Initialize(EnemyData enemyData)
    {
        _enemyPortraitSprite = enemyData.DefaultSprite;
    }

    public UniTask ShowPlayerDialogueAsync(string text)
    {
        portraitImage.sprite = playerPortraitSprite;
        return playerNarration.DisplayNarration(text, autoAdvance: false);
    }
    public UniTask HidePlayerDialogueAsync() => playerNarration.HideNarration();

    public UniTask ShowEnemyDialogueAsync(string text)
    {
        portraitImage.sprite = _enemyPortraitSprite;
        return enemyNarration.DisplayNarration(text, autoAdvance: false);
    }
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
