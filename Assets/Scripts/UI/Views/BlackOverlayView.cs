using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// 全画面の黒背景オーバーレイを管理するViewクラス
/// 各種UI表示時の背景として使用される
/// </summary>
[RequireComponent(typeof(Image))]
public class BlackOverlayView : MonoBehaviour
{
    [Header("背景設定")]
    [SerializeField, Range(0f, 1f)] private float maxOpacity = 0.95f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private Image _overlayImage;
    private MotionHandle _currentFadeHandle;
    
    /// <summary>
    /// 現在表示されているかどうか
    /// </summary>
    public bool IsVisible { get; private set; }
    
    private void Awake()
    {
        _overlayImage = GetComponent<Image>();
        
        // 初期状態は非表示
        _overlayImage.color = new Color(0f, 0f, 0f, 0f);
        gameObject.SetActive(false);
        IsVisible = false;
    }
    
    /// <summary>
    /// 背景をフェードイン
    /// </summary>
    public async UniTask FadeIn()
    {
        if (IsVisible) return;
        
        // 現在のアニメーションをキャンセル
        if (_currentFadeHandle.IsActive()) _currentFadeHandle.Cancel();
        
        gameObject.SetActive(true);
        IsVisible = true;
        
        var currentColor = _overlayImage.color;
        var targetColor = new Color(0f, 0f, 0f, maxOpacity);
        _currentFadeHandle = LMotion.Create(currentColor, targetColor, fadeInDuration)
            .WithEase(Ease.OutQuart)
            .BindToColor(_overlayImage)
            .AddTo(gameObject);
        
        await _currentFadeHandle.ToUniTask();
    }
    
    /// <summary>
    /// 背景をフェードアウト
    /// </summary>
    public async UniTask FadeOut()
    {
        if (!IsVisible) return;
        
        // 現在のアニメーションをキャンセル
        if (_currentFadeHandle.IsActive()) _currentFadeHandle.Cancel();
        
        IsVisible = false;
        
        var currentColor = _overlayImage.color;
        var targetColor = new Color(0f, 0f, 0f, 0f);
        _currentFadeHandle = LMotion.Create(currentColor, targetColor, fadeOutDuration)
            .WithEase(Ease.InQuart)
            .BindToColor(_overlayImage)
            .AddTo(gameObject);
        
        await _currentFadeHandle.ToUniTask();
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 即座に表示
    /// </summary>
    public void Show()
    {
        if (_currentFadeHandle.IsActive()) _currentFadeHandle.Cancel();
        
        gameObject.SetActive(true);
        _overlayImage.color = new Color(0f, 0f, 0f, maxOpacity);
        IsVisible = true;
    }
    
    /// <summary>
    /// 即座に非表示
    /// </summary>
    public void Hide()
    {
        if (_currentFadeHandle.IsActive()) _currentFadeHandle.Cancel();
        
        _overlayImage.color = new Color(0f, 0f, 0f, 0f);
        gameObject.SetActive(false);
        IsVisible = false;
    }
    
    private void OnDestroy()
    {
        // アニメーションのクリーンアップ
        if (_currentFadeHandle.IsActive()) _currentFadeHandle.Cancel();
    }
}