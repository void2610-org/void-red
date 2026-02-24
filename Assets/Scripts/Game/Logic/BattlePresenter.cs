using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class BattlePresenter : IStartable, ISceneInitializable
{
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    private readonly BattleUIPresenter _battleUIPresenter;
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly GameProgressService _gameProgressService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    private readonly AllAuctionData _allAuctionData;

    private readonly UniTaskCompletionSource _initializationComplete = new();

    private EnemyData _currentEnemyData;
    private ThemeData _currentTheme;
    private AuctionData _currentAuctionData;
    private readonly List<CardModel> _auctionCards = new();

    private readonly ReactiveProperty<GameState> _currentGameState = new(GameState.ThemeAnnouncement);

    public BattlePresenter(
        BattleUIPresenter battleUIPresenter,
        Player player,
        Enemy enemy,
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllAuctionData allAuctionData)
    {
        _battleUIPresenter = battleUIPresenter;
        _player = player;
        _enemy = enemy;
        _gameProgressService = gameProgressService;
        _sceneTransitionManager = sceneTransitionManager;
        _allAuctionData = allAuctionData;

        InitializeAuctionData();
    }

    public UniTask WaitForInitializationAsync() => _initializationComplete.Task;

    private void InitializeAuctionData()
    {
        _auctionCards.Clear();

        // Player/Enemyの状態をリセット
        _player.ResetPlayerState();
        _enemy.ResetPlayerState();
    }

    private async UniTaskVoid InitializeGameAsync()
    {
        if (!await InitializeGame()) return;
        _initializationComplete.TrySetResult();
        await StartGame();
    }

    private async UniTask<bool> InitializeGame()
    {
        await UniTask.Delay(500);

        // GameProgressServiceからノード情報を取得
        var currentNode = _gameProgressService.GetCurrentNode();
        if (currentNode is not BattleNode battleNode)
        {
            Debug.LogError("[BattlePresenter] 現在のノードがBattleNodeではありません");
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
            return false;
        }

        // オークションデータを取得
        _currentAuctionData = _allAuctionData.GetAuctionById(battleNode.AuctionId);
        if (!_currentAuctionData)
        {
            Debug.LogError("[BattlePresenter] オークションデータが見つかりません");
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
            return false;
        }

        // オークションデータから敵データを取得
        _currentEnemyData = _currentAuctionData.Enemy;
        if (!_currentEnemyData)
        {
            Debug.LogError("[BattlePresenter] 敵データが見つかりません");
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
            return false;
        }

        // 敵を初期化して表示
        _enemy.SetEnemyData(_currentEnemyData);
        _battleUIPresenter.InitializeEnemy(_currentEnemyData);

        // 敵情報をアナウンス
        await _battleUIPresenter.ShowAnnouncement(_currentEnemyData.EnemyName, 1.5f);
        return true;
    }

    private async UniTask StartGame()
    {
        await UniTask.Delay(1000);

        // 1. テーマ公開
        _currentGameState.Value = GameState.ThemeAnnouncement;
        await HandleThemeAnnouncement();

        // 2. カード提示（6枚を場に並べる）
        _currentGameState.Value = GameState.CardReveal;
        await HandleCardReveal();

        // 3. 対話フェーズ（入札前に実施）
        _currentGameState.Value = GameState.DialoguePhase;
        await HandleDialoguePhase();

        // 4. 入札フェーズ
        _currentGameState.Value = GameState.BiddingPhase;
        await HandleBiddingPhase();

        // 5. 落札者判定フェーズ
        _currentGameState.Value = GameState.AuctionResult;
        await HandleAuctionResult();

        // 6. 報酬フェーズ
        _currentGameState.Value = GameState.RewardPhase;
        await HandleRewardPhase();

        // 7. 記憶育成フェーズ
        _currentGameState.Value = GameState.MemoryGrowth;
        await HandleMemoryGrowth();

        // 終了
        _currentGameState.Value = GameState.BattleEnd;
        await HandleBattleEnd();
    }

    // === 1. テーマ公開 ===

    private async UniTask HandleThemeAnnouncement()
    {
        // オークションデータからテーマを取得
        _currentTheme = _currentAuctionData.Theme;

        // 記憶テーマを表示
        await _battleUIPresenter.SetTheme(_currentTheme, isMainTheme: true);

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("BeforeThemeAnnouncement");
    }

    // === 2. カード提示（6枚共有表示） ===

    private async UniTask HandleCardReveal()
    {
        Debug.Log("[BattlePresenter] カード提示フェーズ開始");

        // AuctionDataから6枚取得（プレイヤー/敵の区別なし）
        _auctionCards.Clear();
        foreach (var cardData in _currentAuctionData.AuctionCards)
        {
            _auctionCards.Add(new CardModel(cardData));
        }

        Debug.Log($"[BattlePresenter] オークション対象カード: {_auctionCards.Count}枚");

        // カードを表示
        _battleUIPresenter.ShowAuctionCards(_auctionCards, _player.EmotionResources);

        // トランジション：開く（黒フェードから復帰）
        await _battleUIPresenter.PlayPhaseTransitionOpenAsync();

        await UniTask.Delay(1500);
    }

    // === 3. 対話フェーズ（仮実装） ===

    private async UniTask HandleDialoguePhase()
    {
        Debug.Log("[BattlePresenter] 対話フェーズ開始（仮実装: 対話ボタン→ログ出力のみ）");

        // TODO: AuctionCardViewの対話ボタンによる本実装に置き換え
        await UniTask.Delay(500);

        Debug.Log("[BattlePresenter] 対話フェーズ終了");
    }

    // === 4. 入札フェーズ ===

    private async UniTask HandleBiddingPhase()
    {
        Debug.Log("[BattlePresenter] 入札フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("BiddingPhase");

        // 敵AIで入札を決定
        _enemy.DecideBids(_auctionCards);
        Debug.Log($"[BattlePresenter] 敵の入札完了: 合計{_enemy.Bids.GetTotalBidAmount()}リソース");

        // プレイヤーの入札UI表示・待機
        await _battleUIPresenter.WaitForBiddingAsync(
            _auctionCards,
            _player.Bids,
            EmotionType.Joy,
            _player.EmotionResources);

        Debug.Log($"[BattlePresenter] プレイヤーの入札完了: 合計{_player.Bids.GetTotalBidAmount()}リソース");

        // 入札対象カード公開演出
        await _battleUIPresenter.ShowBidTargetsAsync(_player.Bids, _enemy.Bids, 2f);
    }

    // === 5. 落札者判定フェーズ ===

    private async UniTask HandleAuctionResult()
    {
        Debug.Log("[BattlePresenter] 落札者判定フェーズ開始");

        // AuctionViewを再表示
        _battleUIPresenter.ShowAuctionView();

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("ResultDetermination");

        // 全カードの落札者を判定
        var results = AuctionJudge.JudgeAll(_auctionCards, _player.Bids, _enemy.Bids);

        // 結果を格納しリソースを処理
        _player.ClearWonCards();
        _enemy.ClearWonCards();

        var drawResults = new List<AuctionJudge.AuctionResultEntry>();

        foreach (var result in results)
        {
            if (result.NoBids)
            {
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: 入札なし");
                continue;
            }

            if (result.IsDraw)
            {
                // 競合リストに追加（後で競合フェーズで処理）
                drawResults.Add(result);
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: 引き分け → 競合へ（{result.PlayerBid} vs {result.EnemyBid}）");
                continue;
            }

            if (result.IsPlayerWon)
            {
                // 勝者: リソース消費
                ConsumeBidForCard(_player, result.Card);
                _player.AddWonCard(result.Card);
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: プレイヤー落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
            else
            {
                // 敵勝者: リソース消費
                ConsumeBidForCard(_enemy, result.Card);
                _enemy.AddWonCard(result.Card);
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: 敵落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
        }

        Debug.Log($"[BattlePresenter] 落札結果: プレイヤー{_player.WonCards.Count}枚、敵{_enemy.WonCards.Count}枚、競合{drawResults.Count}枚");

        // 順次演出で結果を表示
        await _battleUIPresenter.ShowAuctionResultsSequentialAsync(results, _currentEnemyData.EnemyColor);

        // 競合カードがある場合は競合フェーズへ
        if (drawResults.Count > 0)
        {
            _currentGameState.Value = GameState.CompetitionPhase;
            await HandleCompetitions(drawResults);
        }

        await UniTask.Delay(1000);
        // オークション完全終了時にクリア
        _battleUIPresenter.ClearAuctionView();
    }

    // === 5.5 競合フェーズ ===

    private async UniTask HandleCompetitions(List<AuctionJudge.AuctionResultEntry> drawResults)
    {
        _battleUIPresenter.HideAuctionView();

        foreach (var drawResult in drawResults)
        {
            await HandleSingleCompetition(drawResult);
        }
    }

    private async UniTask HandleSingleCompetition(AuctionJudge.AuctionResultEntry drawResult)
    {
        var handler = new CompetitionHandler();
        handler.Start(drawResult.Card, drawResult.PlayerBid, drawResult.EnemyBid);

        var selectedEmotion = EmotionType.Joy;
        var disposables = new CompositeDisposable();

        // 競合UI表示
        _battleUIPresenter.ShowCompetition(
            handler.PlayerTotal, handler.EnemyTotal, _player.EmotionResources);

        // プレイヤー上乗せボタン
        _battleUIPresenter.OnCompetitionRaise
            .Subscribe(_ =>
            {
                if (handler.TryPlayerRaise(selectedEmotion, _player))
                {
                    _battleUIPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
                    _battleUIPresenter.UpdateCompetitionResources(_player.EmotionResources);
                }
            })
            .AddTo(disposables);

        // 感情選択変更
        _battleUIPresenter.OnCompetitionEmotionSelected
            .Subscribe(emotion => selectedEmotion = emotion)
            .AddTo(disposables);

        // 敵AIの次回上乗せ時刻
        var nextEnemyRaiseTime = Time.time + Random.Range(2f, 5f);

        // 競合ループ（タイムアウトまで）
        while (!handler.IsTimedOut)
        {
            // タイマー更新
            _battleUIPresenter.UpdateCompetitionTimer(
                handler.RemainingTime, GameConstants.COMPETITION_TIMEOUT_SECONDS);

            // 敵AI上乗せ判定
            if (Time.time >= nextEnemyRaiseTime)
            {
                TryEnemyCompetitionRaise(handler);
                nextEnemyRaiseTime = Time.time + Random.Range(2f, 5f);

                if (handler.EnemyTotal != drawResult.EnemyBid)
                {
                    _battleUIPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
                }
            }

            await UniTask.Yield();
        }

        // 競合終了
        handler.End();
        disposables.Dispose();

        var winner = handler.IsPlayerWon;
        if (winner == true)
        {
            // プレイヤー勝利: 元の入札分を消費（上乗せ分はリアルタイムで消費済み）
            ConsumeBidForCard(_player, drawResult.Card);
            _player.AddWonCard(drawResult.Card);
            Debug.Log($"[BattlePresenter] 競合勝利: {drawResult.Card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else if (winner == false)
        {
            // 敵勝利: 敵の元の入札分を消費
            ConsumeBidForCard(_enemy, drawResult.Card);
            _enemy.AddWonCard(drawResult.Card);
            Debug.Log($"[BattlePresenter] 競合敗北: {drawResult.Card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else
        {
            // 完全引き分け: カード消失
            Debug.Log($"[BattlePresenter] 競合引き分け: {drawResult.Card.Data.CardName} カード消失（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }

        handler.Dispose();
        _battleUIPresenter.HideCompetition();
        await UniTask.Delay(500);
    }

    /// <summary>
    /// 敵AIの競合時上乗せ判定
    /// </summary>
    private void TryEnemyCompetitionRaise(CompetitionHandler handler)
    {
        // 50%の確率で上乗せしない
        if (Random.value < 0.5f) return;

        // リソースが残っている感情からランダムに選択
        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        var available = new List<EmotionType>();
        foreach (var emotion in emotions)
        {
            if (_enemy.GetEmotionAmount(emotion) > 0)
                available.Add(emotion);
        }

        if (available.Count == 0) return;

        var chosen = available[Random.Range(0, available.Count)];
        handler.EnemyRaise(chosen, _enemy);
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
            Debug.Log($"[BattlePresenter] リソース消費: {emotion} -{amount}");
        }
    }

    // === 6. 報酬フェーズ ===

    private async UniTask HandleRewardPhase()
    {
        Debug.Log("[BattlePresenter] 報酬フェーズ開始");

        var isTutorial = _currentEnemyData.EnemyId == "alv";

        // プレイヤーの報酬を計算
        var rewardResults = RewardCalculator.CalculateAll(
            _player.WonCards,
            _player.Bids);

        // 最大リソース値を設定（デフォルト値の3倍を仮の上限とする）
        var maxResources = new Dictionary<EmotionType, int>();
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            maxResources[emotion] = GameConstants.DEFAULT_EMOTION_VALUE * 3;
        }

        await _battleUIPresenter.DisplayCardsAsync(rewardResults);

        if (isTutorial)
            await _battleUIPresenter.StartTutorial("RewardPhase");

        await _battleUIPresenter.WaitForCardAcquisitionCompleteAsync();

        _battleUIPresenter.DisplayResourceGauges(_player.EmotionResources, maxResources);

        if (isTutorial)
            await _battleUIPresenter.StartTutorial("RewardPhase2");

        var rewardedAmounts = await _battleUIPresenter.AnimateResourceRewardsAsync(rewardResults);

        // 報酬を各感情リソースに加算
        foreach (var (emotion, amount) in rewardedAmounts)
        {
            if (amount > 0)
            {
                _player.AddEmotion(emotion, amount);
                Debug.Log($"[BattlePresenter] プレイヤーに報酬付与: {emotion} +{amount}リソース");
            }
        }

        await UniTask.Delay(2000);
    }

    // === 7. 記憶育成フェーズ ===

    private async UniTask HandleMemoryGrowth()
    {
        Debug.Log("[BattlePresenter] 記憶育成フェーズ開始");

        // 全カードの獲得情報を構築（入札がないカードは除外）
        var allCardInfoList = BuildCardAcquisitionInfoList();

        // 入札されたカードがない場合はスキップ
        if (allCardInfoList.Count == 0)
        {
            Debug.Log("[BattlePresenter] 入札カードなし - 記憶育成フェーズをスキップ");
            _battleUIPresenter.HideRewardView();
            return;
        }

        // 使用した感情リソースを計算（プレイヤーの全入札から集計）
        var usedEmotions = CalculateUsedEmotions();

        // 獲得テーマを作成
        var acquiredTheme = new AcquiredTheme(
            _currentTheme,
            allCardInfoList,
            usedEmotions);

        Debug.Log($"[BattlePresenter] 支配的感情: {acquiredTheme.DominantEmotionResult}");
        Debug.Log($"[BattlePresenter] 獲得テーマ作成: {acquiredTheme.ThemeName}");
        Debug.Log($"[BattlePresenter] 勝利{acquiredTheme.WonCount}枚、敗北{acquiredTheme.LostCount}枚");

        // セーブデータに記録
        _gameProgressService.RecordAcquiredThemeAndSave(acquiredTheme);

        // 全獲得テーマを取得
        var allThemes = _gameProgressService.GetAcquiredThemes();
        Debug.Log($"[BattlePresenter] 全獲得テーマ数: {allThemes.Count}");

        _battleUIPresenter.ShowMemoryGrowthView(allThemes);
        _battleUIPresenter.HideRewardView();

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("MemoryGrowthPhase");

        await _battleUIPresenter.WaitForMemoryGrowthCompleteAsync();
    }

    /// <summary>
    /// 全カードの獲得情報リストを構築
    /// </summary>
    private List<CardAcquisitionInfo> BuildCardAcquisitionInfoList()
    {
        var result = new List<CardAcquisitionInfo>();

        foreach (var card in _auctionCards)
        {
            var playerBids = _player.Bids.GetBidsByEmotion(card);
            var enemyBids = _enemy.Bids.GetBidsByEmotion(card);

            // どちらも入札していないカードは除外
            if (playerBids.Count == 0 && enemyBids.Count == 0)
                continue;

            var playerWon = _player.WonCards.Contains(card);

            var cardInfo = new CardAcquisitionInfo(
                card,
                playerBids,
                enemyBids,
                playerWon
            );
            result.Add(cardInfo);
        }

        return result;
    }

    /// <summary>
    /// 使用した感情リソースを計算（プレイヤーの全入札から集計）
    /// </summary>
    private Dictionary<EmotionType, int> CalculateUsedEmotions()
    {
        var result = new Dictionary<EmotionType, int>();

        foreach (var card in _auctionCards)
        {
            var bidsByEmotion = _player.Bids.GetBidsByEmotion(card);
            foreach (var kvp in bidsByEmotion)
            {
                if (!result.ContainsKey(kvp.Key))
                    result[kvp.Key] = 0;
                result[kvp.Key] += kvp.Value;
            }
        }

        return result;
    }

    // === 終了 ===

    private async UniTask HandleBattleEnd()
    {
        await UniTask.Delay(500);

        // Volumeエフェクトを全てデフォルトに戻す
        VolumeController.Instance.ResetToDefault();

        // 現在のノード情報を一旦キャッシュ
        var currentNode = _gameProgressService.GetCurrentNode();

        // バトル完了を記録（勝敗なし）
        _gameProgressService.RecordNovelResultAndSave();

        // ノード設定に基づいてシーン遷移
        await UniTask.Delay(1000);
        if (currentNode.ReturnToHome)
        {
            await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
        }
        else
        {
            var nextScene = _gameProgressService.GetNextSceneType();
            await _sceneTransitionManager.TransitionToSceneWithFade(nextScene);
        }
    }

    public void Start()
    {
        _battleUIPresenter.SetBattlePresenter(this);

        InitializeGameAsync().Forget();
        BgmManager.Instance.PlayBGM("Battle");
    }
}
