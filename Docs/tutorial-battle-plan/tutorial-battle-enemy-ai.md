# チュートリアルバトル実装：敵AI差し替え

> 関連ドキュメント: [プレイヤー操作制約](./tutorial-battle-player-restrictions.md)

## 概要

敵AIを `IEnemyAIController` インターフェース経由で差し替え可能にし、
チュートリアル用の `TutorialEnemyAIController` を注入することで、
敵の全判断（入札・デッキ選択・カード配置・スキル・競合）を固定する。

---

## 新規作成ファイル（2ファイル）

### 1. `Assets/Scripts/Game/Logic/IEnemyAIController.cs`

既存 `EnemyAIController` の全メソッドをインターフェースとして定義：

```csharp
public interface IEnemyAIController
{
    List<CardModel> SelectDeck(List<CardModel> availableCards);
    void DecideBids(IReadOnlyList<CardModel> auctionCards);
    void TryCompetitionRaise(CompetitionHandler handler);
    void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck);
    EmotionType DecideEmotionState();
    bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState);
}
```

---

### 2. `Assets/Scripts/Game/Logic/TutorialEnemyAIController.cs`

チュートリアル用スクリプト済み AI。全ランダム要素を固定値で置き換える：

```csharp
/// <summary>
/// alvチュートリアル専用の敵AI。全判断をスクリプト通りに行う。
/// </summary>
public class TutorialEnemyAIController : IEnemyAIController
{
    private readonly Enemy _enemy;
    private int _roundIndex;

    // ===== チュートリアルスクリプト =====

    // 入札: (カードインデックス, 感情, 量)
    private static readonly (int cardIdx, EmotionType emotion, int amount)[] EnemyBids =
    { (1, EmotionType.Fear, 2) };

    // デッキ選択: enemy.Cardsリスト内のインデックス
    private static readonly int[] EnemyDeckCardIndices = { 0, 1, 2 };

    // バトル感情（ラウンド毎）
    private static readonly EmotionType[] EnemyEmotionPerRound =
    { EmotionType.Fear, EmotionType.Joy, EmotionType.Sadness };

    // カード配置（ラウンド毎、enemyDeck.GetAvailableCards()内のインデックス）
    private static readonly int[] EnemyCardIndexPerRound = { 0, 1, 2 };

    // スキル発動（ラウンド毎）
    private static readonly bool[] EnemySkillPerRound = { false, true, false };

    // 競合上乗せ: 上乗せを行うか
    private const bool EnemyCompetitionDoRaise = true;

    // =================================

    public TutorialEnemyAIController(Enemy enemy) => _enemy = enemy;

    public List<CardModel> SelectDeck(List<CardModel> availableCards)
        => EnemyDeckCardIndices.Select(i => availableCards[i]).ToList();

    public void DecideBids(IReadOnlyList<CardModel> auctionCards)
    {
        foreach (var (cardIdx, emotion, amount) in EnemyBids)
            _enemy.Bids.SetBid(auctionCards[cardIdx], emotion, amount);
    }

    public EmotionType DecideEmotionState()
        => EnemyEmotionPerRound[_roundIndex];

    public void PlaceCard(CardBattleHandler handler, BattleDeckModel enemyDeck)
    {
        var cards = enemyDeck.GetAvailableCards();
        handler.PlaceEnemyCard(cards[EnemyCardIndexPerRound[_roundIndex]]);
    }

    public bool TryActivateSkill(CardBattleHandler handler, BattleDeckModel enemyDeck, EmotionType emotionState)
    {
        var result = EnemySkillPerRound[_roundIndex];
        _roundIndex++;  // スキル判定がラウンド最後の呼び出し
        return result;
    }

    public void TryCompetitionRaise(CompetitionHandler handler)
    {
        if (!EnemyCompetitionDoRaise) return;
        // スクリプト通りの感情・量で上乗せ（実装時に CompetitionHandler の API に合わせる）
        handler.TryEnemyRaise(EmotionType.Fear, _enemy);
    }
}
```

---

## 変更ファイル（3ファイル）

### 3. `Assets/Scripts/Game/Logic/EnemyAIController.cs`

`IEnemyAIController` を実装するように変更（既存コードはそのまま）：

```csharp
public class EnemyAIController : IEnemyAIController
{
    // 既存コードはそのまま（変更なし）
}
```

---

### 4. `Assets/Scripts/VContainer/BattleLifetimeScope.cs`

`IEnemyAIController` を DI コンテナに登録。チュートリアル判定はここで行う：

```csharp
// Enemy インスタンス生成後に追加
var isTutorial = _currentEnemyData.EnemyId == "alv";  // EnemyData参照方法は実装時確認
if (isTutorial)
    builder.RegisterInstance<IEnemyAIController>(new TutorialEnemyAIController(_enemy));
else
    builder.Register<EnemyAIController>(Lifetime.Singleton).As<IEnemyAIController>();
```

※ BattleLifetimeScope が `_currentEnemyData` にアクセスする方法は実装時に確認する
  （`GameProgressService` 経由か、SceneTransitionData 経由か）

---

### 5. `Assets/Scripts/Game/Logic/CompetitionPhaseRunner.cs`

`EnemyAIController` の直接参照を `IEnemyAIController` に変更（フィールド型とコンストラクタ引数のみ）。
ロジックは変更なし。

---

## BattlePresenter への影響

`EnemyAIController` を手動生成していた箇所を DI 注入に変更する：

```csharp
// フィールド変更
// Before: private readonly EnemyAIController _enemyAI;
// After:
private readonly IEnemyAIController _enemyAI;

// コンストラクタ変更
// Before: _enemyAI = new EnemyAIController(_enemy);
// After: コンストラクタ引数に IEnemyAIController _enemyAI を追加（VContainerが注入）
```

---

## 実装時確認事項

- `BattleLifetimeScope` で `_currentEnemyData` にアクセスする方法（GameProgressService または SceneTransitionData）
- `CompetitionPhaseRunner` のコンストラクタ引数（`IEnemyAIController` を受け取るか確認）
- `TutorialEnemyAIController.TryActivateSkill` でのラウンドカウンタのインクリメントタイミング（`TryActivateSkill` が毎ラウンド必ず呼ばれるか確認）
- `CompetitionHandler.TryEnemyRaise` の正確なシグネチャ
- スクリプト内の具体的な数値はゲームデザインに合わせて調整
