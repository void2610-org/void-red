using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 競合フェーズのView
/// 引き分け時のリアルタイム上乗せUIを管理
/// </summary>
public class CompetitionView : BasePhaseView
{
    [Header("タイトル")]
    [SerializeField] private TMP_Text instructionText;

    [Header("タイマー")]
    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_Text timerText;

    [Header("天秤")]
    [SerializeField] private Animator balanceAnimator;

    [Header("入札表示")]
    [SerializeField] private TMP_Text playerBidText;
    [SerializeField] private TMP_Text enemyBidText;

    [Header("感情リソース")]
    [SerializeField] private EmotionResourceDisplayView emotionResourceDisplayView;

    [Header("上乗せボタン")]
    [SerializeField] private Button raiseButton;

    // プレイヤーが上乗せボタンを押した時に発火
    public Observable<Unit> OnRaise => raiseButton.OnClickAsObservable();

    // 感情選択が変更された時に発火
    public Observable<EmotionType> OnEmotionSelected => emotionResourceDisplayView.OnEmotionSelected;

    private static readonly int BID_LEVEL_HASH = Animator.StringToHash("BidLevel");
    private const int MAX_BID_FOR_TILT = 10;

    public override void Show() => CanvasGroup.Show();

    /// <summary>
    /// 感情リソース表示を更新
    /// </summary>
    public void UpdateResources(IReadOnlyDictionary<EmotionType, int> resources) =>
        emotionResourceDisplayView.UpdateResources(resources);

    /// <summary>
    /// 競合UIを初期化して表示
    /// </summary>
    public void Initialize(int playerBid, int enemyBid, IReadOnlyDictionary<EmotionType, int> resources)
    {
        playerBidText.text = playerBid.ToString();
        enemyBidText.text = enemyBid.ToString();
        UpdateBalanceTilt(playerBid, enemyBid);
        emotionResourceDisplayView.UpdateResources(resources);
        emotionResourceDisplayView.SetSelectedEmotion(EmotionType.Joy);
        Show();
    }

    /// <summary>
    /// 入札額を更新
    /// </summary>
    public void UpdateBids(int playerBid, int enemyBid)
    {
        playerBidText.text = playerBid.ToString();
        enemyBidText.text = enemyBid.ToString();
        UpdateBalanceTilt(playerBid, enemyBid);
    }

    /// <summary>
    /// タイマー表示を更新（0-1の割合）
    /// </summary>
    public void UpdateTimer(float remaining, float max)
    {
        var ratio = Mathf.Clamp01(remaining / max);
        timerImage.fillAmount = ratio;
        timerText.text = Mathf.CeilToInt(remaining).ToString();
    }

    /// <summary>
    /// 天秤の傾きを更新（プレイヤー対敵の比率で）
    /// </summary>
    private void UpdateBalanceTilt(int playerBid, int enemyBid)
    {
        if (!balanceAnimator) return;

        // 差分を-MAX～+MAXの範囲で0-1に正規化
        var diff = playerBid - enemyBid;
        var normalized = Mathf.Clamp01((float)(diff + MAX_BID_FOR_TILT) / (2f * MAX_BID_FOR_TILT));
        balanceAnimator.SetFloat(BID_LEVEL_HASH, normalized);
    }
}
