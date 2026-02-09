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
        _battleUIPresenter.InitializeDialogueView(_currentEnemyData);

        // 敵情報をアナウンス
        await _battleUIPresenter.ShowAnnouncement(_currentEnemyData.EnemyName, 1.5f);
        return true;
    }

    private async UniTask StartGame()
    {
        await UniTask.Delay(1000);

        // 1. 出品者フェーズ
        _currentGameState.Value = GameState.ThemeAnnouncement;
        await HandleThemeAnnouncement();
        _currentGameState.Value = GameState.CardDistribution;
        await HandleCardDistribution();
        _currentGameState.Value = GameState.ValueRanking;
        await HandleValueRanking();
        _currentGameState.Value = GameState.CardReveal;
        await HandleCardReveal();

        // 2. 入札者フェーズ
        _currentGameState.Value = GameState.BiddingPhase;
        await HandleBiddingPhase();

        // 3. 対話フェーズ
        _currentGameState.Value = GameState.DialoguePhase;
        await HandleDialoguePhase();

        // 4. 落札者判定フェーズ
        _currentGameState.Value = GameState.AuctionResult;
        await HandleAuctionResult();

        // 5. 報酬フェーズ
        _currentGameState.Value = GameState.RewardPhase;
        await HandleRewardPhase();

        // 6. 記憶育成フェーズ
        _currentGameState.Value = GameState.MemoryGrowth;
        await HandleMemoryGrowth();

        // 終了
        _currentGameState.Value = GameState.BattleEnd;
        await HandleBattleEnd();
    }

    // === 1. 出品者フェーズ ===

    private async UniTask HandleThemeAnnouncement()
    {
        // オークションデータからテーマを取得
        _currentTheme = _currentAuctionData.Theme;

        // 記憶テーマを表示
        await _battleUIPresenter.SetTheme(_currentTheme, isMainTheme: true);

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("BeforeThemeAnnouncement");
    }

    private async UniTask HandleCardDistribution()
    {
        // Player/Enemyにカードを設定
        _player.SetCards(_currentAuctionData.PlayerCards);
        _enemy.SetCards(_currentAuctionData.EnemyCards);

        Debug.Log($"[BattlePresenter] カード配布完了: プレイヤー{_player.Cards.Count}枚、敵{_enemy.Cards.Count}枚");

        // TODO: 配布アニメーション（UIPresenter経由）

        await UniTask.Delay(500);
    }

    private async UniTask HandleValueRanking()
    {
        Debug.Log("[BattlePresenter] 価値順位設定フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
        {
            await _battleUIPresenter.StartTutorial("ValueRanking");
            await _battleUIPresenter.StartTutorial("SpecialMemoryCards");
            await _battleUIPresenter.StartTutorial("ThemeAndGauges");
        }

        // 敵AIで順位をランダム設定
        _enemy.DecideValueRankings();
        Debug.Log("[BattlePresenter] 敵の価値順位を設定完了");

        // プレイヤーの価値順位設定（UI経由）
        var rankedCards = await _battleUIPresenter.WaitForValueRankingAsync(_player.Cards);

        // 結果をPlayerのValueRankingに反映
        for (var i = 0; i < rankedCards.Count; i++)
        {
            _player.ValueRanking.TrySetRanking(rankedCards[i], i + 1);
        }
        Debug.Log("[BattlePresenter] プレイヤーの価値順位設定完了");

        await UniTask.Delay(500);
    }

    private async UniTask HandleCardReveal()
    {
        Debug.Log("[BattlePresenter] カード公開フェーズ開始");

        // 8枚を場に並べる
        _auctionCards.Clear();
        _auctionCards.AddRange(_player.Cards);
        _auctionCards.AddRange(_enemy.Cards);

        Debug.Log($"[BattlePresenter] オークション対象カード: {_auctionCards.Count}枚");

        // 黒画面の裏でカードを表示（プレイヤーの価値順位と感情リソースも渡す）
        _battleUIPresenter.ShowAuctionCards(_player.Cards, _enemy.Cards, _player.EmotionResources, _player.ValueRanking);

        // トランジション：開く（黒フェードから復帰）
        await _battleUIPresenter.PlayPhaseTransitionOpenAsync();

        await UniTask.Delay(1500);
    }

    // === 2. 入札者フェーズ ===

    private async UniTask HandleBiddingPhase()
    {
        Debug.Log("[BattlePresenter] 入札フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("BiddingPhase");

        // 敵AIで入札を決定
        _enemy.DecideBids(_auctionCards, EmotionType.Joy);
        Debug.Log($"[BattlePresenter] 敵の入札完了: 合計{_enemy.Bids.GetTotalBidAmount()}リソース");

        // プレイヤーの入札UI表示・待機（8種類の感情リソースを渡す）
        await _battleUIPresenter.WaitForBiddingAsync(
            _player.Cards,
            _enemy.Cards,
            _player.Bids,
            EmotionType.Joy,
            _player.EmotionResources);

        Debug.Log($"[BattlePresenter] プレイヤーの入札完了: 合計{_player.Bids.GetTotalBidAmount()}リソース");

        // 入札対象カード公開演出（投入リソースは非公開、入札なしカードは順位非表示）
        await _battleUIPresenter.ShowBidTargetsAsync(_player.Bids, _enemy.Bids, 2f);

    }

    // === 3. 対話フェーズ ===

    private async UniTask HandleDialoguePhase()
    {
        _battleUIPresenter.HideAuctionView();

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("DialoguePhase");

        var dialogueData = _currentAuctionData.DialogueData;

        var playerFirstChoice = await HandlePlayerFirstTurn(dialogueData);

        await HandleEnemyFirstTurn(dialogueData, playerFirstChoice);

        await _battleUIPresenter.HideDialogueViewAsync();
    }

    private async UniTask<DialogueChoiceType> HandlePlayerFirstTurn(EnemyDialogueData dialogueData)
    {
        _battleUIPresenter.ShowDialogueView();
        var playerChoice = await _battleUIPresenter.WaitForFourChoiceAsync();

        // プレイヤーの選択をセリフとして表示
        await _battleUIPresenter.ShowPlayerDialogueAsync(playerChoice.ToJapaneseName());

        var response = dialogueData.GetResponse(playerChoice);
        await _battleUIPresenter.ShowEnemyDialogueAsync(response.DialogueText);

        var resultMessage = DialogueEffectApplier.ApplyEffect(response.Effect, _enemy, _auctionCards);
        await _battleUIPresenter.ShowEnemyNarration(resultMessage);

        return playerChoice;
    }

    private async UniTask HandleEnemyFirstTurn(EnemyDialogueData dialogueData, DialogueChoiceType playerFirstChoice)
    {
        var initiation = dialogueData.GetInitiation(playerFirstChoice);

        await UniTask.WhenAll(
            _battleUIPresenter.HidePlayerDialogueAsync(),
            _battleUIPresenter.HideEnemyDialogueAsync(),
            _battleUIPresenter.HideAllAsync()
        );

        await _battleUIPresenter.ShowEnemyDialogueAsync(initiation.EnemyDialogueText);

        var options = initiation.PlayerOptions.Select(o => o.OptionText).ToList();
        var selectedIndex = await _battleUIPresenter.WaitForThreeResponseAsync(options);

        var selectedOption = initiation.PlayerOptions[selectedIndex];

        // プレイヤーの選択をセリフとして表示
        await _battleUIPresenter.ShowPlayerDialogueAsync(selectedOption.OptionText);

        // 敵の返答を表示
        await _battleUIPresenter.ShowEnemyDialogueAsync(selectedOption.ResultText);

        // 効果を適用して結果を表示
        var resultMessage = DialogueEffectApplier.ApplyEffect(selectedOption.Effect, _player, _auctionCards);
        await _battleUIPresenter.ShowPlayerNarration(resultMessage);
    }

    // === 4. 落札者判定フェーズ ===

    private async UniTask HandleAuctionResult()
    {
        Debug.Log("[BattlePresenter] 落札者判定フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("ResultDetermination");

        // 入札に使ったリソースを消費
        ConsumeBidResources(_player);
        ConsumeBidResources(_enemy);

        // AuctionViewを再表示（カードは既に存在する）
        _battleUIPresenter.ShowAuctionView();

        // 全カードの落札者を判定
        var results = AuctionJudge.JudgeAll(_auctionCards, _player.Bids, _enemy.Bids);

        // 結果を格納
        _player.ClearWonCards();
        _enemy.ClearWonCards();

        foreach (var result in results)
        {
            if (result.NoBids)
            {
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: 入札なし");
                continue;
            }

            if (result.IsPlayerWon)
            {
                _player.AddWonCard(result.Card);
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: プレイヤー落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
            else
            {
                _enemy.AddWonCard(result.Card);
                Debug.Log($"[BattlePresenter] {result.Card.Data.CardName}: 敵落札（{result.PlayerBid} vs {result.EnemyBid}）");
            }
        }

        Debug.Log($"[BattlePresenter] 落札結果: プレイヤー{_player.WonCards.Count}枚、敵{_enemy.WonCards.Count}枚");

        // 順次演出で結果を表示（価値順位も公開）
        await _battleUIPresenter.ShowAuctionResultsSequentialAsync(results, _player.ValueRanking, _enemy.ValueRanking, _currentEnemyData.EnemyColor);

        await UniTask.Delay(1000);
        // オークション完全終了時にクリア
        _battleUIPresenter.ClearAuctionView();
    }

    /// <summary>
    /// 入札に使ったリソースを消費する
    /// </summary>
    private void ConsumeBidResources(PlayerPresenter player)
    {
        var bidsByEmotion = player.Bids.GetTotalBidsByEmotion();
        foreach (var (emotion, amount) in bidsByEmotion)
        {
            player.TryConsumeEmotion(emotion, amount);
            Debug.Log($"[BattlePresenter] リソース消費: {emotion} -{amount}");
        }
    }

    // === 5. 報酬フェーズ ===

    private async UniTask HandleRewardPhase()
    {
        Debug.Log("[BattlePresenter] 報酬フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("RewardPhase");

        // プレイヤーの報酬を計算
        var rewardResults = RewardCalculator.CalculateAll(
            _player.WonCards,
            _player.ValueRanking,
            _enemy.ValueRanking,
            _player.Bids,
            _player.Cards);

        // 各カードの報酬詳細をログ出力
        foreach (var (card, result) in rewardResults)
        {
            var ownCardText = result.IsOwnCard ? " [自カード+2]" : "";
            Debug.Log($"[BattlePresenter] {card.Data.CardName}: 基本{result.BaseReward} + 相対{result.RelativeReward}{ownCardText} = 合計{result.TotalReward}");
        }

        // 最大リソース値を設定（デフォルト値の3倍を仮の上限とする）
        var maxResources = new Dictionary<EmotionType, int>();
        foreach (EmotionType emotion in System.Enum.GetValues(typeof(EmotionType)))
        {
            maxResources[emotion] = GameConstants.DEFAULT_EMOTION_VALUE * 3;
        }

        // 報酬演出表示（報酬加算前のリソース値を渡す）
        var rewardedAmounts = await _battleUIPresenter.ShowRewardsAsync(rewardResults, _player.EmotionResources, maxResources);

        // 報酬を各感情リソースに加算（演出で決まったランダムな感情タイプごとに）
        foreach (var (emotion, amount) in rewardedAmounts)
        {
            if (amount > 0)
            {
                _player.AddEmotion(emotion, amount);
                Debug.Log($"[BattlePresenter] プレイヤーに報酬付与: {emotion} +{amount}リソース");
            }
        }

        await UniTask.Delay(2000);
        _battleUIPresenter.HideRewardView();

    }

    // === 6. 記憶育成フェーズ ===

    private async UniTask HandleMemoryGrowth()
    {
        Debug.Log("[BattlePresenter] 記憶育成フェーズ開始");

        if (_currentEnemyData.EnemyId == "alv")
            await _battleUIPresenter.StartTutorial("MemoryGrowthPhase");

        // 全カードの獲得情報を構築（入札がないカードは除外）
        var allCardInfoList = BuildCardAcquisitionInfoList();

        // 入札されたカードがない場合はスキップ
        if (allCardInfoList.Count == 0)
        {
            Debug.Log("[BattlePresenter] 入札カードなし - 記憶育成フェーズをスキップ");
            return;
        }

        // 使用した感情リソースを計算（プレイヤーの全入札から集計）
        var usedEmotions = CalculateUsedEmotions();

        // 獲得テーマを作成（全カード情報を含む、支配的感情は自動計算）
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

        // UIで記憶育成フェーズを表示
        await _battleUIPresenter.ShowMemoryGrowthAsync(allThemes);
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

            var playerValueRank = _player.ValueRanking.GetRanking(card);
            var enemyValueRank = _enemy.ValueRanking.GetRanking(card);
            var playerWon = _player.WonCards.Contains(card);

            var cardInfo = new CardAcquisitionInfo(
                card,
                playerBids,
                enemyBids,
                playerValueRank,
                enemyValueRank,
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
