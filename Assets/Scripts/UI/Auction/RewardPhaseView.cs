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

    public override void Hide()
    {
        cardAcquisitionView.Hide();
        resourceRewardView.Hide();
        base.Hide();
    }

    public async UniTask ShowRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results,
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        Show();

        // 獲得カード一覧を表示
        var cardDataList = results.Keys.Select(card => card.Data);
        await cardAcquisitionView.ShowCardsAsync(cardDataList);

        // リソース報酬をゲージアニメーションで表示
        await resourceRewardView.ShowRewardsAsync(results, currentResources, maxResources);
    }
}
