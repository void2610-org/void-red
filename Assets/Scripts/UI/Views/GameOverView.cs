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

    public Observable<Unit> OnRetryClicked { get; private set; }
    public Observable<Unit> OnTitleClicked { get; private set; }
    
    /// <summary>
    /// ゲームオーバー画面を表示
    /// </summary>
    /// <param name="reason">ゲームオーバーの理由</param>
    public void ShowGameOverScreen(string reason)
    {
        gameOverReasonText.text = reason;
        Show();
    }
    
    protected override void Awake()
    {
        closeButton = titleButton;
        base.Awake();

        // ボタンイベントの設定
        OnRetryClicked = retryButton.OnClickAsObservable();
        OnTitleClicked = titleButton.OnClickAsObservable();

        // デフォルトテキストの設定
        gameOverTitleText.text = "ゲームオーバー";
        gameOverReasonText.text = "";
    }
}