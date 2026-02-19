using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

// オークションフェーズの統合View
// CardReveal, BiddingPhase, AuctionResultで共有
public class AuctionView : BasePhaseView
{
    [Header("カード表示")]
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private Transform enemyCardContainer;
    [SerializeField] private CardView cardPrefab;

    [Header("入札UI")]
    [SerializeField] private BidWindowView bidWindowView;
    [SerializeField] private Button confirmBiddingButton;

    [Header("感情リソース表示")]
    [SerializeField] private EmotionResourceDisplayView emotionResourceDisplayView;

    [Header("結果表示")]
    [SerializeField] private CardBidInfoView cardBidInfoPrefab;

    [Header("カード登場アニメーション")]
    [SerializeField] private StaggeredSlideInGroup playerCardStagger;
    [SerializeField] private StaggeredSlideInGroup enemyCardStagger;

    public Observable<Unit> OnBiddingComplete => confirmBiddingButton.OnClickAsObservable();

    private readonly List<CardView> _cardViews = new();
    private readonly Dictionary<CardView, CardModel> _cardViewToModel = new();
    private readonly Dictionary<CardView, CardBidInfoView> _cardBidInfoViews = new();
    private readonly HashSet<CardModel> _playerCardModels = new();
    private CompositeDisposable _disposables = new();

    private CardView _selectedCard;
    private CardModel _selectedCardModel;
    private BidModel _playerBids;
    private EmotionType _currentEmotion;
    private IReadOnlyDictionary<EmotionType, int> _emotionResources;
    private Dictionary<EmotionType, int> _usedResources = new();

    public void UpdateEmotionResources(IReadOnlyDictionary<EmotionType, int> resources) => emotionResourceDisplayView.UpdateResources(resources);

    public void SetSelectedEmotion(EmotionType emotion) => emotionResourceDisplayView.SetSelectedEmotion(emotion);

    public override void Show() => CanvasGroup.Show();

    // カード表示のみ（CardReveal用）
    public void ShowCards(
        IReadOnlyList<CardModel> playerCards,
        IReadOnlyList<CardModel> enemyCards,
        ValueRankingModel playerRanking = null)
    {
        Clear();

        // プレイヤーのカードを表示
        foreach (var card in playerCards)
        {
            var cardView = Instantiate(cardPrefab, playerCardContainer);
            cardView.Initialize(card.Data);
            cardView.SetInteractable(false);

            _cardViewToModel[cardView] = card;
            _cardViews.Add(cardView);
            _playerCardModels.Add(card);

            // 入札情報Viewを生成
            var bidInfoView = Instantiate(cardBidInfoPrefab, cardView.transform);
            if (playerRanking != null)
            {
                var rank = playerRanking.GetRanking(card);
                bidInfoView.ShowRank(rank, true);
            }
            else
            {
                bidInfoView.HideRank();
            }
            bidInfoView.ShowPlayerBidOnly(0);
            bidInfoView.HideResult();
            _cardBidInfoViews[cardView] = bidInfoView;
        }

        // 敵のカードを表示
        foreach (var card in enemyCards)
        {
            var cardView = Instantiate(cardPrefab, enemyCardContainer);
            cardView.Initialize(card.Data);
            cardView.SetInteractable(false);

            _cardViewToModel[cardView] = card;
            _cardViews.Add(cardView);

            // 入札情報Viewを生成
            var bidInfoView = Instantiate(cardBidInfoPrefab, cardView.transform);
            if (playerRanking != null)
            {
                var rank = playerRanking.GetRanking(card);
                bidInfoView.ShowRank(rank, false);
            }
            else
            {
                bidInfoView.HideRank();
            }
            bidInfoView.ShowPlayerBidOnly(0);
            bidInfoView.HideResult();
            _cardBidInfoViews[cardView] = bidInfoView;
        }

        playerCardStagger.Play();
        enemyCardStagger.Play();
    }

    // 入札フェーズ開始
    public void StartBidding(
        IReadOnlyList<CardModel> playerCards,
        IReadOnlyList<CardModel> enemyCards,
        BidModel playerBids,
        EmotionType initialEmotion,
        IReadOnlyDictionary<EmotionType, int> emotionResources)
    {
        // カードが未表示なら表示
        if (_cardViews.Count == 0)
        {
            ShowCards(playerCards, enemyCards);
        }

        // カードをクリック可能に
        foreach (var cardView in _cardViews)
        {
            cardView.SetInteractable(true);
            cardView.OnClicked
                .Subscribe(OnCardClicked)
                .AddTo(_disposables);
        }

        _playerBids = playerBids;
        _currentEmotion = initialEmotion;
        _emotionResources = emotionResources;

        // 使用リソース初期化
        _usedResources.Clear();
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            _usedResources[emotion] = 0;
        }

        // 感情リソース表示を更新
        emotionResourceDisplayView.UpdateResources(emotionResources);
        emotionResourceDisplayView.SetSelectedEmotion(_currentEmotion);

        // 歯車UI展開SE（入札フェーズ開始時に1回だけ再生）
        SeManager.Instance.PlaySe("SE_GEAR_OPEN", pitch: 1f);

        // 入札UIを初期化
        UpdateRemainingResourceDisplay();

        bidWindowView.OnIncrease
            .Subscribe(_ => OnIncreaseBid())
            .AddTo(_disposables);

        bidWindowView.OnDecrease
            .Subscribe(_ => OnDecreaseBid())
            .AddTo(_disposables);

        bidWindowView.OnClose
            .Subscribe(_ => DeselectCard())
            .AddTo(_disposables);

        confirmBiddingButton.OnClickAsObservable()
            .Subscribe(_ => OnConfirmBidding())
            .AddTo(_disposables);

        // 車輪UIからの感情選択
        emotionResourceDisplayView.OnEmotionSelected
            .Subscribe(OnEmotionChanged)
            .AddTo(_disposables);
    }

    // 入札対象カード公開演出（入札されたカードのみ表示）
    public async UniTask ShowBidTargetsAsync(BidModel playerBids, BidModel enemyBids, float duration = 2f)
    {
        DeselectCard();

        // 各カードの入札状態を入札額表示で表現
        foreach (var cardView in _cardViews)
        {
            var cardModel = _cardViewToModel[cardView];
            var playerBid = playerBids.GetTotalBid(cardModel);
            var enemyBid = enemyBids.GetTotalBid(cardModel);

            cardView.SetInteractable(false);

            if (_cardBidInfoViews.TryGetValue(cardView, out var bidInfoView))
            {
                // 入札対象公開: 自分の入札→数値、相手の入札→?、入札なし→非表示
                bidInfoView.ShowBidTargetReveal(playerBid, enemyBid > 0);
            }
        }

        await UniTask.Delay((int)(duration * 1000));
    }

    // 全カードを再表示
    public void ShowAllCards()
    {
        foreach (var cardView in _cardViews)
        {
            cardView.gameObject.SetActive(true);

            if (_cardBidInfoViews.TryGetValue(cardView, out var bidInfoView))
            {
                bidInfoView.gameObject.SetActive(true);
            }
        }
    }

    // 結果表示（AuctionResult用）
    public void ShowResults(IReadOnlyList<AuctionJudge.AuctionResultEntry> results)
    {
        // カードをクリック不可に
        foreach (var cardView in _cardViews)
        {
            cardView.SetInteractable(false);
        }

        DeselectCard();

        // 結果を表示
        foreach (var result in results)
        {
            // CardModelからCardViewを探す
            CardView targetCardView = null;
            foreach (var kvp in _cardViewToModel)
            {
                if (kvp.Value == result.Card)
                {
                    targetCardView = kvp.Key;
                    break;
                }
            }
            if (targetCardView == null) continue;

            // 既存のCardBidInfoViewを取得、なければ生成
            if (!_cardBidInfoViews.TryGetValue(targetCardView, out var bidInfoView))
            {
                bidInfoView = Instantiate(cardBidInfoPrefab, targetCardView.transform);
                _cardBidInfoViews[targetCardView] = bidInfoView;
            }

            // 両者の入札額を公開
            bidInfoView.ShowBidAmounts(result.PlayerBid, result.EnemyBid);

            if (!result.NoBids)
            {
                if (result.IsDraw)
                    bidInfoView.ShowDraw();
                else
                    bidInfoView.ShowResult(result.IsPlayerWon);
            }
        }
    }

    // 順次結果表示（各カードごとにアニメーション付き）
    public async UniTask ShowResultsSequentialAsync(
        IReadOnlyList<AuctionJudge.AuctionResultEntry> results,
        ValueRankingModel playerRanking,
        ValueRankingModel enemyRanking,
        Color enemyColor,
        float delayBetweenCards = 0.8f)
    {
        // 全カードを表示状態に戻す
        foreach (var cardView in _cardViews)
        {
            cardView.gameObject.SetActive(true);
            cardView.SetInteractable(false);

            if (_cardBidInfoViews.TryGetValue(cardView, out var infoView))
            {
                infoView.gameObject.SetActive(true);
            }
        }

        DeselectCard();

        foreach (var result in results)
        {
            // CardModelからCardViewを探す
            var targetCardView = FindCardView(result.Card);
            if (targetCardView == null) continue;

            // 既存のCardBidInfoViewを取得
            if (!_cardBidInfoViews.TryGetValue(targetCardView, out var bidInfoView)) continue;

            // 価値順位を公開（プレイヤーカードか敵カードかで色を変える）
            var isPlayerCard = _playerCardModels.Contains(result.Card);
            var rank = isPlayerCard ? playerRanking.GetRanking(result.Card) : enemyRanking.GetRanking(result.Card);
            bidInfoView.ShowRank(rank, isPlayerCard);

            // 入札額を公開
            bidInfoView.ShowBidAmounts(result.PlayerBid, result.EnemyBid);

            if (result.NoBids)
            {
                // 入札なし → フェードアウト
                await targetCardView.PlayFadeOutAsync();
            }
            else if (result.IsDraw)
            {
                // 引き分け → グローエフェクト表示、カードは移動させない
                targetCardView.SetGrowEffect(CardView.CardBidState.DrawBid, enemyColor);
                bidInfoView.ShowDraw();
                await UniTask.Delay(300);
            }
            else
            {
                // 落札状態をグローエフェクトで可視化
                var bidState = result.IsPlayerWon ? CardView.CardBidState.PlayerBid : CardView.CardBidState.EnemyBid;
                targetCardView.SetGrowEffect(bidState, enemyColor);

                // 勝敗表示
                bidInfoView.ShowResult(result.IsPlayerWon);
                await UniTask.Delay(300);

                // 落札者側へ移動
                var rt = (RectTransform)targetCardView.transform;
                if (result.IsPlayerWon)
                {
                    await MoveCardToPlayerSideAsync(rt);
                }
                else
                {
                    await MoveCardToEnemySideAsync(rt);
                }
            }

            await UniTask.Delay((int)(delayBetweenCards * 1000));
        }
    }

    public void Clear()
    {
        playerCardStagger.Cancel();
        enemyCardStagger.Cancel();
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        DeselectCard();

        // 入札情報Viewを削除
        foreach (var bidInfoView in _cardBidInfoViews.Values)
        {
            Destroy(bidInfoView.gameObject);
        }
        _cardBidInfoViews.Clear();

        // カードViewを削除
        foreach (var cardView in _cardViews)
        {
            Destroy(cardView.gameObject);
        }
        _cardViews.Clear();
        _cardViewToModel.Clear();
        _playerCardModels.Clear();

        _playerBids = null;
    }

    // CardModelからCardViewを探すヘルパー
    private CardView FindCardView(CardModel cardModel)
    {
        foreach (var kvp in _cardViewToModel)
        {
            if (kvp.Value == cardModel)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private void OnCardClicked(CardView cardView)
    {
        if (_selectedCard == cardView)
        {
            DeselectCard();
            if (bidWindowView.IsShowing)
                bidWindowView.Hide();
            return;
        }

        SelectCard(cardView);
    }

    private void SelectCard(CardView cardView)
    {
        // ウィンドウが既に表示中なら閉じる
        if (bidWindowView.IsShowing)
            bidWindowView.Hide();

        DeselectCard();

        _selectedCard = cardView;
        _selectedCardModel = _cardViewToModel[cardView];
        _selectedCard.SetHighlight(true);

        // ウィンドウに現在の感情の入札値をセット
        bidWindowView.SetEmotion(_currentEmotion);
        var emotionBids = _playerBids.GetBidsByEmotion(_selectedCardModel);
        var currentBid = emotionBids.GetValueOrDefault(_currentEmotion, 0);
        bidWindowView.UpdateBidAmount(currentBid);
        bidWindowView.Show();
    }

    private void DeselectCard()
    {
        if (_selectedCard)
        {
            _selectedCard.SetHighlight(false);
        }
        _selectedCard = null;
        _selectedCardModel = null;
    }

    private void OnIncreaseBid()
    {
        if (_selectedCardModel == null) return;

        // 現在の感情のリソース残量をチェック
        var available = _emotionResources.TryGetValue(_currentEmotion, out var total) ? total : 0;
        var used = _usedResources.TryGetValue(_currentEmotion, out var u) ? u : 0;
        if (used >= available) return;

        // 現在の感情での入札を増加（複数感情対応のためAddBidを使用）
        _playerBids.AddBid(_selectedCardModel, _currentEmotion, 1);
        _usedResources[_currentEmotion] = used + 1;

        // 感情リソース配置SE
        SeManager.Instance.PlaySe(GetResourceSeName(_currentEmotion), pitch: 1f);

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedCard);
        UpdateBidWindowAmount();
    }

    private void OnDecreaseBid()
    {
        if (_selectedCardModel == null) return;

        // 現在の感情での入札量を取得
        var emotionBids = _playerBids.GetBidsByEmotion(_selectedCardModel);
        var currentEmotionBid = emotionBids.TryGetValue(_currentEmotion, out var bid) ? bid : 0;
        if (currentEmotionBid <= 0) return;

        // 現在の感情での入札を減少（SetBidで新しい値を設定）
        _playerBids.SetBid(_selectedCardModel, _currentEmotion, currentEmotionBid - 1);
        var used = _usedResources.TryGetValue(_currentEmotion, out var u) ? u : 0;
        _usedResources[_currentEmotion] = Math.Max(0, used - 1);

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedCard);
        UpdateBidWindowAmount();
    }

    private void UpdateCardBidInfoDisplay(CardView cardView)
    {
        if (_cardBidInfoViews.TryGetValue(cardView, out var bidInfoView))
        {
            var emotionBids = _playerBids.GetBidsByEmotion(_cardViewToModel[cardView]);
            bidInfoView.ShowPlayerBidsWithEmotion(emotionBids);
        }
    }

    private void OnConfirmBidding()
    {
        if (bidWindowView.IsShowing)
            bidWindowView.Hide();
        DeselectCard();
    }

    // ウィンドウ内の入札値表示を更新
    private void UpdateBidWindowAmount()
    {
        if (_selectedCardModel == null) return;
        var emotionBids = _playerBids.GetBidsByEmotion(_selectedCardModel);
        var currentBid = emotionBids.GetValueOrDefault(_currentEmotion, 0);
        bidWindowView.UpdateBidAmount(currentBid);
    }

    private void OnEmotionChanged(EmotionType emotion)
    {
        _currentEmotion = emotion;
        UpdateRemainingResourceDisplay();

        // ウィンドウが表示中なら感情色と入札値を更新
        if (bidWindowView.IsShowing && _selectedCardModel != null)
        {
            bidWindowView.SetEmotion(_currentEmotion);
            UpdateBidWindowAmount();
        }
    }

    private void UpdateRemainingResourceDisplay()
    {
        // 全感情のリソース残量を更新
        var currentResources = new Dictionary<EmotionType, int>();
        foreach (var (emotion, originalAmount) in _emotionResources)
        {
            var usedAmount = _usedResources.GetValueOrDefault(emotion, 0);
            currentResources[emotion] = originalAmount - usedAmount;
        }
        emotionResourceDisplayView.UpdateResources(currentResources);
    }

    // 感情タイプからリソースSE名を取得
    private static string GetResourceSeName(EmotionType emotion) => emotion switch
    {
        EmotionType.Joy => "SE_RESOURCE_JOY",
        EmotionType.Trust => "SE_RESOURCE_TRUST",
        EmotionType.Fear => "SE_RESOURCE_FEAR",
        EmotionType.Surprise => "SE_RESOURCE_WONDER",
        EmotionType.Sadness => "SE_RESOURCE_GRIEF",
        EmotionType.Disgust => "SE_RESOURCE_HATE",
        EmotionType.Anger => "SE_RESOURCE_ANGER",
        EmotionType.Anticipation => "SE_RESOURCE_EXPECT",
        _ => "SE_RESOURCE_JOY"
    };

    private static async UniTask MoveCardToPlayerSideAsync(RectTransform rt, float duration = 0.5f)
    {
        var startY = rt.anchoredPosition.y;
        var targetY = startY - 1000f;
        await LMotion.Create(startY, targetY, duration)
            .WithEase(Ease.OutCubic)
            .Bind(y => rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y))
            .ToUniTask();
    }

    private static async UniTask MoveCardToEnemySideAsync(RectTransform rt, float duration = 0.5f)
    {
        var startY = rt.anchoredPosition.y;
        var targetY = startY + 1000f;
        await LMotion.Create(startY, targetY, duration)
            .WithEase(Ease.OutCubic)
            .Bind(y => rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y))
            .ToUniTask();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
