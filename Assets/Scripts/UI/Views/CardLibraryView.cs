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
public class CardLibraryView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject libraryPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private DeckCardView cardPrefab;
    [SerializeField] private TextMeshProUGUI statisticsText;
    
    private readonly List<DeckCardView> _cardViews = new();
    private readonly CompositeDisposable _disposables = new();
    
    // AllCardDataを保持
    private AllCardData _allCardData;
    
    /// <summary>
    /// カード図鑑を表示
    /// </summary>
    /// <param name="allCardData">全カードデータ</param>
    /// <param name="viewedCardIds">閲覧済みカードIDのセット</param>
    public void Show(AllCardData allCardData, HashSet<string> viewedCardIds)
    {
        // データを保持
        _allCardData = allCardData;
        
        libraryPanel.SetActive(true);
        UpdateLibraryDisplay(viewedCardIds);
    }
    
    /// <summary>
    /// カード図鑑を非表示
    /// </summary>
    private void Hide()
    {
        libraryPanel.SetActive(false);
    }
    
    /// <summary>
    /// カード図鑑表示を更新
    /// </summary>
    /// <param name="viewedCardIds">閲覧済みカードIDのセット</param>
    private void UpdateLibraryDisplay(HashSet<string> viewedCardIds)
    {
        // 既存のカードViewをクリア
        ClearCardViews();
        
        // カードを閲覧済み優先でソートしてから生成
        var sortedCards = new List<CardData>();
        
        // 先に閲覧済みカードを追加
        foreach (var cardData in _allCardData.CardList)
        {
            if (viewedCardIds.Contains(cardData.CardId))
            {
                sortedCards.Add(cardData);
            }
        }
        
        // 次に未閲覧カードを追加
        foreach (var cardData in _allCardData.CardList)
        {
            if (!viewedCardIds.Contains(cardData.CardId))
            {
                sortedCards.Add(cardData);
            }
        }
        
        // カードViewを生成
        foreach (var cardData in sortedCards)
        {
            CreateCardView(cardData, !viewedCardIds.Contains(cardData.CardId));
        }
        
        // 統計情報を更新
        var viewedCount = 0;
        foreach (var cardData in _allCardData.CardList)
        {
            if (viewedCardIds.Contains(cardData.CardId))
            {
                viewedCount++;
            }
        }
        statisticsText.text = $"全カード: {_allCardData.CardList.Count}枚 (閲覧済み: {viewedCount}枚)";
        // スクロール位置をリセット
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// カードViewを生成
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    /// <param name="isUnviewed">未閲覧かどうか</param>
    private void CreateCardView(CardData cardData, bool isUnviewed = false)
    {
        var cardView = Instantiate(cardPrefab, contentContainer);
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel, isUnviewed);
        
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
    
    private void Awake()
    {
        // ボタンイベントの設定
        closeButton.OnClickAsObservable().Subscribe(_ => Hide()).AddTo(_disposables);
        
        // 初期状態では非表示
        Hide();
    }
    
    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}