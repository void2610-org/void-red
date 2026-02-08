using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VoidRed.UI.Views;

/// <summary>
/// リソース報酬のゲージアニメーションを表示するサブView
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ResourceRewardView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private EmotionGaugeView[] emotionGauges;

    /// <summary>
    /// 報酬演出で各感情タイプに加算された量を取得
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> RewardedAmounts => _rewardedAmounts;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private Dictionary<EmotionType, EmotionGaugeView> _gaugeDict = new();
    private Dictionary<EmotionType, int> _currentResourceValues = new();
    private Dictionary<EmotionType, int> _rewardedAmounts = new();
    private Dictionary<EmotionType, int> _maxResources = new();
    private EmotionType[] _emotionTypes;

    /// <summary>
    /// リソース報酬をゲージアニメーションで表示
    /// </summary>
    public async UniTask ShowRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results,
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        Show();

        // リソース情報を保持
        _maxResources = new Dictionary<EmotionType, int>(maxResources);
        _currentResourceValues = new Dictionary<EmotionType, int>();
        _rewardedAmounts = new Dictionary<EmotionType, int>();
        foreach (var (emotion, value) in currentResources)
        {
            _currentResourceValues[emotion] = value;
            _rewardedAmounts[emotion] = 0;
        }

        // 感情バーの初期化（報酬加算前の状態）
        InitializeEmotionGauges(currentResources, maxResources);

        foreach (var (_, result) in results)
        {
            // 次へボタンを待つ
            await _onNextButtonClicked.FirstAsync();

            // ランダムな感情タイプに報酬を加算してバーをアニメーション
            var randomEmotion = GetRandomEmotion();
            AnimateRewardToEmotion(randomEmotion, result.TotalReward);
        }

        await _onNextButtonClicked.FirstAsync();
        Hide();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private EmotionType GetRandomEmotion()
    {
        var index = Random.Range(0, _emotionTypes.Length);
        return _emotionTypes[index];
    }

    private void InitializeEmotionGauges(
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        _emotionTypes = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        _gaugeDict.Clear();

        for (var i = 0; i < emotionGauges.Length && i < _emotionTypes.Length; i++)
        {
            var emotion = _emotionTypes[i];
            var gauge = emotionGauges[i];

            _gaugeDict[emotion] = gauge;
            gauge.SetColor(emotion.GetColor());

            // 報酬加算前のリソース値を設定
            var current = currentResources[emotion];
            var max = maxResources[emotion];
            gauge.SetValue(current, max);
        }
    }

    private void AnimateRewardToEmotion(EmotionType emotion, int rewardAmount)
    {
        var gauge = _gaugeDict[emotion];

        // 現在値を更新
        var currentValue = _currentResourceValues[emotion];
        var newValue = currentValue + rewardAmount;
        _currentResourceValues[emotion] = newValue;

        _rewardedAmounts[emotion] += rewardAmount;
        gauge.AnimateToValue(newValue, _maxResources[emotion]);
    }

    private void Awake()
    {
        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onNextButtonClicked.Dispose();
    }
}
