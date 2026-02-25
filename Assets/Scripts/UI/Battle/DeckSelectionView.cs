using System.Collections.Generic;
using System.Linq;
using Auction;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// デッキ選択画面のView
/// 獲得カードをドラッグ&ドロップして3枚のデッキを構成する
/// </summary>
public class DeckSelectionView : BasePhaseView
{
    [Header("テキスト")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI victoryConditionText;

    [Header("手札エリア")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private DraggableCardView draggableCardPrefab;

    [Header("デッキスロット")]
    [SerializeField] private List<RankingSlotView> deckSlots;

    [Header("扇形配置")]
    [SerializeField] private float fanSpreadWidth = 400f;
    [SerializeField] private float fanHeightCurve = 30f;
    [SerializeField] private float fanMaxAngle = 10f;

    [Header("UI")]
    [SerializeField] private Button confirmButton;

    [Header("ドラッグ演出")]
    [SerializeField] private DragLineView dragLineView;

    [Header("アニメーション")]
    [SerializeField] private StaggeredSlideInGroup slotStagger;
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    /// <summary>選択されたデッキ</summary>
    public IReadOnlyList<BattleCardModel> SelectedCards =>
        deckSlots.Where(s => s.IsOccupied).Select(s => s.PlacedCard.BattleCard).ToList();

    private readonly List<DraggableCardView> _handCards = new();
    private readonly Subject<Unit> _onConfirm = new();
    private CompositeDisposable _disposables = new();
    private bool _wasDroppedToSlot;
    private RectTransform _handContainerRect;

    /// <summary>確定ボタンが押されるまで待機</summary>
    public async UniTask WaitForSelectionAsync() => await _onConfirm.FirstAsync();

    public override void Show()
    {
        base.Show();
        slotStagger.Play();
    }

    public override void Hide()
    {
        slotStagger.Cancel();
        cardStagger.Cancel();
        base.Hide();
    }

    /// <summary>
    /// デッキ選択を開始する
    /// </summary>
    public void Initialize(
        IReadOnlyList<BattleCardModel> wonCards,
        VictoryCondition condition)
    {
        Show();
        Clear();

        // 勝利条件テキスト
        victoryConditionText.text = condition == VictoryCondition.LowerWins
            ? "勝利条件: 数字が小さい方が勝利"
            : "勝利条件: 数字が大きい方が勝利";

        // 獲得カードをドラッグ可能カードとして生成
        for (var i = 0; i < wonCards.Count; i++)
        {
            var card = wonCards[i];
            var draggableCard = Instantiate(draggableCardPrefab, handContainer);
            draggableCard.Initialize(card, i);

            // 初期位置・回転を設定
            SetCardToFanPosition(draggableCard, i, wonCards.Count);

            draggableCard.OnDragStarted.Subscribe(OnCardDragStarted).AddTo(_disposables);
            draggableCard.OnDragEnded.Subscribe(OnCardDragEnded).AddTo(_disposables);
            draggableCard.OnClicked.Subscribe(OnCardClicked).AddTo(_disposables);
            draggableCard.OnDragging.Subscribe(OnCardDragging).AddTo(_disposables);

            _handCards.Add(draggableCard);
        }

        // カードのスライドインアニメーション
        cardStagger.Play();

        // スロットのドロップイベントを購読
        foreach (var slot in deckSlots)
        {
            slot.OnCardDropped
                .Subscribe(tuple => OnCardDroppedToSlot(tuple.slot, tuple.card))
                .AddTo(_disposables);
        }

        confirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmClicked()).AddTo(_disposables);

        UpdateConfirmButton();
    }

    private void Clear()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        foreach (var slot in deckSlots) slot.RemoveCard();
        foreach (var card in _handCards) Destroy(card.gameObject);

        _handCards.Clear();
    }

    private void OnCardDragStarted(DraggableCardView card)
    {
        _wasDroppedToSlot = false;

        // 手札エリアの中心からドラッグ線を表示
        var handCenterWorld = _handContainerRect.position;
        dragLineView.Show(handCenterWorld);
    }

    private void OnCardDragEnded(DraggableCardView card)
    {
        dragLineView.Hide();

        // スロットにドロップされた場合はOnCardDroppedToSlotで処理済み
        if (_wasDroppedToSlot) return;

        // スロット外にドロップされた場合、元のスロットから外して手札に戻す
        if (card.IsPlaced)
        {
            card.CurrentSlot.RemoveCard();
            UpdateConfirmButton();
        }

        var (position, rotation) = CalculateFanPosition(card.HandIndex, _handCards.Count);
        card.PlayReturnToHandAsync(handContainer, position, rotation).Forget();
    }

    private void OnCardDragging(Vector3 cardWorldPos)
    {
        dragLineView.UpdateEndPosition(cardWorldPos);
    }

    private void OnCardDroppedToSlot(RankingSlotView slot, DraggableCardView droppedCard)
    {
        _wasDroppedToSlot = true;

        var previousSlot = droppedCard.CurrentSlot;
        previousSlot?.RemoveCard();

        if (slot.IsOccupied)
        {
            var existingCard = slot.RemoveCard();

            if (previousSlot)
            {
                previousSlot.PlaceCard(existingCard);
                existingCard.PlaySnapToSlotAsync(previousSlot.CardAnchor, Vector2.zero).Forget();
            }
            else
            {
                var (pos, rot) = CalculateFanPosition(existingCard.HandIndex, _handCards.Count);
                existingCard.PlayReturnToHandAsync(handContainer, pos, rot).Forget();
            }
        }

        slot.PlaceCard(droppedCard);
        droppedCard.PlaySnapToSlotAsync(slot.CardAnchor, Vector2.zero).Forget();

        // カードを枠に配置した時のSE
        SeManager.Instance.PlaySe("SE_FRAME_LIGHT", pitch: 1f);

        UpdateConfirmButton();
    }

    private void OnCardClicked(DraggableCardView card)
    {
        if (!card.IsPlaced) return;

        var slot = card.CurrentSlot;
        slot.RemoveCard();

        var (position, rotation) = CalculateFanPosition(card.HandIndex, _handCards.Count);
        card.PlayReturnToHandAsync(handContainer, position, rotation).Forget();

        UpdateConfirmButton();
    }

    private void OnConfirmClicked()
    {
        if (!IsAllSlotsFilled()) return;

        // 決定SE
        SeManager.Instance.PlaySe("SE_DECIDE", pitch: 1f);

        _onConfirm.OnNext(Unit.Default);
    }

    private void UpdateConfirmButton()
    {
        confirmButton.interactable = IsAllSlotsFilled();
    }

    private bool IsAllSlotsFilled() => deckSlots.All(s => s.IsOccupied);

    private (Vector2 position, float rotation) CalculateFanPosition(int index, int totalCount)
    {
        // -1 〜 1 の範囲に正規化（中央が0）
        var t = totalCount > 1
            ? (index - (totalCount - 1) / 2f) / ((totalCount - 1) / 2f)
            : 0f;

        var x = t * fanSpreadWidth / 2f;
        // 放物線で中央が高い
        var y = (1 - t * t) * fanHeightCurve;
        var rotation = -t * fanMaxAngle;

        return (new Vector2(x, y), rotation);
    }

    private void SetCardToFanPosition(DraggableCardView card, int index, int totalCount)
    {
        var (position, rotation) = CalculateFanPosition(index, totalCount);
        var cardRect = card.transform as RectTransform;
        cardRect.anchoredPosition = position;
        card.transform.localEulerAngles = new Vector3(0, 0, rotation);
    }

    protected override void Awake()
    {
        base.Awake();
        _handContainerRect = handContainer as RectTransform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        dragLineView.Initialize(canvas);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onConfirm.Dispose();
    }
}
