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
    [SerializeField] private Transform contentContainer;
    [SerializeField] private DeckCardView cardPrefab;
    [SerializeField] private TextMeshProUGUI statisticsText;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;

    public Observable<CardData> OnCardClicked => _onCardClicked;

    private const int CARDS_PER_PAGE = 8;
    private const int CARDS_PER_PAGE_ROW = 2;

    private readonly List<DeckCardView> _cardViews = new();
    private readonly Subject<CardData> _onCardClicked = new();

    // AllCardDataと閲覧済みカードIDを保持
    private AllCardData _allCardData;
    private HashSet<string> _viewedCardIds;
    private int _currentPage;
    private List<CardData> _sortedCards;

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
        _currentPage = 0;

        // ボタンイベントを設定
        nextPageButton.OnClickAsObservable()
            .Subscribe(_ => NextPage())
            .AddTo(Disposables);
        previousPageButton.OnClickAsObservable()
            .Subscribe(_ => PreviousPage())
            .AddTo(Disposables);

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

        // 閲覧済みカードを優先してソート（初回のみ）
        if (_sortedCards == null)
        {
            _sortedCards = _allCardData.CardList
                .OrderBy(cardData => !viewedCardIds.Contains(cardData.CardId))
                .ToList();
        }

        // 総ページ数を計算
        var totalPages = (_sortedCards.Count + CARDS_PER_PAGE - 1) / CARDS_PER_PAGE;

        // 現在のページのカードを取得
        var startIndex = _currentPage * CARDS_PER_PAGE;
        var cardsToDisplay = _sortedCards.Skip(startIndex).Take(CARDS_PER_PAGE);

        // カードViewを生成
        foreach (var cardData in cardsToDisplay)
            CreateCardView(cardData, viewedCardIds.Contains(cardData.CardId));

        // 統計情報を更新
        var allCardCount = _allCardData.CardList.Count;
        var viewedCount = _allCardData.CardList.Count(cardData => viewedCardIds.Contains(cardData.CardId));
        statisticsText.text = $"全カード: {allCardCount}枚 (閲覧済み: {viewedCount}枚)";
        pageText.text = $"{_currentPage + 1} / {totalPages}";

        // ボタンの有効/無効を設定
        previousPageButton.interactable = _currentPage > 0;
        nextPageButton.interactable = _currentPage < totalPages - 1;
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

        if (isRevealed)
        {
            // カードクリックイベントを購読
            cardView.OnCardClicked
                .Subscribe(clickedCardData => _onCardClicked.OnNext(clickedCardData))
                .AddTo(Disposables);
        }

        _cardViews.Add(cardView);
    }

    /// <summary>
    /// 前のページへ移動
    /// </summary>
    private void PreviousPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            UpdateLibraryDisplay(_viewedCardIds);
        }
    }

    /// <summary>
    /// 次のページへ移動
    /// </summary>
    private void NextPage()
    {
        var totalPages = (_sortedCards.Count + CARDS_PER_PAGE - 1) / CARDS_PER_PAGE;
        if (_currentPage < totalPages - 1)
        {
            _currentPage++;
            UpdateLibraryDisplay(_viewedCardIds);
        }
    }

    /// <summary>
    /// 既存のカードViewをクリア
    /// </summary>
    private void ClearCardViews()
    {
        foreach (var cardView in _cardViews.Where(cardView => cardView))
            Destroy(cardView.gameObject);

        _cardViews.Clear();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onCardClicked?.Dispose();
    }
}