using R3;
using UnityEngine;

/// <summary>
/// リアルタイム競合を管理するハンドラ
/// 引き分け時に両者が1枚ずつリソースを上乗せし、
/// 10秒間上乗せが無ければ終了
/// </summary>
public class CompetitionHandler
{
    public CardModel Card { get; private set; }
    public int PlayerTotal { get; private set; }
    public int EnemyTotal { get; private set; }
    public bool IsActive { get; private set; }

    // UI通知用
    public Observable<(int playerTotal, int enemyTotal)> OnBidChanged => _onBidChanged;
    public Observable<float> OnTimerReset => _onTimerReset;

    /// <summary>
    /// タイムアウト判定（10秒間上乗せ無し）
    /// </summary>
    public bool IsTimedOut => IsActive &&
        Time.time - _lastActionTime >= GameConstants.COMPETITION_TIMEOUT_SECONDS;

    /// <summary>
    /// 残り時間を取得
    /// </summary>
    public float RemainingTime => IsActive
        ? Mathf.Max(0f, GameConstants.COMPETITION_TIMEOUT_SECONDS - (Time.time - _lastActionTime))
        : 0f;

    /// <summary>
    /// 勝者判定（null = 完全引き分け）
    /// </summary>
    public bool? IsPlayerWon =>
        PlayerTotal > EnemyTotal ? true :
        PlayerTotal < EnemyTotal ? false : null;

    private float _lastActionTime;
    private readonly Subject<(int, int)> _onBidChanged = new();
    private readonly Subject<float> _onTimerReset = new();

    /// <summary>
    /// 競合を終了
    /// </summary>
    public void End() => IsActive = false;

    /// <summary>
    /// プレイヤーが1枚上乗せ（任意の感情）
    /// </summary>
    public bool TryPlayerRaise(EmotionType emotion, PlayerPresenter player)
    {
        if (!IsActive) return false;
        if (player.GetEmotionAmount(emotion) <= 0) return false;

        player.TryConsumeEmotion(emotion, 1);
        PlayerTotal++;
        _lastActionTime = Time.time;
        _onBidChanged.OnNext((PlayerTotal, EnemyTotal));
        _onTimerReset.OnNext(GameConstants.COMPETITION_TIMEOUT_SECONDS);
        return true;
    }

    /// <summary>
    /// 敵が1枚上乗せ
    /// </summary>
    public void EnemyRaise(EmotionType emotion, PlayerPresenter enemy)
    {
        if (!IsActive) return;

        enemy.TryConsumeEmotion(emotion, 1);
        EnemyTotal++;
        _lastActionTime = Time.time;
        _onBidChanged.OnNext((PlayerTotal, EnemyTotal));
        _onTimerReset.OnNext(GameConstants.COMPETITION_TIMEOUT_SECONDS);
    }

    /// <summary>
    /// 競合を開始
    /// </summary>
    public void Start(CardModel card, int playerBid, int enemyBid)
    {
        Card = card;
        PlayerTotal = playerBid;
        EnemyTotal = enemyBid;
        _lastActionTime = Time.time;
        IsActive = true;
        _onBidChanged.OnNext((PlayerTotal, EnemyTotal));
    }

    public void Dispose()
    {
        _onBidChanged.Dispose();
        _onTimerReset.Dispose();
    }
}
