using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

// 入札ウィンドウView
// カードクリック時にモーダル表示される入札額調整UI
public class BidWindowView : BaseWindowView
{
    [Header("情報表示")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text emotionNameText;
    [SerializeField] private Image emotionIndicator;
    [SerializeField] private SerializableDictionary<EmotionType, Sprite> emotionSprites = new();

    [Header("天秤アニメーション")]
    [SerializeField] private Animator balanceAnimator;

    [Header("炎演出")]
    [SerializeField] private Image flameImage;
    [SerializeField] private Sprite[] flameSprites;

    [Header("入札操作")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private TMP_Text bidAmountText;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();
    public Observable<Unit> OnClose => closeButton.OnClickAsObservable();

    private static readonly int BID_LEVEL_HASH = Animator.StringToHash("BidLevel");
    private const int MAX_BID_FOR_TILT = 10;
    private const float OVERSHOOT_AMOUNT = 0.2f;
    private const float OVERSHOOT_DURATION = 0.15f;
    private const float SETTLE_DURATION = 0.25f;
    private CancellationTokenSource _tiltCts;

    public void SetCardName(string name) => cardNameText.text = name;

    // 入札値から炎レベル(0〜2)を判定
    public static int GetFlameLevel(int bidAmount) => bidAmount switch
    {
        <= 4 => 0,
        <= 9 => 1,
        _ => 2,
    };

    public void UpdateBidAmount(int amount)
    {
        bidAmountText.text = amount.ToString();
        UpdateFlame(amount);
        UpdateBalanceTilt(amount);
    }

    public void SetEmotion(EmotionType emotion)
    {
        if (emotionSprites.TryGetValue(emotion, out var sprite))
            emotionIndicator.sprite = sprite;
        emotionNameText.text = emotion.ToJapaneseName();
    }

    // 入札値に応じて天秤の傾きをオーバーシュート→戻りでアニメーション
    private void UpdateBalanceTilt(int amount)
    {
        var target = Mathf.Clamp01((float)amount / MAX_BID_FOR_TILT);
        _tiltCts?.Cancel();
        _tiltCts?.Dispose();
        _tiltCts = new CancellationTokenSource();
        AnimateBalanceTiltAsync(target, _tiltCts.Token).Forget();
    }

    // 目標値を超えてから戻る2フェーズアニメーション
    private async UniTaskVoid AnimateBalanceTiltAsync(float target, CancellationToken ct)
    {
        var start = balanceAnimator.GetFloat(BID_LEVEL_HASH);
        var direction = Mathf.Sign(target - start);
        var overshootTarget = Mathf.Clamp(target + direction * OVERSHOOT_AMOUNT, 0f, 1f);

        // フェーズ1: 目標を超えた位置まで素早く移動
        await LerpBidLevelAsync(start, overshootTarget, OVERSHOOT_DURATION, ct);

        // フェーズ2: 目標値に戻る
        await LerpBidLevelAsync(overshootTarget, target, SETTLE_DURATION, ct);
    }

    private async UniTask LerpBidLevelAsync(float from, float to, float duration, CancellationToken ct)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var smoothed = t * t * (3f - 2f * t);
            balanceAnimator.SetFloat(BID_LEVEL_HASH, Mathf.Lerp(from, to, smoothed));
            await UniTask.Yield(ct);
        }
        balanceAnimator.SetFloat(BID_LEVEL_HASH, to);
    }

    private void UpdateFlame(int amount)
    {
        var level = GetFlameLevel(amount);
        flameImage.sprite = flameSprites[level];
    }

    private void OnDestroy()
    {
        _tiltCts?.Cancel();
        _tiltCts?.Dispose();
    }
}
