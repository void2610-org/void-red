# プロジェクト概要

## プロジェクト名
**void-red** - Unity製カードゲームプロジェクト

## 目的
選択式カードバトルシステムを持つストーリー主導のカードゲーム。プレイヤーはカードを選択し、敵と対戦しながらストーリーを進める。カードは使用状況に応じて進化・劣化する進化システムを実装。

## 使用Unity バージョン
Unity 6000.0.50f1

## プロジェクトURL
https://void2610.github.io/void-red/

## アーキテクチャパターン

### 整理された階層構造MVP Pattern
プロジェクトは明確に分離されたMVPアーキテクチャを採用：

```
Game/
├── Core/              → 基本データ構造（Enums, PlayerMove, RandomRangeValue等）
├── Presenters/        → プレゼンター層（PlayerPresenter, Player, Enemy）
├── Models/            → モデル層（PlayerModel, HandModel, DeckModel）
├── Services/          → サービス層（GameProgressService, SaveDataManager, CardPoolService等）
├── Logic/             → ゲームロジック（BattlePresenter, ScoreCalculator, CollapseJudge）
├── Stats/             → 統計・進化システム（EvolutionStatsData, CardStats等）
├── PersonalityLog/    → 人格ログシステム
└── Tutorial/          → チュートリアル関連

UI/
├── Main/              → メインUI（UIPresenter - バトルシーン）
├── Title/             → タイトル画面（TitleUIPresenter）
├── Home/              → ホーム画面（HomeUIPresenter）
├── Novel/             → ノベル画面（NovelUIPresenter, DialogView, ChoiceView）
├── Views/             → UI表示・アニメーション（HandView, CardView, ThemeView等）
├── Presenters/        → UI統合管理（SettingsPresenter）
└── Navigation/        → UIナビゲーション（SafeNavigationManager, MouseHoverUISelector）

VContainer/
└── LifetimeScopes     → DI設定（Root, Battle, Home, Novel, Title）

ScriptableObject/
└── データ定義         → CardData, EnemyData, ThemeData等

Utils/
└── ユーティリティクラス

Debug/
└── デバッグツール
```

## 主要な設計原則

1. **明確なファイル構造**: 機能別ディレクトリで保守性向上
2. **責任の分離**: Stats, Services, Logic, Coreの明確な分離
3. **統合プレゼンター**: PlayerPresenterがカード関連を一元管理
4. **Unity-First**: MonoBehaviourパターンの自然な活用
5. **リアクティブ設計**: R3による状態変更の伝播
6. **YAGNI & KISS**: 必要最小限のシンプルな実装
7. **進化システム**: SubclassSelectorによる条件設定の簡素化

## シーン構成

- **TitleScene**: エントリーポイント
- **HomeScene**: ハブシーン（各シーンへの遷移）
- **BattleScene**: メインゲーム（バトル実行）
- **NovelScene**: ストーリーシーン

シーン遷移はSceneTransitionManagerで型安全に管理。

## VContainer DIパターン

### RootLifetimeScope (クロスシーン共有)
- SaveDataManager, SceneTransitionManager, GameProgressService
- CardPoolService, SettingsManager, ConfirmationDialogService
- SteamService, DiscordService
- BgmManager, SeManager

### BattleLifetimeScope (バトル専用)
- Player, Enemy インスタンス
- UIPresenter, HandView, CardView等
- BattlePresenter (エントリーポイント)

### HomeLifetimeScope / NovelLifetimeScope
- 各シーン専用のUIPresenter
