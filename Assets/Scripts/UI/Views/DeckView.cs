using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

/// <summary>
/// デッキ内容を表示するViewクラス
/// カテゴリ別にカードを表示し、統計情報も提供
/// </summary>
public class DeckView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject deckPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private DeckCardView cardPrefab;
    
    [Header("統計情報")]
    [SerializeField] private TextMeshProUGUI totalCardsText;
    [SerializeField] private TextMeshProUGUI activeCardsText;
    [SerializeField] private TextMeshProUGUI collapsedCardsText;
    
    [Header("カテゴリ切り替え")]
    [SerializeField] private Button showAllButton;
    [SerializeField] private Button showActiveButton; 
    [SerializeField] private Button showCollapsedButton;
    
    private readonly List<DeckCardView> _cardViews = new();
    private readonly CompositeDisposable _disposables = new();
    private DeckDisplayMode _currentMode = DeckDisplayMode.All;
    private System.Action _refreshDataCallback;
    
    private enum DeckDisplayMode
    {
        All,        // 全カード
        Active,     // 使用可能カード
        Collapsed   // 崩壊カード
    }
    
    /// <summary>
    /// デッキを表示
    /// </summary>
    /// <param name="allCards">全カード</param>
    /// <param name="activeCards">使用可能カード</param>
    /// <param name="collapsedCards">崩壊カード</param>
    /// <param name="refreshCallback">データ再取得用のコールバック</param>
    public void ShowDeck(List<CardData> allCards, List<CardData> activeCards, List<CardData> collapsedCards, System.Action refreshCallback = null)
    {
        _refreshDataCallback = refreshCallback;
        deckPanel.SetActive(true);
        UpdateDeckDisplay(allCards, activeCards, collapsedCards);
        UpdateStatistics(allCards, activeCards, collapsedCards);
        UpdateButtonStates();
    }
    
    /// <summary>
    /// デッキを非表示
    /// </summary>
    private void HideDeck()
    {
        deckPanel.SetActive(false);
    }
    
    /// <summary>
    /// 表示モードを設定
    /// </summary>
    private void SetDisplayMode(DeckDisplayMode mode)
    {
        _currentMode = mode;
        UpdateButtonStates();
        
        // データ再取得コールバックがある場合は実行
        _refreshDataCallback?.Invoke();
    }
    
    /// <summary>
    /// デッキ表示を更新
    /// </summary>
    private void UpdateDeckDisplay(List<CardData> allCards, List<CardData> activeCards, List<CardData> collapsedCards)
    {
        // 既存のカードViewをクリア
        ClearCardViews();
        
        // 表示するカードリストを決定
        var displayCards = _currentMode switch
        {
            DeckDisplayMode.All => allCards,
            DeckDisplayMode.Active => activeCards,
            DeckDisplayMode.Collapsed => collapsedCards,
            _ => allCards
        };
        
        // カードViewを生成
        foreach (var card in displayCards)
        {
            CreateCardView(card, activeCards, collapsedCards);
        }
        
        // スクロール位置をリセット
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// カードViewを生成
    /// </summary>
    private void CreateCardView(CardData card, List<CardData> activeCards, List<CardData> collapsedCards)
    {
        var cardView = Instantiate(cardPrefab, contentContainer);
        var isActive = activeCards.Contains(card);
        var isCollapsed = collapsedCards.Contains(card);
        
        cardView.Initialize(card, isActive, isCollapsed);
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
    
    /// <summary>
    /// 統計情報を更新
    /// </summary>
    private void UpdateStatistics(List<CardData> allCards, List<CardData> activeCards, List<CardData> collapsedCards)
    {
        totalCardsText.text = $"全カード: {allCards.Count}枚";
        activeCardsText.text = $"使用可能: {activeCards.Count}枚";
        collapsedCardsText.text = $"崩壊: {collapsedCards.Count}枚";
    }
    
    /// <summary>
    /// ボタンの状態を更新
    /// </summary>
    private void UpdateButtonStates()
    {
        // ボタンの選択状態を更新（色やスケールなどで視覚的に示す）
        var allButtonColor = _currentMode == DeckDisplayMode.All ? Color.yellow : Color.white;
        var activeButtonColor = _currentMode == DeckDisplayMode.Active ? Color.yellow : Color.white;
        var collapsedButtonColor = _currentMode == DeckDisplayMode.Collapsed ? Color.yellow : Color.white;
        
        if (showAllButton) showAllButton.GetComponent<Image>().color = allButtonColor;
        if (showActiveButton) showActiveButton.GetComponent<Image>().color = activeButtonColor;
        if (showCollapsedButton) showCollapsedButton.GetComponent<Image>().color = collapsedButtonColor;
    }
    
    private void Awake()
    {
        // ボタンイベントの設定
        closeButton.OnClickAsObservable().Subscribe(_ => HideDeck()).AddTo(_disposables);
        showAllButton.OnClickAsObservable().Subscribe(_ => SetDisplayMode(DeckDisplayMode.All)).AddTo(_disposables);
        showActiveButton.OnClickAsObservable().Subscribe(_ => SetDisplayMode(DeckDisplayMode.Active)).AddTo(_disposables);
        showCollapsedButton.OnClickAsObservable().Subscribe(_ => SetDisplayMode(DeckDisplayMode.Collapsed)).AddTo(_disposables);
        
        // 初期状態では非表示
        HideDeck();
    }
    
    private void OnDestroy()
    {
        _disposables?.Dispose();
    }
}