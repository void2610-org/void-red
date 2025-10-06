# ゲームシステム詳細

## ゲームフロー

### 状態遷移（GameState enum）
```
ThemeAnnouncement 
  ↓
PlayerCardSelection 
  ↓
EnemyCardSelection 
  ↓
Evaluation 
  ↓
ResultDisplay
  ↓ (ループ)
ThemeAnnouncement
```

**制御**: `BattlePresenter.ChangeState()` で状態遷移を管理

## スコア計算システム

### ScoreCalculator.cs (static class)
```csharp
Score = MatchRate × MentalBet × CardMultiplier

MatchRate = 1.0 + (1.0 - (Distance / √3)) × 0.5
```

**Distance計算**: プレイヤーカードとテーマの3D空間での距離（CardAttribute: Emotion, Sanity, Impulse）

### CollapseJudge.cs
カードの崩壊判定ロジック（条件を満たすとカードが崩壊）

## 進化システム

### 即時進化チェック
```csharp
// BattlePresenter.cs でのゲーム結果記録後に即時進化チェック
var playerCard = _player.RemoveSelectedCard();
var playerCardAfterEvolution = _statsTrackerService.PlayerTracker.CheckCardEvolution(playerCard);

if (playerCardAfterEvolution != playerCard)
{
    await _uiPresenter.ShowAnnouncement($"{playerCard.CardName} が {playerCardAfterEvolution.CardName} に変化しました！", 2f);
}
_player.ReturnCardToDeck(playerCardAfterEvolution);
```

### データ構造
- **EvolutionStatsData**: プレイヤー・敵共通の進化統計データ
- **PlayerSaveData**: プレイヤー固有セーブデータ（EvolutionStatsDataを含む）
- **EnemyStats**: 敵用簡略統計データ
- **IEvolutionStatsData**: 統一インターフェース

### SubclassSelector使用
```csharp
[SerializeReference, SubclassSelector]
public List<EvolutionConditionBase> conditions = new();

// 利用可能な条件タイプ:
// - PlayStyleWinCondition: プレイスタイル勝利条件
// - PlayStyleLoseCondition: プレイスタイル敗北条件
// - TotalWinCondition: 総勝利数条件
// - CollapseCountCondition: 崩壊回数条件
// - ConsecutiveWinCondition: 連続勝利条件
// - TotalUseCondition: 総使用回数条件
// - WinRateCondition: 勝率条件
```

## セーブ・ロードシステム

### SaveDataManager.cs
- JSON形式でPlayerSaveDataをシリアライズ/デシリアライズ
- Application.persistentDataPathに保存
- エラーハンドリングによる安全なセーブ/ロード

### GameProgressService.cs (統合サービス)
```csharp
// 自動ロード（コンストラクタで実行）
// セーブファイルが存在しない場合は新規データ作成

// バトル結果記録 + 自動セーブ
gameProgressService.RecordBattleResultAndSave(playerWon);

// ノベル結果記録 + 自動セーブ
gameProgressService.RecordNovelResultAndSave(choices);

// プレイヤー結果記録
gameProgressService.RecordPlayerGameResult(playerWon, playerMove, playerCollapsed);

// データリセット
gameProgressService.ResetToDefaultData();

// 現在の進行状況取得
var currentNode = gameProgressService.GetCurrentNode();
var mentalPower = gameProgressService.GetPlayerMentalPower();
```

## シーン遷移システム

### SceneTransitionManager.cs
型安全なシーン遷移とデータ受け渡しを管理

```csharp
// SceneType enum
public enum SceneType { Title, Home, Battle, Novel }

// 基本遷移データクラス
public abstract class SceneTransitionData
{
    public abstract SceneType TargetScene { get; }
    public SceneType ReturnScene { get; set; } = SceneType.Home;
}

// バトル専用遷移データ
public class BattleTransitionData : SceneTransitionData
{
    public override SceneType TargetScene => SceneType.Battle;
    public EnemyData TargetEnemy { get; set; }
}
```

### 使用例
```csharp
// 1. 遷移データ作成
var battleData = new BattleTransitionData 
{
    TargetEnemy = enemyData,
    ReturnScene = SceneType.Home
};

// 2. シーン遷移実行
await _sceneTransitionService.TransitionToScene(battleData);

// 3. 遷移先でデータ取得
var receivedData = _sceneTransitionService.GetTransitionData<BattleTransitionData>();
if (receivedData?.TargetEnemy != null) 
{
    // 受け取ったデータを使用
}

// 4. クリーンアップと戻る
_sceneTransitionService.ClearTransitionData();
await _sceneTransitionService.TransitionToScene(SceneType.Home);
```

## カードアニメーションシステム

### CardView.cs (LitMotion使用)
すべてのカードアニメーションをCardViewで管理：

```csharp
PlayDrawAnimation()         // デッキから手札へ
PlayRemoveAnimation()       // 通常削除または崩壊エフェクト
PlayArrangeAnimation()      // 手札内での配置
PlayReturnToDeckAnimation() // 手札からデッキへ
SetHighlight()              // 選択状態の表示
```

## DeckModel デュアル管理

### 二重管理構造
- **DrawPile（山札）**: ドロー用のカードスタック
- **AllCards（デッキ全体）**: 全カードの管理

### 進化時のカード置換
```csharp
// DeckModel.cs
public void ReplaceCard(CardData oldCard, CardData newCard)
{
    // AllCards内の該当カードを置換
    // DrawPile内の該当カードも置換（存在する場合）
}
```

**重要**: 手札との分離により整合性を保証

## 人格ログシステム

### PersonalityLog/ 構造
- **PersonalityLogData**: 全ログデータの統合
- **ChapterLog**: 章ごとのログ
- **TurnLog**: ターンごとのログ
- **MoveLog**: 各手のログ
- **TurnEvents**: ターン内イベント（進化・崩壊等）

### PersonalityLogService.cs
ログデータの記録と管理を担当

## 統計・進化管理

### StatsTracker.cs
- IEvolutionStatsDataインターフェースによる統一的な進化統計データ管理
- 即時進化チェック機能（`CheckCardEvolution()`）
- 進化・劣化条件の判定

### 使用例
```csharp
// ゲーム結果記録
_statsTrackerService.PlayerTracker.RecordGameResult(_playerMove, _npcMove, playerWon, playerCollapse);
_statsTrackerService.EnemyTracker.RecordGameResult(_npcMove, _playerMove, !playerWon, npcCollapse);

// カード進化チェック
var evolvedCard = _statsTrackerService.PlayerTracker.CheckCardEvolution(cardData);
```

## デバッグシステム

### DebugController.cs
- R3を使った倍速実行機能
- インスペクターから直接制御可能
- 「Enable Fast Mode」チェックボックス
- 「Time Scale」スライダー（0.1〜10倍速）
- 実行時でもリアルタイムで速度変更可能

## カードプールサービス

### CardPoolService.cs
- カードデータの一元管理
- カードIDからCardDataの取得
- 全カードデータへのアクセス

## テーマサービス

### ThemeService.cs (存在する場合)
- テーマデータの管理
- ランダムテーマ選択
- テーマ表示制御
