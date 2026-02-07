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
    [Inject] private readonly CardPoolService _cardPoolService;
    [Inject] private readonly GameProgressService _gameProgressService;
    [Inject] private readonly InputActionsProvider _inputActionsProvider;

    private readonly ThemeView _themeView;
    private readonly AnnouncementView _announcementView;
    private readonly NarrationView _playerNarrationView;
    private EnemyView _enemyView;
    private readonly BlackOverlayView _blackOverlayView;

    private readonly EyeBlinkTransitionView _eyeBlinkTransitionView;
    private readonly CompositeDisposable _disposables = new();
    private readonly TutorialPresenter _tutorialPresenter;
    private ThemeData _currentTheme;
    private readonly ValueRankingView _valueRankingView;
    private readonly AuctionView _auctionView;
    private readonly DialoguePhaseView _dialoguePhaseView;
    private readonly RewardPhaseView _rewardPhaseView;
    private readonly MemoryGrowthView _memoryGrowthView;
    private readonly PlayerFaceView _playerFaceView;
    private readonly EnemyFaceView _enemyFaceView;
    private BattlePresenter _battlePresenter;

    public BattleUIPresenter(Player player, AllTutorialData allTutorialData, InputActionsProvider inputActionsProvider)
    {
        // 初期化
        _themeView = Object.FindFirstObjectByType<ThemeView>();
        _announcementView = Object.FindFirstObjectByType<AnnouncementView>();
        _enemyView = Object.FindFirstObjectByType<EnemyView>();
        _blackOverlayView = Object.FindFirstObjectByType<BlackOverlayView>();
        _eyeBlinkTransitionView = Object.FindFirstObjectByType<EyeBlinkTransitionView>();
        _playerNarrationView = Object.FindFirstObjectByType<NarrationView>();

        _valueRankingView = Object.FindFirstObjectByType<ValueRankingView>();
        _valueRankingView.Hide();

        _auctionView = Object.FindFirstObjectByType<AuctionView>();
        _auctionView.Hide();

        _dialoguePhaseView = Object.FindFirstObjectByType<DialoguePhaseView>();
        _dialoguePhaseView.Hide();

        _rewardPhaseView = Object.FindFirstObjectByType<RewardPhaseView>();
        _rewardPhaseView.Hide();

        _memoryGrowthView = Object.FindFirstObjectByType<MemoryGrowthView>();

        _playerFaceView = Object.FindFirstObjectByType<PlayerFaceView>();
        _enemyFaceView = Object.FindFirstObjectByType<EnemyFaceView>();

        _tutorialPresenter = new TutorialPresenter(allTutorialData, inputActionsProvider, player);
    }

    public async UniTask ShowAnnouncement(string message, float duration = 2f) => await _announcementView.DisplayAnnouncement(message, duration);
    public async UniTask ShowPlayerNarration(string message, bool autoAdvance) => await _playerNarrationView.DisplayNarration(message, 2f, autoAdvance);
    public void InitializeEnemyFace(EnemyData enemyData) => _enemyFaceView.Initialize(enemyData);
    public void UpdatePlayerPainGauge(float value) => _playerFaceView.UpdatePainGauge(value);
    public void UpdatePlayerDilutionGauge(float value) => _playerFaceView.UpdateDilutionGauge(value);
    public async UniTask ShowEnemy() => await _enemyView.Show();
    public async UniTask HideEnemy() => await _enemyView.Hide();
    public async UniTask ResetEnemyToDefault() => await _enemyView.ResetToDefaultSprite();
    public async UniTask UpdateEnemySprite(CardAttribute attribute) => await _enemyView.UpdateSpriteForAttribute(attribute);
    public async UniTask ShowBlackOverlay() => await _blackOverlayView.FadeIn();
    public async UniTask HideBlackOverlay() => await _blackOverlayView.FadeOut();
    public async UniTask PlayPhaseTransitionAsync() => await _eyeBlinkTransitionView.PlayTransitionAsync();
    public async UniTask PlayPhaseTransitionOpenAsync() => await _eyeBlinkTransitionView.PlayOpenAsync();
    public async UniTask StartBattleTutorial() => await _tutorialPresenter.StartBattleTutorial();
    public async UniTask StartResultTutorial() => await _tutorialPresenter.StartResultTutorial();
    public UniTask ShowPlayerDialogueAsync(string text) => _dialoguePhaseView.ShowPlayerDialogueAsync(text);
    public UniTask HidePlayerDialogueAsync() => _dialoguePhaseView.HidePlayerDialogueAsync();
    public UniTask ShowEnemyDialogueAsync(string text) => _dialoguePhaseView.ShowEnemyDialogueAsync(text);
    public UniTask HideEnemyDialogueAsync() => _dialoguePhaseView.HideEnemyDialogueAsync();
    public UniTask ShowDialogueResultAsync(string message) => _dialoguePhaseView.ShowResultAsync(message);
    public UniTask HideDialogueResultAsync() => _dialoguePhaseView.HideResultAsync();
    public void ShowDialogueView() => _dialoguePhaseView.Show();
    /// <summary>
    /// 対話フェーズViewを敵データで初期化
    /// </summary>
    public void InitializeDialogueView(EnemyData enemyData) => _dialoguePhaseView.Initialize(enemyData);
    // AuctionViewを表示（カードは再生成しない）
    public void ShowAuctionView() => _auctionView.Show();
    // 結果表示（AuctionResultフェーズ）
    public void ShowAuctionResults(IReadOnlyList<AuctionJudge.AuctionResultEntry> results) => _auctionView.ShowResults(results);
    // 順次結果表示（各カードごとにアニメーション付き）
    public async UniTask ShowAuctionResultsSequentialAsync(
        IReadOnlyList<AuctionJudge.AuctionResultEntry> results,
        ValueRankingModel playerRanking,
        ValueRankingModel enemyRanking) =>
        await _auctionView.ShowResultsSequentialAsync(results, playerRanking, enemyRanking);
    // 入札対象カード公開演出
    public async UniTask ShowBidTargetsAsync(BidModel playerBids, BidModel enemyBids, float duration = 2f) =>
        await _auctionView.ShowBidTargetsAsync(playerBids, enemyBids, duration);
    // 全カードを再表示
    public void ShowAllAuctionCards() => _auctionView.ShowAllCards();
    // オークションView非表示
    public void HideAuctionView() => _auctionView.Hide();
    // 報酬フェーズを非表示
    public void HideRewardView() => _rewardPhaseView.Hide();

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

    // 価値順位設定UIを表示し、完了を待機
    public async UniTask<IReadOnlyList<CardModel>> WaitForValueRankingAsync(IReadOnlyList<CardModel> cards)
    {
        _valueRankingView.Show();
        _valueRankingView.StartRanking(cards);

        // 完了を待機
        await _valueRankingView.OnRankingComplete.FirstAsync();

        var result = _valueRankingView.GetRankedCards();

        // 少し待ってから次のフェーズへ
        await UniTask.Delay(1000);

        // トランジション：閉じる（黒フェードで画面を覆う）
        await _eyeBlinkTransitionView.PlayCloseAsync();

        _valueRankingView.Hide();

        return result;
    }

    // カード公開（CardRevealフェーズ）
    public void ShowAuctionCards(
        IReadOnlyList<CardModel> playerCards,
        IReadOnlyList<CardModel> enemyCards,
        IReadOnlyDictionary<EmotionType, int> emotionResources,
        ValueRankingModel playerRanking = null)
    {
        _auctionView.Show();
        _auctionView.ShowCards(playerCards, enemyCards, playerRanking);
        _auctionView.UpdateEmotionResources(emotionResources);
        _auctionView.SetSelectedEmotion(EmotionType.Joy);
    }

    // 入札待機（BiddingPhaseフェーズ）
    public async UniTask WaitForBiddingAsync(
        IReadOnlyList<CardModel> playerCards,
        IReadOnlyList<CardModel> enemyCards,
        BidModel playerBids,
        EmotionType initialEmotion,
        IReadOnlyDictionary<EmotionType, int> emotionResources)
    {
        _auctionView.Show();
        _auctionView.StartBidding(playerCards, enemyCards, playerBids, initialEmotion, emotionResources);

        await _auctionView.OnBiddingComplete.FirstAsync();
    }

    // AuctionViewをクリアして非表示
    public void ClearAuctionView()
    {
        _auctionView.Clear();
        _auctionView.Hide();
    }

    public async UniTask<DialogueChoiceType> WaitForFourChoiceAsync()
    {
        var labels = new List<string>
        {
            DialogueChoiceType.Provoke.ToJapaneseName() + "する",
            DialogueChoiceType.Empathize.ToJapaneseName() + "する",
            DialogueChoiceType.Persuade.ToJapaneseName() + "する",
            DialogueChoiceType.Silence.ToJapaneseName() + "する"
        };

        await _dialoguePhaseView.SetupChoices(labels);
        _dialoguePhaseView.Show();

        var selectedIndex = await _dialoguePhaseView.OnChoiceSelected.FirstAsync();

        _dialoguePhaseView.HideChoices();

        return (DialogueChoiceType)selectedIndex;
    }

    public async UniTask<int> WaitForThreeResponseAsync(List<string> options)
    {
        await _dialoguePhaseView.SetupChoices(options);

        var selectedIndex = await _dialoguePhaseView.OnChoiceSelected.FirstAsync();

        _dialoguePhaseView.HideChoices();

        return selectedIndex;
    }

    public async UniTask HideDialogueViewAsync()
    {
        await _dialoguePhaseView.HideAllAsync();
        _dialoguePhaseView.Hide();
    }

    // 報酬フェーズ：報酬計算結果を表示
    public async UniTask<IReadOnlyDictionary<EmotionType, int>> ShowRewardsAsync(
        Dictionary<CardModel, RewardCalculator.RewardResult> results,
        IReadOnlyDictionary<EmotionType, int> currentResources,
        IReadOnlyDictionary<EmotionType, int> maxResources)
    {
        await _rewardPhaseView.ShowRewardsAsync(results, currentResources, maxResources);
        return _rewardPhaseView.RewardedAmounts;
    }

    /// <summary>
    /// 記憶育成フェーズを表示
    /// </summary>
    /// <param name="allThemes">全獲得テーマリスト</param>
    public async UniTask ShowMemoryGrowthAsync(IReadOnlyList<AcquiredTheme> allThemes)
    {
        _memoryGrowthView.ShowMemoryGrowth(allThemes);
        await _memoryGrowthView.WaitForContinueAsync();
    }

    // ルートボタンを初期選択
    public void Start() => SafeNavigationManager.SelectRootForceSelectable().Forget();

    // すべてのViewのイベントを解除
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
