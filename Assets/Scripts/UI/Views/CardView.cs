using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Cysharp.Threading.Tasks;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// カードの表示と基本的なロジックを担当するViewクラス
/// 元のCard.csをベースに選択機能とアニメーション機能を追加した簡略化されたMVPパターン
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image cardNameBanner;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Image cardBackImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private Button cardButton;
    [SerializeField] private UIEffect uiEffect;
    
    public CardData CardData { get; private set; }
    public Observable<CardView> OnClicked => _onClicked;
    
    private readonly Subject<CardView> _onClicked = new();
    private Vector2 _originalPosition;
    private RectTransform _rectTransform;
    private bool _isBackside = false;
    
    public void SetInteractable(bool interactable) => cardButton.interactable = interactable;
    public void UpdateOriginalPosition(Vector2 position) => _originalPosition = position;

    public void SetToBackside(Sprite cardBackSprite)
    {
        _isBackside = true;
        cardBackImage.sprite = cardBackSprite;
        cardFrame.color = Color.clear;
        cardNameBanner.color = Color.clear;
        cardImage.color = Color.clear;
        cardNameText.text = string.Empty;
    }

    /// <summary>
    /// カードデータを設定して初期化
    /// </summary>
    public void Initialize(CardData cardData)
    {
        CardData = cardData;
        _originalPosition = _rectTransform.anchoredPosition;
        UpdateDisplay();
        cardButton.onClick.AddListener(OnCardClicked);
    }
    
    /// <summary>
    /// デッキからドローされるアニメーション
    /// </summary>
    public async UniTask PlayDrawAnimation(Vector2 startPosition)
    {
        // 開始位置を設定
        _rectTransform.anchoredPosition = startPosition;
        _rectTransform.localScale = Vector3.one * 0.1f;
        
        // 移動とスケールのアニメーション
        var moveTask = _rectTransform.MoveToAnchored(_originalPosition, 0.5f, Ease.OutBack);
        var scaleTask = _rectTransform.ScaleTo(Vector3.one, 0.5f, Ease.OutBack);
        
        await UniTask.WhenAll(moveTask.ToUniTask(), scaleTask.ToUniTask());
    }
    
    /// <summary>
    /// 削除アニメーション
    /// </summary>
    public async UniTask PlayRemoveAnimation(bool isCollapse = false)
    {
        if (isCollapse)
        {
            // 崩壊アニメーション：ランダムな方向に飛び散りながら回転
            var randomDirection = new Vector2(
                UnityEngine.Random.Range(-200f, 200f),
                UnityEngine.Random.Range(100f, 300f)
            );
            var currentPos = _rectTransform.anchoredPosition;
            var targetPos = currentPos + randomDirection;

            var moveTask = _rectTransform.MoveToAnchored(targetPos, 0.5f, Ease.OutCubic);
            var targetRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-360f, 360f));
            var rotateTask = _rectTransform.RotateTo(targetRotation, 0.5f, Ease.OutCubic);
            var scaleTask = _rectTransform.ScaleTo(Vector3.zero, 0.5f, Ease.InCubic);
                
            await UniTask.WhenAll(moveTask.ToUniTask(), rotateTask.ToUniTask(), scaleTask.ToUniTask());
        }
        else
        {
            // 通常の削除アニメーション：上に移動しながらスケール縮小
            var currentPos = _rectTransform.anchoredPosition;
            var targetPos = new Vector2(currentPos.x, currentPos.y + 100f);

            var moveTask = _rectTransform.MoveToAnchored(targetPos, 0.3f, Ease.InCubic);
            var scaleTask = _rectTransform.ScaleTo(Vector3.one * 0.5f, 0.3f, Ease.InCubic);
                
            await UniTask.WhenAll(moveTask.ToUniTask(), scaleTask.ToUniTask());
        }
    }
    
    /// <summary>
    /// 削除アニメーション後に自己削除
    /// </summary>
    public void PlayRemoveAndDestroy(bool isCollapse = false)
    {
        PlayRemoveAndDestroyAsync(isCollapse).Forget();
    }
    
    private async UniTask PlayRemoveAndDestroyAsync(bool isCollapse)
    {
        await PlayRemoveAnimation(isCollapse);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 配置アニメーション
    /// </summary>
    public async UniTask PlayArrangeAnimation(Vector2 targetPosition, Quaternion targetRotation)
    {
        _originalPosition = targetPosition;
        
        // 位置のアニメーション
        var moveTask = _rectTransform.MoveToAnchored(targetPosition, 0.3f, Ease.OutCubic);
        // 回転のアニメーション
        var rotateTask = _rectTransform.RotateTo(targetRotation, 0.3f, Ease.OutCubic);
        
        await UniTask.WhenAll(moveTask.ToUniTask(), rotateTask.ToUniTask());
    }
    
    /// <summary>
    /// デッキ戻りアニメーション
    /// </summary>
    public async UniTask PlayReturnToDeckAnimation(Vector2 deckPosition)
    {
        // 移動、スケール、回転のアニメーション
        var moveTask = _rectTransform.MoveToAnchored(deckPosition, 0.4f, Ease.InCubic);
        var scaleTask = _rectTransform.ScaleTo(Vector3.one * 0.1f, 0.4f, Ease.InCubic);
        var rotateTask = _rectTransform.RotateTo(Quaternion.identity, 0.4f, Ease.InCubic);
        
        await UniTask.WhenAll(moveTask.ToUniTask(), scaleTask.ToUniTask(), rotateTask.ToUniTask());
    }
    
    /// <summary>
    /// デッキに戻って自己削除するアニメーション
    /// </summary>
    public void PlayReturnToDeckAndDestroy(Vector2 deckPosition, float delay = 0f)
    {
        PlayReturnToDeckAndDestroyAsync(deckPosition, delay).Forget();
    }
    
    private async UniTask PlayReturnToDeckAndDestroyAsync(Vector2 deckPosition, float delay)
    {
        // 遅延がある場合は待機
        if (delay > 0) await UniTask.Delay((int)(delay * 1000));
        
        // アニメーション再生
        await PlayReturnToDeckAnimation(deckPosition);
        
        // 自己削除
        Destroy(gameObject);
    }
    
    /// <summary>
    /// ハイライト表示
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        // 選択時は少し上に移動、非選択時は元の位置に戻る
        var currentPos = _rectTransform.anchoredPosition;
        var targetPos = new Vector2(currentPos.x, highlight ? _originalPosition.y + 30f : _originalPosition.y);
        
        // 位置のアニメーション
        _rectTransform.MoveToAnchored(targetPos, 0.2f, Ease.OutCubic);
        
        if (highlight) SeManager.Instance.PlaySe("CardSelect");
    }
    
    /// <summary>
    /// 崩壊確率とテーマ合致度に基づいて崩壊エフェクトのTransitionRateを更新
    /// </summary>
    /// <param name="collapseChance">崩壊確率（0.0～1.0）</param>
    /// <param name="themeMatchRate">テーマ合致率（0.0～1.0）</param>
    public void UpdateCollapseVisual(float collapseChance, float themeMatchRate)
    {
        if (!CardData || _isBackside) return;
        
        // 崩壊確率に基づいてTransitionRateを設定
        collapseChance = Mathf.Clamp01(collapseChance * 1.75f);
        uiEffect.transitionRate = collapseChance * 0.5f;
    }
    
    /// <summary>
    /// 崩壊エフェクトをリセット（選択解除時用）
    /// </summary>
    public void ResetCollapseVisual()
    {
        if (!CardData || _isBackside) return;
        uiEffect.transitionRate = 0f;
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        if (!CardData) return;
        if (_isBackside) return;
        
        // カード名を設定
        cardNameText.text = CardData.CardName;
        cardNameText.color = CardData.IsTextColorBlack ? Color.black : Color.white;
        
        // カード画像を設定
        if (CardData.CardImage)
            cardImage.sprite = CardData.CardImage;
        else
            cardImage.color = Color.clear;
        
        // バナーとフレームの色をCardDataの色に設定
        cardNameBanner.color = CardData.Color;
        cardFrame.color = CardData.Color;
    }
    
    /// <summary>
    /// カードがクリックされた時の処理
    /// </summary>
    private void OnCardClicked()
    {
        _onClicked.OnNext(this);
    }
    
    private void Awake()
    {
        _rectTransform = this.GetComponent<RectTransform>();
    }
    
    private void OnDestroy()
    {
        _onClicked?.Dispose();
        if (cardButton)
            cardButton.onClick.RemoveListener(OnCardClicked);
    }

}