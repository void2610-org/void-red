# ドラッグ＆ドロップUI活用計画（案B採用）

## 方針

**案B: DraggableCardView + DeckSlotView を DeckSelectionView に組み込む**

既存の DeckSelectionView の構造を維持しつつ、クリック選択 → D&D選択に差し替える。
ValueRankingView のD&Dロジック（扇形配置、スナップアニメ、スロット管理）を DeckSelectionView に移植する。

---

## 復元ファイルと処理方針

| ファイル | アクション |
|---------|-----------|
| `UI/Auction/DraggableCardView.cs` | **改修** — BattleCardModel対応、数字テキスト追加 |
| `UI/Auction/DeckSlotView.cs`（旧DeckSlotView） | **改名済み** — DeckSlotViewとして流用 |
| `UI/Auction/DragLineView.cs` | **変更なし** — そのまま流用 |
| `UI/Auction/ValueRankingView.cs` | **削除** — D&Dロジックは DeckSelectionView に移植 |
| `Game/Models/ValueRankingModel.cs` | **削除** — 順位管理は不要 |
| Prefab: `ValueRankingView.prefab` | **削除** |
| Prefab: `RankTextPrefab.prefab` | **削除** |
| Prefab: `DraggableCardView.prefab` | **Prefab再構築**（CardView → 数字テキスト付きに） |
| Prefab: `DeckSlotView.prefab`（旧DeckSlotView） | **改名済み** |
| `Sprites/Auction/ValueRanking/` | 保持（スロット背景として活用可能） |

### 既存ファイルの変更

| ファイル | アクション |
|---------|-----------|
| `UI/Battle/DeckSelectionView.cs` | **大幅改修** — D&Dロジック統合 |
| `UI/Main/BattleUIPresenter.cs` | **変更なし** — DeckSelectionViewの公開I/Fは維持 |
| `Game/Logic/BattlePresenter.cs` | **変更なし** — UIPresenter経由の呼び出しは同じ |

---

## 実装ステップ

### Step 1: 不要ファイル削除

- `Assets/Scripts/Game/Models/ValueRankingModel.cs` + `.meta` 削除
- `Assets/Scripts/UI/Auction/ValueRankingView.cs` + `.meta` 削除
- `Assets/Prefabs/NewBattleSceneView/ValueRankingView.prefab` + `.meta` 削除
- `Assets/Prefabs/NewBattleSceneView/RankTextPrefab.prefab` + `.meta` 削除

### Step 2: DraggableCardView を BattleCardModel 対応に改修

**変更内容:**
```
現在:
  [SerializeField] private CardView cardView;
  public CardModel CardModel { get; private set; }
  Initialize(CardModel cardModel, int handIndex)
    → cardView.Initialize(cardModel.Data)

改修後:
  [SerializeField] private CardView cardView;
  [SerializeField] private TextMeshProUGUI numberText;  // 追加: 数字オーバーレイ
  public BattleCardModel BattleCard { get; private set; }
  Initialize(BattleCardModel battleCard, int handIndex)
    → cardView.Initialize(battleCard.Card.Data)  // CardViewはCardData表示に使う
    → numberText.text = battleCard.Number.ToString()
```

**ポイント:**
- CardView（カード画像・名前・フレーム表示）は引き続き活用
- BattleCardModel.Card が null のダミーカードの場合は cardView を非表示にし、numberText のみ表示
- プロパティ名を `CardModel` → `BattleCard` に変更

### Step 3: DeckSelectionView にD&Dロジックを移植

ValueRankingView から以下のロジックを移植:

```
移植するもの:
  - 扇形配置: CalculateFanPosition(), SetCardToFanPosition() + SerializeFieldパラメータ
  - D&Dイベント: OnCardDragStarted, OnCardDragEnded, OnCardDragging, OnCardDroppedToSlot
  - クリック取り外し: OnCardClicked
  - DragLineView 連携: Initialize, Show, UpdateEndPosition, Hide
  - StaggeredSlideInGroup アニメーション: slotStagger, cardStagger

移植しないもの:
  - GetRankedCards()（順位は不要、スロットに入っているカードを返すだけ）
```

**DeckSelectionView 改修後の構造:**
```csharp
public class DeckSelectionView : BasePhaseView
{
    // --- 既存フィールド（維持） ---
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI victoryConditionText;
    [SerializeField] private Button confirmButton;

    // --- 変更: BattleCardSlotView → DraggableCardView ---
    [SerializeField] private DraggableCardView draggableCardPrefab;

    // --- 変更: wonCardsContainer → handContainer ---
    [SerializeField] private Transform handContainer;

    // --- 追加: D&D関連 ---
    [SerializeField] private List<DeckSlotView> deckSlots;  // 3つのドロップスロット
    [SerializeField] private DragLineView dragLineView;
    [SerializeField] private float fanSpreadWidth = 400f;
    [SerializeField] private float fanHeightCurve = 30f;
    [SerializeField] private float fanMaxAngle = 10f;
    [SerializeField] private StaggeredSlideInGroup slotStagger;
    [SerializeField] private StaggeredSlideInGroup cardStagger;

    // --- 公開I/F（維持） ---
    public IReadOnlyList<BattleCardModel> SelectedCards => 取得ロジック
    public async UniTask WaitForSelectionAsync() => await _onConfirm.FirstAsync();

    // --- SelectedCards の取得 ---
    // deckSlots のうち IsOccupied なスロットから BattleCard を収集
}
```

**公開I/Fの互換性:**
- `Initialize(IReadOnlyList<BattleCardModel>, VictoryCondition)` — シグネチャ維持
- `SelectedCards` — スロットから収集するよう内部実装を変更
- `WaitForSelectionAsync()` — 変更なし
- `Show()` / `Hide()` — StaggeredSlideInGroupのPlay/Cancelを追加

→ **BattleUIPresenter、BattlePresenter は変更不要**

### Step 4: コンパイル・フォーマット

1. `mcp__uLoopMCP__compile` (ForceRecompile=true)
2. `mcp__uLoopMCP__get-logs` (LogType=Error) でエラー0確認
3. `dotnet format` 3コマンド実行
4. VUA警告修正

### Step 5: Prefab調整（後続作業）

- DeckSelectionView Prefab に DeckSlotView × 3、DragLineView を配置
- DraggableCardView Prefab に numberText を追加
- StaggeredSlideInGroup コンポーネント設定

---

## データフロー

```
BattlePresenter.HandleDeckSelection()
  ↓ playerWonBattleCards: List<BattleCardModel>
BattleUIPresenter.InitializeDeckSelection(wonCards, condition)
  ↓
DeckSelectionView.Initialize(wonCards, condition)
  ↓ 各カードから DraggableCardView を生成
  ↓ DraggableCardView.Initialize(BattleCardModel, handIndex)
  ↓   └→ CardView.Initialize(battleCard.Card.Data) + numberText 表示
  ↓ プレイヤーがD&Dで3つのDeckSlotViewに配置
  ↓ 確定ボタン押下
BattleUIPresenter.GetSelectedDeck()
  ↓ DeckSelectionView.SelectedCards
  ↓   └→ deckSlots.Where(s => s.IsOccupied).Select(s => s.PlacedCard.BattleCard)
BattlePresenter → BattleDeckModel.SetDeck(selectedCards)
```

---

## 注意事項

- **ダミーカード対応**: BattleCardModel.Card == null の場合、CardView は非表示にして数字だけ表示
- **DeckSlotView.PlacedCard**: DraggableCardView を返す。そこから `.BattleCard` で BattleCardModel を取得
- **DraggableCardView のプロパティ名変更**: `CardModel` → `BattleCard` に変更するため、DeckSlotView 等での参照も確認
  - DeckSlotView 内では `PlacedCard.CardModel` は使っていない（DraggableCardView を保持するのみ）→ 影響なし
- **namespace**: DraggableCardView, DeckSlotView, DragLineView は `UI/Auction` ディレクトリにあるが namespace は無し → 移動不要
