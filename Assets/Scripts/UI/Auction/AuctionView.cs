using System;
using System.Collections.Generic;
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
    [SerializeField] private Transform cardContainer;
    [SerializeField] private AuctionCardView auctionCardPrefab;

    [Header("入札UI")]
    [SerializeField] private BidWindowView bidWindowView;
    [SerializeField] private Button confirmBiddingButton;

    [Header("感情リソース表示")]
    [SerializeField] private EmotionResourceDisplayView emotionResourceDisplayView;

    [Header("カード登場アニメーション")]
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    public Observable<CardModel> OnDialogueRequested => _onDialogueRequested;
    public Observable<EmotionType> OnEmotionSelected => _onEmotionSelected;
    public Observable<int> OnCardClickedByIndex => _onCardClickedByIndex;
    public Observable<Unit> OnBidIncreased => _onBidIncreased;
    public Observable<Unit> OnBiddingConfirmed => _onBiddingConfirmed;

    private readonly List<AuctionCardView> _auctionCardViews = new();
    private readonly Subject<CardModel> _onDialogueRequested = new();
    private readonly Subject<EmotionType> _onEmotionSelected = new();
    private readonly Subject<int> _onCardClickedByIndex = new();
    private readonly Subject<Unit> _onBidIncreased = new();
    private readonly Subject<Unit> _onBiddingConfirmed = new();
    private CompositeDisposable _disposables = new();

    private AuctionCardView _selectedAuctionCard;
    private BidModel _playerBids;
    private EmotionType _currentEmotion;
    private IReadOnlyDictionary<EmotionType, int> _emotionResources;
    private Dictionary<EmotionType, int> _usedResources = new();

    public void UpdateEmotionResources(IReadOnlyDictionary<EmotionType, int> resources) => emotionResourceDisplayView.UpdateResources(resources);

    public void SetSelectedEmotion(EmotionType emotion) => emotionResourceDisplayView.SetSelectedEmotion(emotion);

    public void SetConfirmInteractable(bool interactable) => confirmBiddingButton.interactable = interactable;

    public void SetBidIncreaseInteractable(bool interactable) => bidWindowView.SetIncreaseInteractable(interactable);

    public void SetEmotionInteractable(bool interactable) => emotionResourceDisplayView.SetInteractable(interactable);

    public override void Show() => CanvasGroup.Show();

    private void OnDialogueClicked(AuctionCardView auctionCard) => _onDialogueRequested.OnNext(auctionCard.CardModel);

    public void SetAllCardsInteractable(bool interactable)
    {
        foreach (var auctionCardView in _auctionCardViews)
            auctionCardView.SetCardInteractable(interactable);
    }

    public void SetCardInteractable(int index, bool interactable)
    {
        if (index < 0 || index >= _auctionCardViews.Count)
            return;

        _auctionCardViews[index].SetCardInteractable(interactable);
    }

    public void SetAllDialogueInteractable(bool interactable)
    {
        foreach (var auctionCardView in _auctionCardViews)
            auctionCardView.SetDialogueInteractable(interactable);
    }

    public void SetDialogueInteractable(int index, bool interactable)
    {
        if (index < 0 || index >= _auctionCardViews.Count)
            return;

        _auctionCardViews[index].SetDialogueInteractable(interactable);
    }

    // カード表示（6枚共有表示）
    public void ShowCards(IReadOnlyList<CardModel> auctionCards)
    {
        Clear();

        foreach (var card in auctionCards)
        {
            var auctionCard = Instantiate(auctionCardPrefab, cardContainer);
            auctionCard.Initialize(card);
            _auctionCardViews.Add(auctionCard);
        }

        cardStagger.Play();
    }

    // 入札フェーズ開始
    public void StartBidding(
        IReadOnlyList<CardModel> auctionCards,
        BidModel playerBids,
        EmotionType initialEmotion,
        IReadOnlyDictionary<EmotionType, int> emotionResources)
    {
        // カードが未表示なら表示
        if (_auctionCardViews.Count == 0)
        {
            ShowCards(auctionCards);
        }

        // カードのクリックイベントを購読
        foreach (var auctionCard in _auctionCardViews)
        {
            auctionCard.OnCardClicked
                .Subscribe(OnCardClicked)
                .AddTo(_disposables);
            auctionCard.OnDialogueClicked
                .Subscribe(OnDialogueClicked)
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

        // 歯車UI展開SE
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

    // 入札対象カード公開演出
    public async UniTask ShowBidTargetsAsync(BidModel playerBids, BidModel enemyBids, float duration = 2f)
    {
        DeselectCard();

        foreach (var auctionCard in _auctionCardViews)
        {
            var playerBid = playerBids.GetTotalBid(auctionCard.CardModel);
            var enemyBid = enemyBids.GetTotalBid(auctionCard.CardModel);

            auctionCard.BidInfoView.ShowBidTargetReveal(enemyBid > 0);
        }

        await UniTask.Delay((int)(duration * 1000));
    }

    // 順次結果表示
    public async UniTask ShowResultsSequentialAsync(
        IReadOnlyList<AuctionJudge.AuctionResultEntry> results,
        Color enemyColor,
        float delayBetweenCards = 0.8f)
    {
        // 全カードを表示状態に戻す
        foreach (var auctionCard in _auctionCardViews)
        {
            auctionCard.gameObject.SetActive(true);
        }

        DeselectCard();

        foreach (var result in results)
        {
            var targetAuctionCard = FindAuctionCardView(result.Card);
            if (!targetAuctionCard) continue;

            var bidInfoView = targetAuctionCard.BidInfoView;
            var cardView = targetAuctionCard.CardView;

            // 入札額を公開
            bidInfoView.ShowBidAmounts(result.EnemyBid);

            if (result.NoBids)
            {
                // 入札なし → フェードアウト
                await targetAuctionCard.FadeOutAsync();
            }
            else if (result.IsDraw)
            {
                // 引き分け → 競合発生の表示
                cardView.SetGrowEffect(CardView.CardBidState.DrawBid, enemyColor);
                bidInfoView.ShowDraw();
                SeManager.Instance.PlaySe("SE_RESULT_CLASH", pitch: 1f);
                await UniTask.Delay(300);
            }
            else
            {
                // 落札状態をグローエフェクトで可視化
                var bidState = result.IsPlayerWon ? CardView.CardBidState.PlayerBid : CardView.CardBidState.EnemyBid;
                cardView.SetGrowEffect(bidState, enemyColor);

                // 勝敗表示
                bidInfoView.ShowResult(result.IsPlayerWon);
                SeManager.Instance.PlaySe(result.IsPlayerWon ? "SE_RESULT_WIN" : "SE_RESULT_LOSE", pitch: 1f);
                await UniTask.Delay(300);

                // 落札者側へ移動
                var rt = (RectTransform)targetAuctionCard.transform;
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
        cardStagger.Cancel();
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        DeselectCard();

        foreach (var auctionCard in _auctionCardViews)
        {
            Destroy(auctionCard.gameObject);
        }
        _auctionCardViews.Clear();

        _playerBids = null;
    }

    private AuctionCardView FindAuctionCardView(CardModel cardModel)
    {
        foreach (var auctionCard in _auctionCardViews)
        {
            if (auctionCard.CardModel == cardModel)
                return auctionCard;
        }
        return null;
    }

    private void OnCardClicked(AuctionCardView auctionCard)
    {
        _onCardClickedByIndex.OnNext(_auctionCardViews.IndexOf(auctionCard));

        if (_selectedAuctionCard == auctionCard)
        {
            DeselectCard();
            if (bidWindowView.IsShowing)
                bidWindowView.Hide();
            return;
        }

        SelectCard(auctionCard);
    }

    private void SelectCard(AuctionCardView auctionCard)
    {
        if (bidWindowView.IsShowing)
            bidWindowView.Hide();

        DeselectCard();

        _selectedAuctionCard = auctionCard;
        _selectedAuctionCard.CardView.SetHighlight(true);

        var cardModel = auctionCard.CardModel;
        bidWindowView.SetCardName(cardModel.Data.CardName);

        // 現在選択中の感情タイプを表示
        bidWindowView.SetEmotion(_currentEmotion);

        var currentBid = _playerBids.GetTotalBid(cardModel);
        bidWindowView.UpdateBidAmount(currentBid);
        bidWindowView.Show();
    }

    private void DeselectCard()
    {
        if (_selectedAuctionCard)
        {
            _selectedAuctionCard.CardView.SetHighlight(false);
        }
        _selectedAuctionCard = null;
    }

    private void OnIncreaseBid()
    {
        if (_selectedAuctionCard == null) return;
        var cardModel = _selectedAuctionCard.CardModel;

        // 1カード1感情制約: 既にこのカードに別の感情がベットされている場合は拒否
        var existingEmotion = _playerBids.GetBidEmotion(cardModel);
        if (existingEmotion.HasValue && existingEmotion.Value != _currentEmotion)
            return;

        // 現在の感情のリソース残量をチェック
        var available = _emotionResources.TryGetValue(_currentEmotion, out var total) ? total : 0;
        var used = _usedResources.TryGetValue(_currentEmotion, out var u) ? u : 0;
        if (used >= available) return;

        // 入札を増加
        var currentBid = _playerBids.GetTotalBid(cardModel);
        _playerBids.SetBid(cardModel, _currentEmotion, currentBid + 1);
        _usedResources[_currentEmotion] = used + 1;

        // 感情リソース配置SE
        SeManager.Instance.PlaySe(_currentEmotion.ToResourceSeName(), pitch: 1f);

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedAuctionCard);
        UpdateBidWindowAmount();
        _onBidIncreased.OnNext(Unit.Default);
    }

    private void OnDecreaseBid()
    {
        if (_selectedAuctionCard == null) return;
        var cardModel = _selectedAuctionCard.CardModel;

        var currentBid = _playerBids.GetTotalBid(cardModel);
        if (currentBid <= 0) return;

        // 1カード1感情制約: ベット中の感情を取得
        var bidEmotion = _playerBids.GetBidEmotion(cardModel);
        if (!bidEmotion.HasValue) return;

        var emotion = bidEmotion.Value;
        _playerBids.SetBid(cardModel, emotion, currentBid - 1);
        var used = _usedResources.TryGetValue(emotion, out var u) ? u : 0;
        _usedResources[emotion] = Math.Max(0, used - 1);

        UpdateRemainingResourceDisplay();
        UpdateCardBidInfoDisplay(_selectedAuctionCard);
        UpdateBidWindowAmount();
    }

    private void UpdateCardBidInfoDisplay(AuctionCardView auctionCard)
    {
        var emotionBids = _playerBids.GetBidsByEmotion(auctionCard.CardModel);
        auctionCard.BidInfoView.ShowPlayerBidsWithEmotion(emotionBids);
    }

    private void OnConfirmBidding()
    {
        _onBiddingConfirmed.OnNext(Unit.Default);

        if (bidWindowView.IsShowing)
            bidWindowView.Hide();
        DeselectCard();
    }

    private void UpdateBidWindowAmount()
    {
        if (_selectedAuctionCard == null) return;
        var currentBid = _playerBids.GetTotalBid(_selectedAuctionCard.CardModel);
        bidWindowView.UpdateBidAmount(currentBid);
    }

    private void OnEmotionChanged(EmotionType emotion)
    {
        _onEmotionSelected.OnNext(emotion);
        _currentEmotion = emotion;
        UpdateRemainingResourceDisplay();

        if (bidWindowView.IsShowing && _selectedAuctionCard != null)
        {
            bidWindowView.SetEmotion(_currentEmotion);
            UpdateBidWindowAmount();
        }
    }

    private void UpdateRemainingResourceDisplay()
    {
        var currentResources = new Dictionary<EmotionType, int>();
        foreach (var (emotion, originalAmount) in _emotionResources)
        {
            var usedAmount = _usedResources.GetValueOrDefault(emotion, 0);
            currentResources[emotion] = originalAmount - usedAmount;
        }
        emotionResourceDisplayView.UpdateResources(currentResources);
    }



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
        _onDialogueRequested.Dispose();
        _onEmotionSelected.Dispose();
        _onCardClickedByIndex.Dispose();
        _onBidIncreased.Dispose();
        _onBiddingConfirmed.Dispose();
    }
}
