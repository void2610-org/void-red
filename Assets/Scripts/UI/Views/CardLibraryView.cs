using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

/// <summary>
/// カード図鑑を表示するViewクラス
/// ゲーム内の全カードを閲覧できる
/// </summary>
public class CardLibraryView : BaseWindowView
{
    [Header("UIコンポーネント")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private DeckCardView cardPrefab;
    [SerializeField] private TextMeshProUGUI statisticsText;

    public Observable<CardData> OnCardClicked => _onCardClicked;

    private readonly List<DeckCardView> _cardViews = new();
    private readonly Subject<CardData> _onCardClicked = new();

    // AllCardDataと閲覧済みカードIDを保持
    private AllCardData _allCardData;
    private HashSet<string> _viewedCardIds;

    /// <summary>
    /// カード図鑑を表示
    /// </summary>
    /// <param name="allCardData">全カードデータ</param>
    /// <param name="viewedCardIds">閲覧済みカードIDのセット</param>
    public void Show(AllCardData allCardData, HashSet<string> viewedCardIds)
    {
        // データを保持
        _allCardData = allCardData;
        _viewedCardIds = viewedCardIds;

        // パネルを表示
        Show();
    }

    public override void Show()
    {
        UpdateLibraryDisplay(_viewedCardIds);
        base.Show();
    }
    
    /// <summary>
    /// カード図鑑表示を更新
    /// </summary>
    /// <param name="viewedCardIds">閲覧済みカードIDのセット</param>
    private void UpdateLibraryDisplay(HashSet<string> viewedCardIds)
    {
        // 既存のカードViewをクリア
        ClearCardViews();
        
        // 閲覧済みカードを優先してソート
        var sortedCards = _allCardData.CardList
            .OrderBy(cardData => !viewedCardIds.Contains(cardData.CardId))
            .ToList();

        // カードViewを生成
        foreach (var cardData in sortedCards)
        {
            CreateCardView(cardData, viewedCardIds.Contains(cardData.CardId));
        }
        
        // 統計情報を更新
        var allCardCount = _allCardData.CardList.Count;
        var viewedCount = _allCardData.CardList.Count(cardData => viewedCardIds.Contains(cardData.CardId));
        statisticsText.text = $"全カード: {allCardCount}枚 (閲覧済み: {viewedCount}枚)";
        // スクロール位置をリセット
        scrollRect.horizontalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// カードViewを生成
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    /// <param name="isRevealed">閲覧済みかどうか</param>
    private void CreateCardView(CardData cardData, bool isRevealed)
    {
        var cardView = Instantiate(cardPrefab, contentContainer);
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel, isRevealed ? CardDisplayState.Normal : CardDisplayState.Veiled);

        // カードクリックイベントを購読
        cardView.OnCardClicked
            .Subscribe(clickedCardData => _onCardClicked.OnNext(clickedCardData))
            .AddTo(Disposables);

        _cardViews.Add(cardView);
    }
    
    /// <summary>
    /// 既存のカードViewをクリア
    /// </summary>
    private void ClearCardViews()
    {
        foreach (var cardView in _cardViews)
        {
            if (cardView) Destroy(cardView.gameObject);
        }
        _cardViews.Clear();
    }
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onCardClicked?.Dispose();
    }
}