using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// アイテム取得演出を表示するViewクラス
/// 半透明背景、アイテム画像、名前、説明を表示し、クリックで閉じる
/// </summary>
public class ItemGetEffectView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup effectPanelCanvasGroup;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button clickAreaButton;
    [SerializeField] private ParticleSystem particle;
    
    [Header("アニメーション設定")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float itemScaleAnimationDuration = 0.6f;
    [SerializeField] private Vector3 itemImageStartScale = Vector3.zero;
    [SerializeField] private Vector3 itemImageEndScale = Vector3.one;
    [SerializeField] private Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.7f);
    
    private MotionHandle _fadeMotion;
    private MotionHandle _scaleMotion;
    private bool _isWaitingForClick;
    
    private void Awake()
    {
        // EffectPanelの初期状態を設定
        effectPanelCanvasGroup.alpha = 0f;
        effectPanelCanvasGroup.interactable = false;
        effectPanelCanvasGroup.blocksRaycasts = false;
        
        // 背景オーバーレイの色を設定
        backgroundOverlay.color = backgroundOverlayColor;
        
        // アイテム画像の初期スケールを設定
        itemImage.transform.localScale = itemImageStartScale;
        
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
        // UI要素を設定
        SetupUIElements(itemGetData, itemSprite);
        particle.Clear();
        particle.Play();
        
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
        
        // フェードインアニメーション
        CancelActiveMotions();
        
        _fadeMotion = LMotion.Create(0f, 1f, fadeInDuration)
            .WithEase(Ease.OutCubic)
            .Bind(alpha => effectPanelCanvasGroup.alpha = alpha)
            .AddTo(this);
        
        // アイテム画像のスケールアニメーション
        _scaleMotion = LMotion.Create(itemImageStartScale, itemImageEndScale, itemScaleAnimationDuration)
            .WithEase(Ease.OutBack) // バックイーズで少し弾むような演出
            .WithDelay(0.2f) // フェードインの後に開始
            .Bind(scale => itemImage.transform.localScale = scale)
            .AddTo(this);
        
        // アニメーション完了まで待機
        await UniTask.WhenAll(_fadeMotion.ToUniTask(), _scaleMotion.ToUniTask());
        
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
        
        // フェードアウトアニメーション
        CancelActiveMotions();
        
        _fadeMotion = LMotion.Create(1f, 0f, fadeOutDuration)
            .WithEase(Ease.InCubic)
            .Bind(alpha => effectPanelCanvasGroup.alpha = alpha)
            .AddTo(this);
        
        await _fadeMotion.ToUniTask();
        
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
    
    /// <summary>
    /// アクティブなアニメーションを全てキャンセル
    /// </summary>
    private void CancelActiveMotions()
    {
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        if (_scaleMotion.IsActive())
            _scaleMotion.Cancel();
    }
    
    private void OnDestroy()
    {
        CancelActiveMotions();
    }
}