using System.Collections.Generic;
using System.Linq;
using Auction;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// デッキ選択画面のView
/// 獲得カードをドラッグ&ドロップして3枚のデッキを構成する
/// </summary>
public class DeckSelectionView : BasePhaseView
{
    [Header("手札エリア")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private DraggableCardView draggableCardPrefab;

    [Header("デッキスロット")]
    [SerializeField] private List<DeckSlotView> deckSlots;

    [Header("UI")]
    [SerializeField] private Button confirmButton;

    [Header("ドラッグ演出")]
    [SerializeField] private DragLineView dragLineView;

    [Header("アニメーション")]
    [SerializeField] private StaggeredSlideInGroup slotStagger;
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    /// <summary>選択されたデッキ</summary>
    public IReadOnlyList<CardModel> SelectedCards =>
        deckSlots.Where(s => s.IsOccupied).Select(s => s.PlacedCard.CardModel).ToList();

    private readonly List<DraggableCardView> _handCards = new();
    private readonly Subject<Unit> _onConfirm = new();
    private CompositeDisposable _disposables = new();
    private bool _wasDroppedToSlot;
    private bool _isConfirmInteractableByPresenter = true;
    private RectTransform _handContainerRect;

    /// <summary>確定ボタンが押されるまで待機</summary>
    public async UniTask WaitForSelectionAsync() => await _onConfirm.FirstAsync();

    /// <summary>
    /// デッキ選択を開始する
    /// </summary>
    public void Initialize(IReadOnlyList<CardModel> wonCards) => Initialize(wonCards, null);

    public void SetConfirmInteractable(bool interactable)
    {
        _isConfirmInteractableByPresenter = interactable;
        UpdateConfirmButton();
    }

    /// <summary>表示中カードの数字を再描画する</summary>
    public void RefreshCardNumbers()
    {
        // デッキ選択中のスキルで変わった数字を一覧へ反映する
        foreach (var card in _handCards)
        {
            if (card)
                card.UpdateNumber(card.CardModel.BattleNumber);
        }
    }

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

    public void Initialize(IReadOnlyList<CardModel> wonCards, IReadOnlyList<int> allowedCardIndices)
    {
        Show();
        Clear();

        // 獲得カードをドラッグ可能カードとして生成
        for (var i = 0; i < wonCards.Count; i++)
        {
            var card = wonCards[i];
            var draggableCard = Instantiate(draggableCardPrefab, handContainer);
            draggableCard.Initialize(card, i);

            draggableCard.OnDragStarted.Subscribe(OnCardDragStarted).AddTo(_disposables);
            draggableCard.OnDragEnded.Subscribe(OnCardDragEnded).AddTo(_disposables);
            draggableCard.OnClicked.Subscribe(OnCardClicked).AddTo(_disposables);
            draggableCard.OnDragging.Subscribe(OnCardDragging).AddTo(_disposables);

            if (allowedCardIndices != null)
                draggableCard.SetInteractable(allowedCardIndices.Contains(i));

            _handCards.Add(draggableCard);
        }

        // レイアウト計算＋スライドインアニメーション
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

    private void OnCardDragging(Vector3 cardWorldPos)
    {
        dragLineView.UpdateEndPosition(cardWorldPos);
    }

    private void UpdateConfirmButton()
    {
        confirmButton.interactable = IsAllSlotsFilled() && _isConfirmInteractableByPresenter;
    }

    private bool IsAllSlotsFilled()
    {
        return deckSlots.All(s => s.IsOccupied);
    }

    private void Clear()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        _isConfirmInteractableByPresenter = true;

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

        ReturnCardToHand(card);
    }

    private void OnCardDroppedToSlot(DeckSlotView slot, DraggableCardView droppedCard)
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
                ReturnCardToHand(existingCard);
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

        ReturnCardToHand(card);

        UpdateConfirmButton();
    }

    /// <summary>カードを手札コンテナに戻してレイアウトを再計算する</summary>
    private void ReturnCardToHand(DraggableCardView card)
    {
        var rt = card.transform as RectTransform;
        card.transform.SetParent(handContainer);
        card.transform.localScale = card.OriginalScale;
        card.transform.localEulerAngles = Vector3.zero;

        // アンカーをリセット（ドラッグ中にrootCanvasに移動しているため）
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = card.OriginalSizeDelta;

        // HandIndexの順番に並べ直す
        card.transform.SetSiblingIndex(GetSiblingIndexForHandIndex(card.HandIndex));

        // StaggeredSlideInGroupでレイアウト再計算
        cardStagger.ApplyLayout();
    }

    /// <summary>HandIndexに基づいて正しいSiblingIndexを計算する</summary>
    private int GetSiblingIndexForHandIndex(int handIndex)
    {
        for (var i = 0; i < handContainer.childCount; i++)
        {
            var child = handContainer.GetChild(i).GetComponent<DraggableCardView>();
            if (child && child.HandIndex > handIndex) return i;
        }

        return handContainer.childCount;
    }

    private void OnConfirmClicked()
    {
        if (!IsAllSlotsFilled()) return;

        // 決定SE
        SeManager.Instance.PlaySe("SE_DECIDE", pitch: 1f);

        _onConfirm.OnNext(Unit.Default);
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
