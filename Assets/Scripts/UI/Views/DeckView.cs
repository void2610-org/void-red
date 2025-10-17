using System.Collections.Generic;
using System.Linq;
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
    
    public Observable<CardData> OnCardClicked => _onCardClicked;

    private readonly List<DeckCardView> _cardViews = new();
    private readonly CompositeDisposable _disposables = new();
    private DeckDisplayMode _currentMode = DeckDisplayMode.All;
    private readonly Subject<CardData> _onCardClicked = new();

    // CardModelリストを保持
    private List<CardModel> _cardModels;
    
    private enum DeckDisplayMode
    {
        All,        // 全カード
        Active,     // 使用可能カード
        Collapsed   // 崩壊カード
    }
    
    /// <summary>
    /// デッキを表示
    /// </summary>
    /// <param name="cardModels">カードモデルのリスト</param>
    public void ShowDeck(List<CardModel> cardModels)
    {
        // データを保持
        _cardModels = cardModels;
        
        deckPanel.SetActive(true);
        SafeNavigationManager.SetSelectedGameObjectSafe(closeButton.gameObject);
        UpdateDeckDisplay();
        UpdateStatistics();
        UpdateButtonStates();
    }
    
    /// <summary>
    /// デッキを非表示
    /// </summary>
    private void HideDeck()
    {
        deckPanel.SetActive(false);
        SafeNavigationManager.SelectRootForceSelectable();
    }
    
    /// <summary>
    /// 表示モードを設定
    /// </summary>
    private void SetDisplayMode(DeckDisplayMode mode)
    {
        _currentMode = mode;
        UpdateButtonStates();
        UpdateDeckDisplay();  // 保持されたデータで表示を更新
    }
    
    /// <summary>
    /// デッキ表示を更新
    /// </summary>
    private void UpdateDeckDisplay()
    {
        // 既存のカードViewをクリア
        ClearCardViews();
        
        // データがない場合は何もしない
        if (_cardModels == null || _cardModels.Count == 0) return;
        
        // 表示するカードリストを決定
        var displayCards = _currentMode switch
        {
            DeckDisplayMode.All => _cardModels,
            DeckDisplayMode.Active => _cardModels.Where(cm => !cm.IsCollapsed).ToList(),
            DeckDisplayMode.Collapsed => _cardModels.Where(cm => cm.IsCollapsed).ToList(),
            _ => _cardModels
        };
        
        // カードViewを生成
        foreach (var cardModel in displayCards)
        {
            CreateCardView(cardModel);
        }
        
        // スクロール位置をリセット
        scrollRect.horizontalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// カードViewを生成
    /// </summary>
    private void CreateCardView(CardModel cardModel)
    {
        var cardView = Instantiate(cardPrefab, contentContainer);
        cardView.Initialize(cardModel, cardModel.IsCollapsed ? CardDisplayState.Collapsed : CardDisplayState.Normal);

        // カードクリックイベントを購読
        cardView.OnCardClicked
            .Subscribe(cardData => _onCardClicked.OnNext(cardData))
            .AddTo(_disposables);

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
    private void UpdateStatistics()
    {
        if (_cardModels == null)
        {
            totalCardsText.text = "全カード: 0枚";
            activeCardsText.text = "使用可能: 0枚";
            collapsedCardsText.text = "崩壊: 0枚";
            return;
        }
        
        var totalCount = _cardModels.Count;
        var activeCount = _cardModels.Count(cm => !cm.IsCollapsed);
        var collapsedCount = _cardModels.Count(cm => cm.IsCollapsed);
        
        totalCardsText.text = $"全カード: {totalCount}枚";
        activeCardsText.text = $"使用可能: {activeCount}枚";
        collapsedCardsText.text = $"崩壊: {collapsedCount}枚";
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
        
        showAllButton.GetComponent<Image>().color = allButtonColor;
        showActiveButton.GetComponent<Image>().color = activeButtonColor;
        showCollapsedButton.GetComponent<Image>().color = collapsedButtonColor;
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
        _onCardClicked?.Dispose();
    }
}