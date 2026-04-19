using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// チュートリアル用のバトル進行を担当するPresenter
/// </summary>
public class TutorialBattlePresenter : BattlePresenter
{
    private readonly TutorialBattlePlayerData _tutorialBattlePlayerData;
    private readonly TutorialCompetitionPhaseRunner _auctionCompetitionPhaseRunner;

    public TutorialBattlePresenter(
        BattleUIPresenter battleUIPresenter,
        Player player,
        Enemy enemy,
        GameProgressService gameProgressService,
        SceneTransitionManager sceneTransitionManager,
        AllAuctionData allAuctionData,
        TutorialBattlePlayerData tutorialBattlePlayerData)
        : base(
            battleUIPresenter,
            player,
            enemy,
            gameProgressService,
            sceneTransitionManager,
            allAuctionData)
    {
        _tutorialBattlePlayerData = tutorialBattlePlayerData;
        EnemyAI = new TutorialEnemyAIController(Enemy);
        _auctionCompetitionPhaseRunner = new TutorialCompetitionPhaseRunner(
            Player,
            EnemyAI,
            BattleUIPresenter,
            _tutorialBattlePlayerData.AuctionCompetitionRequiredRaises,
            _tutorialBattlePlayerData.AuctionCompetitionForcedEmotion);
        CompetitionPhaseRunner = new TutorialCompetitionPhaseRunner(
            Player,
            EnemyAI,
            BattleUIPresenter,
            _tutorialBattlePlayerData.BattleCompetitionRequiredRaises,
            _tutorialBattlePlayerData.BattleCompetitionForcedEmotion);
        AuctionProcessor = new AuctionProcessor(Player, Enemy, BattleUIPresenter, _auctionCompetitionPhaseRunner);
    }

    protected override void InitializeDeckSelectionView(IReadOnlyList<CardModel> wonCards)
    {
        BattleUIPresenter.InitializeDeckSelection(wonCards, _tutorialBattlePlayerData.DeckAllowedCardIndices);
    }

    protected override bool CanUseDeckSelectionSkill(EmotionType playerSkill)
    {
        return false;
    }

    protected override EmotionType GetDeckSelectionSkill(EmotionType defaultSkill)
    {
        return _tutorialBattlePlayerData.BattleForcedSkillEmotion;
    }

    protected override EmotionType GetBattleSkill(EmotionType defaultSkill)
    {
        return _tutorialBattlePlayerData.BattleForcedSkillEmotion;
    }

    protected override bool RequiresDeckSelectionSkillActivation(EmotionType playerSkill)
    {
        return false;
    }

    protected override bool CanUseBattleSkill(CardBattleHandler handler, EmotionType playerSkill)
    {
        return handler.PlayerSkillAvailable && handler.CurrentRound == _tutorialBattlePlayerData.SkillRoundIndex;
    }

    protected override bool RequiresBattleSkillActivation(CardBattleHandler handler, EmotionType playerSkill)
    {
        return handler.CurrentRound == _tutorialBattlePlayerData.SkillRoundIndex;
    }

    protected override VictoryCondition GetBattleVictoryCondition(VictoryCondition defaultVictoryCondition)
    {
        return _tutorialBattlePlayerData.BattleVictoryCondition;
    }

    protected override EmotionType GetEnemyBattleEmotionState(CardBattleHandler handler, EmotionType currentEmotionState)
    {
        return EnemyAI.DecideEmotionState();
    }

    protected override int? GetForcedDialogueChoiceIndex()
    {
        return 0;
    }

    protected override async UniTask OnDeckSelectionShownAsync()
    {
        await BattleUIPresenter.StartTutorial("DeckSelectionPhase");
    }

    protected override async UniTask OnBeforeMemoryGrowthContinueAsync()
    {
        await BattleUIPresenter.StartTutorial("MemoryGrowthPhase");
    }

    protected override async UniTask OnAfterCardRevealAsync()
    {
        await BattleUIPresenter.StartTutorial("BeforeThemeAnnouncement");

        // ポーズ→ヘルプへの誘導（プレイヤーが実際にヘルプボタンを押すまで待機）
        // await BattleUIPresenter.StartTutorial("HelpGuidance");
        // await BattleUIPresenter.OnHelpButtonClickedInPause.FirstAsync();
    }

    protected override List<CardModel> BuildPlayerDeckCards(
        IReadOnlyList<CardModel> selectedCards,
        IReadOnlyList<CardModel> wonCards)
    {
        var wonCardOrder = wonCards
            .Select((card, index) => new { card, index })
            .ToDictionary(x => x.card, x => x.index);
        return selectedCards
            .OrderBy(card => wonCardOrder[card])
            .ToList();
    }

    protected override async UniTask HandleBiddingPhase()
    {
        Debug.Log("[TutorialBattlePresenter] 入札フェーズ開始");

        EnemyAI.DecideBids(AuctionCards);

        await RunTutorialBiddingAsync();

        Debug.Log($"[TutorialBattlePresenter] プレイヤーの入札完了: 合計{Player.Bids.GetTotalBidAmount()}リソース");
        await BattleUIPresenter.ShowBidTargetsAsync(Player.Bids, Enemy.Bids);
    }

    protected override async UniTask OnAfterCardsDisplayed()
    {
        await BattleUIPresenter.StartTutorial("RewardPhase");
    }

    protected override async UniTask OnAfterResourceRewardsAnimated()
    {
        await UniTask.Delay(800);
        await BattleUIPresenter.StartTutorial("RewardPhase2");
    }

    protected override void DecideFirstPlayer(CardBattleHandler handler)
    {
        var round = handler.CurrentRound;
        var coinFlipPerRound = _tutorialBattlePlayerData.CoinFlipPerRound;
        if (round < 0 || round >= coinFlipPerRound.Count)
        {
            base.DecideFirstPlayer(handler);
            return;
        }

        handler.SetFirstPlayer(coinFlipPerRound[round]);
    }

    protected override async UniTask<CardModel> SelectBattleCardAsync(CardBattleHandler handler, BattleDeckModel playerDeck)
    {
        var availableCards = playerDeck.GetAvailableCards();
        var requiresSkillActivation = RequiresBattleSkillActivation(handler, GetBattleSkill(EmotionType.Joy));
        if (requiresSkillActivation)
        {
            BattleUIPresenter.ShowPlayerHand(availableCards);
            BattleUIPresenter.SetBattleHandInteractable(false);
            BattleUIPresenter.SetSkillButtonInteractable(false);
            await BattleUIPresenter.StartTutorial("BattleSkillPhase");
            BattleUIPresenter.SetSkillButtonInteractable(true);
            await UniTask.WaitUntil(() => !handler.PlayerSkillAvailable);
            BattleUIPresenter.SetBattleHandInteractable(true);
        }

        var round = handler.CurrentRound;
        var forcedCardPerRound = _tutorialBattlePlayerData.ForcedCardPerRound;
        if (round < 0 || round >= forcedCardPerRound.Count || !forcedCardPerRound[round].HasValue) return requiresSkillActivation ? await BattleUIPresenter.OnBattleCardSelected.FirstAsync() : await base.SelectBattleCardAsync(handler, playerDeck);

        var forcedDeckCardIndex = forcedCardPerRound[round].Value;
        if (forcedDeckCardIndex < 0 || forcedDeckCardIndex >= playerDeck.Cards.Count)
        {
            Debug.LogError($"[TutorialBattlePresenter] 不正な強制カードインデックス: {forcedDeckCardIndex}");
            return await base.SelectBattleCardAsync(handler, playerDeck);
        }

        var forcedCard = playerDeck.Cards[forcedDeckCardIndex];
        var forcedAvailableCardIndex = -1;
        for (var i = 0; i < availableCards.Count; i++)
        {
            if (availableCards[i] != forcedCard) continue;

            forcedAvailableCardIndex = i;
            break;
        }

        if (forcedAvailableCardIndex < 0)
        {
            Debug.LogError($"[TutorialBattlePresenter] 強制カードが未使用札に存在しません: {forcedCard}");
            return await base.SelectBattleCardAsync(handler, playerDeck);
        }

        // カードを表示してからチュートリアルを再生
        BattleUIPresenter.ShowPlayerHand(availableCards, forcedAvailableCardIndex);
        if (handler.CurrentRound == 0)
            await BattleUIPresenter.StartTutorial("BattlePhase");
        return await BattleUIPresenter.OnBattleCardSelected.FirstAsync();
    }

    private void UpdateTutorialBidConfirmState()
    {
        BattleUIPresenter.SetAuctionConfirmInteractable(Player.Bids.GetTotalBidAmount() == _tutorialBattlePlayerData.BidRequiredAmount);
    }

    private async UniTask RunTutorialBiddingAsync()
    {
        var forcedCard = AuctionCards[_tutorialBattlePlayerData.BidForcedCardIndex];
        BattleUIPresenter.StartAuctionBidding(
            AuctionCards,
            Player.Bids,
            _tutorialBattlePlayerData.BidForcedEmotion.GetPreviousEmotion(),
            Player.EmotionResources);

        // === フェーズ1: 対話ボタンの説明 ===
        BattleUIPresenter.SetAuctionAllCardsInteractable(false);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);
        BattleUIPresenter.SetAuctionConfirmInteractable(false);
        BattleUIPresenter.SetAuctionBidIncreaseInteractable(false);
        BattleUIPresenter.SetAuctionEmotionInteractable(false);
        await BattleUIPresenter.StartTutorial("BiddingPhase");

        // 対話ボタンのみ有効化 → プレイヤーが押す → 対話実行（挑発を選ぶ）
        BattleUIPresenter.SetAuctionDialogueInteractable(_tutorialBattlePlayerData.BidForcedCardIndex, true);
        await BattleUIPresenter.OnAuctionDialogueRequested
            .Where(card => card == forcedCard)
            .FirstAsync();
        await ShowAuctionDialogueAsync(forcedCard);

        // === フェーズ2: 感情リソース選択の説明 ===
        BattleUIPresenter.SetAuctionAllCardsInteractable(false);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);
        BattleUIPresenter.SetAuctionEmotionInteractable(false);
        await BattleUIPresenter.StartTutorial("BiddingPhaseEmotion");

        // 感情ボタンのみ有効化 → 正しい感情を選ぶ
        BattleUIPresenter.SetAuctionEmotionInteractable(true);
        await BattleUIPresenter.OnAuctionEmotionSelected
            .Where(emotion => emotion == _tutorialBattlePlayerData.BidForcedEmotion)
            .FirstAsync();

        // カード選択（対象カードをクリック）
        BattleUIPresenter.SetAuctionEmotionInteractable(false);
        BattleUIPresenter.SetAuctionCardInteractable(_tutorialBattlePlayerData.BidForcedCardIndex, true);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);
        await BattleUIPresenter.OnAuctionCardClicked
            .Where(index => index == _tutorialBattlePlayerData.BidForcedCardIndex)
            .FirstAsync();

        // === フェーズ3: 入札量の説明 ===
        await BattleUIPresenter.StartTutorial("BiddingPhaseBid");

        // 入札ボタン有効化 → 必要枚数ベット → 確定
        BattleUIPresenter.SetAuctionBidIncreaseInteractable(true);
        var bidChangedDisposable = BattleUIPresenter.OnAuctionBidChanged
            .Subscribe(_ => UpdateTutorialBidConfirmState());
        UpdateTutorialBidConfirmState();

        await BattleUIPresenter.OnAuctionBidChanged
            .Where(_ => Player.Bids.GetTotalBidAmount() >= _tutorialBattlePlayerData.BidRequiredAmount)
            .FirstAsync();

        UpdateTutorialBidConfirmState();

        try
        {
            await BattleUIPresenter.OnAuctionBiddingConfirmed.FirstAsync();
        }
        finally
        {
            bidChangedDisposable.Dispose();
        }
    }
}

/// <summary>
/// チュートリアル用の競合フェーズを担当するRunner
/// </summary>
public class TutorialCompetitionPhaseRunner : CompetitionPhaseRunner
{
    private const int ENEMY_RAISE_DELAY_MIN_MILLISECONDS = 700;
    private const int ENEMY_RAISE_DELAY_MAX_MILLISECONDS = 1300;

    private readonly Player _player;
    private readonly IEnemyAIController _enemyAI;
    private readonly BattleUIPresenter _uiPresenter;
    private readonly int _requiredRaises;
    private readonly EmotionType _forcedEmotion;

    public TutorialCompetitionPhaseRunner(
        Player player,
        IEnemyAIController enemyAI,
        BattleUIPresenter uiPresenter,
        int requiredRaises,
        EmotionType forcedEmotion)
        : base(player, enemyAI, uiPresenter)
    {
        _player = player;
        _enemyAI = enemyAI;
        _uiPresenter = uiPresenter;
        _requiredRaises = requiredRaises;
        _forcedEmotion = forcedEmotion;
    }

    public override async UniTask<CompetitionHandler> RunAsync(CardModel card, int playerTotal, int enemyTotal, string instruction)
    {
        var handler = new CompetitionHandler();
        handler.Start(card, playerTotal, enemyTotal);
        var pendingEnemyRaiseCount = 0;
        var nextEnemyRaiseTime = 0f;

        using var disposables = new CompositeDisposable();
        _uiPresenter.SetBattleInstruction(instruction);
        _uiPresenter.ShowCompetition(handler.PlayerTotal, handler.EnemyTotal, _player.EmotionResources);
        _uiPresenter.SetCompetitionEmotion(_forcedEmotion.GetPreviousEmotion());
        _uiPresenter.SetCompetitionRaiseInteractable(false);
        _uiPresenter.SetCompetitionEmotionInteractable(false);

        // 競合UIが表示された後、操作を有効にする前にチュートリアルを表示
        await _uiPresenter.StartTutorial("ResultDetermination");
        _uiPresenter.SetCompetitionEmotionInteractable(true);

        await _uiPresenter.OnCompetitionEmotionSelected
            .Where(emotion => emotion == _forcedEmotion)
            .FirstAsync();

        handler.ResetTimeout();
        _uiPresenter.SetCompetitionEmotionInteractable(false);
        _uiPresenter.SetCompetitionRaiseInteractable(true);
        _uiPresenter.OnCompetitionRaise
            .Subscribe(_ =>
            {
                if (handler.PlayerRaises.Count >= _requiredRaises) return;

                if (!handler.TryPlayerRaise(_forcedEmotion, _player)) return;

                SeManager.Instance.PlaySe(_forcedEmotion.ToResourceSeName(), pitch: 1f);
                _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
                _uiPresenter.UpdateCompetitionResources(_player.EmotionResources);

                if (handler.PlayerRaises.Count >= _requiredRaises)
                {
                    _uiPresenter.SetCompetitionRaiseInteractable(false);
                    return;
                }

                pendingEnemyRaiseCount++;
                if (pendingEnemyRaiseCount == 1)
                {
                    nextEnemyRaiseTime = Time.time
                        + Random.Range(ENEMY_RAISE_DELAY_MIN_MILLISECONDS, ENEMY_RAISE_DELAY_MAX_MILLISECONDS + 1) / 1000f;
                }
            })
            .AddTo(disposables);

        while (handler.PlayerRaises.Count < _requiredRaises || !handler.IsTimedOut)
        {
            var remainingTime = handler.PlayerRaises.Count < _requiredRaises
                ? GameConstants.COMPETITION_TIMEOUT_SECONDS
                : handler.RemainingTime;
            _uiPresenter.UpdateCompetitionTimer(remainingTime, GameConstants.COMPETITION_TIMEOUT_SECONDS);

            if (pendingEnemyRaiseCount > 0 && Time.time >= nextEnemyRaiseTime)
            {
                _enemyAI.TryCompetitionRaise(handler);
                pendingEnemyRaiseCount--;
                _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);

                if (pendingEnemyRaiseCount > 0)
                {
                    nextEnemyRaiseTime = Time.time
                        + Random.Range(ENEMY_RAISE_DELAY_MIN_MILLISECONDS, ENEMY_RAISE_DELAY_MAX_MILLISECONDS + 1) / 1000f;
                }
            }

            await UniTask.Yield();
        }

        _uiPresenter.SetCompetitionRaiseInteractable(false);
        handler.End();
        _uiPresenter.HideCompetition();
        await UniTask.Delay(500);
        return handler;
    }
}
