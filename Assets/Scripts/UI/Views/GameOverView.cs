using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using System;
using LitMotion;
using LitMotion.Extensions;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

/// <summary>
/// ゲームオーバー画面を管理するViewクラス
/// </summary>
public class GameOverView : BaseWindowView
{
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI gameOverReasonText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    public Observable<Unit> OnRetryClicked => _retryClicked;
    public Observable<Unit> OnTitleClicked => _titleClicked;

    private readonly Subject<Unit> _retryClicked = new();
    private readonly Subject<Unit> _titleClicked = new();
    
    /// <summary>
    /// ゲームオーバー画面を表示
    /// </summary>
    /// <param name="reason">ゲームオーバーの理由</param>
    public void ShowGameOverScreen(string reason)
    {
        gameOverReasonText.text = reason;
        Show();
    }
    
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
    
    protected override void Awake()
    {
        closeButton = titleButton;
        base.Awake();

        // ボタンイベントの設定
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        titleButton.onClick.AddListener(OnTitleButtonClicked);

        // デフォルトテキストの設定
        gameOverTitleText.text = "ゲームオーバー";
        gameOverReasonText.text = "";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

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