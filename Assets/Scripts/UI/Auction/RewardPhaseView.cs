using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;

public class RewardPhaseView : MonoBehaviour
{
    [Header("表示")]
    [SerializeField] private TextMeshProUGUI rewardInfoText;

    [Header("次へボタン")]
    [SerializeField] private Button nextButton;

    [Header("演出設定")]
    [SerializeField] private float cardDisplayDuration = 1.5f;
    [SerializeField] private float rewardCountDuration = 1f;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private int _totalReward;
    
    public async UniTask WaitForNextAsync() => await _onNextButtonClicked.FirstAsync();

    public async UniTask ShowRewardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results)
    {
        gameObject.SetActive(true);
        _totalReward = 0;

        foreach (var (card, result) in results)
        {
            _totalReward += result.TotalReward;
            ShowCardReward(card, result);
            await UniTask.Delay((int)(cardDisplayDuration * 1000));
        }

        await PlayCountUpAsync(0, _totalReward, rewardCountDuration);
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

    private async UniTask PlayCountUpAsync(int from, int to, float duration)
    {
        await LMotion.Create(from, to, duration)
            .WithEase(Ease.OutCubic)
            .Bind(value => rewardInfoText.text = $"【報酬確定】\n\n合計報酬: {value}")
            .ToUniTask();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    private void Awake()
    {
        nextButton.OnClickAsObservable().Subscribe(_ => _onNextButtonClicked.OnNext(Unit.Default)).AddTo(this);
    }

    private void OnDestroy()
    {
        _onNextButtonClicked.Dispose();
    }
}
