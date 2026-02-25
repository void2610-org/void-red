using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// デッキ選択画面のView
/// 獲得カードから3枚を選んでデッキを構成する
/// </summary>
public class DeckSelectionView : BasePhaseView
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI victoryConditionText;
    [SerializeField] private Transform wonCardsContainer;
    [SerializeField] private Transform selectedDeckContainer;
    [SerializeField] private Button confirmButton;
    [SerializeField] private BattleCardSlotView cardSlotPrefab;

    /// <summary>選択されたデッキ</summary>
    public IReadOnlyList<BattleCardModel> SelectedCards => _selectedCards;

    private readonly List<BattleCardSlotView> _wonCardSlots = new();
    private readonly List<BattleCardModel> _selectedCards = new();
    private readonly List<BattleCardSlotView> _deckSlots = new();
    private readonly Subject<Unit> _onConfirm = new();

    /// <summary>確定ボタンが押されるまで待機</summary>
    public async UniTask WaitForSelectionAsync() => await _onConfirm.FirstAsync();

    /// <summary>
    /// デッキ選択を開始する
    /// </summary>
    public void Initialize(
        IReadOnlyList<BattleCardModel> wonCards,
        VictoryCondition condition)
    {
        Show();
        _selectedCards.Clear();
        ClearSlots();

        // 勝利条件テキスト
        victoryConditionText.text = condition == VictoryCondition.LowerWins
            ? "勝利条件: 数字が小さい方が勝利"
            : "勝利条件: 数字が大きい方が勝利";

        // 獲得カードをスロットとして生成
        foreach (var card in wonCards)
        {
            var slot = Instantiate(cardSlotPrefab, wonCardsContainer);
            slot.Initialize(card, showNumber: true);
            slot.OnClicked.Subscribe(_ => OnCardSlotClicked(card, slot)).AddTo(this);
            _wonCardSlots.Add(slot);
        }

        UpdateConfirmButton();
    }

    private void OnCardSlotClicked(BattleCardModel card, BattleCardSlotView slot)
    {
        if (_selectedCards.Contains(card))
        {
            // 選択解除
            _selectedCards.Remove(card);
            slot.SetSelected(false);
        }
        else if (_selectedCards.Count < GameConstants.DECK_SIZE)
        {
            // 選択
            _selectedCards.Add(card);
            slot.SetSelected(true);
        }

        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        confirmButton.interactable = _selectedCards.Count == GameConstants.DECK_SIZE;
    }

    private void ClearSlots()
    {
        foreach (var slot in _wonCardSlots)
            Destroy(slot.gameObject);
        _wonCardSlots.Clear();

        foreach (var slot in _deckSlots)
            Destroy(slot.gameObject);
        _deckSlots.Clear();
    }

    protected override void Awake()
    {
        base.Awake();
        confirmButton.OnClickAsObservable()
            .Subscribe(_ => _onConfirm.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onConfirm.Dispose();
    }
}
