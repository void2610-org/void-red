using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カードバトル画面のView
/// 3本勝負のカードバトルUIを管理する
/// </summary>
public class CardBattleView : BasePhaseView
{
    [Header("ラウンド情報")]
    [SerializeField] private TextMeshProUGUI victoryConditionText;
    [SerializeField] private Image[] roundMarkers;

    [Header("プレイヤー側")]
    [SerializeField] private Transform playerDeckContainer;
    [SerializeField] private Transform playerFieldSlot;

    [Header("敵側")]
    [SerializeField] private Transform enemyFieldSlot;

    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;
    [SerializeField] private Button skillButton;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Button nextButton;
    [SerializeField] private BattleCardSlotView cardSlotPrefab;

    [Header("色設定")]
    [SerializeField] private Color winColor = Color.yellow;
    [SerializeField] private Color loseColor = Color.gray;
    [SerializeField] private Color pendingColor = Color.white;

    /// <summary>プレイヤーがカードを選択した</summary>
    public Observable<BattleCardModel> OnCardSelected => _onCardSelected;

    /// <summary>スキルボタンが押された</summary>
    public Observable<Unit> OnSkillActivated => _onSkillActivated;

    /// <summary>次へボタンが押された</summary>
    public Observable<Unit> OnNextClicked => _onNextClicked;

    private readonly List<BattleCardSlotView> _playerHandSlots = new();
    private readonly Subject<BattleCardModel> _onCardSelected = new();
    private readonly Subject<Unit> _onSkillActivated = new();
    private readonly Subject<Unit> _onNextClicked = new();

    private BattleCardSlotView _playerFieldCardSlot;
    private BattleCardSlotView _enemyFieldCardSlot;

    /// <summary>先攻/後攻を表示</summary>
    public void ShowTurnIndicator(bool isPlayerFirst) =>
        turnIndicatorText.text = isPlayerFirst ? "先攻" : "後攻";

    /// <summary>指示テキストを設定</summary>
    public void SetInstruction(string text) => instructionText.text = text;

    /// <summary>スキルボタンの表示状態を設定</summary>
    public void SetSkillButtonVisible(bool visible) => skillButton.gameObject.SetActive(visible);

    /// <summary>
    /// バトルを初期化する
    /// </summary>
    public void Initialize(VictoryCondition condition, EmotionType playerSkill)
    {
        Show();

        victoryConditionText.text = condition == VictoryCondition.LowerWins
            ? "勝利条件: 数字が小さい方が勝利"
            : "勝利条件: 数字が大きい方が勝利";

        skillNameText.text = $"{playerSkill.ToJapaneseName()}\n{SkillEffectApplier.GetDescription(playerSkill)}";

        // ラウンドマーカー初期化
        foreach (var marker in roundMarkers)
            marker.color = pendingColor;

        nextButton.gameObject.SetActive(false);
    }

    /// <summary>プレイヤーの手札を表示する</summary>
    public void ShowPlayerHand(IReadOnlyList<BattleCardModel> availableCards)
    {
        ClearPlayerHand();

        foreach (var card in availableCards)
        {
            var slot = Instantiate(cardSlotPrefab, playerDeckContainer);
            slot.Initialize(card, showNumber: true);
            var capturedCard = card;
            slot.OnClicked.Subscribe(_ => _onCardSelected.OnNext(capturedCard)).AddTo(this);
            _playerHandSlots.Add(slot);
        }
    }

    /// <summary>プレイヤーのカードを場に伏せる</summary>
    public void PlacePlayerCard(BattleCardModel card)
    {
        ClearPlayerHand();
        _playerFieldCardSlot = Instantiate(cardSlotPrefab, playerFieldSlot);
        _playerFieldCardSlot.Initialize(card, showNumber: false);
        _playerFieldCardSlot.ShowBack();
    }

    /// <summary>敵のカードを場に伏せる</summary>
    public void PlaceEnemyCard()
    {
        if (_enemyFieldCardSlot)
            Destroy(_enemyFieldCardSlot.gameObject);

        _enemyFieldCardSlot = Instantiate(cardSlotPrefab, enemyFieldSlot);
        _enemyFieldCardSlot.ShowBack();
        _enemyFieldCardSlot.SetInteractable(false);
    }

    /// <summary>両者のカードをオープンする</summary>
    public void RevealCards(BattleCardModel playerCard, BattleCardModel enemyCard)
    {
        if (_playerFieldCardSlot)
        {
            _playerFieldCardSlot.Initialize(playerCard, showNumber: true);
            _playerFieldCardSlot.ShowFront();
        }

        if (_enemyFieldCardSlot)
        {
            _enemyFieldCardSlot.Initialize(enemyCard, showNumber: true);
            _enemyFieldCardSlot.ShowFront();
        }
    }

    /// <summary>ラウンド結果を反映する</summary>
    public void SetRoundResult(int round, RoundResult result)
    {
        if (round < roundMarkers.Length)
        {
            roundMarkers[round].color = result == RoundResult.PlayerWin ? winColor : loseColor;
        }
    }

    /// <summary>次へボタンを表示して待機</summary>
    public async UniTask WaitForNextAsync()
    {
        nextButton.gameObject.SetActive(true);
        await _onNextClicked.FirstAsync();
        nextButton.gameObject.SetActive(false);
    }

    /// <summary>場のカードをクリアする</summary>
    public void ClearField()
    {
        if (_playerFieldCardSlot)
        {
            Destroy(_playerFieldCardSlot.gameObject);
            _playerFieldCardSlot = null;
        }

        if (_enemyFieldCardSlot)
        {
            Destroy(_enemyFieldCardSlot.gameObject);
            _enemyFieldCardSlot = null;
        }
    }

    private void ClearPlayerHand()
    {
        foreach (var slot in _playerHandSlots)
            Destroy(slot.gameObject);
        _playerHandSlots.Clear();
    }

    protected override void Awake()
    {
        base.Awake();

        skillButton.OnClickAsObservable()
            .Subscribe(_ => _onSkillActivated.OnNext(Unit.Default))
            .AddTo(this);

        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onCardSelected.Dispose();
        _onSkillActivated.Dispose();
        _onNextClicked.Dispose();
    }
}
