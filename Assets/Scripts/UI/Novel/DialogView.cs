using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 小説システム用のViewクラス - 純粋なUI表示のみを担当
/// MVPパターンに準拠し、表示とユーザー入力のハンドリングのみ行う
/// </summary>
public class DialogView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image characterImage;
    [SerializeField] private GameObject nextIndicator;
    
    [Header("フェード設定")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private MotionHandle _typewriterMotion;
    private MotionHandle _fadeMotion;
    private MotionHandle _indicatorMotion;
    
    private bool _isTyping;
    private bool _isWaitingForNext;
    private CancellationTokenSource _inputCancellationTokenSource;

    private void Awake()
    {
        // 初期状態を設定
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        dialogText.text = "";
        speakerText.text = "";
        
        if (characterImage != null)
        {
            characterImage.sprite = null;
            characterImage.gameObject.SetActive(false);
        }
        
        if (nextIndicator != null)
        {
            nextIndicator.SetActive(false);
        }
        
        _isTyping = false;
        _isWaitingForNext = false;
    }

    private void Update()
    {
        // マウスクリックまたはタッチ入力を検知
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            OnClick();
        }
    }

    /// <summary>
    /// クリック時の処理
    /// </summary>
    private void OnClick()
    {
        // CanvasGroup が非アクティブな場合は無視
        if (canvasGroup.alpha == 0f || !canvasGroup.interactable)
            return;
            
        if (_isTyping)
        {
            // タイピング中の場合はスキップ
            if (_typewriterMotion.IsActive())
                _typewriterMotion.Cancel();
            _isTyping = false;
        }
        else if (_isWaitingForNext)
        {
            // 次へ進む
            _isWaitingForNext = false;
            _inputCancellationTokenSource?.Cancel();
        }
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    public async UniTask FadeIn()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        
        _fadeMotion = LMotion.Create(0f, 1f, fadeInDuration)
            .WithEase(Ease.OutCubic)
            .Bind(alpha => canvasGroup.alpha = alpha)
            .AddTo(this);
        
        await _fadeMotion.ToUniTask();
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    public async UniTask FadeOut()
    {
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        
        _fadeMotion = LMotion.Create(canvasGroup.alpha, 0f, fadeOutDuration)
            .WithEase(Ease.OutCubic)
            .Bind(alpha => canvasGroup.alpha = alpha)
            .AddTo(this);
        
        await _fadeMotion.ToUniTask();
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// キャラクター画像の表示
    /// </summary>
    public void SetCharacterImage(Sprite sprite)
    {
        if (sprite != null)
        {
            characterImage.sprite = sprite;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 話者名の設定 (View is responsible for display only)
    /// </summary>
    public void SetSpeakerName(string speakerName)
    {
        speakerText.text = speakerName ?? string.Empty;
    }

    /// <summary>
    /// ダイアログテキストの表示 (View is responsible for display only)
    /// </summary>
    public async UniTask DisplayText(string text)
    {
        dialogText.text = "";
        _isTyping = true;
        
        if (nextIndicator != null)
            nextIndicator.SetActive(false);
        
        if (_typewriterMotion.IsActive())
            _typewriterMotion.Cancel();

        const float duration = 0.05f;
        
        _typewriterMotion = LMotion.Create(0, text.Length, duration * text.Length)
            .WithEase(Ease.Linear)
            .Bind(length => 
            {
                int charCount = Mathf.RoundToInt(length);
                dialogText.text = text.Substring(0, Mathf.Min(charCount, text.Length));
            })
            .AddTo(this);
        
        await _typewriterMotion.ToUniTask();
        
        _isTyping = false;
        ShowNextIndicator();
    }

    /// <summary>
    /// 次の入力を待機 (View is responsible for input handling only)
    /// </summary>
    public async UniTask WaitForNextInput()
    {
        _isWaitingForNext = true;
        _inputCancellationTokenSource?.Cancel();
        _inputCancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            await UniTask.WaitWhile(() => _isWaitingForNext, cancellationToken: _inputCancellationTokenSource.Token);
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合は正常終了
        }
        
        if (nextIndicator != null)
            nextIndicator.SetActive(false);
    }

    /// <summary>
    /// 次へ進むインジケーターを表示してアニメーション
    /// </summary>
    private void ShowNextIndicator()
    {
        if (nextIndicator == null) return;
        
        nextIndicator.SetActive(true);
        
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
        
        var rectTransform = nextIndicator.GetComponent<RectTransform>();
        if (rectTransform)
        {
            var originalPos = rectTransform.anchoredPosition;
            _indicatorMotion = LMotion.Create(0f, 1f, 1f)
                .WithLoops(-1, LoopType.Yoyo)
                .WithEase(Ease.InOutSine)
                .Bind(t =>
                {
                    var pos = originalPos;
                    pos.y += Mathf.Sin(t * Mathf.PI) * 5f;
                    rectTransform.anchoredPosition = pos;
                })
                .AddTo(this);
        }
    }

    private void OnDestroy()
    {
        if (_typewriterMotion.IsActive())
            _typewriterMotion.Cancel();
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
        
        _inputCancellationTokenSource?.Cancel();
        _inputCancellationTokenSource?.Dispose();
    }
}
