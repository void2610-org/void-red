using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

/// <summary>
/// オークション結果判定・リソース消費・競合フェーズの実行を統合管理する
/// </summary>
public class AuctionProcessor
{
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CompetitionPhaseRunner _competitionPhaseRunner;
    private readonly BattleUIPresenter _uiPresenter;

    public AuctionProcessor(Player player, Enemy enemy, BattleUIPresenter uiPresenter,
        CompetitionPhaseRunner competitionPhaseRunner)
    {
        _player = player;
        _enemy = enemy;
        _competitionPhaseRunner = competitionPhaseRunner;
        _uiPresenter = uiPresenter;
    }

    /// <summary>
    /// オークション結果の判定・処理・競合を全て実行
    /// </summary>
    public async UniTask ProcessAuctionResultAsync(
        IReadOnlyList<CardModel> auctionCards,
        EnemyData enemyData,
        ReactiveProperty<GameState> currentGameState)
    {
        Debug.Log("[AuctionProcessor] 落札者判定フェーズ開始");

        // AuctionViewを再表示
        _uiPresenter.ShowAuctionView();

        // 全カードの落札者を判定
        var results = AuctionJudge.JudgeAll(auctionCards, _player.Bids, _enemy.Bids);

        // 結果を格納しリソースを処理
        _player.ClearWonCards();
        _enemy.ClearWonCards();

        var drawResults = new List<AuctionJudge.AuctionResultEntry>();

        foreach (var result in results)
        {
            if (result.NoBids)
            {
                Debug.Log($"[AuctionProcessor] {result.Card.Data.CardName}: 入札なし");
                continue;
            }

            if (result.IsDraw)
            {
                // 競合リストに追加し、元の入札分は両者即消費（競合フェーズでの再利用を防ぐ）
                drawResults.Add(result);
                ConsumeBidForCard(_player, result.Card);
                ConsumeBidForCard(_enemy, result.Card);
                Debug.Log($"[AuctionProcessor] {result.Card.Data.CardName}: 引き分け → 競合へ（{result.PlayerBid} vs {result.EnemyBid}）");
                continue;
            }

            if (result.IsPlayerWon)
            {
                // 勝者: リソース消費
                ConsumeBidForCard(_player, result.Card);
                _player.AddWonCard(result.Card);
                Debug.Log($"[AuctionProcessor] {result.Card.Data.CardName}: プレイヤー落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
            else
            {
                // 敵勝者: リソース消費
                ConsumeBidForCard(_enemy, result.Card);
                _enemy.AddWonCard(result.Card);
                Debug.Log($"[AuctionProcessor] {result.Card.Data.CardName}: 敵落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
        }

        Debug.Log($"[AuctionProcessor] 落札結果: プレイヤー{_player.WonCards.Count}枚、敵{_enemy.WonCards.Count}枚、競合{drawResults.Count}枚");

        // 順次演出で結果を表示
        await _uiPresenter.ShowAuctionResultsSequentialAsync(results, enemyData.EnemyColor);

        // 競合カードがある場合は競合フェーズへ
        if (drawResults.Count > 0)
        {
            _uiPresenter.HideAuctionView();

            currentGameState.Value = GameState.CompetitionPhase;
            foreach (var drawResult in drawResults)
                await HandleSingleCompetition(drawResult);
        }

        await UniTask.Delay(1000);
        // オークション完全終了時にクリア
        _uiPresenter.ClearAuctionView();
    }

    /// <summary>
    /// 単一カードの競合フェーズ実行
    /// </summary>
    private async UniTask HandleSingleCompetition(AuctionJudge.AuctionResultEntry drawResult)
    {
        var handler = await _competitionPhaseRunner.RunAsync(
            drawResult.Card,
            drawResult.PlayerBid,
            drawResult.EnemyBid,
            "競合発生！");

        ProcessCompetitionResult(handler, drawResult.Card);
    }

    /// <summary>
    /// 競合結果処理（勝者判定・カード付与）
    /// </summary>
    private void ProcessCompetitionResult(CompetitionHandler handler, CardModel card)
    {
        var winner = handler.IsPlayerWon;
        if (winner == true)
        {
            _player.AddWonCard(card);
            // 敗者（敵）の上乗せ分を返却
            ReturnRaises(_enemy, handler.EnemyRaises);
            Debug.Log($"[AuctionProcessor] 競合勝利: {card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else if (winner == false)
        {
            _enemy.AddWonCard(card);
            // 敗者（プレイヤー）の上乗せ分を返却
            ReturnRaises(_player, handler.PlayerRaises);
            Debug.Log($"[AuctionProcessor] 競合敗北: {card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else
        {
            // 完全引き分け: 両者の上乗せ分を返却、カード消失
            ReturnRaises(_player, handler.PlayerRaises);
            ReturnRaises(_enemy, handler.EnemyRaises);
            Debug.Log($"[AuctionProcessor] 競合引き分け: {card.Data.CardName} カード消失（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
    }

    /// <summary>
    /// 特定カードの入札分のリソースを消費する（勝者のみ）
    /// </summary>
    private static void ConsumeBidForCard(PlayerPresenter player, CardModel card)
    {
        var bidsByEmotion = player.Bids.GetBidsByEmotion(card);
        foreach (var (emotion, amount) in bidsByEmotion)
        {
            player.TryConsumeEmotion(emotion, amount);
            Debug.Log($"[AuctionProcessor] リソース消費: {emotion} -{amount}");
        }
    }

    /// <summary>
    /// 競合の上乗せ分のリソースを返却する
    /// </summary>
    private static void ReturnRaises(PlayerPresenter player, IReadOnlyList<EmotionType> raises)
    {
        foreach (var emotion in raises)
        {
            player.AddEmotion(emotion, 1);
            Debug.Log($"[AuctionProcessor] リソース返却: {emotion} +1");
        }
    }
}
