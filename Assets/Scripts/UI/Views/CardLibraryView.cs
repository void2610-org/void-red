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
    public void Show(AllCardData allCardData)
    {
        // データを保持
        _allCardData = allCardData;
        
        libraryPanel.SetActive(true);
        UpdateLibraryDisplay();
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
    private void UpdateLibraryDisplay()
    {
        // 既存のカードViewをクリア
        ClearCardViews();
        
        // カードViewを生成
        foreach (var cardData in _allCardData.CardList.OrderBy(cd => cd.CardId))
        {
            CreateCardView(cardData);
        }
        
        // 統計情報を更新
        statisticsText.text = $"全カード: {_allCardData.CardList.Count}枚";
        // スクロール位置をリセット
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// カードViewを生成
    /// </summary>
    private void CreateCardView(CardData cardData)
    {
        var cardView = Instantiate(cardPrefab, contentContainer);
        // CardDataをCardModelに変換して表示（図鑑なので崩壊状態は常にfalse）
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel);
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