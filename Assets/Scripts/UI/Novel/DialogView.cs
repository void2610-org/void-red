using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;
using System.Threading;

/// <summary>
/// DialogDataのリストを受け取って順番に表示するViewクラス
/// 話者名、セリフ、SE再生機能を提供
/// </summary>
public class DialogView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private GameObject nextIndicator; // 次へ進むインジケーター（▼など）
    [SerializeField] private Image characterImage;
    
    [Header("文字送り設定")]
    [SerializeField] private float defaultCharSpeed = 0.05f; // デフォルトの1文字表示間隔（秒）
    [SerializeField] private float autoNextDelay = 3f; // 自動で次へ進むまでの待機時間（秒）
    
    [Header("フェード設定")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("話者名表示設定")]
    [SerializeField] private GameObject speakerNamePanel; // 話者名パネル（話者がいない場合は非表示）
    
    [SerializeField] private List<Sprite> characterSprites; // キャラクター画像のリスト (簡易版)
    
    private List<DialogData> _dialogList;
    private int _currentIndex;
    private bool _isTyping;
    private bool _isWaitingForNext;
    private bool _isCompleted;
    
    private MotionHandle _typewriterMotion;
    private MotionHandle _fadeMotion;
    private MotionHandle _indicatorMotion;
    
    // ダイアログ完了時のイベント
    public event Action OnDialogCompleted;
    
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
    /// ダイアログの表示を開始する
    /// </summary>
    /// <param name="dialogList">表示するDialogDataのリスト</param>
    public async UniTask StartDialog(List<DialogData> dialogList)
    {
        _dialogList = new List<DialogData>(dialogList);
        _currentIndex = 0;
        _isCompleted = false;
        
        // フェードイン
        await FadeIn();
        
        // 最初のダイアログを表示
        await ShowNextDialog();
    }
    
    /// <summary>
    /// 次のダイアログを表示する
    /// </summary>
    private async UniTask ShowNextDialog()
    {
        if (_currentIndex >= _dialogList.Count)
        {
            // すべてのダイアログを表示完了
            await CompleteDialog();
            return;
        }
        
        var currentDialog = _dialogList[_currentIndex];
        _currentIndex++;
        
        // SE再生
        if (currentDialog.HasSE && currentDialog.PlaySEOnStart)
        {
            SeManager.Instance.PlaySe(currentDialog.SEClipName);
        }
        
        // 話者名を設定
        SetSpeakerName(currentDialog.SpeakerName);
        
        // キャラクター画像を設定
        var sprite = characterSprites.Find(s => s.name == currentDialog.CharacterImageName);
        if (sprite && !characterImage.sprite) characterImage.FadeIn(0.5f);
        else if (!sprite && characterImage.sprite) characterImage.FadeOut(0.5f);
        if (sprite) characterImage.sprite = sprite;
        
        // ダイアログテキストをクリア
        dialogText.text = "";
        
        // インジケーターを非表示
        nextIndicator.SetActive(false);
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
        
        // 文字送りアニメーションを開始
        _isTyping = true;
        _isWaitingForNext = false;
        
        // 文字速度を決定（カスタム速度またはデフォルト速度）
        float charSpeed = currentDialog.UseDefaultCharSpeed ? defaultCharSpeed : currentDialog.CustomCharSpeed;
        
        await dialogText.TypewriterAnimation(currentDialog.DialogText, charSpeed, true, this.GetCancellationTokenOnDestroy());
        await UniTask.Yield(); // スキップと進行の競合を防ぐために1フレーム待つ
        
        // アニメーション完了後の状態をリセット
        _isTyping = false;
        _isWaitingForNext = true;
        
        // インジケーターを表示してアニメーション
        ShowNextIndicator();
        
        // 自動進行またはユーザー入力待ち
        if (currentDialog.AutoAdvance)
        {
            await WaitForNextWithTimeout();
        }
        else
        {
            await WaitForNext();
        }
        
        // 次のダイアログへ
        await ShowNextDialog();
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
            await UniTask.Delay(TimeSpan.FromSeconds(autoNextDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
    
    /// <summary>
    /// マウスクリック検知による進行処理
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
            // 次へ進む
            _isWaitingForNext = false;
        }
    }
    
    /// <summary>
    /// 次へ進むインジケーターを表示
    /// </summary>
    private void ShowNextIndicator()
    {
        nextIndicator.SetActive(true);
        
        // 最後の文字の横にインジケーターを配置
        PositionIndicatorAtLastCharacter();
        
        // 上下にゆらゆら動くアニメーション
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
    /// インジケーターを最後の文字の横に配置する
    /// </summary>
    private void PositionIndicatorAtLastCharacter()
    {
        var indicatorRectTransform = nextIndicator.GetComponent<RectTransform>();
        // TextMeshProUGUIのtextInfoを使用して最後の文字の位置を取得
        dialogText.ForceMeshUpdate();
        
        var textInfo = dialogText.textInfo;
        if (textInfo.characterCount == 0) return;
        
        // 最後の可視文字のインデックスを取得
        var lastVisibleCharIndex = textInfo.characterCount - 1;
        
        // 改行や空白文字を除いた実際の最後の文字を探す
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
            
            // 最後の文字の右端の位置を計算
            var lastCharPosition = new Vector3(lastCharInfo.topRight.x, lastCharInfo.bottomRight.y, 0);
            
            // テキストのRectTransformからワールド座標に変換
            var textRectTransform = dialogText.GetComponent<RectTransform>();
            var worldPos = textRectTransform.TransformPoint(lastCharPosition);
            
            // インジケーターの親のRectTransformでローカル座標に変換
            var localPos = indicatorRectTransform.parent.GetComponent<RectTransform>().InverseTransformPoint(worldPos);
            
            // インジケーターの位置を設定（少し右にオフセット）
            indicatorRectTransform.anchoredPosition = new Vector2(localPos.x + 30f, localPos.y + 5f);
        }
    }
    
    /// <summary>
    /// ダイアログ完了処理
    /// </summary>
    private async UniTask CompleteDialog()
    {
        _isCompleted = true;
        await FadeOut();
        OnDialogCompleted?.Invoke();
    }
    
    /// <summary>
    /// フェードイン
    /// </summary>
    private async UniTask FadeIn()
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
    private async UniTask FadeOut()
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
    /// ダイアログが完了しているかどうか
    /// </summary>
    public bool IsCompleted => _isCompleted;
    
    private void OnDestroy()
    {
        // モーションを停止
        if (_typewriterMotion.IsActive())
            _typewriterMotion.Cancel();
        if (_fadeMotion.IsActive())
            _fadeMotion.Cancel();
        if (_indicatorMotion.IsActive())
            _indicatorMotion.Cancel();
    }
}
