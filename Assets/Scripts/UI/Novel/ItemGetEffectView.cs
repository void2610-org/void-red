using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using R3;
using Void2610.UnityTemplate;

/// <summary>
/// アイテム取得演出を表示するViewクラス
/// 半透明背景、アイテム画像、名前、説明を表示し、クリックで閉じる
/// </summary>
public class ItemGetEffectView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup effectPanelCanvasGroup;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Image itemImageBackground;
    [SerializeField] private Image itemImage;
    [SerializeField] private DeckCardView deckCardView; // カード表示用
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private ParticleSystem particle;
    
    [Header("アニメーション設定")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float itemScaleAnimationDuration = 0.6f;
    [SerializeField] private Vector3 itemImageStartScale = Vector3.zero;
    [SerializeField] private Vector3 itemImageEndScale = Vector3.one;
    [SerializeField] private Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.7f);

    private readonly Subject<Unit> _clickSubject = new();

    public bool IsShowing => effectPanelCanvasGroup.alpha > 0f;
    public void OnClick() => _clickSubject.OnNext(Unit.Default);

    private void Awake()
    {
        effectPanelCanvasGroup.alpha = 0f;
        
        backgroundOverlay.color = backgroundOverlayColor;
        itemImageBackground.color = Color.clear;
        itemImage.transform.localScale = itemImageStartScale;

        // DeckCardViewの初期設定
        deckCardView.transform.localScale = itemImageStartScale;
        deckCardView.gameObject.SetActive(false);

        itemNameText.text = "";
        itemDescriptionText.text = "";
        
        particle.Stop();
        particle.Clear();
    }
    
    /// <summary>
    /// アイテム取得演出を表示
    /// </summary>
    /// <param name="itemGetData">アイテム取得データ</param>
    /// <param name="itemSprite">アイテムの画像</param>
    public async UniTask ShowItemGetEffect(ItemGetData itemGetData, Sprite itemSprite = null)
    {
        // フェードインアニメーション
        await effectPanelCanvasGroup.FadeIn(fadeDuration, Ease.InCubic);
        
        // UI要素を設定（通常のアイテム表示）
        SetupUIElements(itemGetData, itemSprite);
        
        // 演出を開始
        await PlayShowAnimation();
        // ユーザーの入力待ち
        await _clickSubject.FirstAsync();
        // 演出を終了
        await PlayHideAnimation();
        
        particle.Stop();
        particle.Clear();
    }

    /// <summary>
    /// カード取得演出を表示（DeckCardView使用）
    /// </summary>
    /// <param name="itemGetData">カード取得データ</param>
    /// <param name="cardModel">表示するカードモデル</param>
    public async UniTask ShowCardGetEffect(ItemGetData itemGetData, CardModel cardModel)
    {
        // フェードインアニメーション
        await effectPanelCanvasGroup.FadeIn(fadeDuration, Ease.InCubic);
        
        // UI要素を設定（カード表示）
        SetupUIElementsForCard(itemGetData, cardModel);
        
        // 演出を開始
        await PlayShowAnimationForCard();
        // ユーザーの入力待ち
        await _clickSubject.FirstAsync();
        // 演出を終了
        await PlayHideAnimation();
        
        particle.Stop();
        particle.Clear();
    }
    
    /// <summary>
    /// UI要素を設定（通常アイテム用）
    /// </summary>
    private void SetupUIElements(ItemGetData itemGetData, Sprite itemSprite)
    {
        // アイテム名を設定
        itemNameText.text = itemGetData.ItemName;
        
        // アイテム説明を設定
        itemDescriptionText.text = itemGetData.ItemDescription;
        
        // アイテム画像を設定
        itemImage.sprite = itemSprite;
        itemImage.color = itemSprite != null ? Color.white : Color.clear;
        itemImage.transform.localScale = itemImageStartScale;
        
        // カード表示を非表示
        deckCardView.gameObject.SetActive(false);
        
        // アイテム画像を表示
        itemImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// UI要素を設定（カード用）
    /// </summary>
    private void SetupUIElementsForCard(ItemGetData itemGetData, CardModel cardModel)
    {
        // カード名と説明を設定
        itemNameText.text = itemGetData.ItemName;
        itemDescriptionText.text = itemGetData.ItemDescription;
        
        // DeckCardViewでカードを表示
        deckCardView.gameObject.SetActive(true);
        deckCardView.Initialize(cardModel);
        deckCardView.transform.localScale = itemImageStartScale;
        
        // 通常のアイテム画像を非表示
        itemImage.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 表示アニメーションを再生（通常アイテム用）
    /// </summary>
    private async UniTask PlayShowAnimation()
    {
        await UniTask.Delay(350);
        particle.Play();
        await UniTask.Delay(150);
        itemImageBackground.color = Color.clear;
        itemImageBackground.ColorTo(Color.white, 1f, Ease.OutCubic).ToUniTask().Forget();
        
        // アイテム画像のスケールアニメーション
        await itemImage.transform.ScaleTo(itemImageEndScale, itemScaleAnimationDuration, Ease.OutBack);
    }

    /// <summary>
    /// 表示アニメーションを再生（カード用）
    /// </summary>
    private async UniTask PlayShowAnimationForCard()
    {
        await UniTask.Delay(350);
        particle.Play();
        await UniTask.Delay(150);
        itemImageBackground.color = Color.clear;
        itemImageBackground.ColorTo(Color.white, 1f, Ease.OutCubic).ToUniTask().Forget();
        
        // DeckCardViewのスケールアニメーション
        await deckCardView.transform.ScaleTo(itemImageEndScale, itemScaleAnimationDuration, Ease.OutBack);
    }
    
    /// <summary>
    /// 非表示アニメーションを再生
    /// </summary>
    private async UniTask PlayHideAnimation()
    {
        effectPanelCanvasGroup.interactable = false;
        await effectPanelCanvasGroup.FadeOut(fadeDuration, Ease.InCubic);
        effectPanelCanvasGroup.blocksRaycasts = false;
    }
    
    private void OnDestroy()
    {
        _clickSubject?.Dispose();
    }
}