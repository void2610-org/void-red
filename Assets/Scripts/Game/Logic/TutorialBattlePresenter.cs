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
        CompetitionPhaseRunner = new TutorialCompetitionPhaseRunner(
            Player,
            EnemyAI,
            BattleUIPresenter,
            _tutorialBattlePlayerData);
        AuctionProcessor = new AuctionProcessor(Player, Enemy, BattleUIPresenter, CompetitionPhaseRunner);
    }

    protected override void InitializeDeckSelectionView(IReadOnlyList<CardModel> wonCards) =>
        BattleUIPresenter.InitializeDeckSelection(wonCards, _tutorialBattlePlayerData.DeckAllowedCardIndices);

    protected override EmotionType GetDeckSelectionSkill(EmotionType defaultSkill) =>
        _tutorialBattlePlayerData.DeckSelectionForcedSkillEmotion;

    protected override EmotionType GetBattleSkill(EmotionType defaultSkill) =>
        _tutorialBattlePlayerData.BattleForcedSkillEmotion;

    protected override bool RequiresDeckSelectionSkillActivation(EmotionType playerSkill) => true;

    protected override bool IsBattleSkillAvailable(CardBattleHandler handler) =>
        handler.PlayerSkillAvailable && handler.CurrentRound == _tutorialBattlePlayerData.SkillRoundIndex;

    protected override bool RequiresBattleSkillActivation(CardBattleHandler handler, EmotionType playerSkill) =>
        handler.CurrentRound == _tutorialBattlePlayerData.SkillRoundIndex;

    protected override VictoryCondition GetBattleVictoryCondition(VictoryCondition defaultVictoryCondition) =>
        _tutorialBattlePlayerData.BattleVictoryCondition;

    protected override EmotionType GetEnemyBattleEmotionState(CardBattleHandler handler, EmotionType currentEmotionState) =>
        EnemyAI.DecideEmotionState();

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

    protected override async UniTask HandleAuctionResult()
    {
        await BattleUIPresenter.StartTutorial("ResultDetermination");
        await base.HandleAuctionResult();
    }

    protected override async UniTask HandleThemeAnnouncement()
    {
        await base.HandleThemeAnnouncement();
        await BattleUIPresenter.StartTutorial("BeforeThemeAnnouncement");
    }

    protected override async UniTask HandleBiddingPhase()
    {
        Debug.Log("[TutorialBattlePresenter] 入札フェーズ開始");

        EnemyAI.DecideBids(AuctionCards);

        await BattleUIPresenter.StartTutorial("BiddingPhase");
        await RunTutorialBiddingAsync();

        Debug.Log($"[TutorialBattlePresenter] プレイヤーの入札完了: 合計{Player.Bids.GetTotalBidAmount()}リソース");
        await BattleUIPresenter.ShowBidTargetsAsync(Player.Bids, Enemy.Bids);
    }

    protected override async UniTask OnAfterCardsDisplayed()
    {
        await BattleUIPresenter.StartTutorial("RewardPhase");
    }

    protected override async UniTask OnAfterResourceGaugesDisplayed()
    {
        await BattleUIPresenter.StartTutorial("RewardPhase2");
    }

    protected override async UniTask OnBeforeMemoryGrowthContinueAsync()
    {
        await BattleUIPresenter.StartTutorial("MemoryGrowthPhase");
    }

    protected override void DecideFirstPlayer(CardBattleHandler handler)
    {
        var round = handler.CurrentRound;
        var coinFlipPerRound = _tutorialBattlePlayerData.CoinFlipPerRound;
        if (round < 0 || round >= coinFlipPerRound.Length)
        {
            base.DecideFirstPlayer(handler);
            return;
        }

        handler.SetFirstPlayer(coinFlipPerRound[round]);
    }

    protected override async UniTask<CardModel> SelectBattleCardAsync(CardBattleHandler handler, BattleDeckModel playerDeck)
    {
        if (RequiresBattleSkillActivation(handler, GetBattleSkill(EmotionType.Joy)))
            await BattleUIPresenter.OnSkillActivated.FirstAsync();

        var round = handler.CurrentRound;
        var forcedCardPerRound = _tutorialBattlePlayerData.ForcedCardPerRound;
        if (round < 0 || round >= forcedCardPerRound.Length || !forcedCardPerRound[round].HasValue)
            return await base.SelectBattleCardAsync(handler, playerDeck);

        var forcedDeckCardIndex = forcedCardPerRound[round].Value;
        if (forcedDeckCardIndex < 0 || forcedDeckCardIndex >= playerDeck.Cards.Count)
        {
            Debug.LogError($"[TutorialBattlePresenter] 不正な強制カードインデックス: {forcedDeckCardIndex}");
            return await base.SelectBattleCardAsync(handler, playerDeck);
        }

        var forcedCard = playerDeck.Cards[forcedDeckCardIndex];
        var availableCards = playerDeck.GetAvailableCards();
        var forcedAvailableCardIndex = -1;
        for (var i = 0; i < availableCards.Count; i++)
        {
            if (availableCards[i] != forcedCard)
                continue;

            forcedAvailableCardIndex = i;
            break;
        }

        if (forcedAvailableCardIndex < 0)
        {
            Debug.LogError($"[TutorialBattlePresenter] 強制カードが未使用札に存在しません: {forcedCard}");
            return await base.SelectBattleCardAsync(handler, playerDeck);
        }

        BattleUIPresenter.ShowPlayerHand(availableCards, forcedAvailableCardIndex);
        return await BattleUIPresenter.OnBattleCardSelected.FirstAsync();
    }

    private async UniTask RunTutorialBiddingAsync()
    {
        var forcedCard = AuctionCards[_tutorialBattlePlayerData.BidForcedCardIndex];
        BattleUIPresenter.StartAuctionBidding(
            AuctionCards,
            Player.Bids,
            _tutorialBattlePlayerData.BidForcedEmotion.GetPreviousEmotion(),
            Player.EmotionResources);
        BattleUIPresenter.SetAuctionAllCardsInteractable(false);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);
        BattleUIPresenter.SetAuctionConfirmInteractable(false);
        BattleUIPresenter.SetAuctionBidIncreaseInteractable(false);
        BattleUIPresenter.SetAuctionEmotionInteractable(false);
        BattleUIPresenter.SetAuctionDialogueInteractable(_tutorialBattlePlayerData.BidForcedCardIndex, true);

        await BattleUIPresenter.OnAuctionDialogueRequested
            .Where(card => card == forcedCard)
            .FirstAsync();

        await ShowAuctionDialogueAsync(forcedCard);

        BattleUIPresenter.SetAuctionAllCardsInteractable(false);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);
        BattleUIPresenter.SetAuctionEmotionInteractable(true);

        await BattleUIPresenter.OnAuctionEmotionSelected
            .Where(emotion => emotion == _tutorialBattlePlayerData.BidForcedEmotion)
            .FirstAsync();

        BattleUIPresenter.SetAuctionEmotionInteractable(false);
        BattleUIPresenter.SetAuctionCardInteractable(_tutorialBattlePlayerData.BidForcedCardIndex, true);
        BattleUIPresenter.SetAuctionAllDialogueInteractable(false);

        await BattleUIPresenter.OnAuctionCardClicked
            .Where(index => index == _tutorialBattlePlayerData.BidForcedCardIndex)
            .FirstAsync();

        BattleUIPresenter.SetAuctionBidIncreaseInteractable(true);

        await BattleUIPresenter.OnAuctionBidIncreased
            .Where(_ => Player.Bids.GetTotalBidAmount() >= _tutorialBattlePlayerData.BidRequiredAmount)
            .FirstAsync();

        BattleUIPresenter.SetAuctionBidIncreaseInteractable(false);
        BattleUIPresenter.SetAuctionConfirmInteractable(true);
        await BattleUIPresenter.OnAuctionBiddingConfirmed.FirstAsync();
    }
}

/// <summary>
/// チュートリアル用の競合フェーズを担当するRunner
/// </summary>
public class TutorialCompetitionPhaseRunner : CompetitionPhaseRunner
{
    private readonly Player _player;
    private readonly IEnemyAIController _enemyAI;
    private readonly BattleUIPresenter _uiPresenter;
    private readonly TutorialBattlePlayerData _tutorialBattlePlayerData;

    public TutorialCompetitionPhaseRunner(
        Player player,
        IEnemyAIController enemyAI,
        BattleUIPresenter uiPresenter,
        TutorialBattlePlayerData tutorialBattlePlayerData)
        : base(player, enemyAI, uiPresenter)
    {
        _player = player;
        _enemyAI = enemyAI;
        _uiPresenter = uiPresenter;
        _tutorialBattlePlayerData = tutorialBattlePlayerData;
    }

    public override async UniTask<CompetitionHandler> RunAsync(CardModel card, int playerTotal, int enemyTotal, string instruction)
    {
        var isAuctionCompetition = instruction == "競合発生！";
        var requiredRaises = isAuctionCompetition
            ? _tutorialBattlePlayerData.AuctionCompetitionRequiredRaises
            : _tutorialBattlePlayerData.BattleCompetitionRequiredRaises;
        var forcedEmotion = isAuctionCompetition
            ? _tutorialBattlePlayerData.AuctionCompetitionForcedEmotion
            : _tutorialBattlePlayerData.BattleCompetitionForcedEmotion;

        var handler = new CompetitionHandler();
        handler.Start(card, playerTotal, enemyTotal);

        using var disposables = new CompositeDisposable();

        _uiPresenter.SetBattleInstruction(instruction);
        _uiPresenter.ShowCompetition(handler.PlayerTotal, handler.EnemyTotal, _player.EmotionResources);
        _uiPresenter.SetCompetitionEmotion(forcedEmotion.GetPreviousEmotion());
        _uiPresenter.SetCompetitionRaiseInteractable(false);
        _uiPresenter.SetCompetitionEmotionInteractable(true);

        await _uiPresenter.OnCompetitionEmotionSelected
            .Where(emotion => emotion == forcedEmotion)
            .FirstAsync();

        _uiPresenter.SetCompetitionEmotionInteractable(false);
        _uiPresenter.SetCompetitionRaiseInteractable(true);

        while (handler.PlayerRaises.Count < requiredRaises)
        {
            await _uiPresenter.OnCompetitionRaise.FirstAsync();
            if (!handler.TryPlayerRaise(forcedEmotion, _player))
                continue;

            SeManager.Instance.PlaySe(forcedEmotion.ToResourceSeName(), pitch: 1f);
            _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
            _uiPresenter.UpdateCompetitionResources(_player.EmotionResources);

            if (handler.PlayerRaises.Count >= requiredRaises)
                break;

            _enemyAI.TryCompetitionRaise(handler);
            _uiPresenter.UpdateCompetitionBids(handler.PlayerTotal, handler.EnemyTotal);
        }

        _uiPresenter.SetCompetitionRaiseInteractable(false);
        handler.End();
        _uiPresenter.HideCompetition();
        await UniTask.Delay(500);
        return handler;
    }
}
