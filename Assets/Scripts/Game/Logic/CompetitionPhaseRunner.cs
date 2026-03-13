using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// オークション/バトル共通の競合フェーズ進行を担当する
/// </summary>
public class CompetitionPhaseRunner
{
    private readonly Player _player;
    private readonly IEnemyAIController _enemyAI;
    private readonly BattleUIPresenter _uiPresenter;

    public CompetitionPhaseRunner(Player player, IEnemyAIController enemyAI, BattleUIPresenter uiPresenter)
    {
        _player = player;
        _enemyAI = enemyAI;
        _uiPresenter = uiPresenter;
    }

    /// <summary>
    /// 競合フェーズを最後まで進行し、最終結果を持つハンドラを返す
    /// </summary>
    /// <param name="card">競合対象のカード</param>
    /// <param name="playerTotal">プレイヤー側の初期値</param>
    /// <param name="enemyTotal">敵側の初期値</param>
    /// <param name="instruction">競合開始時に表示する文言</param>
    public virtual async UniTask<CompetitionHandler> RunAsync(CardModel card, int playerTotal, int enemyTotal, string instruction)
    {
        var handler = new CompetitionHandler();
        handler.Start(card, playerTotal, enemyTotal);

        var selectedEmotion = EmotionType.Joy;
        using var disposables = new CompositeDisposable();

        _uiPresenter.SetBattleInstruction(instruction);
        _uiPresenter.ShowCompetition(handler.PlayerTotal, handler.EnemyTotal, _player.EmotionResources);

        _uiPresenter.OnCompetitionRaise
            .Subscribe(_ =>
            {
                if (!handler.TryPlayerRaise(selectedEmotion, _player)) return;

                SeManager.Instance.PlaySe(selectedEmotion.ToResourceSeName(), pitch: 1f);
                _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
                _uiPresenter.UpdateCompetitionResources(_player.EmotionResources);
            })
            .AddTo(disposables);

        _uiPresenter.OnCompetitionEmotionSelected
            .Subscribe(emotion => selectedEmotion = emotion)
            .AddTo(disposables);

        var nextEnemyRaiseTime = Time.time + Random.Range(2f, 5f);
        while (!handler.IsTimedOut)
        {
            _uiPresenter.UpdateCompetitionTimer(handler.RemainingTime, GameConstants.COMPETITION_TIMEOUT_SECONDS);

            if (Time.time >= nextEnemyRaiseTime)
            {
                _enemyAI.TryCompetitionRaise(handler);
                nextEnemyRaiseTime = Time.time + Random.Range(2f, 5f);
                _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
            }

            await UniTask.Yield();
        }

        handler.End();
        _uiPresenter.HideCompetition();
        await UniTask.Delay(500);
        return handler;
    }
}
