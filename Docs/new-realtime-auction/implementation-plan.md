# 新リアルタイムオークションシステム 実装プラン

## 1. 企画書サマリー

### 1-1. オークションパート（カード提示・対話）
- **6枚の記憶カード**が場に並ぶ
- 【記憶カード】バトルパートに向けた記憶カード選びの指標。一部特殊効果を持つ記憶カードも
- 【対話】任意のカードを選択し、対話ボタンを押すと相手のブラフやヒントを聞ける。選択肢次第でこちら側から揺さぶりも可能

### 1-2. オークションパート（入札・勝負）
- 感情リソースをどのカードにいくつベットするかを選択
- 感情リソースは**8種類×各3枚 = 合計24枚**
- **1カードあたり1種類の感情リソースのみ**ベット可能
- カードを何枚落札するかは自由
- **感情リソースが全て無くなるとゲームオーバー**
- ベット終了後、勝負 → 相手より上回れば落札、下回れば**リソース返却**

### 1-3. オークションパート（競合）
- 両者のベットが同数の場合、**競合発生**
- リアルタイムで1枚ずつ感情リソースを上乗せ可能（別種でも可）
- **10秒間**上乗せが無ければ競合終了、落札者決定

---

## 2. 現行仕様との差分比較

| 項目 | 現行仕様 | 新仕様 | 変更の大きさ |
|------|---------|--------|------------|
| カード枚数 | 8枚（プレイヤー4+敵4） | 6枚（共有） | **大** |
| 価値順位設定 | あり（1-4のドラッグ&ドロップ） | なし | **大（削除）** |
| 感情リソース初期値 | 8種類×各10 = 合計80 | 8種類×各3 = 合計24 | **中** |
| 入札制約 | 1カードに複数感情ベット可 | 1カードにつき1種類のみ | **中** |
| フェーズ順序 | 入札 → 対話 → 判定 | 対話 → 入札 → 判定 | **中** |
| 対話フェーズ | 入札後、固定選択肢 | 入札前、カード選択→対話 | **大** |
| 引き分け処理 | 引き分け表示のみ | リアルタイム競合（10秒タイマー） | **大（新規）** |
| 落札失敗時 | リソース消費 | リソース返却 | **小** |
| ゲームオーバー条件 | なし | 感情リソース全消費 | **中（新規）** |
| 報酬計算 | 価値順位ベース | 要再設計（価値順位廃止のため） | **大** |

---

## 3. 変更対象ファイル一覧

### 3.1 削除・大幅改修が必要なファイル

| ファイル | 現在の役割 | 対応 |
|--------|----------|------|
| `Assets/Scripts/Game/Models/ValueRankingModel.cs` | 価値順位管理 | **削除** |
| `Assets/Scripts/UI/Auction/ValueRankingView.cs` | 価値順位UI | **削除** |
| `Assets/Scripts/UI/Auction/DraggableCardView.cs` | ドラッグ操作 | **削除** |
| `Assets/Scripts/UI/Auction/RankingSlotView.cs` | 順位スロット | **削除** |
| `Assets/Scripts/Game/Services/RewardCalculator.cs` | 報酬計算 | **大幅改修** |

### 3.2 改修が必要なファイル

| ファイル | 変更内容 |
|--------|---------|
| `Assets/Scripts/Game/Logic/BattlePresenter.cs` | フェーズ順序変更、競合処理追加、リソース返却ロジック |
| `Assets/Scripts/Game/Models/BidModel.cs` | 1カード1感情制約の追加 |
| `Assets/Scripts/Game/Services/AuctionJudge.cs` | 競合（Draw）時の新判定ロジック |
| `Assets/Scripts/Game/Models/PlayerModel.cs` | 初期リソース値変更（10→3） |
| `Assets/Scripts/Game/Core/GameConstants.cs` | 定数値変更 |
| `Assets/Scripts/Game/Core/Enums.cs` (GameState) | ValueRanking削除、競合フェーズ追加 |
| `Assets/Scripts/Game/Presenters/PlayerPresenter.cs` | ValueRanking参照削除 |
| `Assets/Scripts/Game/Presenters/Enemy.cs` | 入札AI改修、競合AI追加 |
| `Assets/Scripts/ScriptableObject/AuctionData.cs` | 6枚カード構成に変更 |
| `Assets/Scripts/UI/Auction/AuctionView.cs` | 6枚表示、1感情制約UI、対話連携 |
| `Assets/Scripts/UI/Auction/BidWindowView.cs` | 1感情制約対応 |
| `Assets/Scripts/UI/Main/BattleUIPresenter.cs` | フェーズ順序・UIフロー変更 |

### 3.3 新規作成ファイル

| ファイル | 役割 |
|--------|------|
| `Assets/Scripts/UI/Auction/AuctionCardView.cs` | オークション用カードラッパー（CardView + BidInfo + 対話ボタン） |
| `Assets/Scripts/UI/Auction/CompetitionView.cs` | リアルタイム競合UI（天秤＋タイマー） |
| `Assets/Scripts/Game/Logic/CompetitionHandler.cs` | 競合ロジック（タイマー・上乗せ管理） |

---

## 3.5 Prefab・ヒエラルキー変更一覧

### 3.5.1 削除するPrefab

| Prefabパス | 理由 |
|-----------|------|
| `Assets/Prefabs/NewBattleSceneView/ValueRankingView.prefab` | 価値順位システム廃止 |
| `Assets/Prefabs/NewBattleSceneView/ValueRanking/DraggableCardView.prefab` | 価値順位ドラッグ操作廃止 |
| `Assets/Prefabs/NewBattleSceneView/ValueRanking/RankingSlotView.prefab` | 順位スロット廃止 |
| `Assets/Prefabs/NewBattleSceneView/RankTextPrefab.prefab` | 価値順位テキスト廃止 |

対応するスクリプトも削除:
- `Assets/Scripts/UI/Auction/ValueRankingView.cs`
- `Assets/Scripts/UI/Auction/DraggableCardView.cs`
- `Assets/Scripts/UI/Auction/RankingSlotView.cs`
- `Assets/Scripts/UI/Auction/DragLineView.cs`

### 3.5.2 削除するヒエラルキーGameObject

BattleScene の `Canvas` 以下:

```
Canvas
 └─ ValueRankingView              ← 全体を削除
     ├─ TextBanner
     │   └─ InstructionText
     ├─ SlotContainer
     │   ├─ RankingSlotView1      (SlotGlow, SlotBack, RankingNumberFrame)
     │   ├─ RankingSlotView2
     │   ├─ RankingSlotView3
     │   └─ RankingSlotView4
     ├─ HandAnkers
     ├─ AuctionNormalButton
     └─ DragLineView
```

**操作**: `execute-dynamic-code` で `Canvas/ValueRankingView` を `DestroyImmediate` し、Prefab変更を保存

### 3.5.3 改修するPrefab

#### AuctionCardView.prefab（新規ラッパーPrefab）
**パス**: `Assets/Prefabs/NewBattleSceneView/AuctionCardView.prefab`

企画書の画像では、各カードの右下に個別の対話ボタンが配置されている。
現在の AuctionView では `CardView` と `CardBidInfoView` を別々にインスタンス化しているが、
新仕様ではこれらを **AuctionCardView** というラッパーPrefabにまとめ、対話ボタンも含める。

```
AuctionCardView (AuctionCardView.cs)
 ├─ CardView                     ← 既存CardView prefabをネスト
 ├─ CardBidInfoView              ← 既存CardBidInfoView prefabをネスト（現在は動的生成していた）
 └─ DialogueButton               ← 新規（カード右下の小さい対話ボタン）
     └─ DialogueIcon (Image)
```

**対応スクリプト**: `Assets/Scripts/UI/Auction/AuctionCardView.cs` (新規作成)
```csharp
/// <summary>
/// オークション用カードのラッパーView
/// CardView + CardBidInfoView + 対話ボタンを統合
/// </summary>
public class AuctionCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardBidInfoView cardBidInfoView;
    [SerializeField] private Button dialogueButton;

    public CardView CardView => cardView;
    public CardBidInfoView BidInfoView => cardBidInfoView;
    public Observable<AuctionCardView> OnDialogueClicked => ...;
    public Observable<AuctionCardView> OnCardClicked => ...;
}
```

#### AuctionView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/AuctionView.prefab`

現在のヒエラルキー:
```
AuctionView
 ├─ Back
 ├─ TextBanner / InstructionText
 ├─ PlayerCardContainer          ← 削除
 ├─ EnemyCardContainer           ← 削除
 ├─ EmotionResourceDisplayView
 ├─ AuctionNormalButton (入札確定)
 └─ BidWindowView
```

変更後:
```
AuctionView
 ├─ Back
 ├─ TextBanner / InstructionText
 ├─ CardContainer                ← 新規（6枚共有コンテナ、GridLayoutGroup 3列×2行）
 │   └─ (AuctionCardView × 6 を動的生成)
 ├─ EmotionResourceDisplayView
 ├─ AuctionNormalButton (入札確定)
 └─ BidWindowView
```

**操作**:
1. `PlayerCardContainer` と `EnemyCardContainer` を削除
2. 新しい `CardContainer` を作成（GridLayoutGroup: 3列×2行 or HorizontalLayoutGroup: 6枚横並び）
3. AuctionView の SerializeField 変更:
   - `cardPrefab` (CardView) → `auctionCardPrefab` (AuctionCardView) に変更
   - `cardBidInfoPrefab` は不要に（AuctionCardView内に含まれるため）
4. AuctionView.cs でカード生成時に `AuctionCardView` をインスタンス化

#### BidWindowView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/BidWindowView.prefab`

現在のヒエラルキー:
```
BidWindowView
 ├─ BackButton
 └─ Panel
     ├─ BidHeaderText
     ├─ Text (CardNameText, Cross, EmotionNameText)
     ├─ Card
     ├─ EmotionTypeImage
     ├─ Balance (天秤アニメーション)
     │   ├─ BalanceCenter/Bar/Left/Right
     │   ├─ CurrentBitFire
     │   └─ CurrentBitText
     └─ Button (Increase/Decrease)
```

変更:
- **EmotionLockIndicator** を追加: 1カード1感情制約を視覚表示
- 既にベット中の感情が異なる場合、感情切替の警告/確認UI追加

#### CardBidInfoView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/CardBidInfoView.prefab`

変更: 価値順位(Rank)表示部分の削除 or 非表示化。CardBidInfoView.cs の `ShowRank` メソッド関連UIは不要に。

### 3.5.4 新規作成するPrefab

#### AuctionCardView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/AuctionCardView.prefab`

各カードをラップするPrefab。現在は AuctionView が CardView と CardBidInfoView を別々に生成しているが、
新仕様では対話ボタンも含めて1つのPrefabにまとめる。

```
AuctionCardView (RectTransform, AuctionCardView.cs)
 ├─ CardView                     (既存CardView prefabをネスト配置)
 ├─ CardBidInfoView              (既存CardBidInfoView prefabをネスト配置)
 └─ DialogueButton               (Button, 右下に小さく配置)
     └─ DialogueIcon (Image, 吹き出しアイコン)
```

**対応スクリプト**: `Assets/Scripts/UI/Auction/AuctionCardView.cs` (新規)

#### CompetitionView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/CompetitionView.prefab`

```
CompetitionView (CanvasGroup, CompetitionView.cs)
 ├─ Background (Image, 半透明黒背景)
 ├─ TitleText ("競合発生" TextMeshProUGUI)
 ├─ CardDisplay (対象カードの表示領域)
 ├─ BalanceView (天秤表示、BidWindowViewのBalance構造を流用)
 │   ├─ BalanceCenter
 │   ├─ BalanceBar
 │   ├─ BalanceLeft (プレイヤー側)
 │   │   └─ PlayerBidText
 │   └─ BalanceRight (敵側)
 │       └─ EnemyBidText
 ├─ TimerDisplay (タイマー表示)
 │   ├─ TimerFill (Image, Filled)
 │   └─ TimerText (残り秒数)
 ├─ EmotionResourceDisplayView (感情選択車輪、既存prefab流用)
 └─ RaiseButton (上乗せボタン, AuctionNormalButton流用)
```

**対応スクリプト**: `Assets/Scripts/UI/Auction/CompetitionView.cs` (新規作成)

### 3.5.5 ヒエラルキーへの新規追加

BattleScene の `Canvas` に追加:

```
Canvas
 ├─ ... (既存のView群)
 ├─ AuctionView                  ← 改修（3.5.3参照）
 ├─ CompetitionView              ← 新規追加（AuctionViewとRewardPhaseViewの間）
 ├─ RewardPhaseView
 └─ ...
```

**操作**: `execute-dynamic-code` で CompetitionView prefab をインスタンス化し、Canvas直下の適切な位置（AuctionViewの次、RewardPhaseViewの前）に配置

### 3.5.6 Prefab・ヒエラルキー作業チェックリスト

```
削除:
  [ ] ヒエラルキーから Canvas/ValueRankingView を削除（execute-dynamic-code）
  [ ] ValueRankingView.prefab 削除
  [ ] ValueRanking/DraggableCardView.prefab 削除
  [ ] ValueRanking/RankingSlotView.prefab 削除
  [ ] RankTextPrefab.prefab 削除
  [ ] ValueRanking/ フォルダ自体を削除
  [ ] シーンを保存

改修:
  [ ] AuctionView: PlayerCardContainer/EnemyCardContainer → 単一CardContainer に統合
  [ ] AuctionView: SerializeField を auctionCardPrefab (AuctionCardView) に変更
  [ ] BidWindowView: EmotionLockIndicator 追加
  [ ] CardBidInfoView: Rank表示UIの削除/非表示化
  [ ] AuctionView.prefab に Apply
  [ ] BidWindowView.prefab に Apply
  [ ] CardBidInfoView.prefab に Apply

新規作成:
  [ ] AuctionCardView.prefab 作成（CardView + CardBidInfoView + DialogueButton のラッパー）
  [ ] CompetitionView.prefab 作成
  [ ] ヒエラルキーに CompetitionView を追加（Canvas直下）
  [ ] CompetitionView の SerializeField 設定
  [ ] シーンを保存
```

### 3.5.7 ScriptableObjectアセットの変更

AuctionData の構造変更（`playerCards` + `enemyCards` → `auctionCards`）に伴い、既存アセットの再設定が必要:

```
Assets/ScriptableObject/AuctionData/ 以下の全アセット
  - playerCards, enemyCards フィールドが消えて auctionCards フィールドに変更
  - 既存データは失われるため、事前にメモ or 移行スクリプトで対応
```

**推奨手順**:
1. 変更前に各AuctionDataアセットのカード構成をメモ
2. AuctionData.cs を変更
3. `execute-dynamic-code` で各アセットの `auctionCards` フィールドに6枚を再設定

---

## 4. 実装ステップ（推奨順序）

### Phase 1: 基盤変更（データモデル・定数）

#### Step 1.1: GameConstants 変更
**ファイル**: `Assets/Scripts/Game/Core/GameConstants.cs`

```diff
- public const int DEFAULT_EMOTION_VALUE = 10;
+ public const int DEFAULT_EMOTION_VALUE = 3;

- public const int CARDS_PER_PLAYER = 4;
+ public const int AUCTION_CARD_COUNT = 6;

+ /// <summary>
+ /// 競合フェーズのタイムアウト時間（秒）
+ /// </summary>
+ public const float COMPETITION_TIMEOUT_SECONDS = 10f;
```

VALUE_RANKING_BASE_RESOURCE, BASE_REWARD_MAX は価値順位廃止に伴い削除。
OWN_CARD_BONUS はカード所有の概念廃止（共有6枚）に伴い削除。

#### Step 1.2: GameState 変更
**ファイル**: `Assets/Scripts/Game/Core/Enums.cs`

```diff
 public enum GameState
 {
     ThemeAnnouncement,
-    CardDistribution,
-    ValueRanking,
     CardReveal,
+    DialoguePhase,
     BiddingPhase,
-    DialoguePhase,
     AuctionResult,
+    CompetitionPhase,
     RewardPhase,
     MemoryGrowth,
     BattleEnd,
 }
```

#### Step 1.3: BidModel に1カード1感情制約を追加
**ファイル**: `Assets/Scripts/Game/Models/BidModel.cs`

現在の `AddBid` / `SetBid` を変更し、1カードにつき1種類の感情のみ設定可能にする。

```csharp
/// <summary>
/// 入札を設定（1カード1感情制約）
/// 既に別の感情がセットされている場合はクリアしてから設定
/// </summary>
public void SetBid(CardModel card, EmotionType emotion, int amount)
{
    if (!_bids.ContainsKey(card))
        _bids[card] = new Dictionary<EmotionType, int>();

    // 1カード1感情制約: 既存の他感情をクリア
    _bids[card].Clear();

    if (amount > 0)
        _bids[card][emotion] = amount;
    else
        _bids.Remove(card);
}

/// <summary>
/// カードに設定されている感情タイプを取得（1カード1感情制約）
/// </summary>
public EmotionType? GetBidEmotion(CardModel card)
{
    if (!_bids.TryGetValue(card, out var emotions) || emotions.Count == 0)
        return null;
    // 1種類しかないはずなので先頭を返す
    foreach (var kvp in emotions)
        return kvp.Key;
    return null;
}
```

#### Step 1.4: AuctionData を6枚構成に変更
**ファイル**: `Assets/Scripts/ScriptableObject/AuctionData.cs`

```diff
- [SerializeField] private List<CardData> playerCards = new();
- [SerializeField] private List<CardData> enemyCards = new();
+ [SerializeField] private List<CardData> auctionCards = new();

- public IReadOnlyList<CardData> PlayerCards => playerCards;
- public IReadOnlyList<CardData> EnemyCards => enemyCards;
+ public IReadOnlyList<CardData> AuctionCards => auctionCards;
```

既存のAuctionData ScriptableObjectアセットも6枚構成に再設定が必要。

---

### Phase 2: 価値順位システムの削除

#### Step 2.1: ValueRankingModel 削除
- `Assets/Scripts/Game/Models/ValueRankingModel.cs` を削除

#### Step 2.2: PlayerPresenter から ValueRanking 参照を削除
**ファイル**: `Assets/Scripts/Game/Presenters/PlayerPresenter.cs`

```diff
- public ValueRankingModel ValueRanking => _valueRanking;
  ...
- private readonly ValueRankingModel _valueRanking = new();
  ...
  public virtual void InitializeAuctionData()
  {
      _cards.Clear();
-     _valueRanking.Clear();
      _bids.Clear();
      _wonCards.Clear();
  }
```

#### Step 2.3: Enemy から価値順位AI削除
**ファイル**: `Assets/Scripts/Game/Presenters/Enemy.cs`

`DecideValueRankings()` メソッドを削除。

#### Step 2.4: UI削除
以下のファイルを削除:
- `Assets/Scripts/UI/Auction/ValueRankingView.cs`
- `Assets/Scripts/UI/Auction/DraggableCardView.cs`
- `Assets/Scripts/UI/Auction/RankingSlotView.cs`

対応するprefab/GameObjectもシーンから除去。

#### Step 2.5: RewardCalculator を再設計
**ファイル**: `Assets/Scripts/Game/Services/RewardCalculator.cs`

価値順位が無くなるため、報酬計算ロジックを再設計する必要がある。
企画書には報酬計算の詳細が記載されていないため、**ユーザーに確認が必要**。

仮案:
```csharp
public static RewardResult Calculate(int bidAmount, bool isOwnCard)
{
    // 基本報酬 = 固定値（例: 3）
    // 相対報酬 = なし（価値順位がないため）
    // 自カードボーナス = 維持
    // 合計 = 基本報酬 + 自カードボーナス
}
```

---

### Phase 3: フェーズ順序変更

#### Step 3.1: BattlePresenter のフロー変更
**ファイル**: `Assets/Scripts/Game/Logic/BattlePresenter.cs`

`StartGame()` メソッドを以下のように変更:

```csharp
private async UniTask StartGame()
{
    await UniTask.Delay(1000);

    // 1. テーマ公開
    _currentGameState.Value = GameState.ThemeAnnouncement;
    await HandleThemeAnnouncement();

    // 2. カード提示（6枚を場に並べる）
    _currentGameState.Value = GameState.CardReveal;
    await HandleCardReveal();

    // 3. 対話フェーズ（入札前に実施）
    _currentGameState.Value = GameState.DialoguePhase;
    await HandleDialoguePhase();

    // 4. 入札フェーズ
    _currentGameState.Value = GameState.BiddingPhase;
    await HandleBiddingPhase();

    // 5. 落札者判定フェーズ
    _currentGameState.Value = GameState.AuctionResult;
    await HandleAuctionResult();

    // 6. 競合フェーズ（引き分けカードがある場合のみ）
    // HandleAuctionResult内で競合をトリガー

    // 7. 報酬フェーズ
    _currentGameState.Value = GameState.RewardPhase;
    await HandleRewardPhase();

    // 8. 記憶育成フェーズ
    _currentGameState.Value = GameState.MemoryGrowth;
    await HandleMemoryGrowth();

    // 終了
    _currentGameState.Value = GameState.BattleEnd;
    await HandleBattleEnd();
}
```

#### Step 3.2: HandleCardReveal 変更
プレイヤー/敵の区別なく6枚表示:

```csharp
private async UniTask HandleCardReveal()
{
    _auctionCards.Clear();
    // AuctionDataから6枚取得（プレイヤー/敵の区別なし）
    foreach (var cardData in _currentAuctionData.AuctionCards)
    {
        _auctionCards.Add(new CardModel(cardData));
    }

    _battleUIPresenter.ShowAuctionCards(_auctionCards);
    await _battleUIPresenter.PlayPhaseTransitionOpenAsync();
    await UniTask.Delay(1500);
}
```

#### Step 3.3: HandleCardDistribution と HandleValueRanking を削除
これら2メソッドは不要になるため削除。

---

### Phase 4: 旧対話システム削除 + 仮実装

対話フェーズは今後大きく変わるため、現行の対話システムを完全に削除し、
カードの対話ボタンを押すとログが流れるだけの仮実装に置き換える。

#### Step 4.1: 旧対話システムの削除

**削除するスクリプト:**
- `Assets/Scripts/UI/Auction/DialoguePhaseView.cs` — 旧対話フェーズView
- `Assets/Scripts/UI/Auction/DialogueChoicesView.cs` — 選択肢UI
- `Assets/Scripts/UI/Auction/DialoguePortraitView.cs` — 立ち絵View
- `Assets/Scripts/UI/Auction/DialogueCutInView.cs` — カットインView
- `Assets/Scripts/Game/Logic/DialogueEffectApplier.cs` — 対話効果適用ロジック
- `Assets/Scripts/Game/Core/DialogueEnums.cs` — DialogueChoiceType等のenum
- `Assets/Scripts/ScriptableObject/EnemyDialogueData.cs` — 敵対話データ定義

**削除するPrefab:**
- `Assets/Prefabs/NewBattleSceneView/DialoguePhase/` フォルダ全体
  - DialoguePhaseView.prefab
  - DialogueChoicesView.prefab
  - DialogueCutInView.prefab
  - DialoguePortraitView.prefab
- `Assets/Prefabs/BattleSceneView/DialogueChoiceButton.prefab`

**削除するScriptableObjectアセット:**
- `Assets/ScriptableObjects/EnemyDialogueData/` フォルダ全体（Alv.asset, Cerica.asset）
- `Assets/ScriptableObjects/TutorialData/FirstBattle/DialoguePhase.asset`
- `Assets/ScriptableObjects/TutorialData/FirstBattle/DialoguePhase2.asset`

**削除するスプライト:**
- `Assets/Sprites/Auction/Dialogue/` フォルダ全体

**削除するヒエラルキーGameObject:**
- `Canvas/DialoguePhaseView`（子含む全体を削除）

#### Step 4.2: 参照コードの修正

**BattleUIPresenter.cs** — 対話関連メソッドを全て削除:
- `_dialoguePhaseView` フィールド
- `ShowPlayerDialogueAsync`, `HidePlayerDialogueAsync`
- `ShowEnemyDialogueAsync`, `HideEnemyDialogueAsync`
- `ShowPlayerNarration`, `ShowEnemyNarration`
- `HideAllAsync`, `ShowDialogueView`, `InitializeDialogueView`
- `WaitForFourChoiceAsync`, `WaitForThreeResponseAsync`
- `HideDialogueViewAsync`

**BattlePresenter.cs** — HandleDialoguePhaseを仮実装に置き換え:
- `HandleDialoguePhase` — Debug.Logのみのスタブに変更
- `HandlePlayerFirstTurn`, `HandleEnemyFirstTurn` — 削除

**AuctionData.cs** — `DialogueData` フィールドを削除

#### Step 4.3: AuctionCardView（ラッパー）新規作成

**ファイル**: `Assets/Scripts/UI/Auction/AuctionCardView.cs`

```csharp
public class AuctionCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardBidInfoView cardBidInfoView;
    [SerializeField] private Button dialogueButton;

    public CardView CardView => cardView;
    public CardBidInfoView BidInfoView => cardBidInfoView;
    public CardModel CardModel { get; private set; }

    public Observable<AuctionCardView> OnCardClicked =>
        cardView.OnClicked.Select(_ => this);

    public Observable<AuctionCardView> OnDialogueClicked =>
        dialogueButton.OnClickAsObservable().Select(_ => this);

    public void Initialize(CardModel cardModel) { ... }
    public void SetDialogueButtonVisible(bool visible) => ...;
}
```

#### Step 4.4: 対話ボタンの仮実装

**UIフロー**:
1. 6枚のカードそれぞれに対話ボタンが表示される
2. プレイヤーが任意のカードの対話ボタンを押す
3. Debug.Logでカード名が出力される（仮実装）
4. 入札確定ボタンで対話フェーズ終了 → 入札フェーズへ

対話ボタンを押すとDebug.Logでカード名が出力されるだけの仮実装。
対話フェーズの詳細は後日設計する。

---

### Phase 5: 入札フェーズ改修

#### Step 5.1: AuctionView の入札ロジック変更
**ファイル**: `Assets/Scripts/UI/Auction/AuctionView.cs`

主な変更点:
1. **6枚表示**（プレイヤー/敵コンテナの区別を廃止し、共有コンテナに変更）
2. **1カード1感情制約**: 感情選択後にベット数のみ調整可能に
3. **BidWindowView**: 感情切り替え時に既存ベットをクリアする確認

```csharp
private void OnIncreaseBid()
{
    if (_selectedCardModel == null) return;

    // 既にこのカードに別の感情がベットされている場合は拒否
    var existingEmotion = _playerBids.GetBidEmotion(_selectedCardModel);
    if (existingEmotion.HasValue && existingEmotion.Value != _currentEmotion)
    {
        // UIで警告表示 or 自動切り替え
        return;
    }

    var available = GetAvailableResource(_currentEmotion);
    if (available <= 0) return;

    _playerBids.SetBid(_selectedCardModel, _currentEmotion,
        _playerBids.GetTotalBid(_selectedCardModel) + 1);
    _usedResources[_currentEmotion]++;

    // UI更新...
}
```

#### Step 5.2: Enemy の入札AI改修
**ファイル**: `Assets/Scripts/Game/Presenters/Enemy.cs`

```csharp
public void DecideBids(IReadOnlyList<CardModel> auctionCards)
{
    Bids.Clear();
    if (auctionCards.Count == 0) return;

    // 各感情リソース（各3）を使って入札を決定
    // 1カード1感情制約を遵守
    var shuffledCards = auctionCards.OrderBy(_ => Random.value).ToList();
    var emotions = (EmotionType[])System.Enum.GetValues(typeof(EmotionType));
    var remainingResources = new Dictionary<EmotionType, int>();

    foreach (var emotion in emotions)
        remainingResources[emotion] = GetEmotionAmount(emotion);

    foreach (var card in shuffledCards)
    {
        // ランダムに感情を選択
        var emotion = emotions[Random.Range(0, emotions.Length)];
        var available = remainingResources[emotion];
        if (available <= 0) continue;

        var amount = Random.Range(1, available + 1);
        Bids.SetBid(card, emotion, amount);
        remainingResources[emotion] -= amount;
    }
}
```

---

### Phase 6: 落札判定と競合システム（最大の新規実装）

#### Step 6.1: AuctionJudge の改修
**ファイル**: `Assets/Scripts/Game/Services/AuctionJudge.cs`

落札失敗時のリソース返却ルールを追加:

```csharp
public struct AuctionResultEntry
{
    public CardModel Card;
    public bool IsPlayerWon;
    public int PlayerBid;
    public int EnemyBid;
    public bool NoBids;
    public bool IsDraw;          // 同数 → 競合発生
    public bool ShouldRefund;    // 敗者はリソース返却
}
```

判定ロジック変更:
- 勝利: リソース消費
- **敗北: リソース返却**（現行は消費）
- **引き分け: 競合フェーズへ移行**

#### Step 6.2: リソース返却ロジック
**ファイル**: `Assets/Scripts/Game/Logic/BattlePresenter.cs`

```csharp
private void ProcessAuctionResults(List<AuctionJudge.AuctionResultEntry> results)
{
    foreach (var result in results)
    {
        if (result.NoBids) continue;

        if (result.IsDraw)
        {
            // 競合リストに追加（Phase 6.3で処理）
            _competitionCards.Add(result);
            continue;
        }

        if (result.IsPlayerWon)
        {
            // プレイヤー勝利: リソース消費
            ConsumeBidForCard(_player, result.Card);
            _player.AddWonCard(result.Card);
            // 敵: リソース返却（消費しない）
        }
        else
        {
            // 敵勝利: 敵リソース消費
            ConsumeBidForCard(_enemy, result.Card);
            _enemy.AddWonCard(result.Card);
            // プレイヤー: リソース返却（消費しない）
        }
    }
}
```

#### Step 6.3: CompetitionHandler 新規作成
**ファイル**: `Assets/Scripts/Game/Logic/CompetitionHandler.cs`

```csharp
/// <summary>
/// リアルタイム競合を管理するハンドラ
/// 引き分け時に両者が1枚ずつリソースを上乗せし、
/// 10秒間上乗せが無ければ終了
/// </summary>
public class CompetitionHandler
{
    private CardModel _card;
    private int _playerTotal;
    private int _enemyTotal;
    private float _lastActionTime;
    private bool _isActive;

    /// <summary>
    /// 競合を開始
    /// </summary>
    public void Start(CardModel card, int playerBid, int enemyBid)
    {
        _card = card;
        _playerTotal = playerBid;
        _enemyTotal = enemyBid;
        _lastActionTime = Time.time;
        _isActive = true;
    }

    /// <summary>
    /// プレイヤーが1枚上乗せ（任意の感情）
    /// </summary>
    public bool TryPlayerRaise(EmotionType emotion, PlayerPresenter player)
    {
        if (!_isActive) return false;
        if (player.GetEmotionAmount(emotion) <= 0) return false;

        player.TryConsumeEmotion(emotion, 1);
        _playerTotal++;
        _lastActionTime = Time.time;
        return true;
    }

    /// <summary>
    /// 敵が1枚上乗せ
    /// </summary>
    public void EnemyRaise(EmotionType emotion, PlayerPresenter enemy)
    {
        if (!_isActive) return;

        enemy.TryConsumeEmotion(emotion, 1);
        _enemyTotal++;
        _lastActionTime = Time.time;
    }

    /// <summary>
    /// タイムアウトチェック（10秒）
    /// </summary>
    public bool IsTimedOut => _isActive &&
        Time.time - _lastActionTime >= GameConstants.COMPETITION_TIMEOUT_SECONDS;

    /// <summary>
    /// 勝者判定
    /// </summary>
    public bool? IsPlayerWon =>
        _playerTotal > _enemyTotal ? true :
        _playerTotal < _enemyTotal ? false : null;

    public int PlayerTotal => _playerTotal;
    public int EnemyTotal => _enemyTotal;
}
```

#### Step 6.4: CompetitionView 新規作成
**ファイル**: `Assets/Scripts/UI/Auction/CompetitionView.cs`

```
UI要素:
- 天秤（既存のBidWindowViewと同様のアニメーション）
- 両者の現在のベット数表示
- タイマー表示（10秒カウントダウン）
- 感情リソース選択（8種類の車輪UI再利用）
- 「上乗せ」ボタン
- 「競合発生」タイトル表示
```

#### Step 6.5: BattlePresenter に競合処理を組み込み

```csharp
private async UniTask HandleCompetitions(List<AuctionJudge.AuctionResultEntry> drawResults)
{
    foreach (var drawResult in drawResults)
    {
        var handler = new CompetitionHandler();
        handler.Start(drawResult.Card, drawResult.PlayerBid, drawResult.EnemyBid);

        // 競合UI表示
        _battleUIPresenter.ShowCompetitionView(drawResult.Card, handler);

        // 10秒タイマーでプレイヤー/敵の上乗せを待機
        while (!handler.IsTimedOut)
        {
            // プレイヤー入力はUI側で処理
            // 敵AIの上乗せ判定
            TryEnemyCompetitionRaise(handler);
            await UniTask.Yield();
        }

        // 結果判定
        var winner = handler.IsPlayerWon;
        if (winner == true)
            _player.AddWonCard(drawResult.Card);
        else if (winner == false)
            _enemy.AddWonCard(drawResult.Card);
        // 完全な引き分けの場合はカード消失

        _battleUIPresenter.HideCompetitionView();
    }
}
```

---

### Phase 7: ゲームオーバー条件の追加

#### Step 7.1: PlayerPresenter にリソース枯渇チェック追加

```csharp
/// <summary>
/// 全ての感情リソースが0かどうか
/// </summary>
public bool IsAllResourcesDepleted()
{
    foreach (var (_, amount) in EmotionResources)
    {
        if (amount > 0) return false;
    }
    return true;
}
```

#### Step 7.2: BattlePresenter でゲームオーバー判定

入札確定後にリソース枯渇チェック:
```csharp
if (_player.IsAllResourcesDepleted())
{
    await HandleGameOver();
    return;
}
```

---

### Phase 8: UI調整

#### Step 8.1: AuctionView のレイアウト変更
- プレイヤー/敵のカードコンテナを**1つの共有コンテナ**に統合
- 6枚を横一列 or 2段×3列で配置
- 対話ボタンの追加

#### Step 8.2: BidWindowView の変更
- 感情選択UIを明確に（1カード1感情制約を視覚的に表現）
- 既にベット済みの感情を明示
- 別感情に切り替える際の確認/自動クリア

#### Step 8.3: EmotionResourceDisplayView の変更
- 初期値3を反映した表示
- 返却時の演出追加

---

## 5. 実装作業の具体的な手順

### 5.1 準備作業（コード変更前）
1. `impl-realtime-auction` ブランチであることを確認
2. 既存のAuctionData ScriptableObjectアセットのバックアップ

### 5.2 作業順序チェックリスト

```
Phase 1: 基盤変更 ✅ 完了
  [x] 1.1 GameConstants の定数変更（DEFAULT_EMOTION_VALUE=3, AUCTION_CARD_COUNT=6, COMPETITION_TIMEOUT_SECONDS=10f）
  [x] 1.2 GameState enum の変更（CardDistribution/ValueRanking削除、CompetitionPhase追加、DialoguePhase順序変更）
  [x] 1.3 BidModel に1カード1感情制約を追加（SetBid/GetBidEmotion）
  [x] 1.4 AuctionData を6枚構成に変更（playerCards+enemyCards → auctionCards）
  [x] コンパイル確認

Phase 2: 価値順位削除 ✅ 完了
  [x] 2.1 ValueRankingModel.cs 削除
  [x] 2.2 PlayerPresenter から参照削除
  [x] 2.3 Enemy から DecideValueRankings 削除、DecideBids を1カード1感情制約対応に書き換え
  [x] 2.4 スクリプト削除: ValueRankingView.cs, DraggableCardView.cs, RankingSlotView.cs
       ※ DragLineView.cs は将来利用の可能性があるため残留
  [x] 2.5 Prefab削除: ValueRankingView.prefab, ValueRanking/DraggableCardView.prefab, ValueRanking/RankingSlotView.prefab, RankTextPrefab.prefab
  [x] 2.6 ヒエラルキー: Canvas/ValueRankingView を削除（execute-dynamic-code）
  [x] 2.7 RewardCalculator をスタブ実装に書き換え（報酬計算は後で設計）
  [x] 2.8 BattlePresenter/BattleUIPresenter から ValueRanking 関連コード削除
  [x] 2.9 CardBidInfoView: ShowRank/HideRank/rankText を削除
  [x] 2.10 CardAcquisitionInfo/SavedCardAcquisitionInfo から PlayerValueRank/EnemyValueRank を削除
  [x] 2.11 GameStateRepository の ConvertSavedThemeToAcquiredTheme を更新
  [x] 2.12 DialogueEffectApplier: AddBid → SetBid に修正（※ Phase 4 でファイル自体を削除済み）
  [x] 2.13 スプライト削除: Sprites/Auction/ValueRanking/ フォルダ
  [x] 2.14 チュートリアルアセット削除: ValueRanking.asset
       ※ CardDragLine.shadergraph は将来利用の可能性があるため残留
  [x] コンパイル確認 + シーン保存

Phase 3: フェーズ順序変更 ✅ 完了
  [x] 3.1 BattlePresenter.StartGame() のフロー書き換え（テーマ→カード提示→対話→入札→判定→報酬→記憶育成）
  [x] 3.2 HandleCardReveal を6枚共有表示に変更（AuctionCards から CardModel 生成）
  [x] 3.3 HandleCardDistribution, HandleValueRanking 削除
  [x] 3.4 HandleAuctionResult: 勝者のみリソース消費、敗者はリソース返却（ConsumeBidForCard 新設）
  [x] 3.5 HandleRewardPhase: 新 RewardCalculator シグネチャ対応
  [x] 3.6 HandleBiddingPhase: 新 ShowAuctionCards/WaitForBiddingAsync シグネチャ対応
  [x] 3.7 BuildCardAcquisitionInfoList: ValueRank 参照削除
  [x] コンパイル確認

Phase 4: 旧対話システム削除 + 仮実装 ✅ 完了
  [x] 4.1 旧対話システム削除
       - スクリプト: DialoguePhaseView, DialogueChoicesView, DialoguePortraitView, DialogueCutInView,
         DialogueEffectApplier, DialogueEnums, EnemyDialogueData を削除
       - Prefab: NewBattleSceneView/DialoguePhase/ フォルダ全体、DialogueChoiceButton.prefab を削除
       - ScriptableObject: EnemyDialogueData/ フォルダ、DialoguePhase.asset, DialoguePhase2.asset を削除
       - スプライト: Sprites/Auction/Dialogue/ フォルダを削除
       - ヒエラルキー: Canvas/DialoguePhaseView を削除
  [x] 4.2 参照コードの修正
       - BattleUIPresenter: _dialoguePhaseView, 対話関連メソッド群を全て削除
       - BattlePresenter: HandleDialoguePhase をスタブに置き換え、HandlePlayerFirstTurn/HandleEnemyFirstTurn を削除
       - AuctionData: dialogueData フィールドを削除
  [x] 4.3 AuctionCardView.cs 新規作成（CardView + CardBidInfoView + DialogueButton ラッパー）
  [x] コンパイル確認 + フォーマット修正

Phase 5: Prefab統合 + AuctionCardView組み込み ✅ 完了
  [x] 5.1 AuctionView の1カード1感情制約UI（OnIncreaseBid/OnDecreaseBidで制約チェック済み）
  [x] 5.2 Enemy 入札AI改修（1カード1感情制約対応済み）
  [x] 5.3 Prefab新規: AuctionCardView.prefab 作成（CardView + CardBidInfoView + DialogueButton をネスト）
  [x] 5.4 AuctionView.cs を AuctionCardView ベースに書き換え
       - cardPrefab (CardView) → auctionCardPrefab (AuctionCardView) に変更
       - cardBidInfoPrefab を削除（AuctionCardView内に含まれるため）
       - _cardViewToModel / _cardBidInfoViews の辞書を List<AuctionCardView> に統合
  [x] 5.5 Prefab改修: AuctionView（PlayerCardContainer+EnemyCardContainer → 単一CardContainer統合）
       - EnemyCardContainer 削除、PlayerCardContainer → CardContainer にリネーム
       - enemyCardStagger 削除、playerCardStagger → cardStagger に統合
       - AuctionView SerializeField を更新（cardContainer, auctionCardPrefab, cardStagger）
  [ ] 5.6 Prefab改修: BidWindowView（EmotionLockIndicator追加）※ 後回し可
  [x] コンパイル確認 + フォーマット修正

Phase 6: 競合システム
  [ ] 6.1 AuctionJudge 改修（ShouldRefundフィールドは不要、現在のIsDraw判定で十分）
  [x] 6.2 BattlePresenter のリソース返却処理（勝者のみ消費、敗者は返却）※ 実装済み
       引き分け時は暫定で両者リソース返却（競合未実装のため）
  [ ] 6.3 CompetitionHandler.cs 新規作成
  [ ] 6.4 CompetitionView.cs 新規作成
  [ ] 6.5 Prefab新規: CompetitionView.prefab 作成
  [ ] 6.6 ヒエラルキー: Canvas直下に CompetitionView 追加（execute-dynamic-code）
  [ ] 6.7 CompetitionView の SerializeField 設定
  [ ] 6.8 BattlePresenter に競合処理組み込み（HandleAuctionResult内のIsDraw分岐を競合フェーズへ）
  [ ] 6.9 敵AI の競合時ロジック
  [ ] コンパイル確認 + シーン保存

Phase 7: ゲームオーバー
  [ ] 7.1 リソース枯渇チェック追加
  [ ] 7.2 ゲームオーバーUI（既存のものがあれば流用）
  [ ] コンパイル確認

Phase 8: UI調整
  [ ] 8.1 AuctionView レイアウト変更
  [ ] 8.2 BidWindowView 変更
  [ ] 8.3 EmotionResourceDisplayView 変更
  [ ] 8.4 CompetitionView の演出
  [ ] コンパイル確認・動作確認
```

---

## 6. 確認済み事項・未確認事項

### 確認済み
1. **カードの出所**: **区別なし（共有6枚）** → プレイヤー/敵の所有概念を廃止。AuctionDataは `auctionCards` のみ
2. **報酬計算**: **後で決める** → RewardCalculatorは一旦スタブ実装（TotalReward=1固定）。報酬フェーズ自体は残すが計算ロジックは仮
3. **リソース消費ルール**: **勝者は消費、敗者は返却** → 落札者のみベット分を消費、敗者はベット分が手元に戻る。実装済み（ConsumeBidForCard）
4. **ゲームオーバー後**: **後で決める** → ゲームオーバー判定のみ実装、遷移先はTODOとする
5. **DragLineView/CardDragLine.shadergraph**: 将来利用の可能性があるため残留

### 未確認（実装時に確認）
6. **対話フェーズの詳細**: カード選択→対話の具体的なセリフデータ構造。現在のEnemyDialogueDataをカード別に拡張する必要があるか
7. **特殊効果カード**: 「一部特殊効果を持つ記憶カード」の詳細仕様
8. **敵AIの賢さ**: 競合時の敵AIはどの程度賢くすべきか
9. **落札失敗時のリソース返却演出**: どの程度の演出が必要か
10. **競合時に別感情使用可能**: 企画書に記載あり。最初のベットと異なる感情で上乗せ可能でよいか

---

## 7. リスク・注意点

1. **ScriptableObjectアセットの再設定**: AuctionDataの構造変更に伴い、既存のアセットが壊れる可能性。移行スクリプトの準備が必要
2. **対話システムの改修規模**: カード選択→対話の新フローは既存のDialoguePhaseViewの大幅改修が必要
3. **リアルタイム競合のネットワーク対応**: 現在はローカルAI対戦だが、将来的なオンライン対応を考慮すべきか
4. **テスト**: 競合タイマーのテストはUniTaskのテストフレームワークが必要
5. **チュートリアル**: 既存のチュートリアル（`_currentEnemyData.EnemyId == "alv"` チェック）も全面改修が必要
