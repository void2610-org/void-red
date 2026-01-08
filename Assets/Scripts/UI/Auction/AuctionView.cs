using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;

// オークションフェーズの統合View
// CardReveal, BiddingPhase, AuctionResultで共有
public class AuctionView : MonoBehaviour
{
    [Header("カード表示")]
    [SerializeField] private Transform playerCardContainer;
    [SerializeField] private Transform enemyCardContainer;
    [SerializeField] private CardView cardPrefab;

    [Header("入札UI")]
    [SerializeField] private BidPanelView bidPanelView;

    [Header("感情リソース表示")]
    [SerializeField] private EmotionResourceDisplayView emotionResourceDisplayView;

    [Header("結果表示")]
    [SerializeField] private CardBidInfoView cardBidInfoPrefab;

    public Observable<Unit> OnBiddingComplete => _onBiddingComplete;

    private readonly List<CardView> _cardViews = new();
    private readonly Dictionary<CardView, CardModel> _cardViewToModel = new();
    private readonly Dictionary<CardView, CardBidInfoView> _cardBidInfoViews = new();
    private readonly Subject<Unit> _onBiddingComplete = new();
    private CompositeDisposable _disposables = new();

    private CardView _selectedCard;
    private CardModel _selectedCardModel;
    private BidModel _playerBids;
    private EmotionType _currentEmotion;
    private IReadOnlyDictionary<EmotionType, int> _emotionResources;
    private Dictionary<EmotionType, int> _usedResources = new();

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

            // 入札情報Viewを生成
            var bidInfoView = Instantiate(cardBidInfoPrefab, cardView.transform);
            if (playerRanking != null)
            {
                var rank = playerRanking.GetRanking(card);
                bidInfoView.ShowPlayerRankOnly(rank);
            }
            else
            {
                bidInfoView.HideRanks();
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
                bidInfoView.ShowPlayerRankOnly(rank);
            }
            else
            {
                bidInfoView.HideRanks();
            }
            bidInfoView.ShowPlayerBidOnly(0);
            bidInfoView.HideResult();
            _cardBidInfoViews[cardView] = bidInfoView;
        }
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

        // 感情リソース表示を初期化
        emotionResourceDisplayView.Initialize();
        emotionResourceDisplayView.UpdateResources(emotionResources);
        emotionResourceDisplayView.SetSelectedEmotion(_currentEmotion);

        // 入札パネルを初期化
        bidPanelView.UpdateCurrentEmotion(_currentEmotion);
        UpdateRemainingResourceDisplay();

        bidPanelView.OnIncrease
            .Subscribe(_ => OnIncreaseBid())
            .AddTo(_disposables);

        bidPanelView.OnDecrease
            .Subscribe(_ => OnDecreaseBid())
            .AddTo(_disposables);

        bidPanelView.OnConfirm
            .Subscribe(_ => OnConfirmBidding())
            .AddTo(_disposables);

        bidPanelView.OnEmotionCycle
            .Subscribe(_ => OnCycleEmotion())
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
                bidInfoView.ShowResult(result.IsPlayerWon);
            }
        }
    }

    // 順次結果表示（各カードごとにアニメーション付き）
    public async UniTask ShowResultsSequentialAsync(
        IReadOnlyList<AuctionJudge.AuctionResultEntry> results,
        ValueRankingModel playerRanking,
        ValueRankingModel enemyRanking,
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

            // 価値順位を公開
            var playerRank = playerRanking.GetRanking(result.Card);
            var enemyRank = enemyRanking.GetRanking(result.Card);
            bidInfoView.ShowRanks(playerRank, enemyRank);

            // 入札額を公開
            bidInfoView.ShowBidAmounts(result.PlayerBid, result.EnemyBid);

            if (result.NoBids)
            {
                // 入札なし → フェードアウト
                await targetCardView.PlayFadeOutAsync();
            }
            else
            {
                // 勝敗表示
                bidInfoView.ShowResult(result.IsPlayerWon);
                await UniTask.Delay(300);

                // 落札者側へ移動
                if (result.IsPlayerWon)
                {
                    await targetCardView.PlayMoveToPlayerSideAsync();
                }
                else
                {
                    await targetCardView.PlayMoveToEnemySideAsync();
                }
            }

            await UniTask.Delay((int)(delayBetweenCards * 1000));
        }
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

    public void Clear()
    {
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

        _playerBids = null;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnCardClicked(CardView cardView)
    {
        if (_selectedCard == cardView)
        {
            DeselectCard();
            return;
        }

        SelectCard(cardView);
    }

    private void SelectCard(CardView cardView)
    {
        DeselectCard();

        _selectedCard = cardView;
        _selectedCardModel = _cardViewToModel[cardView];
        _selectedCard.SetHighlight(true);
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

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedCard);
    }

    private void OnDecreaseBid()
    {
        if (_selectedCardModel == null) return;

        // 現在の感情での入札量を取得
        var emotionBids = _playerBids.GetBidsByEmotion(_selectedCardModel);
        var currentEmotionBid = emotionBids.TryGetValue(_currentEmotion, out var bid) ? bid : 0;
        if (currentEmotionBid <= 0) return;

        // 現在の感情での入札を減少
        _playerBids.AddBid(_selectedCardModel, _currentEmotion, -1);
        var used = _usedResources.TryGetValue(_currentEmotion, out var u) ? u : 0;
        _usedResources[_currentEmotion] = Math.Max(0, used - 1);

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedCard);
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
        _onBiddingComplete.OnNext(Unit.Default);
    }

    private void OnCycleEmotion()
    {
        _currentEmotion = GetNextEmotion(_currentEmotion);
        bidPanelView.UpdateCurrentEmotion(_currentEmotion);
        emotionResourceDisplayView.SetSelectedEmotion(_currentEmotion);
        UpdateRemainingResourceDisplay();
    }

    private EmotionType GetNextEmotion(EmotionType current)
    {
        var values = (EmotionType[])Enum.GetValues(typeof(EmotionType));
        var currentIndex = Array.IndexOf(values, current);
        return values[(currentIndex + 1) % values.Length];
    }

    private void UpdateRemainingResourceDisplay()
    {
        // 現在の感情のリソース残量を計算
        var available = _emotionResources.TryGetValue(_currentEmotion, out var total) ? total : 0;
        var used = _usedResources.TryGetValue(_currentEmotion, out var u) ? u : 0;
        var remaining = available - used;
        bidPanelView.UpdateRemainingResource(remaining);

        // 全感情のリソース残量を更新
        var currentResources = new Dictionary<EmotionType, int>();
        foreach (var (emotion, originalAmount) in _emotionResources)
        {
            var usedAmount = _usedResources.TryGetValue(emotion, out var uAmount) ? uAmount : 0;
            currentResources[emotion] = originalAmount - usedAmount;
        }
        emotionResourceDisplayView.UpdateResources(currentResources);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onBiddingComplete.Dispose();
    }
}
