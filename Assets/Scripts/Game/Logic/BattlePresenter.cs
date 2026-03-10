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

    private readonly EnemyAIController _enemyAI;
    private readonly AuctionProcessor _auctionProcessor;

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

        _enemyAI = new EnemyAIController(_enemy);
        _auctionProcessor = new AuctionProcessor(_player, _enemy, _battleUIPresenter, _enemyAI);

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

#if UNITY_EDITOR
        if (DebugBattleSettings.SkipAuction)
        {
            await StartDebugBattleAsync();
            return;
        }
#endif

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
        await _auctionProcessor.ProcessAuctionResultAsync(_auctionCards, _currentEnemyData, _currentGameState);

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
        var (playerDeck, enemyDeck, isPlayerSkillUsedInDeckSelection) = await HandleDeckSelection(playerEmotionState);

        // 9. カードバトル（3本勝負）
        _currentGameState.Value = GameState.CardBattle;
        await HandleCardBattle(playerDeck, enemyDeck, playerEmotionState, _currentAuctionData.VictoryCondition,
            isPlayerSkillUsedInDeckSelection);

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
        _enemyAI.DecideBids(_auctionCards);
        Debug.Log($"[BattlePresenter] 敵の入札完了: 合計{_enemy.Bids.GetTotalBidAmount()}リソース");

        // プレイヤーの入札UI表示・待機
        await _battleUIPresenter.WaitForBiddingAsync(_auctionCards, _player.Bids, EmotionType.Joy, _player.EmotionResources);

        Debug.Log($"[BattlePresenter] プレイヤーの入札完了: 合計{_player.Bids.GetTotalBidAmount()}リソース");

        // 入札対象カード公開演出
        await _battleUIPresenter.ShowBidTargetsAsync(_player.Bids, _enemy.Bids, 2f);
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

    private async UniTask<(BattleDeckModel playerDeck, BattleDeckModel enemyDeck, bool isPlayerSkillUsed)> HandleDeckSelection(EmotionType playerSkill)
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
        BattleDeckModel.FillWithDefaults(playerWonBattleCards);

        // デッキ選択中のスキル適用結果を、そのまま選択UIへ反映させるための作業用デッキ
        var previewDeck = new BattleDeckModel();
        previewDeck.SetDeck(playerWonBattleCards);
        var isPlayerSkillUsed = false;
        using var disposables = new CompositeDisposable();

        // デッキ選択UIとスキルボタンを表示
        _battleUIPresenter.InitializeDeckSelection(playerWonBattleCards);
        _battleUIPresenter.SetSkillButtonVisible(SkillEffectApplier.CanUseInDeckSelection(playerSkill));

        _battleUIPresenter.OnSkillActivated
            .Subscribe(_ =>
            {
                if (isPlayerSkillUsed || !SkillEffectApplier.CanUseInDeckSelection(playerSkill))
                    return;

                // デッキ選択中に使った場合は、その後のカードバトルでは再使用させない
                isPlayerSkillUsed = true;
                SkillEffectApplier.Apply(playerSkill, null, null, previewDeck, null);
                _battleUIPresenter.RefreshDeckSelectionCardNumbers();
                _battleUIPresenter.SetSkillButtonVisible(false);
                Debug.Log($"[BattlePresenter] デッキ選択中にスキル発動: {playerSkill}");
            })
            .AddTo(disposables);

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
        BattleDeckModel.FillWithDefaults(enemyWonBattleCards);

        var enemyDeck = new BattleDeckModel();
        enemyDeck.SetDeck(_enemyAI.SelectDeck(enemyWonBattleCards));

        Debug.Log($"[BattlePresenter] プレイヤーデッキ: {playerDeck.Cards.Count}枚, 敵デッキ: {enemyDeck.Cards.Count}枚");

        return (playerDeck, enemyDeck, isPlayerSkillUsed);
    }

    // === 9. カードバトルフェーズ ===

    private async UniTask<bool> HandleCardBattle(BattleDeckModel playerDeck, BattleDeckModel enemyDeck, EmotionType playerEmotionState,
        VictoryCondition victoryCondition, bool isPlayerSkillUsedInDeckSelection)
    {
        Debug.Log("[BattlePresenter] カードバトルフェーズ開始");

        // デッキ選択中に使用済みなら、バトル開始時点でスキル使用権を閉じる
        var handler = new CardBattleHandler(victoryCondition, !isPlayerSkillUsedInDeckSelection);

        // 敵の感情状態（ランダム）
        var enemyEmotionState = _enemyAI.DecideEmotionState();

        // バトルUI初期化
        _battleUIPresenter.InitializeBattle(victoryCondition);

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
                // 先攻時は敵カードが未確定なので、相手依存スキルは後段で解決する
                var shouldApplySkillAfterEnemyPlacement = await PlayerPlaceCard(handler, playerDeck, playerEmotionState);
                _enemyAI.PlaceCard(handler, enemyDeck);
                _battleUIPresenter.PlaceEnemyCard(handler.EnemyCard);

                if (shouldApplySkillAfterEnemyPlacement)
                {
                    SkillEffectApplier.Apply(playerEmotionState, handler.PlayerCard, handler.EnemyCard, playerDeck, handler);
                    _battleUIPresenter.RefreshBattleCardNumbers();
                }
            }
            else
            {
                _enemyAI.PlaceCard(handler, enemyDeck);
                _battleUIPresenter.PlaceEnemyCard(handler.EnemyCard);
                await PlayerPlaceCard(handler, playerDeck, playerEmotionState);
            }

            _battleUIPresenter.SetSkillButtonVisible(false);

            // 敵AIのスキル発動判定
            if (_enemyAI.TryActivateSkill(handler, enemyDeck, enemyEmotionState))
            {
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
    private async UniTask<bool> PlayerPlaceCard(CardBattleHandler handler, BattleDeckModel playerDeck, EmotionType playerSkill)
    {
        using var disposables = new CompositeDisposable();

        // カード選択前にスキルボタンが押された場合のフラグ
        // カード未選択で押したスキルを、カード確定後に解決するためのフラグ
        var isSkillActivatedBeforePlacement = false;
        // 相手カードが必要なスキルを、敵配置後へ持ち越すためのフラグ
        var shouldApplySkillAfterEnemyPlacement = false;

        _battleUIPresenter.SetBattleInstruction("伏せるカードを選んでください");
        _battleUIPresenter.ShowPlayerHand(playerDeck.GetAvailableCards());

        // スキル発動の購読（カード選択と同時に使用可能）
        _battleUIPresenter.OnSkillActivated
            .Subscribe(_ =>
            {
                var selectedBattleCard = _battleUIPresenter.SelectedBattleCard;
                if (selectedBattleCard != null)
                {
                    // 複数回押されても最初の1回だけを有効にする
                    if (!handler.MarkPlayerSkillUsed())
                        return;

                    if (playerSkill == EmotionType.Fear && handler.EnemyCard == null)
                    {
                        // Fearは相手カードが出るまで解決できないので後ろへ送る
                        shouldApplySkillAfterEnemyPlacement = true;
                    }
                    else
                    {
                        SkillEffectApplier.Apply(playerSkill, selectedBattleCard, handler.EnemyCard, playerDeck, handler);

                        _battleUIPresenter.RefreshBattleCardNumbers();
                    }

                    _battleUIPresenter.SetSkillButtonVisible(false);
                    _battleUIPresenter.SetBattleInstruction($"{playerSkill.ToJapaneseName()}スキル発動！");
                    Debug.Log($"[BattlePresenter] プレイヤーがスキル発動: {playerSkill}");
                }
                else if (handler.MarkPlayerSkillUsed())
                {
                    switch (playerSkill)
                    {
                        case EmotionType.Anger:
                        case EmotionType.Anticipation:
                        case EmotionType.Trust:
                            // カードを選ばなくても成立するスキルは即時処理する
                            SkillEffectApplier.Apply(playerSkill, null, handler.EnemyCard, playerDeck, handler);
                            _battleUIPresenter.RefreshBattleCardNumbers();
                            break;

                        default:
                            // カード依存スキルはカード選択後まで予約する
                            isSkillActivatedBeforePlacement = true;
                            break;
                    }

                    _battleUIPresenter.SetSkillButtonVisible(false);
                    _battleUIPresenter.SetBattleInstruction($"{playerSkill.ToJapaneseName()}スキル発動！");
                    Debug.Log($"[BattlePresenter] プレイヤーがスキル発動（カード選択前）: {playerSkill}");
                }
            })
            .AddTo(disposables);

        var selectedCard = await _battleUIPresenter.OnBattleCardSelected.FirstAsync();
        handler.PlacePlayerCard(selectedCard);
        playerDeck.MarkAsUsed(selectedCard);
        _battleUIPresenter.PlacePlayerCard(selectedCard);

        // カード選択前にスキルが押されていた場合、カード確定後に効果を適用
        if (isSkillActivatedBeforePlacement)
        {
            if (playerSkill == EmotionType.Fear && handler.EnemyCard == null)
            {
                // 先攻時のFearは敵カード確定後に適用する
                shouldApplySkillAfterEnemyPlacement = true;
            }
            else
            {
                SkillEffectApplier.Apply(playerSkill, handler.PlayerCard, handler.EnemyCard, playerDeck, handler);
                _battleUIPresenter.RefreshBattleCardNumbers();
            }
        }

        return shouldApplySkillAfterEnemyPlacement;
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

        // メモリに記録（ディスク保存はシーン遷移直前に行う）
        _gameProgressService.RecordAcquiredTheme(acquiredTheme);

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

#if UNITY_EDITOR
    // === デバッグ: オークションスキップ ===

    /// <summary>
    /// デバッグ用：オークションをスキップしてバトルフェーズから開始する
    /// </summary>
    private async UniTask StartDebugBattleAsync()
    {
        Debug.Log("[BattlePresenter] デバッグ: オークションスキップ開始");

        // デバッグ用カード（数字1〜6）を生成
        _auctionCards.Clear();
        for (var i = 1; i <= GameConstants.AUCTION_CARD_COUNT; i++)
            _auctionCards.Add(new CardModel(i));

        // カード番号マップを構築（入札0、数字は順番通り）
        _cardNumbers = new System.Collections.Generic.Dictionary<CardModel, CardNumberAssigner.CardNumberInfo>();
        for (var i = 0; i < _auctionCards.Count; i++)
        {
            _cardNumbers[_auctionCards[i]] = new CardNumberAssigner.CardNumberInfo
            {
                Number = i + 1,
                TotalBid = 0,
            };
        }

        // プレイヤーに全カードを付与（デッキ選択フェーズで3枚選ぶ）
        foreach (var card in _auctionCards)
            _player.AddWonCard(card);

        // スキルボタン初期化（DebugBattleSettingsで設定したスキルを使用）
        var playerEmotionState = DebugBattleSettings.PlayerSkill;
        Debug.Log($"[BattlePresenter] デバッグ: スキル={playerEmotionState}, 勝利条件={DebugBattleSettings.VictoryCondition}");

        _battleUIPresenter.InitializeSkillButton(playerEmotionState);
        _currentGameState.Value = GameState.DeckSelection;

        // デッキ選択（通常フローと同じ）
        var (playerDeck, enemyDeck, isPlayerSkillUsedInDeckSelection) = await HandleDeckSelection(playerEmotionState);

        // カードバトル（通常フローと同じ）
        _currentGameState.Value = GameState.CardBattle;
        await HandleCardBattle(playerDeck, enemyDeck, playerEmotionState, DebugBattleSettings.VictoryCondition,
            isPlayerSkillUsedInDeckSelection);

        // 終了（記憶育成はスキップしてHome遷移）
        _currentGameState.Value = GameState.BattleEnd;
        VolumeController.Instance.ResetToDefault();
        await UniTask.Delay(1000);
        await _sceneTransitionManager.TransitionToSceneWithFade(SceneType.Home);
    }
#endif
}
