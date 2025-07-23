# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. Respond in Japanese and avoid excessive comments. When refactoring, implement clean replacements rather than maintaining backward compatibility.

## Unity Project Overview

void-red is a Unity card game project using Unity 6000.0.50f1 with VContainer for dependency injection, R3 for reactive programming, LitMotion for animations, UniTask for async operations, and Unity-SerializeReferenceExtensions for simplified editor customization.

## Core Commands

```bash
# Compile and check errors
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .

# Check compilation errors only
./unity-tools/unity-compile.sh check .
```

## Architecture: 整理された階層構造MVP Pattern

The project uses a well-organized MVP pattern with clear separation of concerns:

```
Game/
├── Core/              → 基本データ構造
│   ├── Enums.cs       → GameState, PlayStyle, CardAttribute
│   ├── PlayerMove.cs  → プレイヤーの手
│   └── RandomRangeValue.cs → ランダム値管理
├── Presenters/        → プレゼンター層（統合制御）
│   ├── PlayerPresenter.cs → カード管理 + UI制御 + ゲームロジック統合
│   ├── Player.cs      → プレイヤー実装
│   └── Enemy.cs       → 敵実装
├── Models/            → モデル層（データ管理）
│   ├── PlayerModel.cs → 精神力などプレイヤー属性
│   ├── HandModel.cs   → 手札データ (R3 Reactive)
│   └── DeckModel.cs   → デッキデータ（DrawPile + AllCards）
├── Services/          → サービス層
│   ├── CardPoolService.cs → カードプール管理
│   ├── ThemeService.cs → テーマデータ管理
│   ├── StatsTrackerService.cs → プレイヤー・敵の統計管理
│   └── CardViewFactory.cs → カードビューファクトリ
├── Logic/             → ゲームロジック
│   ├── GameManager.cs → ゲーム進行制御 (IStartable)
│   ├── ScoreCalculator.cs → スコア計算ロジック (static)
│   └── CollapseJudge.cs → カード崩壊判定ロジック (static)
└── Stats/             → 統計・進化システム
    ├── IEvolutionStatsData.cs → 進化統計データ共通インターフェース
    ├── EvolutionStatsData.cs → プレイヤー・敵共通の進化統計データ
    ├── PlayerSaveData.cs → プレイヤー固有のセーブデータ
    ├── EnemyStats.cs  → 敵用簡略統計データ
    ├── StatsTracker.cs → 統計データ管理 + 即時進化チェック
    ├── CardStats.cs   → カード統計データ
    ├── EvolutionConditions.cs → 進化条件（SubclassSelector使用）
    └── EvolutionConditionGroup.cs → 進化条件グループ

UI/
├── Main/              → メインUI
│   └── UIPresenter.cs → UI Views統合管理 + イベント処理
├── Title/             → タイトル画面
└── Views/             → UI表示・アニメーション
    ├── HandView.cs    → 手札表示 + カードアニメーション
    ├── CardView.cs    → 個別カード表示 + アニメーション
    └── その他Views     → Theme, Announcement, PlayButton等

Debug/
└── DebugController.cs → デバッグ機能（倍速実行等）
```

### 責任分離の詳細

**PlayerPresenter (統合プレゼンター)**
- HandModel, DeckModel, PlayerModelの直接管理
- HandViewとの連携によるUI制御
- カード操作ロジック（ドロー、プレイ、選択）
- 精神力管理との統合
- 即時進化チェック機能の統合

**StatsTrackerService (統計管理サービス)**
- PlayerSaveData（プレイヤー固有）とEnemyStats（敵用簡略）の管理
- PlayerTracker, EnemyTrackerの提供
- 敵統計リセット機能（ResetEnemyStats）
- 将来のセーブ機能に向けたPlayerSaveDataアクセス

**StatsTracker (統計・進化管理)**
- IEvolutionStatsDataインターフェースによる統一的な進化統計データ管理
- 即時進化チェック機能（CheckCardEvolution）
- 進化・劣化条件の判定

**新しいデータ構造**
- **EvolutionStatsData**: プレイヤー・敵共通の進化統計データ（進化機能用）
- **PlayerSaveData**: プレイヤー固有データ（進化統計 + 将来の章クリア状況等）
- **EnemyStats**: 敵用簡略統計（進化に必要な最小限のみ）
- **IEvolutionStatsData**: 進化統計データの統一インターフェース

**DeckModel (デッキ管理)**
- DrawPile（山札）とAllCards（デッキ全体）の二重管理
- 進化時のカード置換機能
- 手札との分離による整合性保証

**進化システム (SubclassSelector活用)**
- EvolutionConditionBase抽象クラスの継承構造
- SubclassSelectorによる直感的な条件設定
- 複雑なカスタムエディタの簡素化

**UI系 (UIPresenter + Views)**
- UIPresenterが各Viewを統合管理
- R3 Observableによるイベント通信
- MonoBehaviourパターンの活用

**デバッグシステム (DebugController)**
- R3を使った効率的な倍速実行機能
- インスペクターからの直接制御
- 開発効率の向上

### Key Design Principles
- **明確なファイル構造**: 機能別ディレクトリで保守性向上
- **独立した進化システム**: プレイヤーと敵が個別に進化
- **統合プレゼンター**: PlayerPresenterがカード関連を一元管理
- **責任の明確化**: Stats, Services, Logic, Coreの分離
- **Unity-First**: MonoBehaviourパターンの自然な活用
- **リアクティブ設計**: R3による状態変更の伝播
- **エディタ拡張の活用**: SubclassSelectorによる開発効率化

## Critical Implementation Details

### VContainer Setup
```csharp
// MainLifetimeScope.cs pattern
builder.RegisterInstance(player);
builder.RegisterInstance(enemy);
builder.Register<CardPoolService>(Lifetime.Singleton);
builder.Register<ThemeService>(Lifetime.Singleton);
builder.Register<StatsTrackerService>(Lifetime.Singleton);
builder.RegisterEntryPoint<GameManager>();
builder.RegisterComponentInHierarchy<UIPresenter>();
```

### 進化システムの重要実装

#### 即時進化チェック
```csharp
// GameManager.cs での即時進化（統計記録後）
var playerWon = playerScore > npcScore;
_statsTrackerService.PlayerTracker.RecordGameResult(_playerMove, _npcMove, playerWon, playerCollapse);
_statsTrackerService.EnemyTracker.RecordGameResult(_npcMove, _playerMove, !playerWon, npcCollapse);

// カードプレイ時の即時進化
var playerCard = _player.RemoveSelectedCard();
var playerCardAfterEvolution = _statsTrackerService.PlayerTracker.CheckCardEvolution(playerCard);
if (playerCardAfterEvolution != playerCard)
{
    await _uiPresenter.ShowAnnouncement($"プレイヤーの {playerCard.CardName} が {playerCardAfterEvolution.CardName} に変化しました！", 2f);
}
_player.ReturnCardToDeck(playerCardAfterEvolution);
```

#### SubclassSelector使用例
```csharp
// EvolutionConditionGroup.cs
[SerializeReference, SubclassSelector]
public List<EvolutionConditionBase> conditions = new();

// 利用可能な条件タイプ（自動表示）:
// - PlayStyleWinCondition: プレイスタイル勝利条件
// - PlayStyleLoseCondition: プレイスタイル敗北条件  
// - TotalWinCondition: 総勝利数条件
// - CollapseCountCondition: 崩壊回数条件
// - ConsecutiveWinCondition: 連続勝利条件
// - TotalUseCondition: 総使用回数条件
// - WinRateCondition: 勝率条件
```

### Game Flow State Machine
GameManager uses GameState enum: ThemeAnnouncement → PlayerCardSelection → EnemyCardSelection → Evaluation → ResultDisplay

### Card Animation System
CardView contains all animations using LitMotion:
- PlayDrawAnimation: Deck to hand
- PlayRemoveAnimation: Normal removal or collapse effect
- PlayArrangeAnimation: Hand positioning
- PlayReturnToDeckAnimation: Hand to deck
- SetHighlight: Selection state

### Reactive Patterns
```csharp
// R3 pattern for card selection
public ReadOnlyReactiveProperty<CardView> SelectedCard => handView?.SelectedCard;

// Event subscription
handView.OnCardSelected.Subscribe(OnCardSelected).AddTo(this);
```

### Score Calculation
```csharp
Score = MatchRate × MentalBet × CardMultiplier
MatchRate = 1.0 + (1.0 - (Distance / √3)) × 0.5
```

## Unity-Specific Guidelines

### Null Checking
```csharp
// ❌ Avoid for Unity objects (causes Rider warnings)
if (cardButton != null)

// ✅ Correct Unity pattern
if (cardButton)
if (!_cardData) return;
```

### Inspector Dependencies
Don't null-check SerializeField components that should be set in Inspector. Let NullReferenceException indicate configuration errors.

### Async Operations
Use UniTask for all async operations. Prefer `.Forget()` for fire-and-forget operations.

## Scene Structure

- **TitleScene**: Entry point with TitleMenu
- **MainScene**: Game scene with Player, Enemy, UIPresenter components

## Dependencies

Key packages from manifest.json:
- VContainer (hadashiA/VContainer) - 依存性注入
- R3 (Cysharp/R3) - リアクティブプログラミング
- UniTask (Cysharp/UniTask) - 非同期処理
- LitMotion (AnnulusGames/LitMotion) - アニメーション
- Unity Template (void2610/my-unity-template) - プロジェクトテンプレート
- Unity-SerializeReferenceExtensions (mackysoft/Unity-SerializeReferenceExtensions) - SubclassSelector

## Development Workflow

1. Make code changes
2. Unity auto-compiles on focus
3. Use unity-compile.sh to verify compilation
4. Test in Unity Editor
5. Build for WebGL deployment

## Common Patterns

### Adding New UI Components
1. Create View class extending MonoBehaviour
2. Add to UIPresenter with [SerializeField]
3. Implement display logic in View
4. Control from UIPresenter

### Modifying Game Flow
1. Update GameState enum if needed
2. Modify state transitions in GameManager.ChangeState()
3. Handle new states appropriately

### 進化条件の追加
1. EvolutionConditionBase から継承した新しい条件クラスを作成
2. `IsSatisfied`, `GetDescription`, `GetConditionTypeName` メソッドを実装
3. SubclassSelectorが自動的にInspectorで利用可能に

### デバッグ機能の使用
1. 任意のGameObjectにDebugControllerコンポーネントを追加
2. インスペクターで「Enable Fast Mode」にチェック
3. 「Time Scale」スライダーで0.1〜10倍速を調整
4. 実行時でもリアルタイムで速度変更可能

### 統計データのアクセス
```csharp
// StatsTrackerServiceを注入
public GameManager(StatsTrackerService statsTrackerService)

// プレイヤーの統計データ取得
var playerStats = statsTrackerService.PlayerTracker.GetCardStats(cardData);
var canEvolve = statsTrackerService.PlayerTracker.CanCardEvolve(cardData);

// 敵の統計データ取得  
var enemyStats = statsTrackerService.EnemyTracker.GetCardStats(cardData);
```

## Coding Guidelines

### Class Member Declaration Order
Follow this order when declaring class members (based on CardView.cs):

1. **SerializeField** - Inspector設定可能なフィールド（[Header]でグループ化）
2. **public プロパティ** - 外部アクセス可能なプロパティ
3. **定数** - const, static readonly等の定数定義
4. **private フィールド** - 内部状態管理用フィールド
5. **public メソッド** - 外部から呼び出されるメソッド（Initialize, Play~Animation, Set~等）
6. **private メソッド** - 内部処理用メソッド（UpdateDisplay, OnCardClicked等）
7. **Unity イベント関数** - Awake, Start, Update等（呼び出し順序で記述）
8. **クリーンアップ関数** - OnDestroy, Dispose等

```csharp
public class ExampleView : MonoBehaviour
{
    // 1. SerializeField
    [Header("UIコンポーネント")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    
    // 2. public プロパティ
    public bool IsEnabled { get; private set; }
    public Observable<Unit> OnClicked => _onClicked;
    
    // 3. 定数
    private const float ANIMATION_DURATION = 0.5f;
    private static readonly Vector3 DEFAULT_SCALE = Vector3.one;
    
    // 4. private フィールド
    private readonly Subject<Unit> _onClicked = new();
    private bool _isInitialized;
    
    // 5. public メソッド
    public void Initialize() { }
    public void SetEnabled(bool enabled) { }
    
    // 6. private メソッド
    private void UpdateDisplay() { }
    private void OnButtonClicked() { }
    
    // 7. Unity イベント関数
    private void Awake() { }
    private void Start() { }
    
    // 8. クリーンアップ関数
    private void OnDestroy() { }
}
```