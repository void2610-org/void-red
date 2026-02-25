using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// UIのビジネスロジックとイベント処理を担当するPresenterクラス
/// VContainerで依存性注入される
/// </summary>
public class BattleUIPresenter : IStartable, System.IDisposable
{
    public Observable<Unit> OnCompetitionRaise => _competitionView.OnRaise;
    public Observable<EmotionType> OnCompetitionEmotionSelected => _competitionView.OnEmotionSelected;
    public Observable<BattleCardModel> OnBattleCardSelected => _cardBattleView.OnCardSelected;
    public Observable<Unit> OnSkillActivated => _skillButtonView.OnActivated;
    public Observable<Unit> OnBattleNextClicked => _cardBattleView.OnNextClicked;

    [Inject] private readonly CardPoolService _cardPoolService;
    [Inject] private readonly GameProgressService _gameProgressService;
    [Inject] private readonly InputActionsProvider _inputActionsProvider;

    private readonly ThemeView _themeView;
    private readonly AnnouncementView _announcementView;
    private EnemyView _enemyView;
    private readonly BlackOverlayView _blackOverlayView;

    private readonly EyeBlinkTransitionView _eyeBlinkTransitionView;
    private readonly CompositeDisposable _disposables = new();
    private readonly TutorialPresenter _tutorialPresenter;
    private ThemeData _currentTheme;
    private readonly AuctionView _auctionView;
    private readonly CompetitionView _competitionView;
    private readonly RewardPhaseView _rewardPhaseView;
    private readonly MemoryGrowthView _memoryGrowthView;
    private readonly PlayerFaceView _playerFaceView;
    private readonly EnemyFaceView _enemyFaceView;
    private readonly SkillButtonView _skillButtonView;
    private readonly DeckSelectionView _deckSelectionView;
    private readonly CardBattleView _cardBattleView;
    private BattlePresenter _battlePresenter;

    public BattleUIPresenter(Player player, AllTutorialData allTutorialData, InputActionsProvider inputActionsProvider)
    {
        // 初期化
        _themeView = Object.FindFirstObjectByType<ThemeView>();
        _announcementView = Object.FindFirstObjectByType<AnnouncementView>();
        _enemyView = Object.FindFirstObjectByType<EnemyView>();
        _blackOverlayView = Object.FindFirstObjectByType<BlackOverlayView>();
        _eyeBlinkTransitionView = Object.FindFirstObjectByType<EyeBlinkTransitionView>();
        _auctionView = Object.FindFirstObjectByType<AuctionView>();
        _competitionView = Object.FindFirstObjectByType<CompetitionView>();
        _rewardPhaseView = Object.FindFirstObjectByType<RewardPhaseView>();
        _memoryGrowthView = Object.FindFirstObjectByType<MemoryGrowthView>();
        _playerFaceView = Object.FindFirstObjectByType<PlayerFaceView>();
        _enemyFaceView = Object.FindFirstObjectByType<EnemyFaceView>();
        _skillButtonView = Object.FindFirstObjectByType<SkillButtonView>(FindObjectsInactive.Include);
        _deckSelectionView = Object.FindFirstObjectByType<DeckSelectionView>();
        _cardBattleView = Object.FindFirstObjectByType<CardBattleView>();

        _tutorialPresenter = new TutorialPresenter(allTutorialData, inputActionsProvider);
    }

    public async UniTask ShowAnnouncement(string message, float duration = 2f) => await _announcementView.DisplayAnnouncement(message, duration);
    public void UpdatePlayerPainGauge(float value) => _playerFaceView.UpdatePainGauge(value);
    public void UpdatePlayerDilutionGauge(float value) => _playerFaceView.UpdateDilutionGauge(value);
    public async UniTask PlayPhaseTransitionOpenAsync() => await _eyeBlinkTransitionView.PlayOpenAsync();
    public async UniTask PlayPhaseTransitionCloseAsync() => await _eyeBlinkTransitionView.PlayCloseAsync();
    public async UniTask StartTutorial(string tutorialId, params string[] args) => await _tutorialPresenter.StartTutorial(tutorialId, args);
    public async UniTask DisplayCardsAsync(Dictionary<CardModel, RewardCalculator.RewardResult> results) =>
        await _rewardPhaseView.DisplayCardsAsync(results);
    public async UniTask WaitForCardAcquisitionCompleteAsync() =>
        await _rewardPhaseView.WaitForCardAcquisitionCompleteAsync();
    public void ShowAuctionView() => _auctionView.Show();
    public async UniTask ShowAuctionResultsSequentialAsync(
        IReadOnlyList<AuctionJudge.AuctionResultEntry> results, Color enemyColor) =>
        await _auctionView.ShowResultsSequentialAsync(results, enemyColor);
    public async UniTask ShowBidTargetsAsync(BidModel playerBids, BidModel enemyBids, float duration = 2f) => await _auctionView.ShowBidTargetsAsync(playerBids, enemyBids, duration);
    public void HideAuctionView() => _auctionView.Hide();
    public void HideRewardView() => _rewardPhaseView.Hide();
    public void ShowMemoryGrowthView(IReadOnlyList<AcquiredTheme> allThemes) => _memoryGrowthView.ShowMemoryGrowth(allThemes);
    public UniTask WaitForMemoryGrowthCompleteAsync() => _memoryGrowthView.WaitForContinueAsync();
    public void DisplayResourceGauges(
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources) =>
        _rewardPhaseView.DisplayResourceGauges(currentResources, maxResources);

    // 競合フェーズ
    public void ShowCompetition(int playerBid, int enemyBid, IReadOnlyDictionary<EmotionType, int> resources) =>
        _competitionView.Initialize(playerBid, enemyBid, resources);
    public void UpdateCompetitionBids(int playerBid, int enemyBid) => _competitionView.UpdateBids(playerBid, enemyBid);
    public void UpdateCompetitionTimer(float remaining, float max) => _competitionView.UpdateTimer(remaining, max);
    public void UpdateCompetitionResources(IReadOnlyDictionary<EmotionType, int> resources) =>
        _competitionView.UpdateResources(resources);
    public void HideCompetition() => _competitionView.Hide();

    // デッキ選択フェーズ
    public void InitializeDeckSelection(IReadOnlyList<BattleCardModel> wonCards) =>
        _deckSelectionView.Initialize(wonCards);
    public async UniTask WaitForDeckSelectionAsync() => await _deckSelectionView.WaitForSelectionAsync();
    public IReadOnlyList<BattleCardModel> GetSelectedDeck() => _deckSelectionView.SelectedCards;
    public void HideDeckSelection() => _deckSelectionView.Hide();

    // スキルボタン
    public void InitializeSkillButton(EmotionType emotion) => _skillButtonView.Initialize(emotion);
    public void SetSkillButtonVisible(bool visible) => _skillButtonView.SetVisible(visible);

    // カードバトルフェーズ
    public void InitializeBattle(VictoryCondition condition) =>
        _cardBattleView.Initialize(condition);
    public void ShowPlayerHand(IReadOnlyList<BattleCardModel> availableCards) =>
        _cardBattleView.ShowPlayerHand(availableCards);
    public void PlacePlayerCard(BattleCardModel card) => _cardBattleView.PlacePlayerCard(card);
    public void PlaceEnemyCard() => _cardBattleView.PlaceEnemyCard();
    public void RevealCards(BattleCardModel playerCard, BattleCardModel enemyCard) =>
        _cardBattleView.RevealCards(playerCard, enemyCard);
    public void SetBattleInstruction(string text) => _cardBattleView.SetInstruction(text);
    public async UniTask WaitForBattleNextAsync() => await _cardBattleView.WaitForNextAsync();
    public void ClearBattleField() => _cardBattleView.ClearField();
    public void HideBattle() => _cardBattleView.Hide();

    public void SetBattlePresenter(BattlePresenter battlePresenter)
    {
        _battlePresenter = battlePresenter;
        // BattlePresenterが設定されたらキーバインドをセットアップ
        BattleKeyBindings.Setup(_inputActionsProvider, this, _themeView, _battlePresenter.CurrentGameState, _disposables);
    }

    public async UniTask SetTheme(ThemeData theme, bool isMainTheme)
    {
        _currentTheme = theme;
        await _themeView.DisplayThemeWithKeywords(theme, isMainTheme);
    }

    public void InitializeEnemy(EnemyData enemyData)
    {
        _enemyView = Object.FindFirstObjectByType<EnemyView>();
        _enemyView.Initialize(enemyData);
    }

    // カード提示（6枚共有表示）
    public void ShowAuctionCards(
        IReadOnlyList<CardModel> auctionCards,
        IReadOnlyDictionary<EmotionType, int> emotionResources)
    {
        _auctionView.Show();
        _auctionView.ShowCards(auctionCards);
        _auctionView.UpdateEmotionResources(emotionResources);
        _auctionView.SetSelectedEmotion(EmotionType.Joy);
    }

    // 入札待機（BiddingPhaseフェーズ）
    public async UniTask WaitForBiddingAsync(
        IReadOnlyList<CardModel> auctionCards,
        BidModel playerBids,
        EmotionType initialEmotion,
        IReadOnlyDictionary<EmotionType, int> emotionResources)
    {
        _auctionView.Show();
        _auctionView.StartBidding(auctionCards, playerBids, initialEmotion, emotionResources);

        await _auctionView.OnBiddingComplete.FirstAsync();
    }

    // AuctionViewをクリアして非表示
    public void ClearAuctionView()
    {
        _auctionView.Clear();
        _auctionView.Hide();
    }

    public async UniTask<IReadOnlyDictionary<EmotionType, int>> AnimateResourceRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results)
    {
        await _rewardPhaseView.AnimateResourceRewardsAsync(results);
        return _rewardPhaseView.RewardedAmounts;
    }

    // ルートボタンを初期選択
    public void Start() => SafeNavigationManager.SelectRootForceSelectable().Forget();

    // すべてのViewのイベントを解除
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
