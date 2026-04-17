using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;
using VoidRed.UI.Views;

/// <summary>
/// リソース報酬のゲージアニメーションを表示するサブView
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ResourceRewardView : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private EmotionGaugeView[] emotionGauges;

    /// <summary>
    /// 報酬演出で各感情タイプに加算された量を取得
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> RewardedAmounts => _rewardedAmounts;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly Dictionary<EmotionType, EmotionGaugeView> _gaugeDict = new();
    private Dictionary<EmotionType, int> _currentResourceValues = new();
    private Dictionary<EmotionType, int> _rewardedAmounts = new();
    private Dictionary<EmotionType, int> _maxResources = new();
    private EmotionType[] _emotionTypes;
    private CanvasGroup _canvasGroup;

    public void Hide() => _canvasGroup.Hide();

    public async UniTask WaitForNextAsync() => await _onNextButtonClicked.FirstAsync();

    /// <summary>
    /// リソース報酬をゲージアニメーションで表示
    /// </summary>
    public void DisplayGauges(
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
    }

    public async UniTask AnimateRewardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results)
    {
        foreach (var (_, result) in results)
        {
            // 次へボタンを待つ
            await _onNextButtonClicked.FirstAsync();

            // カードの司る感情に報酬を加算してバーをアニメーション
            AnimateRewardToEmotion(result.CardEmotion, result.TotalReward);
        }
    }

    private void Show()
    {
        _canvasGroup.Show();
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
        var seName = rewardAmount >= 0 ? "SE_RESOURCE_GAIN" : "SE_RESOURCE_LOSE";
        SeManager.Instance.PlaySe(seName, pitch: 1f);
        gauge.AnimateToValue(newValue, _maxResources[emotion]);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.Hide();

        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onNextButtonClicked.Dispose();
    }
}
