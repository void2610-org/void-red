using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;
using R3;

/// <summary>
/// 単一のダイアログ表示を担当するViewクラス
/// Presenterから個別のダイアログを受け取って表示する
/// </summary>
public class DialogView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup dialogTextPanelCanvasGroup;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private GameObject nextIndicator;
    
    [Header("操作ボタン")]
    [SerializeField] private Button autoButton;
    [SerializeField] private TextMeshProUGUI autoButtonText;
    [SerializeField] private Button skipButton;
    
    [Header("オートボタン表示設定")]
    [SerializeField] private Color autoButtonNormalColor = Color.white;
    [SerializeField] private Color autoButtonActiveColor = Color.yellow;
    
    [Header("文字送り設定")]
    [SerializeField] private float defaultCharSpeed = 0.03f; // デフォルトの1文字表示間隔（秒）
    [SerializeField] private float autoNextDelay = 3f; // 自動で次へ進むまでの待機時間（秒）
    
    private bool _isTyping;
    private bool _isWaitingForNext;
    private bool _isAutoMode;
    private CancellationTokenSource _typingCancellationTokenSource;
    private CancellationTokenSource _waitCancellationTokenSource;
    private CancellationTokenSource _dialogSeCancellationTokenSource;
    private DialogData _currentDialogData;

    private MotionHandle _fadeMotion;
    private MotionHandle _indicatorMotion;
    private MotionHandle _panelFadeMotion;

    private string _currentImageName;
    private float _additionalWaitTime;
    
    // イベント
    private readonly Subject<Unit> _onSkipRequested = new();
    
    public Observable<Unit> OnSkipRequested => _onSkipRequested;

    private void Awake()
    {
        dialogText.text = "";

        nextIndicator.SetActive(false);

        autoButton.OnClickAsObservable().Subscribe(_ => ToggleAutoMode()).AddTo(this);
        skipButton.OnClickAsObservable().Subscribe(_ => _onSkipRequested.OnNext(Unit.Default)).AddTo(this);

        // オートボタンの初期色を設定
        UpdateAutoButtonColor();
    }
    
    /// <summary>
    /// 単一のダイアログを表示する
    /// </summary>
    /// <param name="dialogData">ダイアログデータ</param>
    /// <param name="additionalWaitTime">追加の待機時間（SE再生時間など）</param>
    public async UniTask ShowSingleDialog(DialogData dialogData, float additionalWaitTime = 0f)
    {
        if (!this) return;
        
        // 現在のダイアログデータを保存
        _currentDialogData = dialogData;
        _additionalWaitTime = additionalWaitTime;
        
        // 話者名を設定
        SetSpeakerName(dialogData.SpeakerName);

        // ダイアログテキストをクリア
        dialogText.text = "";
        
        // インジケーターを非表示
        nextIndicator.SetActive(false);
        CancelActiveMotions();
        
        // 文字送りアニメーションを開始
        _isTyping = true;
        _isWaitingForNext = false;

        // 既存のSEループをキャンセル
        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();
        _dialogSeCancellationTokenSource = new CancellationTokenSource();
        SeManager.Instance.PlaySeLoop("Dialog2", cancellationToken: _dialogSeCancellationTokenSource.Token).Forget();

        _typingCancellationTokenSource?.Cancel();
        _typingCancellationTokenSource?.Dispose();
        _typingCancellationTokenSource = new CancellationTokenSource();
        var charSpeed = dialogData.HasCustomCharSpeed ? defaultCharSpeed / dialogData.CustomCharSpeed : defaultCharSpeed;
        try
        {
            // skipOnClick: falseで旧InputSystemの競合を回避（OnClick経由でCTSキャンセルによりスキップ）
            await dialogText.TypewriterAnimation(dialogData.DialogText, charSpeed, skipOnClick: false, cancellationToken: _typingCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // OnClick()からのキャンセル - テキストは全文表示済み
        }

        // dialogSeループを停止
        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();
        _dialogSeCancellationTokenSource = null;

        // 状態をリセット
        _isTyping = false;
        _isWaitingForNext = true;

        // インジケーターを表示
        ShowNextIndicator();
        
        // 自動進行またはユーザー入力待ち
        if (_isAutoMode || dialogData.HasAutoAdvance)
        {
            await WaitForNextWithTimeout();
        }

        // オートモード解除等で待機がキャンセルされた場合、手動入力を待つ
        if (_isWaitingForNext)
        {
            await WaitForNext();
        }
    }
    
    /// <summary>
    /// オートモードの切り替え
    /// </summary>
    public void ToggleAutoMode()
    {
        _isAutoMode = !_isAutoMode;
        UpdateAutoButtonColor();

        if (!_isWaitingForNext) return;

        if (_isAutoMode)
        {
            // オートモードON: 自動進行を開始
            _waitCancellationTokenSource?.Cancel();
            _waitCancellationTokenSource?.Dispose();
            _waitCancellationTokenSource = null;
            StartAutoProgress().Forget();
        }
        else
        {
            // オートモードOFF: タイムアウト待機をキャンセル（WaitForNextにフォールバック）
            _waitCancellationTokenSource?.Cancel();
        }
    }

    /// <summary>
    /// 現在のダイアログを強制的に完了（スキップ用）
    /// </summary>
    public void ForceComplete()
    {
        // 文字送りアニメーションをキャンセル
        _typingCancellationTokenSource?.Cancel();
        _typingCancellationTokenSource?.Dispose();
        _typingCancellationTokenSource = null;

        // 待機をキャンセル
        _waitCancellationTokenSource?.Cancel();
        _waitCancellationTokenSource?.Dispose();
        _waitCancellationTokenSource = null;

        // SEループを停止
        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();
        _dialogSeCancellationTokenSource = null;

        // 状態をリセット
        _isTyping = false;
        _isWaitingForNext = false;
    }

    /// <summary>
    /// 話者名を設定する
    /// </summary>
    private void SetSpeakerName(string speakerName)
    {
        var hasSpeaker = !string.IsNullOrEmpty(speakerName);
        speakerNameText.text = hasSpeaker ? speakerName : "";
    }
    
    /// <summary>
    /// 次へ進むのを待つ
    /// </summary>
    private async UniTask WaitForNext()
    {
        _waitCancellationTokenSource = new CancellationTokenSource();
        try
        {
            while (_isWaitingForNext)
            {
                await UniTask.Yield(_waitCancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _waitCancellationTokenSource?.Dispose();
            _waitCancellationTokenSource = null;
        }
    }
    
    /// <summary>
    /// タイムアウト付きで次へ進むのを待つ
    /// </summary>
    private async UniTask WaitForNextWithTimeout()
    {
        if (_isWaitingForNext)
        {
            _waitCancellationTokenSource = new CancellationTokenSource();
            try
            {
                // SE再生時間がある場合は先に待つ
                if (_additionalWaitTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_additionalWaitTime), cancellationToken: _waitCancellationTokenSource.Token);
                    
                    // 待機後もまだ待機中の場合のみ続行
                    if (!_isWaitingForNext) return;
                }
                
                // 現在のダイアログのAutoAdvance時間を使用、設定されていない場合はデフォルト値
                var delay = _currentDialogData.HasAutoAdvance ? _currentDialogData.AutoAdvance : autoNextDelay;
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _waitCancellationTokenSource.Token);
                
                // タイムアウト後もまだ待機中の場合は自動で進む
                if (_isWaitingForNext) _isWaitingForNext = false;
            }
            catch (OperationCanceledException)
            {
                // ユーザーがクリックして手動で進んだ場合
            }
            finally
            {
                _waitCancellationTokenSource?.Dispose();
                _waitCancellationTokenSource = null;
            }
        }
    }
    
    /// <summary>
    /// オートモード用の自動進行開始
    /// </summary>
    private async UniTaskVoid StartAutoProgress()
    {
        if (!_isWaitingForNext || !_isAutoMode) return;
        
        try
        {
            // 追加の待機時間（SE再生時間など）がある場合は待つ
            if (_additionalWaitTime > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_additionalWaitTime), cancellationToken: this.GetCancellationTokenOnDestroy());
                
                // 待機後も待機中かつオートモードの場合のみ進む
                if (!_isWaitingForNext || !_isAutoMode) return;
            }
            
            // 現在のダイアログのAutoAdvance時間を使用、設定されていない場合はデフォルト値
            var delay = _currentDialogData.HasAutoAdvance ? _currentDialogData.AutoAdvance : autoNextDelay;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            if (_isWaitingForNext && _isAutoMode) _isWaitingForNext = false;
        }
        catch (OperationCanceledException) { }
    }
    
    /// <summary>
    /// クリック時の処理
    /// </summary>
    public void OnClick()
    {
        if (_isTyping)
        {
            // 文字送り中のクリックで即座に全文表示（フラグ管理はShowSingleDialogに委譲）
            _typingCancellationTokenSource?.Cancel();
            return;
        }

        if (!_isWaitingForNext) return;

        // オートモード中のクリックはオートモードを解除
        if (_isAutoMode)
        {
            _isAutoMode = false;
            UpdateAutoButtonColor();
            _waitCancellationTokenSource?.Cancel();
            return;
        }

        // 通常モードのクリックで次へ進む
        _isWaitingForNext = false;
    }
    
    /// <summary>
    /// 次へ進むインジケーターを表示
    /// </summary>
    private void ShowNextIndicator()
    {
        nextIndicator.SetActive(true);
        PositionIndicatorAtLastCharacter();
        
        CancelActiveMotions();
        
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
    
    /// <summary>
    /// ダイアログパネルと立ち絵を表示/非表示
    /// nextIndicatorは即時切り替え。
    /// </summary>
    public async UniTask SetDialogPanelVisible(bool visible)
    {
        CancelActiveMotions();
        nextIndicator.SetActive(visible);

        const float fadeDuration = 0.5f;
        if (visible)
        {
            _panelFadeMotion = dialogTextPanelCanvasGroup.FadeIn(fadeDuration, Ease.InCubic).AddTo(this);
            await _panelFadeMotion.ToUniTask();
        }
        else
        {
            _panelFadeMotion = dialogTextPanelCanvasGroup.FadeOut(fadeDuration, Ease.InCubic).AddTo(this);
            await _panelFadeMotion.ToUniTask();
        }
    }
    
    /// <summary>
    /// アクティブなアニメーションを全てキャンセル
    /// </summary>
    private void CancelActiveMotions()
    {
        _fadeMotion.TryCancel();
        _indicatorMotion.TryCancel();
        _panelFadeMotion.TryCancel();
    }
    
    private void UpdateAutoButtonColor()
    {
        autoButtonText.color = _isAutoMode ? autoButtonActiveColor : autoButtonNormalColor;
    }
    
    private void OnDestroy()
    {
        CancelActiveMotions();

        _waitCancellationTokenSource?.Cancel();
        _waitCancellationTokenSource?.Dispose();

        _dialogSeCancellationTokenSource?.Cancel();
        _dialogSeCancellationTokenSource?.Dispose();

        _onSkipRequested.Dispose();
    }
}
