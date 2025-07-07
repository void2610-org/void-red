using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// カードの進化・劣化を管理するクラス
/// </summary>
public class EvolutionManager
{
    private readonly StatsTracker _statsTracker;
    private readonly UIPresenter _uIPresenter;
    
    public EvolutionManager(StatsTracker statsTracker, UIPresenter uIPresenter)
    {
        _uIPresenter = uIPresenter;
        _statsTracker = statsTracker;
    }
    
    /// <summary>
    /// 進化可能なカードをチェックして進化を実行
    /// </summary>
    /// <param name="deckModel">デッキモデル</param>
    /// <returns>進化したカードのリスト</returns>
    private List<(CardData original, CardData evolved)> CheckAndEvolveCards(DeckModel deckModel)
    {
        var evolvedCards = new List<(CardData, CardData)>();
        
        // デッキ内の全カードをチェック
        var cardsToEvolve = new List<(int index, CardData original, CardData target)>();
        
        for (int i = 0; i < deckModel.AllCards.Count; i++)
        {
            var card = deckModel.AllCards[i];
            if (_statsTracker.CanCardEvolve(card))
            {
                cardsToEvolve.Add((i, card, card.EvolutionTarget));
            }
        }
        
        // 進化を実行
        foreach (var (index, original, target) in cardsToEvolve)
        {
            deckModel.ReplaceCard(index, target);
            evolvedCards.Add((original, target));
        }
        
        return evolvedCards;
    }
    
    /// <summary>
    /// 劣化可能なカードをチェックして劣化を実行
    /// </summary>
    /// <param name="deckModel">デッキモデル</param>
    /// <returns>劣化したカードのリスト</returns>
    private List<(CardData original, CardData degraded)> CheckAndDegradeCards(DeckModel deckModel)
    {
        var degradedCards = new List<(CardData, CardData)>();
        
        // デッキ内の全カードをチェック
        var cardsToDegrade = new List<(int index, CardData original, CardData target)>();
        
        for (var i = 0; i < deckModel.AllCards.Count; i++)
        {
            var card = deckModel.AllCards[i];
            if (_statsTracker.CanCardDegrade(card))
            {
                cardsToDegrade.Add((i, card, card.DegradationTarget));
            }
        }
        
        // 劣化を実行
        foreach (var (index, original, target) in cardsToDegrade)
        {
            deckModel.ReplaceCard(index, target);
            degradedCards.Add((original, target));
        }
        
        return degradedCards;
    }
    
    /// <summary>
    /// プレイヤーのデッキ内の全カードの進化・劣化チェックを実行（ゲーム終了時に呼ぶ）
    /// </summary>
    /// <param name="deckModel">デッキモデル</param>
    /// <returns>進化または劣化が発生したかどうか</returns>
    public async UniTask ProcessEvolutions(DeckModel deckModel)
    {
        // まず進化をチェック（進化優先）
        var evolvedCards = CheckAndEvolveCards(deckModel);
        // 進化しなかったカードのみ劣化をチェック
        var degradedCards = new List<(CardData, CardData)>();
        if (evolvedCards.Count == 0)
            degradedCards = CheckAndDegradeCards(deckModel);
        
        foreach (var (original, evolved) in evolvedCards)
        {
           await _uIPresenter.ShowAnnouncement($"{original.CardName} が {evolved.CardName} に進化しました！", 2f);
           await UniTask.Delay(200);
        }
        foreach (var (original, degraded) in degradedCards)
        {
            await _uIPresenter.ShowAnnouncement($"{original.CardName} が {degraded.CardName} に劣化しました...", 2f);
            await UniTask.Delay(200);
        }
    }
}