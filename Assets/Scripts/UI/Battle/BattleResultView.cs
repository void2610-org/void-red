using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バトル結果画面のView
/// 勝利時: 記憶に入れるカードの選択、敗北時: テキスト演出
/// </summary>
public class BattleResultView : BasePhaseView
{
    [Header("結果表示")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("勝利時: カード選択")]
    [SerializeField] private Transform cardSelectionContainer;
    [SerializeField] private BattleCardSlotView cardSlotPrefab;

    [Header("敗北時: 演出")]
    [SerializeField] private TextMeshProUGUI plantedMemoryText;

    [Header("UI要素")]
    [SerializeField] private Button confirmButton;

    /// <summary>選択されたカード（勝利時のみ）</summary>
    public BattleCardModel SelectedCard => _selectedCard;

    private readonly List<BattleCardSlotView> _cardSlots = new();
    private readonly Subject<Unit> _onConfirm = new();
    private BattleCardModel _selectedCard;

    /// <summary>確定ボタンが押されるまで待機</summary>
    public async UniTask WaitForConfirmAsync() => await _onConfirm.FirstAsync();

    /// <summary>勝利時の表示: カード選択UI</summary>
    public void ShowWin(IReadOnlyList<BattleCardModel> playerCards)
    {
        Show();
        ClearSlots();

        resultText.text = "WIN!";
        descriptionText.text = "記憶に入れるカードを選んでください";
        plantedMemoryText.gameObject.SetActive(false);
        cardSelectionContainer.gameObject.SetActive(true);

        _selectedCard = null;

        foreach (var card in playerCards)
        {
            var slot = Instantiate(cardSlotPrefab, cardSelectionContainer);
            slot.Initialize(card, showNumber: true);
            var capturedCard = card;
            slot.OnClicked.Subscribe(_ => OnCardClicked(capturedCard, slot)).AddTo(this);
            _cardSlots.Add(slot);
        }

        confirmButton.interactable = false;
    }

    /// <summary>敗北時の表示: テキスト演出のみ</summary>
    public void ShowLose()
    {
        Show();
        ClearSlots();

        resultText.text = "LOSE";
        descriptionText.text = "望まぬ記憶が植えつけられた...";
        cardSelectionContainer.gameObject.SetActive(false);
        plantedMemoryText.gameObject.SetActive(true);
        plantedMemoryText.text = "...";

        confirmButton.interactable = true;
    }

    private void OnCardClicked(BattleCardModel card, BattleCardSlotView slot)
    {
        // 全スロットの選択解除
        foreach (var s in _cardSlots)
            s.SetSelected(false);

        _selectedCard = card;
        slot.SetSelected(true);
        confirmButton.interactable = true;
    }

    private void ClearSlots()
    {
        foreach (var slot in _cardSlots)
            Destroy(slot.gameObject);
        _cardSlots.Clear();
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
