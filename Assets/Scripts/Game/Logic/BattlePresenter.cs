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

    // バトル関連
    private Dictionary<CardModel, CardNumberAssigner.CardNumberInfo> _cardNumbers;

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

        // ===== オークションパート =====

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

        // ===== リザルトパート =====

        // 6. リザルトフェーズ（獲得カード表示 + 感情リソース報酬 + 感情状態判定）
        _currentGameState.Value = GameState.ResultPhase;
        var playerEmotionState = await HandleResultPhase();

        // ===== バトルパート =====

        // 7. カード数字割り当て
        _cardNumbers = CardNumberAssigner.AssignNumbers(_auctionCards, _player.Bids, _enemy.Bids);

        // 8. スキルボタン初期化 → デッキ選択
        _battleUIPresenter.InitializeSkillButton(playerEmotionState);
        _currentGameState.Value = GameState.DeckSelection;
        var (playerDeck, enemyDeck) = await HandleDeckSelection();

        // 9. カードバトル（3本勝負）
        _currentGameState.Value = GameState.CardBattle;
        await HandleCardBattle(playerDeck, enemyDeck, playerEmotionState);

        // ===== 終了処理 =====

        // 10. 記憶育成フェーズ
        _currentGameState.Value = GameState.MemoryGrowth;
        await HandleMemoryGrowth();

        // 12. 終了
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
    }

    // === 3. 対話フェーズ（入札と同時進行） ===

    private async UniTask HandleDialoguePhase()
    {
        // 対話ボタンは入札フェーズ中にカード上で利用可能
        // TODO: 対話ボタン押下時の本実装に置き換え
        await UniTask.CompletedTask;
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
        await _battleUIPresenter.WaitForBiddingAsync(_auctionCards, _player.Bids, EmotionType.Joy, _player.EmotionResources);

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

                _battleUIPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
            }

            await UniTask.Yield();
        }

        // 競合終了
        handler.End();
        disposables.Dispose();

        ProcessCompetitionResult(handler, drawResult.Card);

        _battleUIPresenter.HideCompetition();
        await UniTask.Delay(500);
    }

    /// <summary>
    /// 競合結果を処理（勝者判定・カード付与・ログ出力）
    /// </summary>
    private void ProcessCompetitionResult(CompetitionHandler handler, CardModel card)
    {
        var winner = handler.IsPlayerWon;
        if (winner == true)
        {
            ConsumeBidForCard(_player, card);
            _player.AddWonCard(card);
            Debug.Log($"[BattlePresenter] 競合勝利: {card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else if (winner == false)
        {
            ConsumeBidForCard(_enemy, card);
            _enemy.AddWonCard(card);
            Debug.Log($"[BattlePresenter] 競合敗北: {card.Data.CardName}（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
        else
        {
            Debug.Log($"[BattlePresenter] 競合引き分け: {card.Data.CardName} カード消失（{handler.PlayerTotal} vs {handler.EnemyTotal}）");
        }
    }

    /// <summary>
    /// 敵AIの競合時上乗せ判定
    /// </summary>
    private void TryEnemyCompetitionRaise(CompetitionHandler handler)
    {
        // 既にプレイヤーより多い場合は無駄に消費しない
        if (handler.EnemyTotal > handler.PlayerTotal) return;

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

    // === 6. リザルトフェーズ ===

    private async UniTask<EmotionType> HandleResultPhase()
    {
        Debug.Log("[BattlePresenter] リザルトフェーズ開始");

        var isTutorial = _currentEnemyData.EnemyId == "alv";

        // プレイヤーの報酬を計算
        var rewardResults = RewardCalculator.CalculateAll(_player.WonCards, _player.Bids);

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

        // 感情状態を判定（最も多い感情リソース = スキルの種類）
        var emotionState = MemoryEmotionCalculator.CalculateFromEmotionTotals(_player.EmotionResources);
        Debug.Log($"[BattlePresenter] プレイヤーの感情状態: {emotionState}");

        await UniTask.Delay(2000);
        return emotionState;
    }

    // === 8. デッキ選択フェーズ ===

    private async UniTask<(BattleDeckModel playerDeck, BattleDeckModel enemyDeck)> HandleDeckSelection()
    {
        Debug.Log("[BattlePresenter] デッキ選択フェーズ開始");

        // カードにバトルデータを初期化
        foreach (var card in _auctionCards)
        {
            var info = _cardNumbers[card];
            card.InitializeBattleData(info.Number, info.TotalBid);
        }

        // プレイヤーの獲得カード
        var playerWonBattleCards = _player.WonCards
            .Where(c => _auctionCards.Contains(c))
            .ToList();

        // 不足分を補完（数字3のダミーカード）
        FillDeckWithDefaults(playerWonBattleCards);

        // デッキ選択UIとスキルボタンを表示
        _battleUIPresenter.InitializeDeckSelection(playerWonBattleCards);
        _battleUIPresenter.SetSkillButtonVisible(true);

        await _battleUIPresenter.WaitForDeckSelectionAsync();

        _battleUIPresenter.SetSkillButtonVisible(false);

        // プレイヤーのデッキ構築
        var playerDeck = new BattleDeckModel();
        playerDeck.SetDeck(_battleUIPresenter.GetSelectedDeck().ToList());
        _battleUIPresenter.HideDeckSelection();

        // 敵AIのデッキ選択
        var enemyWonBattleCards = _enemy.WonCards
            .Where(c => _auctionCards.Contains(c))
            .ToList();
        FillDeckWithDefaults(enemyWonBattleCards);

        var enemyDeck = new BattleDeckModel();
        var enemySelectedCards = SelectEnemyDeck(enemyWonBattleCards);
        enemyDeck.SetDeck(enemySelectedCards);

        Debug.Log($"[BattlePresenter] プレイヤーデッキ: {playerDeck.Cards.Count}枚, 敵デッキ: {enemyDeck.Cards.Count}枚");

        return (playerDeck, enemyDeck);
    }

    /// <summary>
    /// 不足分をデフォルトカード（数字3）で補完する
    /// </summary>
    private static void FillDeckWithDefaults(List<CardModel> cards)
    {
        while (cards.Count < GameConstants.DECK_SIZE)
        {
            // ダミーカード（数字3）
            cards.Add(new CardModel(GameConstants.DEFAULT_CARD_NUMBER));
        }
    }

    /// <summary>
    /// 敵AIのデッキ選択（ランダムで3枚選択）
    /// </summary>
    private static List<CardModel> SelectEnemyDeck(List<CardModel> availableCards)
    {
        return availableCards
            .OrderBy(_ => Random.value)
            .Take(GameConstants.DECK_SIZE)
            .ToList();
    }

    // === 9. カードバトルフェーズ ===

    private async UniTask<bool> HandleCardBattle(
        BattleDeckModel playerDeck,
        BattleDeckModel enemyDeck,
        EmotionType playerEmotionState)
    {
        Debug.Log("[BattlePresenter] カードバトルフェーズ開始");

        var handler = new CardBattleHandler(_currentAuctionData.VictoryCondition);

        // 敵の感情状態（ランダム）
        var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
        var enemyEmotionState = emotions[Random.Range(0, emotions.Length)];

        // バトルUI初期化
        _battleUIPresenter.InitializeBattle(_currentAuctionData.VictoryCondition);

        // ラウンドループ
        while (!handler.IsFinished)
        {
            Debug.Log($"[BattlePresenter] ラウンド {handler.CurrentRound + 1} 開始");

            // コイントス
            handler.DecideFirstPlayer();
            await _battleUIPresenter.PlayCoinFlipAsync(handler.IsPlayerFirst);

            // スキルボタン表示（カード選択中に使用可能）
            _battleUIPresenter.SetSkillButtonVisible(handler.PlayerSkillAvailable);

            // カード伏せフェーズ
            if (handler.IsPlayerFirst)
            {
                await PlayerPlaceCard(handler, playerDeck, playerEmotionState);
                EnemyPlaceCard(handler, enemyDeck);
                _battleUIPresenter.PlaceEnemyCard(handler.EnemyCard);
            }
            else
            {
                EnemyPlaceCard(handler, enemyDeck);
                _battleUIPresenter.PlaceEnemyCard(handler.EnemyCard);
                await PlayerPlaceCard(handler, playerDeck, playerEmotionState);
            }

            _battleUIPresenter.SetSkillButtonVisible(false);

            // 敵AIのスキル発動判定（ランダム50%）
            if (handler.EnemySkillAvailable && Random.value > 0.5f)
            {
                handler.TryActivateEnemySkill(enemyEmotionState, enemyDeck);
                _battleUIPresenter.SetBattleInstruction($"敵が{enemyEmotionState.ToJapaneseName()}スキルを発動！");
                Debug.Log($"[BattlePresenter] 敵がスキル発動: {enemyEmotionState}");
                await UniTask.Delay(1500);
            }

            // カードオープン
            _battleUIPresenter.SetBattleInstruction("カードオープン！");
            _battleUIPresenter.RevealCards(handler.PlayerCard, handler.EnemyCard);
            await UniTask.Delay(1000);

            // 勝敗判定
            var result = handler.ResolveRound();

            var resultText = result == RoundResult.PlayerWin ? "プレイヤー勝利！" : "敵の勝利...";
            _battleUIPresenter.SetBattleInstruction(resultText);
            Debug.Log($"[BattlePresenter] ラウンド {handler.CurrentRound + 1} 結果: {result}");

            // 次へボタン待機
            await _battleUIPresenter.WaitForBattleNextAsync();

            // 次ラウンド準備
            if (!handler.IsFinished)
            {
                handler.NextRound();
                _battleUIPresenter.ClearBattleField();
            }
        }

        // バトル結果
        var isPlayerWon = handler.IsPlayerWon;
        Debug.Log($"[BattlePresenter] バトル終了: {(isPlayerWon ? "プレイヤー勝利" : "プレイヤー敗北")}");

        _battleUIPresenter.HideBattle();
        return isPlayerWon;
    }

    /// <summary>
    /// プレイヤーがカードを選んで伏せる（スキルボタンも同時に操作可能）
    /// </summary>
    private async UniTask PlayerPlaceCard(
        CardBattleHandler handler,
        BattleDeckModel playerDeck,
        EmotionType playerSkill)
    {
        using var disposables = new CompositeDisposable();

        _battleUIPresenter.SetBattleInstruction("伏せるカードを選んでください");
        _battleUIPresenter.ShowPlayerHand(playerDeck.GetAvailableCards());

        // スキル発動の購読（カード選択と同時に使用可能）
        _battleUIPresenter.OnSkillActivated
            .Subscribe(_ =>
            {
                if (handler.TryActivatePlayerSkill(playerSkill, playerDeck))
                {
                    _battleUIPresenter.SetSkillButtonVisible(false);
                    _battleUIPresenter.SetBattleInstruction($"{playerSkill.ToJapaneseName()}スキル発動！");
                    Debug.Log($"[BattlePresenter] プレイヤーがスキル発動: {playerSkill}");
                }
            })
            .AddTo(disposables);

        var selectedCard = await _battleUIPresenter.OnBattleCardSelected.FirstAsync();
        handler.PlacePlayerCard(selectedCard);
        playerDeck.MarkAsUsed(selectedCard);
        _battleUIPresenter.PlacePlayerCard(selectedCard);
    }

    /// <summary>
    /// 敵AIがカードを選んで伏せる
    /// </summary>
    private static void EnemyPlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck)
    {
        var availableCards = enemyDeck.GetAvailableCards();
        if (availableCards.Count == 0) return;

        var card = availableCards[Random.Range(0, availableCards.Count)];
        handler.PlaceEnemyCard(card);
        enemyDeck.MarkAsUsed(card);
    }

    // === 10. 記憶育成フェーズ ===

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
        var acquiredTheme = new AcquiredTheme(_currentTheme, allCardInfoList, usedEmotions);

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
            var cardInfo = new CardAcquisitionInfo(card, playerBids, enemyBids, playerWon);
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
                result.TryAdd(kvp.Key, 0);
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
