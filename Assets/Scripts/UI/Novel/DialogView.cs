using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// 単一のダイアログ表示を担当するViewクラス
/// Presenterから個別のダイアログを受け取って表示する
/// </summary>
public class DialogView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private GameObject nextIndicator;
    [SerializeField] private Image characterImage;
    
    [Header("文字送り設定")]
    [SerializeField] private float defaultCharSpeed = 0.05f; // デフォルトの1文字表示間隔（秒）
    [SerializeField] private float autoNextDelay = 3f; // 自動で次へ進むまでの待機時間（秒）
    
    [Header("フェード設定")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("話者名表示設定")]
    [SerializeField] private GameObject speakerNamePanel;
    
    private bool _isTyping;
    private bool _isWaitingForNext;
    private bool _isCompleted;
    
    private MotionHandle _typewriterMotion;
    private MotionHandle _fadeMotion;
    private MotionHandle _indicatorMotion;
    
    // キャラクター画像のコールバック（Presenterから設定）
    private System.Func<string, UniTask<Sprite>> _loadCharacterImageCallback;
    private string _currentImageName;
    
    // イベント
    public event Action OnDialogCompleted;
    public event Action OnUserClickDetected;
    
    private void Awake()
    {
        // 初期状態を設定
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        dialogText.text = "";
        
        nextIndicator.SetActive(false);
        speakerNamePanel.SetActive(false);
        characterImage.color = new Color(1f, 1f, 1f, 0f);
        characterImage.sprite = null;
        
        _isCompleted = false;
    }
    
    /// <summary>
    /// キャラクター画像読み込みコールバックを設定
    /// </summary>
    public void SetCharacterImageLoader(System.Func<string, UniTask<Sprite>> loadCallback)
    {
        _loadCharacterImageCallback = loadCallback;
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
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        
        _fadeMotion = LMotion.Create(1f, 0f, fadeOutDuration)
            .WithEase(Ease.InCubic)
            .Bind(alpha => canvasGroup.alpha = alpha)
            .AddTo(this);
        
        await _fadeMotion.ToUniTask();
    }
    
    /// <summary>
    /// 単一のダイアログを表示する
    /// </summary>
    public async UniTask ShowSingleDialog(DialogData dialogData)
    {
        // SE再生
        if (dialogData.HasSE && dialogData.PlaySEOnStart)
        {
            SeManager.Instance.PlaySe(dialogData.SEClipName);
        }
        
        // 話者名を設定
        SetSpeakerName(dialogData.SpeakerName);
        
        // キャラクター画像を設定
        await SetCharacterImage(dialogData.CharacterImageName);
        
        // ダイアログテキストをクリア
        dialogText.text = "";
        
        // インジケーターを非表示
        nextIndicator.SetActive(false);
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
        
        // 文字送りアニメーションを開始
        _isTyping = true;
        _isWaitingForNext = false;
        
        // 文字速度を決定
        float charSpeed = dialogData.UseDefaultCharSpeed ? defaultCharSpeed : dialogData.CustomCharSpeed;
        
        await dialogText.TypewriterAnimation(dialogData.DialogText, charSpeed, true, this.GetCancellationTokenOnDestroy());
        await UniTask.Yield();
        
        // アニメーション完了後の状態をリセット
        _isTyping = false;
        _isWaitingForNext = true;
        
        // インジケーターを表示
        ShowNextIndicator();
        
        // 自動進行またはユーザー入力待ち
        if (dialogData.AutoAdvance)
        {
            await WaitForNextWithTimeout();
        }
        else
        {
            await WaitForNext();
        }
    }
    
    /// <summary>
    /// ダイアログ完了を表示
    /// </summary>
    public async UniTask ShowDialogComplete()
    {
        _isCompleted = true;
        await FadeOut();
        OnDialogCompleted?.Invoke();
    }
    
    /// <summary>
    /// 話者名を設定する
    /// </summary>
    private void SetSpeakerName(string speakerName)
    {
        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);
        
        if (speakerNamePanel)
        {
            speakerNamePanel.SetActive(hasSpeaker);
        }
        
        if (speakerNameText)
        {
            speakerNameText.text = hasSpeaker ? speakerName : "";
        }
    }
    
    /// <summary>
    /// キャラクター画像を設定
    /// </summary>
    private async UniTask SetCharacterImage(string imageName)
    {
        // 同じ画像の場合は処理をスキップ
        if (_currentImageName == imageName)
            return;
        
        // 前の画像をフェードアウト
        if (!string.IsNullOrEmpty(_currentImageName))
        {
            await characterImage.FadeOut(0.3f);
        }
        
        _currentImageName = imageName;
        
        // 新しい画像を設定
        if (!string.IsNullOrEmpty(imageName) && _loadCharacterImageCallback != null)
        {
            var sprite = await _loadCharacterImageCallback(imageName);
            if (sprite)
            {
                characterImage.sprite = sprite;
                await characterImage.FadeIn(0.3f);
            }
        }
        else
        {
            characterImage.sprite = null;
        }
    }
    
    /// <summary>
    /// 次へ進むのを待つ
    /// </summary>
    private async UniTask WaitForNext()
    {
        while (_isWaitingForNext)
        {
            await UniTask.Yield();
        }
    }
    
    /// <summary>
    /// タイムアウト付きで次へ進むのを待つ
    /// </summary>
    private async UniTask WaitForNextWithTimeout()
    {
        if (_isWaitingForNext)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(autoNextDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
    
    /// <summary>
    /// マウスクリック検知
    /// </summary>
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
            
        if (!_isTyping && _isWaitingForNext)
        {
            _isWaitingForNext = false;
            OnUserClickDetected?.Invoke();
        }
    }
    
    /// <summary>
    /// 次へ進むインジケーターを表示
    /// </summary>
    private void ShowNextIndicator()
    {
        nextIndicator.SetActive(true);
        PositionIndicatorAtLastCharacter();
        
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
    
    /// <summary>
    /// インジケーターを最後の文字の横に配置
    /// </summary>
    private void PositionIndicatorAtLastCharacter()
    {
        var indicatorRectTransform = nextIndicator.GetComponent<RectTransform>();
        dialogText.ForceMeshUpdate();
        
        var textInfo = dialogText.textInfo;
        if (textInfo.characterCount == 0) return;
        
        var lastVisibleCharIndex = textInfo.characterCount - 1;
        
        while (lastVisibleCharIndex >= 0)
        {
            var charInfo = textInfo.characterInfo[lastVisibleCharIndex];
            if (charInfo.isVisible && !char.IsWhiteSpace(charInfo.character))
            {
                break;
            }
            lastVisibleCharIndex--;
        }
        
        if (lastVisibleCharIndex >= 0)
        {
            var lastCharInfo = textInfo.characterInfo[lastVisibleCharIndex];
            var lastCharPosition = new Vector3(lastCharInfo.topRight.x, lastCharInfo.bottomRight.y, 0);
            var textRectTransform = dialogText.GetComponent<RectTransform>();
            var worldPos = textRectTransform.TransformPoint(lastCharPosition);
            var localPos = indicatorRectTransform.parent.GetComponent<RectTransform>().InverseTransformPoint(worldPos);
            indicatorRectTransform.anchoredPosition = new Vector2(localPos.x + 30f, localPos.y + 5f);
        }
    }
    
    public bool IsCompleted => _isCompleted;
    
    private void OnDestroy()
    {
        if (_typewriterMotion.IsActive())
            _typewriterMotion.Cancel();
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
    }
}
