using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VoidRed.UI.Views;

[RequireComponent(typeof(CanvasGroup))]
public class RewardPhaseView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private EmotionGaugeView[] emotionGauges;

    /// <summary>
    /// 報酬演出で各感情タイプに加算された量を取得
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> RewardedAmounts => _rewardedAmounts;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly List<CardView> _instantiatedCards = new();
    private Dictionary<EmotionType, EmotionGaugeView> _gaugeDict = new();
    private Dictionary<EmotionType, int> _currentResourceValues = new();
    private Dictionary<EmotionType, int> _rewardedAmounts = new();
    private Dictionary<EmotionType, int> _maxResources = new();
    private EmotionType[] _emotionTypes;

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        ClearInstantiatedCards();
    }

    public async UniTask ShowRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results,
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);

        ClearInstantiatedCards();

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

        foreach (var (card, result) in results)
        {
            // カードをコンテナに生成
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(card.Data);
            cardView.SetInteractable(false);
            _instantiatedCards.Add(cardView);

            // 次へボタンを待つ
            await _onNextButtonClicked.FirstAsync();

            // ランダムな感情タイプに報酬を加算してバーをアニメーション
            var randomEmotion = GetRandomEmotion();
            AnimateRewardToEmotion(randomEmotion, result.TotalReward);
        }

        await _onNextButtonClicked.FirstAsync();
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
            var current = currentResources.TryGetValue(emotion, out var c) ? c : 0;
            var max = maxResources.TryGetValue(emotion, out var m) ? m : 1;
            gauge.SetValue(current, max);
        }
    }

    private void AnimateRewardToEmotion(EmotionType emotion, int rewardAmount)
    {
        if (!_gaugeDict.TryGetValue(emotion, out var gauge)) return;

        // 現在値を更新
        var currentValue = _currentResourceValues.GetValueOrDefault(emotion, 0);
        var newValue = currentValue + rewardAmount;
        _currentResourceValues[emotion] = newValue;

        // 報酬量を記録
        _rewardedAmounts[emotion] = _rewardedAmounts.GetValueOrDefault(emotion, 0) + rewardAmount;

        // 最大値を取得
        var max = _maxResources.TryGetValue(emotion, out var m) ? m : 1;

        // アニメーションで変更
        gauge.AnimateToValue(newValue, max);
    }

    private void ClearInstantiatedCards()
    {
        foreach (var card in _instantiatedCards)
            Destroy(card.gameObject);
        _instantiatedCards.Clear();
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
