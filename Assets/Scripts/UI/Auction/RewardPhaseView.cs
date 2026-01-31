using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using R3;
using VoidRed.UI.Views;

[RequireComponent(typeof(CanvasGroup))]
public class RewardPhaseView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private EmotionGaugeView[] emotionGauges;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly List<CardView> _instantiatedCards = new();
    private int _totalReward;

    public void Hide()
    {
        // CanvasGroupを無効化して非表示
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        // 生成したカードを削除
        ClearInstantiatedCards();
    }

    public async UniTask ShowRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results,
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        // CanvasGroupを有効化して表示
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);

        // 既存のカードをクリア
        ClearInstantiatedCards();

        // 感情バーの初期化
        InitializeEmotionGauges(currentResources, maxResources);

        _totalReward = 0;

        foreach (var (card, result) in results)
        {
            _totalReward += result.TotalReward;

            // カードをコンテナに生成
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(card.Data);
            cardView.SetInteractable(false);
            _instantiatedCards.Add(cardView);

            await _onNextButtonClicked.FirstAsync();
        }

        await _onNextButtonClicked.FirstAsync();
    }

    private void InitializeEmotionGauges(
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        var emotionTypes = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));

        for (var i = 0; i < emotionGauges.Length && i < emotionTypes.Length; i++)
        {
            var emotion = emotionTypes[i];
            var gauge = emotionGauges[i];

            // 色を設定
            gauge.SetColor(emotion.GetColor());

            // 現在値/最大値を正規化して設定
            var current = currentResources.TryGetValue(emotion, out var c) ? c : 0;
            var max = maxResources.TryGetValue(emotion, out var m) ? m : 1;
            var normalizedValue = max > 0 ? (float)current / max : 0f;
            gauge.SetValue(normalizedValue);
        }
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
