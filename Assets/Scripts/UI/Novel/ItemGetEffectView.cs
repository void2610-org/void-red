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
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button clickAreaButton;
    [SerializeField] private ParticleSystem particle;
    
    [Header("アニメーション設定")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float itemScaleAnimationDuration = 0.6f;
    [SerializeField] private Vector3 itemImageStartScale = Vector3.zero;
    [SerializeField] private Vector3 itemImageEndScale = Vector3.one;
    [SerializeField] private Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.7f);
    
    private bool _isWaitingForClick;
    
    private void Awake()
    {
        // EffectPanelの初期状態を設定
        effectPanelCanvasGroup.alpha = 0f;
        effectPanelCanvasGroup.interactable = false;
        effectPanelCanvasGroup.blocksRaycasts = false;
        
        backgroundOverlay.color = backgroundOverlayColor;
        itemImageBackground.color = Color.clear;
        itemImage.transform.localScale = itemImageStartScale;

        itemNameText.text = "";
        itemDescriptionText.text = "";
        
        particle.Stop();
        particle.Clear();
        
        // クリックイベントを購読
        clickAreaButton.OnClickAsObservable().Subscribe(_ => OnClick()).AddTo(this);
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
        
        // UI要素を設定
        SetupUIElements(itemGetData, itemSprite);
        
        // 演出を開始
        await PlayShowAnimation();
        // ユーザーの入力待ち
        await WaitForUserInput();
        // 演出を終了
        await PlayHideAnimation();
        
        particle.Stop();
        particle.Clear();
    }
    
    /// <summary>
    /// UI要素を設定
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
    }
    
    /// <summary>
    /// 表示アニメーションを再生
    /// </summary>
    private async UniTask PlayShowAnimation()
    {
        effectPanelCanvasGroup.interactable = false;
        effectPanelCanvasGroup.blocksRaycasts = true;
        
        await UniTask.Delay(350);
        particle.Play();
        await UniTask.Delay(150);
        itemImageBackground.color = Color.clear;
        itemImageBackground.ColorTo(Color.white, 1f, Ease.OutCubic);
        
        // アイテム画像のスケールアニメーション
        await itemImage.transform.ScaleTo(itemImageEndScale, itemScaleAnimationDuration, Ease.OutBack);
        
        // クリック可能にする
        effectPanelCanvasGroup.interactable = true;
    }
    
    /// <summary>
    /// ユーザーの入力を待つ
    /// </summary>
    private async UniTask WaitForUserInput()
    {
        _isWaitingForClick = true;
        
        // クリックされるまで待機
        await UniTask.WaitUntil(() => !_isWaitingForClick);
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
    
    /// <summary>
    /// クリック処理
    /// </summary>
    private void OnClick()
    {
        if (!effectPanelCanvasGroup.interactable || !_isWaitingForClick) return;
        
        // 入力待ちを終了
        _isWaitingForClick = false;
    }
}