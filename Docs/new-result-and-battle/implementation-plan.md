# リザルト&バトルパート 実装プラン

## 1. 企画書サマリー

### 2-1. リザルトパート（獲得カード一覧）
- 獲得した記憶カードが一覧で表示される
- **感情マッチ**: 記憶カードにはそれぞれ司る感情があり、それにマッチした感情リソースをベットしていた場合、**通常の1.5倍**で付与
- **自己記憶**: 獲得カードが自己記憶の場合、**通常の2倍**で付与
- その他（他者記憶、曖昧記憶）は固定値で付与

### 2-2. リザルトパート（感情リソースグラフ）
- 最終的な手持ちの感情リソースがグラフ化して表示
- **感情状態&スキル付与**: 最も多い感情リソースの感情が主人公に現れる（同数の場合は強さ順）
- 感情状態によってバトルパートで使用できるスキルやストーリーパートでの選択肢の種類が変化

### 3-1. バトルパート（デッキ選択）
- 手に入れた記憶カードを用いた決闘（3本勝負、先に2本先取で勝利）
- **勝利条件**は各階層のオークションパート前に開示（例: 数字がより小さい方が勝利）
- **カードの数字**: オークションパートでお互いがかけたリソース量が多い順に **1〜6** が割り当て
- **デッキ選択**: 落札した記憶カードから3枚を選ぶ（相手も同様に3枚選択）
- 枚数が不足している場合、自動的に数字3の記憶カード（感情は被りなしのランダム）が追加
- 一度デッキで使用したカードは次のバトルでは使用できなくなる

### 3-2. バトルパート（カード伏せ & スキル発動）
- コインで先攻後攻を決め、順にデッキの中から記憶カード1枚を選び場に伏せる
- **スキル発動**: カードオープンする前の自由なタイミングでスキルを発動するかを選べる（バトル中1回のみ使用可能）
- 相手のスキル発動後も発動が可能

### 3-3. バトルパート（カードオープン & 勝敗）
- 勝利条件に基づいて比較（例: 数字が小さい方が勝利）
- 同数の場合は記憶カードの司る感情の強さで決まる（引き分けなし）
- **先に2本先取した方の勝利**
- バトルで使用しても手持ちの記憶カードがなくなることはない
- **勝利**: 自分の所持している記憶カードから好きな1枚を記憶に入れる
- **敗北**: 相手に望まぬ記憶を植えつけられる

### 感情状態によるスキル効果

| 感情 | スキル効果 |
|------|-----------|
| 怒り | 次のターンのみ勝利条件が逆になる |
| 期待 | 自分の残りカードの数字を全てランダムに変える（被りあり） |
| 喜び | 自分の出したカードの数字を2倍にする |
| 信頼 | 一度使ったカードがもう一度使える |
| 恐れ | 相手と自分のカードの数字を入れ替える |
| 驚き | 自分の出したカードの数字をランダムに変える |
| 嫌悪 | 自分の出したカードの数字を2分の1にする |
| 悲しみ | デッキ内の任意のカードの数字を3に変える |

---

## 2. 現行仕様との差分

### 2.1 GameState フロー変更

```
【現行フロー】
ThemeAnnouncement → CardReveal → DialoguePhase → BiddingPhase
→ AuctionResult (CompetitionPhase) → RewardPhase → MemoryGrowth → BattleEnd

【新フロー】
ThemeAnnouncement → CardReveal → DialoguePhase → BiddingPhase
→ AuctionResult (CompetitionPhase) → ResultPhase → DeckSelection
→ CardBattle → MemoryGrowth → BattleEnd
```

### 2.2 変更の大きさ

| 項目 | 変更の大きさ | 内容 |
|------|:----------:|------|
| GameState enum | 中 | RewardPhase → ResultPhase に改名、DeckSelection/CardBattle を追加 |
| RewardCalculator | 大 | 感情マッチ倍率・記憶タイプ倍率を組み込む |
| CardData | 中 | EmotionType フィールドを追加 |
| AuctionData | 小 | VictoryCondition フィールドを追加 |
| BattlePresenter | 大 | 新フェーズ（デッキ選択・カードバトル）を追加 |
| BattleUIPresenter | 大 | 新View群への委譲メソッドを追加 |
| 新規UI: DeckSelectionView | 大（新規） | デッキ選択画面 |
| 新規UI: CardBattleView | 大（新規） | カードバトル画面 |
| 新規ロジック: CardBattleHandler | 大（新規） | 3本勝負のロジック（ターン管理・勝敗判定・スキル処理） |
| 新規ロジック: SkillEffectApplier | 中（新規） | 8種類のスキル効果適用 |
| 新規モデル: BattleCardModel | 小（新規） | カードに数字を割り当てたバトル用モデル |

---

## 3. 変更対象ファイル一覧

### 3.1 改修が必要なファイル

| ファイル | 変更内容 |
|--------|---------|
| `Assets/Scripts/Game/Core/Enums.cs` | GameState に DeckSelection, CardBattle を追加。RewardPhase → ResultPhase に改名 |
| `Assets/Scripts/Game/Core/GameConstants.cs` | バトル関連定数を追加 |
| `Assets/Scripts/ScriptableObject/CardData.cs` | EmotionType フィールドを追加 |
| `Assets/Scripts/ScriptableObject/AuctionData.cs` | VictoryCondition フィールドを追加 |
| `Assets/Scripts/Game/Services/RewardCalculator.cs` | 感情マッチ・記憶タイプ倍率の本実装 |
| `Assets/Scripts/Game/Logic/BattlePresenter.cs` | 新フェーズ群（DeckSelection, CardBattle）を追加 |
| `Assets/Scripts/UI/Main/BattleUIPresenter.cs` | 新View群への委譲メソッドを追加 |
| `Assets/Scripts/UI/Auction/RewardPhaseView.cs` | ResultPhaseView に改名、感情マッチ表示を追加 |
| `Assets/Scripts/UI/Auction/ResourceRewardView.cs` | 感情状態表示・スキル付与演出を追加 |
| `Assets/Scripts/UI/Auction/CardAcquisitionView.cs` | 感情マッチ情報・記憶タイプ表示を追加 |
| `Assets/Scripts/VContainer/BattleLifetimeScope.cs` | 新サービスの登録 |

### 3.2 新規作成ファイル

| ファイル | 役割 |
|--------|------|
| `Assets/Scripts/Game/Core/VictoryCondition.cs` | 勝利条件 enum（LowerWins, HigherWins） |
| `Assets/Scripts/Game/Models/BattleCardModel.cs` | バトル用カードモデル（数字割り当て付き） |
| `Assets/Scripts/Game/Models/BattleDeckModel.cs` | デッキモデル（3枚 + 使用済み管理） |
| `Assets/Scripts/Game/Logic/CardBattleHandler.cs` | カードバトルの3本勝負ロジック |
| `Assets/Scripts/Game/Logic/CardNumberAssigner.cs` | オークション入札量に基づくカード数字割り当て |
| `Assets/Scripts/Game/Logic/SkillEffectApplier.cs` | 8種スキル効果の適用ロジック |
| `Assets/Scripts/UI/Battle/DeckSelectionView.cs` | デッキ選択UI |
| `Assets/Scripts/UI/Battle/CardBattleView.cs` | カードバトル画面UI |
| `Assets/Scripts/UI/Battle/BattleCardSlotView.cs` | バトル場のカードスロット表示 |
| `Assets/Scripts/UI/Battle/SkillButtonView.cs` | スキル発動ボタンUI |
| `Assets/Scripts/UI/Battle/RoundIndicatorView.cs` | ラウンド表示（3本中何本目か） |
| `Assets/Scripts/UI/Battle/CoinFlipView.cs` | コインフリップ演出UI |

---

## 4. 詳細設計

### 4.1 VictoryCondition（勝利条件）

```csharp
// Assets/Scripts/Game/Core/VictoryCondition.cs
public enum VictoryCondition
{
    /// <summary>数字が小さい方が勝利</summary>
    LowerWins,
    /// <summary>数字が大きい方が勝利</summary>
    HigherWins,
}
```

### 4.2 CardData へのEmotionType追加

```csharp
// Assets/Scripts/ScriptableObject/CardData.cs
[Header("記憶情報")]
[SerializeField] private MemoryType memoryType;
[SerializeField] private EmotionType cardEmotion;  // 追加: カードが司る感情
[SerializeField, Range(0, GameConstants.MAX_GAUGE_VALUE)] private int effectAmount;

public EmotionType CardEmotion => cardEmotion;
```

### 4.3 AuctionData へのVictoryCondition追加

```csharp
// Assets/Scripts/ScriptableObject/AuctionData.cs
[SerializeField] private VictoryCondition victoryCondition;
public VictoryCondition VictoryCondition => victoryCondition;
```

### 4.4 GameConstants に追加する定数

```csharp
/// <summary>バトルのラウンド数（3本勝負）</summary>
public const int BATTLE_ROUND_COUNT = 3;
/// <summary>バトルの勝利に必要な本数</summary>
public const int BATTLE_WINS_REQUIRED = 2;
/// <summary>デッキの枚数</summary>
public const int DECK_SIZE = 3;
/// <summary>不足カードのデフォルト数字</summary>
public const int DEFAULT_CARD_NUMBER = 3;
/// <summary>感情マッチ倍率</summary>
public const float EMOTION_MATCH_MULTIPLIER = 1.5f;
/// <summary>自己記憶倍率</summary>
public const float SELF_MEMORY_MULTIPLIER = 2.0f;
```

### 4.5 GameState enum の変更

```csharp
public enum GameState
{
    ThemeAnnouncement,
    CardReveal,
    DialoguePhase,
    BiddingPhase,
    AuctionResult,
    CompetitionPhase,
    ResultPhase,        // 旧 RewardPhase → 改名
    DeckSelection,      // 新規: デッキ選択
    CardBattle,         // 新規: カードバトル
    MemoryGrowth,
    BattleEnd,
}
```

### 4.6 カード数字割り当てロジック

```csharp
// Assets/Scripts/Game/Logic/CardNumberAssigner.cs
/// <summary>
/// オークション入札量に基づいてカードに1〜6の数字を割り当てる
/// 両者の入札合計が多い順に1から割り当て
/// 同じ入札量のカードは同じ数字になる（ランク方式）
/// 例: [5, 3, 3, 2, 0, 0] → [1, 2, 2, 4, 5, 5]
/// </summary>
public static class CardNumberAssigner
{
    public static Dictionary<CardModel, int> AssignNumbers(
        IReadOnlyList<CardModel> auctionCards,
        BidModel playerBids,
        BidModel enemyBids)
    {
        // 各カードの合計入札量（プレイヤー＋敵）を計算し、降順ソート
        var cardTotals = auctionCards
            .Select(card => (card, total: playerBids.GetTotalBid(card) + enemyBids.GetTotalBid(card)))
            .OrderByDescending(x => x.total)
            .ToList();

        // ランク方式: 同じ入札量は同じ数字
        var result = new Dictionary<CardModel, int>();
        var rank = 1;
        for (var i = 0; i < cardTotals.Count; i++)
        {
            if (i > 0 && cardTotals[i].total < cardTotals[i - 1].total)
                rank = i + 1;
            result[cardTotals[i].card] = rank;
        }
        return result;
    }
}
```

### 4.7 BattleCardModel

```csharp
// Assets/Scripts/Game/Models/BattleCardModel.cs
/// <summary>
/// バトル用カードモデル（CardModel + 割り当て数字）
/// </summary>
public class BattleCardModel
{
    public CardModel Card { get; }
    public int Number { get; private set; }
    public EmotionType Emotion => Card.Data.CardEmotion;
    public bool IsUsed { get; set; }

    public BattleCardModel(CardModel card, int number)
    {
        Card = card;
        Number = number;
    }

    public void SetNumber(int number) => Number = number;
}
```

### 4.8 BattleDeckModel

```csharp
// Assets/Scripts/Game/Models/BattleDeckModel.cs
/// <summary>
/// バトルデッキモデル（3枚管理）
/// </summary>
public class BattleDeckModel
{
    public IReadOnlyList<BattleCardModel> Cards => _cards;
    private readonly List<BattleCardModel> _cards = new();
    private readonly Stack<BattleCardModel> _usedHistory = new();

    public void SetDeck(List<BattleCardModel> cards) => ...;

    /// <summary>使用可能なカードを取得</summary>
    public IReadOnlyList<BattleCardModel> GetAvailableCards()
        => _cards.Where(c => !c.IsUsed).ToList();

    /// <summary>カードを使用済みにする</summary>
    public void MarkAsUsed(BattleCardModel card)
    {
        card.IsUsed = true;
        _usedHistory.Push(card);
    }

    /// <summary>直前に使用したカードを取得（信頼スキル用）</summary>
    public BattleCardModel GetLastUsedCard()
        => _usedHistory.Count > 0 ? _usedHistory.Peek() : null;

    /// <summary>使用済みカードを未使用に戻す（信頼スキル用）</summary>
    public void RestoreUsedCard(BattleCardModel card) => card.IsUsed = false;
}
```

### 4.9 CardBattleHandler（カードバトルロジック）

```csharp
// Assets/Scripts/Game/Logic/CardBattleHandler.cs
/// <summary>
/// 3本勝負のカードバトルを管理する
/// </summary>
public class CardBattleHandler
{
    public VictoryCondition BaseCondition { get; }
    public int PlayerWins { get; private set; }
    public int EnemyWins { get; private set; }
    public int CurrentRound { get; private set; }
    public bool IsPlayerFirst { get; private set; }
    public bool IsFinished => PlayerWins >= BATTLE_WINS_REQUIRED
                           || EnemyWins >= BATTLE_WINS_REQUIRED;

    // 現ラウンドの状態
    public BattleCardModel PlayerCard { get; private set; }
    public BattleCardModel EnemyCard { get; private set; }

    // スキル使用権（バトル全体で1回のみ）
    private bool _playerSkillAvailable = true;
    private bool _enemySkillAvailable = true;

    // 勝利条件の一時反転（怒りスキル用）
    private bool _conditionReversedNextTurn;

    // カード数字割り当てマップ（タイブレーク用にオークション入札総量を保持）
    private Dictionary<CardModel, int> _auctionBidTotals;

    /// <summary>コイントスで先攻後攻を決定</summary>
    public void DecideFirstPlayer() => IsPlayerFirst = Random.value > 0.5f;

    /// <summary>カードを伏せる</summary>
    public void PlacePlayerCard(BattleCardModel card) { ... }
    public void PlaceEnemyCard(BattleCardModel card) { ... }

    /// <summary>スキル発動</summary>
    public bool TryActivatePlayerSkill(EmotionType skill,
        BattleDeckModel playerDeck, BattleDeckModel enemyDeck) { ... }
    public bool TryActivateEnemySkill(EmotionType skill,
        BattleDeckModel playerDeck, BattleDeckModel enemyDeck) { ... }

    /// <summary>
    /// カードオープンして勝敗判定
    /// 同数の場合はオークション入札リソース総量で比較（引き分けなし）
    /// </summary>
    public RoundResult ResolveRound()
    {
        var condition = _conditionReversedNextTurn
            ? ReverseCondition(BaseCondition)
            : BaseCondition;
        _conditionReversedNextTurn = false;

        // 数字が異なる場合
        if (PlayerCard.Number != EnemyCard.Number)
        {
            return condition == VictoryCondition.LowerWins
                ? (PlayerCard.Number < EnemyCard.Number ? RoundResult.PlayerWin : RoundResult.EnemyWin)
                : (PlayerCard.Number > EnemyCard.Number ? RoundResult.PlayerWin : RoundResult.EnemyWin);
        }

        // 同数の場合: オークション入札リソース総量で比較
        var playerBidTotal = _auctionBidTotals[PlayerCard.Card];
        var enemyBidTotal = _auctionBidTotals[EnemyCard.Card];
        // 入札量も同じ場合はランダム
        if (playerBidTotal == enemyBidTotal)
            return Random.value > 0.5f ? RoundResult.PlayerWin : RoundResult.EnemyWin;
        return playerBidTotal > enemyBidTotal ? RoundResult.PlayerWin : RoundResult.EnemyWin;
    }

    /// <summary>次ラウンドへ進む</summary>
    public void NextRound() { ... }
}

public enum RoundResult
{
    PlayerWin,
    EnemyWin,
}
```

### 4.10 SkillEffectApplier（スキル効果）

```csharp
// Assets/Scripts/Game/Logic/SkillEffectApplier.cs
/// <summary>
/// 感情状態に応じたスキル効果を適用する
/// </summary>
public static class SkillEffectApplier
{
    /// <summary>
    /// スキル効果を適用する
    /// 悲しみスキルの場合、targetCardForSadness に対象カードを指定する
    /// </summary>
    public static void Apply(EmotionType emotion,
        BattleCardModel playerCard, BattleCardModel enemyCard,
        BattleDeckModel playerDeck, BattleDeckModel enemyDeck,
        CardBattleHandler handler,
        BattleCardModel targetCardForSadness = null)
    {
        switch (emotion)
        {
            case EmotionType.Anger:
                // 次のターンのみ勝利条件が逆になる
                handler.ReverseConditionNextTurn();
                break;
            case EmotionType.Anticipation:
                // 自分の残りカードの数字を全てランダムに変える（被りあり）
                foreach (var card in playerDeck.GetAvailableCards())
                    card.SetNumber(Random.Range(1, 7));
                break;
            case EmotionType.Joy:
                // 自分の出したカードの数字を2倍にする
                playerCard.SetNumber(playerCard.Number * 2);
                break;
            case EmotionType.Trust:
                // 直前に使ったカードが自動的に手札に戻る
                var lastUsed = playerDeck.GetLastUsedCard();
                if (lastUsed != null)
                    playerDeck.RestoreUsedCard(lastUsed);
                break;
            case EmotionType.Fear:
                // 相手と自分のカードの数字を入れ替える
                (playerCard.Number, enemyCard.Number) =
                    (enemyCard.Number, playerCard.Number);
                break;
            case EmotionType.Surprise:
                // 自分の出したカードの数字をランダムに変える
                playerCard.SetNumber(Random.Range(1, 7));
                break;
            case EmotionType.Disgust:
                // 自分の出したカードの数字を2分の1にする（切り捨て、最低1）
                playerCard.SetNumber(Mathf.Max(1, playerCard.Number / 2));
                break;
            case EmotionType.Sadness:
                // 未使用カードから1枚を選んで数字を3に変える
                // targetCardForSadness はUI側で選択されたカード
                if (targetCardForSadness != null)
                    targetCardForSadness.SetNumber(GameConstants.DEFAULT_CARD_NUMBER);
                break;
        }
    }
}
```

### 4.11 RewardCalculator 本実装

基本報酬は **固定値1**。倍率のみ変動する設計。

```csharp
// Assets/Scripts/Game/Services/RewardCalculator.cs
public static class RewardCalculator
{
    public struct RewardResult
    {
        /// <summary>基本報酬（固定値1）</summary>
        public int BaseReward;
        /// <summary>倍率（感情マッチ or 記憶タイプ）</summary>
        public float Multiplier;
        /// <summary>最終報酬（切り上げ: 1→1, 1.5→2, 2→2）</summary>
        public int TotalReward;
        /// <summary>投入リソース</summary>
        public int BidAmount;
        /// <summary>感情マッチしたか</summary>
        public bool IsEmotionMatched;
        /// <summary>自己記憶か</summary>
        public bool IsSelfMemory;
        /// <summary>入札した感情タイプ</summary>
        public EmotionType BidEmotion;
        /// <summary>カードの感情タイプ</summary>
        public EmotionType CardEmotion;
    }

    public static RewardResult Calculate(CardModel card, BidModel playerBids)
    {
        var bidEmotion = playerBids.GetBidEmotion(card);
        var bidAmount = playerBids.GetTotalBid(card);
        var cardEmotion = card.Data.CardEmotion;
        var isSelfMemory = card.Data.MemoryType == MemoryType.SelfMemory;
        var isEmotionMatched = bidEmotion.HasValue && bidEmotion.Value == cardEmotion;

        // 基本報酬 = 固定値1
        const int baseReward = 1;

        // 倍率決定（自己記憶 > 感情マッチ > 通常）
        float multiplier;
        if (isSelfMemory)
            multiplier = GameConstants.SELF_MEMORY_MULTIPLIER;  // 2.0x → 報酬2
        else if (isEmotionMatched)
            multiplier = GameConstants.EMOTION_MATCH_MULTIPLIER; // 1.5x → 報酬2（切り上げ）
        else
            multiplier = 1.0f;                                   // 1.0x → 報酬1

        var totalReward = Mathf.CeilToInt(baseReward * multiplier);

        return new RewardResult
        {
            BaseReward = baseReward,
            Multiplier = multiplier,
            TotalReward = totalReward,
            BidAmount = bidAmount,
            IsEmotionMatched = isEmotionMatched,
            IsSelfMemory = isSelfMemory,
            BidEmotion = bidEmotion ?? default,
            CardEmotion = cardEmotion,
        };
    }
}
```

---

## 5. Prefab・ヒエラルキー変更一覧

### 5.1 新規作成するPrefab

#### DeckSelectionView.prefab（D&D方式）
**パス**: `Assets/Prefabs/NewBattleSceneView/Battle/DeckSelectionView.prefab`

```
DeckSelectionView (CanvasGroup, DeckSelectionView.cs, BasePhaseView継承)
 ├─ Background (Image, 半透明背景)
 ├─ TitleText ("デッキ選択" TextMeshProUGUI)
 ├─ VictoryConditionText ("勝利条件: 数字が小さい方が勝利")
 ├─ HandContainer (Transform, 扇形配置エリア, StaggeredSlideInGroup)
 │   └─ (DraggableCardView × N を動的生成)
 ├─ DeckSlotsContainer (StaggeredSlideInGroup)
 │   ├─ DeckSlot1 (RankingSlotView, rank=1)
 │   ├─ DeckSlot2 (RankingSlotView, rank=2)
 │   └─ DeckSlot3 (RankingSlotView, rank=3)
 ├─ DragLineView (UILineRenderer, ベジェ曲線演出)
 └─ ConfirmButton (Button, "決定")

DraggableCardView.prefab:
 DraggableCardView (CanvasGroup, DraggableCardView.cs)
 ├─ CardView (既存CardViewコンポーネント)
 └─ NumberText (TextMeshProUGUI, 数字オーバーレイ)
```

#### CardBattleView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/Battle/CardBattleView.prefab`

```
CardBattleView (CanvasGroup, CardBattleView.cs)
 ├─ Background
 ├─ RoundIndicator (ラウンド表示)
 │   ├─ Round1Marker (Image)
 │   ├─ Round2Marker
 │   └─ Round3Marker
 ├─ VictoryConditionText ("数字が小さい方が勝利")
 ├─ PlayerSide (プレイヤー側)
 │   ├─ PlayerDeckContainer (手札: BattleCardSlotView × 残りカード)
 │   ├─ PlayerFieldSlot (場に伏せたカード)
 │   └─ PlayerCharacterImage (立ち絵)
 ├─ EnemySide (敵側)
 │   ├─ EnemyFieldSlot (場に伏せたカード)
 │   └─ EnemyCharacterImage (立ち絵)
 ├─ TurnIndicator ("先攻" / "後攻" 表示)
 ├─ SkillButton (Button, スキル発動ボタン)
 │   ├─ SkillNameText
 │   └─ SkillDescriptionText
 ├─ InstructionText ("伏せるカードを選んでください" 等)
 └─ NextButton (Button, カードオープン後の進行)
```

#### BattleCardSlotView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/Battle/BattleCardSlotView.prefab`

```
BattleCardSlotView (BattleCardSlotView.cs)
 ├─ CardBack (Image, カード裏面)
 ├─ CardFront (Image, カード表面)
 │   ├─ CardImage
 │   ├─ NumberText (TextMeshProUGUI, 大きく中央に表示)
 │   └─ EmotionIcon (Image, 感情アイコン)
 └─ SelectionHighlight (Image, 選択時のハイライト)
```

#### CoinFlipView.prefab
**パス**: `Assets/Prefabs/NewBattleSceneView/Battle/CoinFlipView.prefab`

```
CoinFlipView (CanvasGroup, CoinFlipView.cs)
 ├─ CoinImage (Image, コインのスプライト)
 └─ ResultText ("先攻" / "後攻")
```

### 5.2 改修するPrefab

#### RewardPhaseView.prefab → ResultPhaseView に改名
**パス**: `Assets/Prefabs/NewBattleSceneView/ResultPhaseView.prefab`

変更:
- RewardPhaseView → ResultPhaseView にリネーム
- CardAcquisitionView に感情マッチ表示を追加
- ResourceRewardView に感情状態判定・スキル表示を追加

### 5.3 ヒエラルキーへの追加

BattleScene の `Canvas` に追加:

```
Canvas
 ├─ ... (既存のView群)
 ├─ AuctionView
 ├─ CompetitionView
 ├─ ResultPhaseView           ← 改名（旧 RewardPhaseView）
 ├─ DeckSelectionView         ← 新規追加
 ├─ CardBattleView            ← 新規追加
 ├─ CoinFlipView              ← 新規追加
 ├─ MemoryGrowthView
 └─ ...
```

---

## 6. 実装ステップ（推奨順序）

### Phase 1: 基盤変更（データモデル・定数・enum）

#### Step 1.1: GameState enum の変更
- RewardPhase → ResultPhase に改名
- DeckSelection, CardBattle を追加

#### Step 1.2: GameConstants に定数を追加
- BATTLE_ROUND_COUNT, BATTLE_WINS_REQUIRED, DECK_SIZE, DEFAULT_CARD_NUMBER
- EMOTION_MATCH_MULTIPLIER, SELF_MEMORY_MULTIPLIER

#### Step 1.3: VictoryCondition enum の新規作成

#### Step 1.4: CardData に EmotionType フィールドを追加

#### Step 1.5: AuctionData に VictoryCondition フィールドを追加

#### Step 1.6: コンパイル確認 + 既存コードの RewardPhase → ResultPhase 参照を修正

---

### Phase 2: リザルトパート改修（報酬計算の本実装）

#### Step 2.1: RewardCalculator を本実装
- 基本報酬 = 入札量
- 感情マッチ: 1.5倍
- 自己記憶: 2.0倍
- RewardResult 構造体に IsEmotionMatched, IsSelfMemory, CardEmotion 等を追加

#### Step 2.2: CardAcquisitionView に感情マッチ情報を表示
- 各カード横に「感情マッチ! x1.5」「自己記憶! x2.0」等の表示
- カードの司る感情アイコンを表示

#### Step 2.3: ResourceRewardView に感情状態判定を追加
- 報酬アニメーション後、最多感情を判定して感情状態を表示
- 付与されるスキル名と効果の表示

#### Step 2.4: BattlePresenter の HandleRewardPhase → HandleResultPhase に改名・改修
- RewardCalculator の新シグネチャに対応
- 報酬付与ロジック: 感情マッチ・記憶タイプに応じた倍率適用
- 感情状態の判定とスキル決定

#### Step 2.5: コンパイル確認 + フォーマット修正

---

### Phase 3: カード数字割り当て

#### Step 3.1: CardNumberAssigner 新規作成
- 全6枚のカードに、両者の入札合計が多い順に1〜6を割り当て

#### Step 3.2: BattleCardModel 新規作成
- CardModel をラップし、数字(Number)とバトル状態(IsUsed)を追加

#### Step 3.3: BattleDeckModel 新規作成
- 3枚のデッキ管理

#### Step 3.4: BattlePresenter に数字割り当て処理を追加
- HandleAuctionResult 後に CardNumberAssigner で数字を割り当て、BattlePresenter に保持

#### Step 3.5: コンパイル確認

---

### Phase 4: デッキ選択フェーズ

#### Step 4.1: DeckSelectionView 新規作成
- 獲得カード一覧表示（各カードに割り当て数字を表示）
- 3枚選択UI
- 枚数不足時の自動補完ロジック

#### Step 4.2: DeckSelectionView.prefab 作成 (uLoop)
- ヒエラルキー構成に従ってPrefab作成
- Canvas直下に配置

#### Step 4.3: 敵AIのデッキ選択ロジック
- Enemy クラスに `SelectDeck` メソッドを追加
- ランダム or 数字が有利なカードを優先する簡易AI

#### Step 4.4: BattlePresenter に HandleDeckSelection を追加
- プレイヤーのデッキ選択待機
- 敵AIのデッキ選択
- 不足分の自動補完

#### Step 4.5: BattleUIPresenter にデッキ選択メソッドを追加

#### Step 4.6: コンパイル確認 + フォーマット修正

---

### Phase 5: カードバトルフェーズ（コアロジック）

#### Step 5.1: CardBattleHandler 新規作成
- 3本勝負の管理
- 先攻/後攻のコイントス
- カード伏せ → スキル発動タイミング → カードオープン → 勝敗判定
- ラウンド結果の記録

#### Step 5.2: SkillEffectApplier 新規作成
- 8種スキル効果の実装
- 悲しみスキルのカード選択はUI側から指定を受ける

#### Step 5.3: 敵AIのバトルロジック
- Enemy クラスに `SelectBattleCard` メソッドを追加
- Enemy クラスに `ShouldUseSkill` メソッドを追加

#### Step 5.4: コンパイル確認

---

### Phase 6: カードバトルフェーズ（UI）

#### Step 6.1: BattleCardSlotView 新規作成
- カード表裏の表示
- 数字の大きな表示
- 感情アイコン表示
- 選択ハイライト

#### Step 6.2: CoinFlipView 新規作成
- コインフリップアニメーション
- 先攻/後攻の結果表示

#### Step 6.3: CardBattleView 新規作成
- ラウンド表示
- プレイヤー手札表示
- カード伏せ操作
- カードオープン演出
- 勝敗表示
- 進行ボタン

#### Step 6.4: SkillButtonView 新規作成
- スキル発動ボタン（1回のみ）
- スキル名と効果の表示
- 発動済み状態の表示

#### Step 6.5: RoundIndicatorView 新規作成
- 3ラウンドの勝敗状態表示

#### Step 6.6: Prefab群の作成 (uLoop)
- CardBattleView.prefab
- BattleCardSlotView.prefab
- CoinFlipView.prefab
- SkillButtonView.prefab
- RoundIndicatorView.prefab
- ヒエラルキーに配置

#### Step 6.7: BattlePresenter に HandleCardBattle を追加
- ラウンドループ
- 先攻/後攻の交互進行
- スキル発動タイミング管理
- カードオープン演出待ち
- 2本先取判定

#### Step 6.8: BattleUIPresenter にカードバトルメソッドを追加

#### Step 6.9: コンパイル確認 + フォーマット修正

---

### Phase 7: 統合・テスト・調整

#### Step 7.1: BattlePresenter の全フロー統合
- StartGame メソッドに新フェーズ群を組み込み
- フェーズ間のデータ受け渡し確認

#### Step 7.2: VContainer の登録更新
- BattleLifetimeScope に新サービスを登録

#### Step 7.3: ScriptableObject アセットの更新
- CardData アセットに EmotionType を設定
- AuctionData アセットに VictoryCondition を設定

#### Step 7.4: UI レイアウト調整・演出

#### Step 7.5: 全フロー通しテスト

---

## 7. BattlePresenter の新フロー（完成形）

```csharp
private async UniTask StartGame()
{
    await UniTask.Delay(1000);

    // ===== オークションパート =====

    // 1. テーマ公開
    _currentGameState.Value = GameState.ThemeAnnouncement;
    await HandleThemeAnnouncement();

    // 2. カード提示（6枚を場に並べる）
    _currentGameState.Value = GameState.CardReveal;
    await HandleCardReveal();

    // 3. 対話フェーズ
    _currentGameState.Value = GameState.DialoguePhase;
    await HandleDialoguePhase();

    // 4. 入札フェーズ
    _currentGameState.Value = GameState.BiddingPhase;
    await HandleBiddingPhase();

    // 5. 落札者判定（競合含む）
    _currentGameState.Value = GameState.AuctionResult;
    await HandleAuctionResult();

    // ===== リザルトパート =====

    // 6. リザルトフェーズ（獲得カード表示 + 感情リソース報酬 + 感情状態判定）
    _currentGameState.Value = GameState.ResultPhase;
    var emotionState = await HandleResultPhase();

    // ===== バトルパート =====

    // 7. カード数字割り当て
    var cardNumbers = CardNumberAssigner.AssignNumbers(
        _auctionCards, _player.Bids, _enemy.Bids);

    // 8. デッキ選択
    _currentGameState.Value = GameState.DeckSelection;
    var (playerDeck, enemyDeck) = await HandleDeckSelection(cardNumbers);

    // 9. カードバトル（3本勝負）
    _currentGameState.Value = GameState.CardBattle;
    await HandleCardBattle(playerDeck, enemyDeck, emotionState);

    // ===== 終了処理 =====

    // 10. 記憶育成フェーズ
    _currentGameState.Value = GameState.MemoryGrowth;
    await HandleMemoryGrowth();

    // 11. 終了
    _currentGameState.Value = GameState.BattleEnd;
    await HandleBattleEnd();
}
```

---

## 8. 確認事項（全て確認済み）

1. **カード数字の割り当て**: 両者の入札合計が多い順に1〜6。**同じ入札量のカードは同じ数字**になる（ランク方式）
   - 例: 入札合計 [5, 3, 3, 2, 0, 0] → 数字 [1, 2, 2, 4, 5, 5]
2. **勝利条件**: AuctionData にフィールドとして持たせる
3. **スキル効果**: 企画書の8種類をそのまま実装
4. **感情マッチの基本報酬**: **固定値（1リソース）**。倍率で1.5または2になるのみ
5. **バトル回数**: 1回のBattleScene内にオークション→バトルは **1回のみ**。デッキ使用制限は将来対応
6. **感情の強さ順（同数字時のタイブレーク）**: **そのカードのオークション入札リソース総量**で比較
7. **信頼スキル**: **直前に使ったカード**が自動的に手札に戻る（選択不要）
8. **悲しみスキル**: **未使用カード（手札にあるカード）のみ**から1枚選択して数字を3に変更
9. **敵AIのスキル使用**: **後で決める**（まずはランダム確率で仮実装）
10. **RewardPhaseView → ResultPhaseView のリネーム**: Prefabアセットの参照切れに注意
11. **バトル結果画面**: 仕様未定のためスコープ外。カードバトル後は直接 MemoryGrowth へ遷移

---

## 9. リスク・注意点

1. **CardData に EmotionType を追加**: 既存の CardData ScriptableObject アセットに新フィールドが追加されるため、全アセットの設定が必要
2. **AuctionData に VictoryCondition を追加**: 同様に全アセットの設定が必要
3. **RewardPhase → ResultPhase のリネーム**: GameState の参照箇所が広範囲に及ぶため、慎重にリファクタリング
4. **カードバトルUIの複雑さ**: 先攻後攻の交互進行、スキル発動タイミング、カードオープン演出など、UIのステートマシンが複雑
5. **スキルの相互作用**: 一部スキルは効果が矛盾する場合がある（例: 怒り+恐れの組み合わせ）
6. **敵AIの賢さ**: 単純すぎるとつまらなく、複雑すぎると実装コスト増大
7. **実装規模**: Phase 4〜6 が特に大きいため、段階的にテスト可能な形で進める

---

## 10. 作業順序チェックリスト

```
Phase 1: 基盤変更 ✅ 完了
  [x] 1.1 GameState enum 変更（ResultPhase, DeckSelection, CardBattle 追加）
  [x] 1.2 GameConstants に定数追加
  [x] 1.3 VictoryCondition enum 新規作成
  [x] 1.4 CardData に EmotionType フィールド追加
  [x] 1.5 AuctionData に VictoryCondition フィールド追加
  [x] 1.6 コンパイル確認 + RewardPhase→ResultPhase 参照修正
  [x] フォーマット修正

Phase 2: リザルトパート改修 ⚠️ 一部未完了
  [x] 2.1 RewardCalculator 本実装（感情マッチ・記憶タイプ倍率）
  [ ] 2.2 CardAcquisitionView に感情マッチ情報表示 ← 未実装
  [ ] 2.3 ResourceRewardView に感情状態判定・スキル表示追加 ← 未実装
  [x] 2.4 BattlePresenter HandleResultPhase 実装
  [x] 2.5 コンパイル確認 + フォーマット修正
  [ ] RewardPhaseView → ResultPhaseView リネーム ← 未実施

Phase 3: カード数字割り当て ✅ 完了
  [x] 3.1 CardNumberAssigner 新規作成
  [x] 3.2 BattleCardModel 新規作成
  [x] 3.3 BattleDeckModel 新規作成
  [x] 3.4 BattlePresenter に数字割り当て処理追加
  [x] 3.5 コンパイル確認

Phase 4: デッキ選択フェーズ ✅ 完了（D&D UI統合済み）
  [x] 4.1 DeckSelectionView 新規作成（D&D方式に改修済み）
  [x] 4.2 DeckSelectionView Prefab 再構築（DraggableCardView + RankingSlotView×3 + DragLineView + StaggeredSlideInGroup）
  [x] 4.3 敵AIデッキ選択ロジック（BattlePresenter.SelectEnemyDeck）
  [x] 4.4 BattlePresenter HandleDeckSelection 実装
  [x] 4.5 BattleUIPresenter にメソッド追加
  [x] 4.6 コンパイル確認 + フォーマット修正

Phase 5: カードバトルロジック ✅ 完了
  [x] 5.1 CardBattleHandler 新規作成
  [x] 5.2 SkillEffectApplier 新規作成
  [x] 5.3 敵AIバトルロジック（BattlePresenter内に仮実装: ランダム選択・50%スキル使用）
  [x] 5.4 コンパイル確認

Phase 6: カードバトルUI ✅ 完了
  [x] 6.1 BattleCardSlotView 新規作成
  [-] 6.2 CoinFlipView → 未作成（BattlePresenter内でログ出力のみ）
  [x] 6.3 CardBattleView 新規作成
  [x] 6.4 SkillButtonView 新規作成（独立Prefab）
  [-] 6.5 RoundIndicatorView → CardBattleView内に統合済み（独立Viewなし）
  [x] 6.6 Prefab群の作成（CardBattleView.prefab, BattleCardSlotView.prefab, SkillButtonView.prefab）
  [x] 6.7 BattlePresenter HandleCardBattle 実装
  [x] 6.8 BattleUIPresenter にメソッド追加
  [x] 6.9 コンパイル確認 + フォーマット修正

Phase 7: 統合・テスト ⚠️ 一部未完了
  [x] 7.1 BattlePresenter 全フロー統合（StartGame に全フェーズ組み込み済み）
  [-] 7.2 VContainer 登録更新 → 不要（新サービスは全てPresenter内で生成）
  [x] 7.3 ScriptableObject アセット更新（CardData EmotionType, AuctionData VictoryCondition 設定済み）
  [x] 7.4 Prefab 作成 & ヒエラルキー配置（全View完了）
  [ ] 7.5 全フロー通しテスト ← 未実施
```

## 11. 残作業サマリー

### スクリプト実装状況

#### ロジック・モデル: ✅ 完了
- CardNumberAssigner, BattleCardModel, BattleDeckModel, CardBattleHandler, SkillEffectApplier, RewardCalculator

#### UI View: ⚠️ 一部未完了
- DeckSelectionView ✅, CardBattleView ✅, BattleCardSlotView ✅, SkillButtonView ✅
- CardAcquisitionView — 感情マッチ表示 ❌ 未実装
- ResourceRewardView — 感情状態・スキル表示 ❌ 未実装

#### Presenter: ✅ 完了
- BattlePresenter: HandleResultPhase ✅, HandleDeckSelection ✅, HandleCardBattle ✅
- BattleUIPresenter: デッキ選択・カードバトル関連メソッド ✅

### Prefab・アセット・ヒエラルキー: ✅ 完了
- DeckSelectionView.prefab ✅（D&D方式: DraggableCardView + RankingSlotView×3 + DragLineView + StaggeredSlideInGroup）
- CardBattleView.prefab ✅（BattleCardSlotView, SkillButton, RoundIndicator, FieldSlot 等）
- BattleCardSlotView.prefab ✅（表裏切替, 数字テキスト, 感情アイコン, ハイライト）
- SkillButtonView.prefab ✅（感情アイコン, 名前, 説明テキスト）
- DraggableCardView.prefab ✅（numberText 追加済み）
- ScriptableObject: CardData EmotionType, AuctionData VictoryCondition 設定済み ✅
- BattleScene Canvas に DeckSelectionView, CardBattleView 配置済み ✅

### 未実施作業一覧

| # | 作業 | 種別 | 優先度 |
|---|------|------|--------|
| 1 | CardAcquisitionView に感情マッチ表示追加 | スクリプト | 中 |
|   | 各カード横に「感情マッチ! x1.5」「自己記憶! x2.0」等の表示 | | |
| 2 | ResourceRewardView に感情状態判定・スキル表示追加 | スクリプト | 中 |
|   | 最多感情の判定→感情状態表示→スキル名と効果の表示 | | |
| 3 | RewardPhaseView → ResultPhaseView リネーム | リファクタ | 低 |
|   | クラス名・Prefab名の改名（参照切れに注意） | | |
| 4 | **全フロー通しテスト** | テスト | 高 |
|   | オークション → リザルト → デッキ選択(D&D) → カードバトル → 記憶育成 | | |

### スコープ外（仕様未定）
- **BattleResultView**: バトル結果画面（勝利時の記憶選択・敗北時の演出）は仕様が未定のためスコープ外。現在のフローは CardBattle → MemoryGrowth に直接遷移

### 設計で省略したもの（後回し可）
- CoinFlipView: コインフリップ演出（現在はログ出力のみ）
- 敵AIの高度化（現在はランダム選択・50%スキル使用）
- HandleDialoguePhase の対話ボタン本実装（現在は TODO コメントあり）
