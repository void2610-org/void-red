using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

/// <summary>
/// カード詳細情報を表示するモーダルViewクラス
/// 既存のDeckCardViewを活用してカード表示の一貫性を保つ
/// </summary>
public class CardDetailView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button playButton;
    [SerializeField] private DeckCardView cardView;
    [SerializeField] private TextMeshProUGUI attributeText;
    [SerializeField] private TextMeshProUGUI scoreMultiplierText;
    [SerializeField] private TextMeshProUGUI collapseThresholdText;
    [SerializeField] private TextMeshProUGUI evolutionInfoText;
    
    public Observable<Unit> PlayButtonClicked => _playButtonClicked;
    
    private readonly CompositeDisposable _disposables = new();
    private readonly Subject<Unit> _playButtonClicked = new();
    
    /// <summary>
    /// カード詳細を表示
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    public void ShowCardDetail(CardData cardData)
    {
        if (cardData == null) return;
        
        // カード詳細情報を設定
        UpdateCardDisplay(cardData);
        
        // パネルを表示
        detailPanel.SetActive(true);
    }
    
    /// <summary>
    /// カード詳細を非表示
    /// </summary>
    public void Hide()
    {
        detailPanel.SetActive(false);
    }
    
    /// <summary>
    /// カード表示を更新
    /// </summary>
    /// <param name="cardData">表示するカードデータ</param>
    private void UpdateCardDisplay(CardData cardData)
    {
        // 新しいカードViewを作成
        var cardModel = new CardModel(cardData);
        cardView.Initialize(cardModel, false);
        
        // 詳細情報を更新
        attributeText.text = $"属性: {cardData.Attribute.ToJapaneseName()}";
        scoreMultiplierText.text = $"スコア倍率: {cardData.ScoreMultiplier:F1}x";
        collapseThresholdText.text = $"崩壊閾値: {cardData.CollapseThreshold}";
        
        // 進化情報
        UpdateEvolutionInfo(cardData);
    }
    
    /// <summary>
    /// 進化情報を更新
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    private void UpdateEvolutionInfo(CardData cardData)
    {
        var evolutionInfo = "";
        
        if (cardData.CanEvolve)
        {
            evolutionInfo += $"進化先: {cardData.EvolutionTarget.CardName}\n";
        }
        
        if (cardData.CanDegrade)
        {
            evolutionInfo += $"劣化先: {cardData.DegradationTarget.CardName}\n";
        }
        
        if (string.IsNullOrEmpty(evolutionInfo))
        {
            evolutionInfo = "進化・劣化なし";
        }
        
        evolutionInfoText.text = evolutionInfo.TrimEnd('\n');
    }
    
    /// <summary>
    /// プレイボタンクリック時の処理
    /// </summary>
    private void OnPlayButtonClicked()
    {
        _playButtonClicked.OnNext(Unit.Default);
    }
    
    private void Awake()
    {
        // ボタンイベントの設定
        closeButton.OnClickAsObservable().Subscribe(_ => Hide()).AddTo(_disposables);
        playButton.OnClickAsObservable().Subscribe(_ => OnPlayButtonClicked()).AddTo(_disposables);
        
        // 初期状態では非表示
        Hide();
    }
    
    private void OnDestroy()
    {
        _playButtonClicked?.Dispose();
        _disposables?.Dispose();
    }
}