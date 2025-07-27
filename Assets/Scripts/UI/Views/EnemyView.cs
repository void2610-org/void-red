using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

/// <summary>
/// 敵の表示を担当するViewクラス
/// カード属性に応じてSpriteを切り替える
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class EnemyView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image enemyImage;
    
    [Header("アニメーション設定")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.2f;
    
    private EnemyData _enemyData;
    private RectTransform _rectTransform;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(EnemyData enemyData)
    {
        _enemyData = enemyData;
        // 初期状態では非表示
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 指定された属性に応じて敵のSpriteを更新
    /// </summary>
    public async UniTask UpdateSpriteForAttribute(CardAttribute attribute)
    {
        if (!_enemyData) return;
        
        var newSprite = _enemyData.GetSpriteForAttribute(attribute);
        if (!newSprite) return;
        
        // スプライト切り替えアニメーション
        await PlaySpriteChangeAnimation(newSprite);
    }
    
    /// <summary>
    /// 敵を表示
    /// </summary>
    public async UniTask Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        
        // フェードイン
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            await LMotion.Create(0f, 1f, fadeDuration)
                .WithEase(Ease.OutQuad)
                .Bind(alpha => canvasGroup.alpha = alpha)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
    
    /// <summary>
    /// 敵を非表示
    /// </summary>
    public async UniTask Hide()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            await LMotion.Create(1f, 0f, fadeDuration)
                .WithEase(Ease.InQuad)
                .Bind(alpha => canvasGroup.alpha = alpha)
                .AddTo(gameObject)
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// スプライト変更時のアニメーション
    /// </summary>
    private async UniTask PlaySpriteChangeAnimation(Sprite newSprite)
    {
        if (!enemyImage) return;
        
        // スケールを小さくしながらフェードアウト
        var scaleMotion = LMotion.Create(Vector3.one, Vector3.one * 0.8f, scaleDuration / 2)
            .WithEase(Ease.InQuad)
            .BindToLocalScale(_rectTransform);
            
        var canvasGroup = GetComponent<CanvasGroup>();
        var fadeOutMotion = LMotion.Create(1f, 0f, scaleDuration / 2)
            .WithEase(Ease.InQuad)
            .Bind(alpha => canvasGroup.alpha = alpha);
            
        await UniTask.WhenAll(
            scaleMotion.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy()),
            fadeOutMotion.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
        );
        
        // スプライトを変更
        enemyImage.sprite = newSprite;
        
        // スケールを戻しながらフェードイン
        var scaleBackMotion = LMotion.Create(Vector3.one * 0.8f, Vector3.one, scaleDuration / 2)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(_rectTransform);
            
        var fadeInMotion = LMotion.Create(0f, 1f, scaleDuration / 2)
            .WithEase(Ease.OutQuad)
            .Bind(alpha => canvasGroup.alpha = alpha);
            
        await UniTask.WhenAll(
            scaleBackMotion.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy()),
            fadeInMotion.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
        );
    }
    
    private void Awake()
    {
        _rectTransform = this.GetComponent<RectTransform>();
    }
}