# チュートリアルバトル実装：プレイヤー操作制約

> 関連ドキュメント: [敵AI差し替え](./tutorial-battle-enemy-ai.md)

## 概要

チュートリアルバトル（敵ID "alv"）で、プレイヤーが行える操作をフェーズ毎に制限する。
**Template Method パターン**を採用し、`BattlePresenter` を継承した `TutorialBattlePresenter` に
全チュートリアル制御を集中させる。`BattlePresenter` 本体は `EnemyId` チェックを一切持たない。

### アーキテクチャ

```
IEnemyAIController
├── EnemyAIController         (既存: ランダム)
└── TutorialEnemyAIController (既存: スクリプト通り)

BattlePresenter               (基底: protected virtual フェーズメソッド、EnemyId チェックなし)
└── TutorialBattlePresenter   (新規: 全チュートリアル制御がここに集中)

BattleLifetimeScope           (チュートリアル判定を集約、条件付き DI 登録)
```

---

## 制約の全体像

| Step | フェーズ | 操作 | 制約内容 |
|------|---------|------|--------|
| 1 | 対話 | カードクリック | 指定カード1枚のみクリック可 |
| 2a | 入札：感情選択 | 車輪UIで感情切り替え | 指定感情が選ばれたときのみ Step 2b へ進む |
| 2b | 入札：カード選択 | カードクリック→入札ウィンドウ | 指定カード1枚のみクリック可 |
| 2c | 入札：ベット | +ボタン | 必要量に達したら増加不可 |
| 2d | 入札：確定 | 確定ボタン | 必要量達成後のみ有効 |
| 2e | 競合フェーズ | レイズボタン・感情選択 | 入札引き分けを意図的に発生させ、指定感情で指定回数レイズさせる |
| 3 | デッキ選択 | カードD&D | 指定カードのみドラッグ可 |
| 3s | スキル（デッキ選択中） | スキルボタン | 指定スキルを使わせる |
| 4 | カードバトル：カード伏せ | カードD&D | ラウンド毎に指定カードのみドラッグ可 |
| 4s | スキル（ラウンド中） | スキルボタン | 指定ラウンドで指定スキルを使わせる |
| - | コインフリップ | （なし） | ラウンド毎に結果を固定 |

---

## 決定論的フロー（チュートリアル全体シーケンス）

### バトル全体シーケンス

```
[対話フェーズ] HandleDialoguePhase()
  → 指定カード1枚のみクリック可
  → OnBiddingComplete 相当のイベントで完了

[入札フェーズ] HandleBiddingPhase()
  → 以下のサブステップを逐次実行

  Step 2a: 感情選択
    - 車輪UIは操作可能（どの感情も回転できる）
    - 強制感情（Joy）が選ばれたときのみ Step 2b へ進む
    - カードクリック・ベットボタン・確定ボタン は無効

  Step 2b: カード選択
    - 指定カード1枚のみクリック可 → BidWindowView 表示
    - ベットボタン・確定ボタン は無効
    - 感情選択は引き続き無効（forcedEmotion 固定済み）

  Step 2c: ベット
    - 増加ボタンのみ有効
    - 減少ボタンは無効（後退なし）
    - 確定ボタンは無効
    - 必要量（BID_REQUIRED_AMOUNT）に達したら Step 2d へ

  Step 2d: 確定
    - 確定ボタンのみ有効 → OnBiddingComplete 発火

  Step 2e: 競合フェーズ（AuctionProcessor が引き分けを検出して自動移行）
    - チュートリアルAIが同一カードに同額入札するよう調整
    - 感情選択 → 指定感情（Joy）のみ受け付ける
    - レイズボタン → 指定回数（COMPETITION_REQUIRED_RAISES）に達したらプレイヤー勝利で終了

[デッキ選択] HandleDeckSelection()
  → 指定カード（DECK_ALLOWED_CARD_INDICES）のみドラッグ可
  → スキルボタンを表示（Anticipationスキルを体験させる）
  → スキルボタン押下 → Anticipationスキル発動のみ受け付ける

[カードバトル] HandleCardBattle()（ラウンド毎）
  → コインフリップ固定（COIN_FLIP_PER_ROUND）
  → 指定カード（FORCED_CARD_PER_ROUND）のみドラッグ可
  → 指定ラウンドでスキルボタンを表示し、指定スキル（Joy）の使用を促す
```

### 各フェーズの逐次ステップ管理

#### Step 1：対話フェーズ
```
TutorialBattlePresenter.HandleBiddingPhase() 開始前に
  → StartTutorial("BiddingPhase") でノベル演出
  → 指定カード（index=0）のみ対話クリック可
    ↓ 指定カードをクリック
  → OnDialogueRequested 発火 → 対話ノベル開始
```

#### Step 2a–2d：入札フェーズ（RunTutorialBiddingAsync）

```
StartAuctionBidding(cards, bids, Joy, resources)
  → SetAuctionAllCardsInteractable(false)
  → SetAuctionConfirmInteractable(false)
  → OnAuctionEmotionSelected を待機
    ↓ Joy が選択される
  → SetAuctionCardInteractable(BID_FORCED_CARD_INDEX, true)
  → OnAuctionCardClicked を待機（index=0 のカードのみ）
    ↓ 指定カードをクリック → BidWindowView 表示
  → OnAuctionBidIncreased を待機（GetTotalBidAmount() >= BID_REQUIRED_AMOUNT）
    ↓ 必要量に達する
  → SetAuctionBidIncreaseInteractable(false)
  → SetAuctionConfirmInteractable(true)
  → OnAuctionBiddingConfirmed を待機
    ↓ 確定ボタンを押す
```

#### Step 2e：競合フェーズ（TutorialCompetitionPhaseRunner）

チュートリアルAIが同額入札することで `AuctionProcessor` が引き分けを検出し自動移行する。

```
TutorialCompetitionPhaseRunner.RunAsync()
  → CompetitionHandler.Start()
  → ShowCompetition() でUI表示
  → OnCompetitionRaise を購読
    ↓ 1回目レイズ（Joy で消費）→ UpdateCompetitionBids()
    ↓ 2回目レイズ（COMPETITION_REQUIRED_RAISES に達する）
  → handler.End() → HideCompetition()
  → プレイヤー勝利でカード獲得
```

#### Step 3：デッキ選択フェーズ

```
InitializeDeckSelection(allowedCardIndices: DECK_ALLOWED_CARD_INDICES)
  → 指定インデックス以外のカードは CanvasGroup.blocksRaycasts = false

スキルボタン（BattlePresenter 基底の GetSkillRound で制御）
  → GetSkillRound() == null のため DeckSelection では通常フロー
  → （デッキ選択中スキルは通常バトルと同じ挙動）
```

#### Step 4：カードバトル（ラウンド毎）

```
ラウンド開始
  → GetForcedCoinFlip(round) → handler.SetFirstPlayer(COIN_FLIP_PER_ROUND[round])

[SKILL_ROUND_INDEX ラウンドのみ]
  → GetSkillRound() == round → スキルボタン表示
    ↓ スキルが発動される
  → スキルボタン非表示

  → GetForcedBattleCard(round) → ShowPlayerHand(forcedCardIndex)
    → 指定カード以外は CanvasGroup.blocksRaycasts = false
      ↓ 指定カードをドラッグ → 確定
```

---

## 変更ファイル一覧（11ファイル）

### 新規作成

#### 1. `Assets/Scripts/Game/Tutorial/TutorialBattlePlayerData.cs`

プレイヤー側チュートリアル制約を管理する専用クラス。
`TutorialEnemyAIController` が敵スクリプトを管理するのと対称的な構造。
static クラスではなく sealed クラス（将来的にDI可能）。

```csharp
public sealed class TutorialBattlePlayerData
{
    // 入札フェーズ
    public const int BID_FORCED_CARD_INDEX = 0;
    public const EmotionType BID_FORCED_EMOTION = EmotionType.Joy;
    public const int BID_REQUIRED_AMOUNT = 3;

    // 競合フェーズ（オークション）
    public const int COMPETITION_REQUIRED_RAISES = 2;
    public const EmotionType COMPETITION_FORCED_EMOTION = EmotionType.Joy;

    // デッキ選択
    public static readonly int[] DECK_ALLOWED_CARD_INDICES = { 0, 1, 2 };

    // カードバトル
    public static readonly bool[] COIN_FLIP_PER_ROUND = { true, false, true };
    public static readonly int?[] FORCED_CARD_PER_ROUND = { 0, null, null };
    public const int SKILL_ROUND_INDEX = 1;
    public const EmotionType FORCED_SKILL_EMOTION = EmotionType.Joy;
}
```

#### 2. `Assets/Scripts/Game/Logic/TutorialBattlePresenter.cs`

`BattlePresenter` を継承し、全チュートリアル制御を担う。
`TutorialCompetitionPhaseRunner` も同ファイルに定義する。

**オーバーライドするメソッド一覧:**
- `HandleThemeAnnouncement()` → `base` 後に `StartTutorial("BeforeThemeAnnouncement")`
- `HandleBiddingPhase()` → `StartTutorial("BiddingPhase")` 後に `RunTutorialBiddingAsync()` を呼ぶ
- `HandleAuctionResult()` → `StartTutorial("ResultDetermination")` 後に `base`
- `OnAfterCardsDisplayed()` → `StartTutorial("RewardPhase")`
- `OnAfterResourceGaugesDisplayed()` → `StartTutorial("RewardPhase2")`
- `OnBeforeMemoryGrowthContinueAsync()` → `StartTutorial("MemoryGrowthPhase")`
- `GetAllowedDeckCardIndices()` → `TutorialBattlePlayerData.DECK_ALLOWED_CARD_INDICES`
- `GetForcedCoinFlip(int round)` → `TutorialBattlePlayerData.COIN_FLIP_PER_ROUND[round]`
- `GetForcedBattleCard(int round)` → `TutorialBattlePlayerData.FORCED_CARD_PER_ROUND[round]`
- `GetSkillRound()` → `TutorialBattlePlayerData.SKILL_ROUND_INDEX`

**`RunTutorialBiddingAsync()` の実装:**
```csharp
private async UniTask RunTutorialBiddingAsync()
{
    // Step 2a: Joy が選ばれるまで待機（全カード・確定ボタンは無効化）
    _battleUIPresenter.StartAuctionBidding(_auctionCards, _player.Bids, EmotionType.Joy, _player.EmotionResources);
    _battleUIPresenter.SetAuctionAllCardsInteractable(false);
    _battleUIPresenter.SetAuctionConfirmInteractable(false);

    await _battleUIPresenter.OnAuctionEmotionSelected
        .Where(e => e == TutorialBattlePlayerData.BID_FORCED_EMOTION)
        .FirstAsync();

    // Step 2b: 指定カードのみ有効化
    _battleUIPresenter.SetAuctionCardInteractable(TutorialBattlePlayerData.BID_FORCED_CARD_INDEX, true);
    await _battleUIPresenter.OnAuctionCardClicked
        .Where(idx => idx == TutorialBattlePlayerData.BID_FORCED_CARD_INDEX)
        .FirstAsync();

    // Step 2c: 必要量ベット後に増加ボタン無効化
    await _battleUIPresenter.OnAuctionBidIncreased
        .Where(_ => _player.Bids.GetTotalBidAmount() >= TutorialBattlePlayerData.BID_REQUIRED_AMOUNT)
        .FirstAsync();
    _battleUIPresenter.SetAuctionBidIncreaseInteractable(false);

    // Step 2d: 確定ボタン有効化 → 押下待機
    _battleUIPresenter.SetAuctionConfirmInteractable(true);
    await _battleUIPresenter.OnAuctionBiddingConfirmed.FirstAsync();
}
```

---

### 変更

#### 3. `Assets/Scripts/Game/Logic/BattlePresenter.cs`

**フィールド (private → protected):**
- `_battleUIPresenter`, `_player`, `_enemy`, `_enemyAI`, `_auctionCards`
- `_competitionPhaseRunner`, `_auctionProcessor`（readonly 除去）

**フェーズメソッド (private → protected virtual):**
- `HandleThemeAnnouncement`, `HandleBiddingPhase`

**新規 protected virtual メソッド追加:**
```csharp
// HandleAuctionResult: AuctionResultを1つのメソッドでラップ
protected virtual async UniTask HandleAuctionResult()
    => await _auctionProcessor.ProcessAuctionResultAsync(_auctionCards, _currentEnemyData, _currentGameState);

// RewardPhase中間フック
protected virtual UniTask OnAfterCardsDisplayed() => UniTask.CompletedTask;
protected virtual UniTask OnAfterResourceGaugesDisplayed() => UniTask.CompletedTask;

// MemoryGrowthPhase中間フック
protected virtual UniTask OnBeforeMemoryGrowthContinueAsync() => UniTask.CompletedTask;

// DeckSelection制限（デフォルトは制限なし）
protected virtual int[] GetAllowedDeckCardIndices() => null;

// CardBattle制限（デフォルトはすべて null = 制限なし）
protected virtual bool? GetForcedCoinFlip(int round) => null;
protected virtual int? GetForcedBattleCard(int round) => null;
protected virtual int? GetSkillRound() => null;
```

**StartGame() の AuctionResult 呼び出し変更:**
```csharp
// Before
await _auctionProcessor.ProcessAuctionResultAsync(_auctionCards, _currentEnemyData, _currentGameState);
// After
await HandleAuctionResult();
```

**HandleDeckSelection() の変更:**
```csharp
_battleUIPresenter.InitializeDeckSelection(playerWonBattleCards, GetAllowedDeckCardIndices());
```

**HandleCardBattle() のラウンドループ変更:**
```csharp
// コイントス（強制指定あれば使用）
var forcedFlip = GetForcedCoinFlip(handler.CurrentRound);
if (forcedFlip.HasValue) handler.SetFirstPlayer(forcedFlip.Value);
else handler.DecideFirstPlayer();

// スキルボタン（スキルラウンド指定があれば指定ラウンドのみ、なければ通常判定）
var skillRound = GetSkillRound();
var showSkillButton = skillRound.HasValue
    ? skillRound.Value == handler.CurrentRound
    : handler.PlayerSkillAvailable;
_battleUIPresenter.SetSkillButtonVisible(showSkillButton);
_battleUIPresenter.SetSkillButtonInteractable(showSkillButton);

// カード伏せ（強制カードインデックスを渡す）
var forcedCard = GetForcedBattleCard(handler.CurrentRound);
await PlayerPlaceCard(handler, playerDeck, playerSkillSession, forcedCard);
```

**HandleResultPhase() の変更:**
```csharp
await _battleUIPresenter.DisplayCardsAsync(rewardResults);
await OnAfterCardsDisplayed();

await _battleUIPresenter.WaitForCardAcquisitionCompleteAsync();
_battleUIPresenter.DisplayResourceGauges(_player.EmotionResources, maxResources);
await OnAfterResourceGaugesDisplayed();
```

**HandleMemoryGrowth() の変更:**
```csharp
_battleUIPresenter.ShowMemoryGrowthView(allThemes);
_battleUIPresenter.HideRewardView();
await OnBeforeMemoryGrowthContinueAsync();
await _battleUIPresenter.WaitForMemoryGrowthCompleteAsync();
```

**削除対象（全4箇所の `EnemyId == "alv"` チェック）:**
- `HandleThemeAnnouncement` 内
- `HandleBiddingPhase` 内
- `HandleResultPhase` 内 (isTutorial ローカル変数)
- `HandleMemoryGrowth` 内

**コンストラクタの変更:**
- `isTutorial` 判定ロジックを削除
- `_enemyAI = new EnemyAIController(_enemy)` に固定

#### 4. `Assets/Scripts/UI/Auction/AuctionView.cs`

チュートリアルステートマシンは**一切追加しない**。制御メソッドのみ追加：
```csharp
public Observable<EmotionType> OnEmotionSelected => emotionResourceDisplayView.OnEmotionSelected;
public Observable<int> OnCardClickedByIndex
    => Observable.Merge(_auctionCardViews.Select((c, i) => c.OnCardClicked.Select(_ => i)));
public void SetAllCardsInteractable(bool value)
    { foreach (var c in _auctionCardViews) c.SetInteractable(value); }
public void SetCardInteractable(int index, bool value) => _auctionCardViews[index].SetInteractable(value);
public void SetConfirmButtonInteractable(bool value) => confirmBiddingButton.interactable = value;
public Observable<Unit> OnBidWindowIncrease => bidWindowView.OnIncrease;
public void SetBidWindowIncreaseInteractable(bool value) => bidWindowView.SetIncreaseInteractable(value);
```

`AuctionCardView` に `SetInteractable(bool)` を追加（`CanvasGroup.blocksRaycasts` で対応）。

#### 5. `Assets/Scripts/UI/Auction/BidWindowView.cs`

```csharp
public void SetIncreaseInteractable(bool value) => increaseButton.interactable = value;
```

#### 6. `Assets/Scripts/UI/Main/BattleUIPresenter.cs`

AuctionView のカプセル化を維持するため、TutorialBattlePresenter が使う委譲メソッドを追加：
```csharp
public void StartAuctionBidding(IReadOnlyList<CardModel> cards, BidModel bids, EmotionType emotion, IReadOnlyDictionary<EmotionType, int> resources)
    => _auctionView.StartBidding(cards, bids, emotion, resources);
public void SetAuctionCardInteractable(int index, bool value) => _auctionView.SetCardInteractable(index, value);
public void SetAuctionAllCardsInteractable(bool value) => _auctionView.SetAllCardsInteractable(value);
public void SetAuctionConfirmInteractable(bool value) => _auctionView.SetConfirmButtonInteractable(value);
public void SetAuctionBidIncreaseInteractable(bool value) => _auctionView.SetBidWindowIncreaseInteractable(value);
public Observable<EmotionType> OnAuctionEmotionSelected => _auctionView.OnEmotionSelected;
public Observable<int> OnAuctionCardClicked => _auctionView.OnCardClickedByIndex;
public Observable<Unit> OnAuctionBidIncreased => _auctionView.OnBidWindowIncrease;
public Observable<Unit> OnAuctionBiddingConfirmed => _auctionView.OnBiddingComplete;

// ShowPlayerHand にオプショナルパラメータを追加
public void ShowPlayerHand(IReadOnlyList<CardModel> availableCards, int? forcedCardIndex = null) =>
    _cardBattleView.ShowPlayerHand(availableCards, forcedCardIndex);

// InitializeDeckSelection にオプショナルパラメータを追加
public void InitializeDeckSelection(IReadOnlyList<CardModel> wonCards, int[] allowedCardIndices = null) =>
    _deckSelectionView.Initialize(wonCards, allowedCardIndices);
```

#### 7. `Assets/Scripts/UI/Battle/CardBattleView.cs`

```csharp
public void ShowPlayerHand(IReadOnlyList<CardModel> availableCards, int? forcedCardIndex = null)
// forcedCardIndex が指定された場合、対象以外のカードは CanvasGroup.blocksRaycasts = false
```

#### 8. `Assets/Scripts/UI/Battle/DeckSelectionView.cs`

```csharp
public void Initialize(IReadOnlyList<CardModel> wonCards, int[] allowedCardIndices = null)
// allowedCardIndices が指定された場合、対象外カードは CanvasGroup.blocksRaycasts = false
```

#### 9. `Assets/Scripts/Game/Logic/CardBattleHandler.cs`

```csharp
public void SetFirstPlayer(bool isPlayerFirst) => IsPlayerFirst = isPlayerFirst;
```

#### 10. `Assets/Scripts/Game/Logic/AuctionProcessor.cs`

`EnemyId == "alv"` チェック（38-39行目）を削除。
`StartTutorial("ResultDetermination")` の呼び出しは `TutorialBattlePresenter.HandleAuctionResult()` へ移動。

#### 11. `Assets/Scripts/VContainer/BattleLifetimeScope.cs`

チュートリアル判定をここに集約し、条件付き登録：
```csharp
var gameProgressService = Parent.Container.Resolve<GameProgressService>();
var currentNode = gameProgressService.GetCurrentNode();
var isTutorial = currentNode is BattleNode bn
    && allAuctionData.GetAuctionById(bn.AuctionId).Enemy.EnemyId == "alv";

if (isTutorial)
    builder.RegisterEntryPoint<TutorialBattlePresenter>().As<BattlePresenter>().As<ISceneInitializable>();
else
    builder.RegisterEntryPoint<BattlePresenter>().AsSelf().As<ISceneInitializable>();
```

---

## 旧実装との比較

| 問題点 | 旧実装 | 本プラン |
|-------|--------|---------|
| BattlePresenter の汚染 | `if (_isTutorialBattle)` 4箇所 | 0箇所（EnemyId チェックなし） |
| AuctionView の汚染 | TutorialBiddingStep enum + フィールド追加 | 制御メソッドのみ（ステートマシンなし） |
| AuctionProcessor の汚染 | `_isTutorial` フラグ + RunTutorialCompetitionAsync | 0箇所 |
| チュートリアルロジックの所在 | BattlePresenter, AuctionProcessor, AuctionView に分散 | TutorialBattlePresenter に完全集中 |
| 既存パターンとの一貫性 | なし | IEnemyAIController と対称 |

---

## 実装時確認事項

- `TutorialBattlePlayerData` の具体的な数値はゲームデザインに合わせて調整
- `TutorialBattlePresenter` のコンストラクタで `_enemyAI`, `_competitionPhaseRunner`, `_auctionProcessor` を上書きする
- `CompetitionPhaseRunner` を non-sealed にして `RunAsync` を virtual にすること
- `BattleLifetimeScope` が `TutorialBattlePresenter` を `BattlePresenter` として登録できるか確認

---

## 検証手順

1. `mcp__uLoopMCP__compile` (ForceRecompile=false) でコンパイル
2. `mcp__uLoopMCP__get-logs` (LogType=Error) でエラーなし確認
3. フォーマット修正コマンドを実行
4. "alv" 敵バトルを繰り返し実行し、以下を確認：
   - 入札フェーズ開始直後：全カード・確定ボタンが無効であることを確認
   - Joy を選択するまでカード選択ができないことを確認
   - 別の感情を選択しても何も変わらないことを確認
   - 指定カードをクリックして BidWindowView が表示されることを確認
   - ベット増加ボタンを必要量まで押した後、確定ボタンが有効になることを確認
   - 確定後に通常のバトルフローに戻ることを確認
   - デッキ選択：指定カードのみドラッグ可
   - カードバトル：コインフリップ固定、指定カードのみドラッグ可
   - デッキ選択中：Anticipation スキルボタンが表示され、押すと発動・非表示になること
   - SKILL_ROUND_INDEX ラウンド開始時：Joy スキルボタンが表示され、押すと発動・非表示になること
   - オークション結果：チュートリアルAIが引き分けを起こし競合フェーズへ移行すること
   - 競合フェーズ：指定回数レイズするとプレイヤー勝利でカードを獲得すること
   - 毎回同一の展開になること
5. 通常バトルも動作確認（回帰テスト）
