using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using System;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;

/// <summary>
/// ゲームオーバー画面を管理するViewクラス
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class GameOverView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI gameOverReasonText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;
    
    // イベント
    public Observable<Unit> OnRetryClicked => _retryClicked;
    public Observable<Unit> OnTitleClicked => _titleClicked;
    
    // 定数
    private const float FADE_IN_DURATION = 0.5f;
    private const float FADE_OUT_DURATION = 0.3f;
    private const float SCALE_ANIMATION_DURATION = 0.4f;
    
    // プライベートフィールド
    private readonly Subject<Unit> _retryClicked = new();
    private readonly Subject<Unit> _titleClicked = new();
    private CanvasGroup _canvasGroup;
    
    /// <summary>
    /// ゲームオーバー画面を表示
    /// </summary>
    /// <param name="reason">ゲームオーバーの理由</param>
    public async UniTask ShowGameOverScreen(string reason)
    {
        // ゲームオーバー理由を設定
        gameOverReasonText.text = reason;
        
        // 初期状態を設定（透明 + 小さいスケール）
        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * 0.8f;
        
        // フェードイン + スケールアニメーション
        var fadeTask = LMotion.Create(0f, 1f, FADE_IN_DURATION)
            .WithEase(Ease.OutQuart)
            .Bind(alpha => _canvasGroup.alpha = alpha)
            .AddTo(gameObject)
            .ToUniTask();
            
        var scaleTask = LMotion.Create(Vector3.one * 0.8f, Vector3.one, SCALE_ANIMATION_DURATION)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(transform)
            .AddTo(gameObject)
            .ToUniTask();
            
        await UniTask.WhenAll(fadeTask, scaleTask);
        
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// ゲームオーバー画面を非表示
    /// </summary>
    public async UniTask HideGameOverScreen()
    {
        // フェードアウト + スケールアニメーション
        var fadeTask = LMotion.Create(1f, 0f, FADE_OUT_DURATION)
            .WithEase(Ease.InQuart)
            .Bind(alpha => _canvasGroup.alpha = alpha)
            .AddTo(gameObject)
            .ToUniTask();
            
        var scaleTask = LMotion.Create(Vector3.one, Vector3.one * 0.8f, FADE_OUT_DURATION)
            .WithEase(Ease.InBack)
            .BindToLocalScale(transform)
            .AddTo(gameObject)
            .ToUniTask();
            
        await UniTask.WhenAll(fadeTask, scaleTask);
        
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// ゲームオーバー画面が表示されているかどうか
    /// </summary>
    public bool IsVisible => _canvasGroup.interactable;
    
    /// <summary>
    /// リトライボタンがクリックされた時の処理
    /// </summary>
    private void OnRetryButtonClicked()
    {
        _retryClicked.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// タイトルボタンがクリックされた時の処理
    /// </summary>
    private void OnTitleButtonClicked()
    {
        _titleClicked.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        
        // ボタンイベントの設定
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);
        
        // デフォルトテキストの設定
        gameOverTitleText.text = "ゲームオーバー";
        gameOverReasonText.text = "";
    }
    
    /// <summary>
    /// クリーンアップ
    /// </summary>
    private void OnDestroy()
    {
        // Subjectの破棄
        _retryClicked?.Dispose();
        _titleClicked?.Dispose();
        
        // ボタンイベントの削除
        if (retryButton)
            retryButton.onClick.RemoveListener(OnRetryButtonClicked);
        if (titleButton)
            titleButton.onClick.RemoveListener(OnTitleButtonClicked);
    }
}