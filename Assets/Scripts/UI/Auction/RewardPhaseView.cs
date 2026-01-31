using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using R3;

[RequireComponent(typeof(CanvasGroup))]
public class RewardPhaseView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardInfoText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button nextButton;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private int _totalReward;

    public void Hide()
    {
        // CanvasGroupを無効化して非表示
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public async UniTask ShowRewardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results)
    {
        // CanvasGroupを有効化して表示
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);

        _totalReward = 0;

        foreach (var (card, result) in results)
        {
            _totalReward += result.TotalReward;
            ShowCardReward(card, result);
            await _onNextButtonClicked.FirstAsync();
        }

        rewardInfoText.text = $"【報酬確定】\n\n合計報酬: {_totalReward}";
        await _onNextButtonClicked.FirstAsync();
    }

    private void ShowCardReward(CardModel card, RewardCalculator.RewardResult result)
    {
        var relativeSign = result.RelativeReward >= 0 ? "+" : "";
        var ownCardLine = result.IsOwnCard ? $"\n自カードボーナス: +{result.OwnCardBonus}" : "";

        rewardInfoText.text = $"【{card.Data.CardName}】\n" +
                              $"順位: {result.ValueRank}  入札: {result.BidAmount}\n" +
                              $"基本報酬: {result.BaseReward}\n" +
                              $"相対報酬: {relativeSign}{result.RelativeReward}" +
                              $"{ownCardLine}\n" +
                              $"= {result.TotalReward}\n\n" +
                              $"合計: {_totalReward}";
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
