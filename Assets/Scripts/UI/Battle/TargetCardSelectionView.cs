using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// 対象選択が必要なスキル用のカード選択モーダル
/// </summary>
public class TargetCardSelectionView : BasePhaseView
{
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private SelectableBattleCardView cardPrefab;
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    private readonly List<SelectableBattleCardView> _cards = new();
    private readonly Subject<CardModel> _onCardSelected = new();
    private CompositeDisposable _disposables = new();

    /// <summary>
    /// 指示文と候補カードを表示し、1枚選ばれるまで待機する
    /// </summary>
    /// <param name="instruction">画面上部へ表示する案内文</param>
    /// <param name="selectableCards">選択候補として表示するカード一覧</param>
    /// <returns>プレイヤーが選んだカード</returns>
    public async UniTask<CardModel> WaitForSelectionAsync(string instruction, IReadOnlyList<CardModel> selectableCards)
    {
        if (selectableCards.Count == 0) return null;

        Show();
        instructionText.text = instruction;
        BuildSelectableCards(selectableCards);

        var selectedCard = await _onCardSelected.FirstAsync();
        ClearCards();
        Hide();
        return selectedCard;
    }

    public override void Hide()
    {
        cardStagger.Cancel();
        base.Hide();
    }

    /// <summary>選択候補カードを横並びで生成し、スタッガー表示する</summary>
    /// <param name="selectableCards">選択候補として表示するカード一覧</param>
    private void BuildSelectableCards(IReadOnlyList<CardModel> selectableCards)
    {
        ClearCards();

        for (var i = 0; i < selectableCards.Count; i++)
        {
            var card = selectableCards[i];
            var selectableCard = Instantiate(cardPrefab, cardContainer);
            selectableCard.Initialize(card);
            selectableCard.OnClicked.Subscribe(OnCardClicked).AddTo(_disposables);
            _cards.Add(selectableCard);
        }

        cardStagger.Play();
    }

    /// <summary>クリックされたカードを選択結果として確定する</summary>
    /// <param name="selectedCard">クリックされたカードモデル</param>
    private void OnCardClicked(CardModel selectedCard)
    {
        SeManager.Instance.PlaySe("SE_DECIDE", pitch: 1f);
        _onCardSelected.OnNext(selectedCard);
    }

    /// <summary>前回表示した候補カードと購読を破棄する</summary>
    private void ClearCards()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        foreach (var card in _cards)
        {
            if (card)
                Destroy(card.gameObject);
        }

        _cards.Clear();
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onCardSelected.Dispose();
    }
}
