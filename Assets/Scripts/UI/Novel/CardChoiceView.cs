using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using R3;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// カード風選択肢表示を担当するViewクラス
/// 2つの選択肢ボタンとカード画像を表示し、ユーザーの選択を受け取る
/// </summary>
public class CardChoiceView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup cardChoicePanelCanvasGroup;
    [SerializeField] private Button choice1Button;
    [SerializeField] private TextMeshProUGUI choice1Text;
    [SerializeField] private Image choice1CardImage;
    [SerializeField] private Button choice2Button;
    [SerializeField] private TextMeshProUGUI choice2Text;
    [SerializeField] private Image choice2CardImage;

    private const float FADE_DURATION = 0.35f;
    
    private readonly Subject<int> _choiceSelectedSubject = new();
    private readonly CompositeDisposable _disposables = new();
    private MotionHandle _panelFadeMotion;
    
    /// <summary>
    /// カード風選択肢を表示して選択を待つ
    /// </summary>
    public async UniTask<int> ShowCardChoice(CardChoiceData cardChoiceData, Sprite cardImage1, Sprite cardImage2)
    {
        SetupUIElements(cardChoiceData, cardImage1, cardImage2);
        await ShowPanel();
        SafeNavigationManager.SetSelectedGameObjectSafe(choice1Button.gameObject);
        var selectedIndex = await _choiceSelectedSubject.FirstAsync();
        await HidePanel();
        
        return selectedIndex;
    }
    
    /// <summary>
    /// UI要素を設定
    /// </summary>
    private void SetupUIElements(CardChoiceData cardChoiceData, Sprite cardImage1, Sprite cardImage2)
    {
        choice1Text.text = cardChoiceData.Option1;
        choice2Text.text = cardChoiceData.Option2;
        
        choice1CardImage.sprite = cardImage1;
        choice2CardImage.sprite = cardImage2;
    }
    
    /// <summary>
    /// パネルを表示状態にする
    /// </summary>
    private async UniTask ShowPanel()
    {
        _panelFadeMotion = cardChoicePanelCanvasGroup.FadeIn(FADE_DURATION, Ease.InCubic);
        await _panelFadeMotion.ToUniTask();

        cardChoicePanelCanvasGroup.interactable = true;
        cardChoicePanelCanvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// パネルを非表示状態にする
    /// </summary>
    private async UniTask HidePanel()
    {
        cardChoicePanelCanvasGroup.interactable = false;
        cardChoicePanelCanvasGroup.blocksRaycasts = false;
        
        _panelFadeMotion = cardChoicePanelCanvasGroup.FadeOut(FADE_DURATION, Ease.InCubic);
        await _panelFadeMotion.ToUniTask();
    }
    
    /// <summary>
    /// 選択肢ボタンがクリックされた時の処理
    /// </summary>
    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (!cardChoicePanelCanvasGroup.interactable) 
            return;
        
        _choiceSelectedSubject.OnNext(choiceIndex);
    }
    
    private void OnDestroy()
    {
        _choiceSelectedSubject?.Dispose();
        _disposables?.Dispose();
    }
    
    private void Awake()
    {
        cardChoicePanelCanvasGroup.alpha = 0f;
        cardChoicePanelCanvasGroup.interactable = false;
        cardChoicePanelCanvasGroup.blocksRaycasts = false;
        
        choice1Text.text = "";
        choice2Text.text = "";
        
        // ボタンクリックイベントを購読
        choice1Button.OnClickAsObservable()
            .Subscribe(_ => OnChoiceButtonClicked(0))
            .AddTo(_disposables);
        
        choice2Button.OnClickAsObservable()
            .Subscribe(_ => OnChoiceButtonClicked(1))
            .AddTo(_disposables);
    }
}
