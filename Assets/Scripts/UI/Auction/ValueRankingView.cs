using System.Collections.Generic;
using UnityEngine;
using TMPro;
using R3;

// 価値順位設定UI
// プレイヤーがカードをクリックした順に順位1-4を割り当てる
public class ValueRankingView : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;

    [Header("順位表示")]
    [SerializeField] private TextMeshProUGUI rankTextPrefab;

    private readonly List<CardView> _cardViews = new();
    private readonly List<CardModel> _rankedCards = new();
    private readonly Dictionary<CardView, CardModel> _cardViewToModel = new();
    private readonly Dictionary<CardView, TextMeshProUGUI> _rankTexts = new();
    private readonly Subject<Unit> _onRankingComplete = new();
    private CompositeDisposable _disposables = new();

    public Observable<Unit> OnRankingComplete => _onRankingComplete;

    // カードを表示して順位選択を開始
    public void StartRanking(IReadOnlyList<CardModel> cards)
    {
        Clear();

        foreach (var card in cards)
        {
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(card.Data);
            cardView.SetInteractable(true);

            // CardViewとCardModelの対応を保存
            _cardViewToModel[cardView] = card;

            // クリックイベント購読
            cardView.OnClicked
                .Subscribe(OnCardClicked)
                .AddTo(_disposables);

            _cardViews.Add(cardView);
        }
    }

    private void OnCardClicked(CardView cardView)
    {
        // CardViewからCardModelを取得
        if (!_cardViewToModel.TryGetValue(cardView, out var cardModel)) return;

        // 既に順位設定済みなら無視
        if (_rankedCards.Contains(cardModel)) return;

        // 順位を設定（クリック順）
        var rank = _rankedCards.Count + 1;
        _rankedCards.Add(cardModel);
        ShowRankText(cardView, rank);
        cardView.SetInteractable(false);

        // 全て設定完了したら通知
        if (_rankedCards.Count == _cardViews.Count)
        {
            _onRankingComplete.OnNext(Unit.Default);
        }
    }

    // カード上に順位テキストを表示
    private void ShowRankText(CardView cardView, int rank)
    {
        var rankText = Instantiate(rankTextPrefab, cardView.transform);
        rankText.rectTransform.anchoredPosition = Vector2.zero;
        rankText.text = rank.ToString();

        _rankTexts[cardView] = rankText;
    }

    // 設定結果を取得（順位順のカードリスト）
    public IReadOnlyList<CardModel> GetRankedCards() => _rankedCards;

    public void Clear()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        // 順位テキストを削除
        foreach (var rankText in _rankTexts.Values)
        {
            Destroy(rankText.gameObject);
        }
        _rankTexts.Clear();

        // カードViewを削除
        foreach (var cardView in _cardViews)
        {
            Destroy(cardView.gameObject);
        }
        _cardViews.Clear();
        _rankedCards.Clear();
        _cardViewToModel.Clear();
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onRankingComplete.Dispose();
    }
}
