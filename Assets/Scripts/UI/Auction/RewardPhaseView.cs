using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 報酬フェーズのコーディネーター
/// CardAcquisitionView → ResourceRewardView の順にサブViewを呼び出す
/// </summary>
public class RewardPhaseView : BasePhaseView
{
    [SerializeField] private CardAcquisitionView cardAcquisitionView;
    [SerializeField] private ResourceRewardView resourceRewardView;

    /// <summary>
    /// 報酬演出で各感情タイプに加算された量を取得
    /// </summary>
    public IReadOnlyDictionary<EmotionType, int> RewardedAmounts => resourceRewardView.RewardedAmounts;

    public void DisplayResourceGauges(IReadOnlyDictionary<EmotionType, int> currentResources, IReadOnlyDictionary<EmotionType, int> maxResources) => resourceRewardView.DisplayGauges(currentResources, maxResources);

    public async UniTask AnimateResourceRewardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results) => await resourceRewardView.AnimateRewardsAsync(results);

    public async UniTask WaitForCardAcquisitionCompleteAsync() => await cardAcquisitionView.WaitForNextAndHideAsync();

    public override void Hide()
    {
        cardAcquisitionView.Hide();
        resourceRewardView.Hide();
        base.Hide();
    }

    public async UniTask DisplayCardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results)
    {
        Show();
        var cardDataList = results.Keys.Select(card => card.Data);
        await cardAcquisitionView.DisplayCardsAsync(cardDataList);
    }
}
