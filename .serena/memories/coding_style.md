# コーディングスタイルと規約

## 言語設定
- **コメント**: すべて日本語で記述
- **変数名・メソッド名**: 英語（命名規則に従う）

## 命名規則

### PascalCase
- クラス名: `public class TestClass{}`
- メソッド名: `private void TestMethod()`
- public/protectedフィールド: `protected int TestField = 0;`
- Enum: `public enum TestEnum{}`

### camelCase
- [SerializeField]フィールド: `[SerializeField] private int testField = 0;`
- ローカル変数: `var sumNumber = firstNumber + secondNumber;`
- 仮引数: `private void Sum(int firstNumber, int secondNumber)`

### _camelCase (アンダースコア付き)
- プライベートフィールド: `private int _testField = 0;`
- プライベート読み取り専用フィールド: `private readonly string _testString = "test";`

### UPPER_SNAKE_CASE
- 定数: `private const int TEST_CONSTANT = 0;`

### IPascalCase
- インターフェース: `public interface ITestInterface(){}`

## クラスメンバー宣言順序

CardView.csを参考にした推奨順序：

1. **SerializeField** - Inspector設定可能なフィールド（[Header]でグループ化）
2. **public プロパティ** - 外部アクセス可能なプロパティ
3. **定数** - const, static readonly等
4. **private フィールド** - 内部状態管理用
5. **public メソッド** - 外部から呼び出されるメソッド（Initialize, Play~, Set~等）
6. **private メソッド** - 内部処理用メソッド
7. **Unity イベント関数** - Awake, Start, Update等（呼び出し順序で記述）
8. **クリーンアップ関数** - OnDestroy, Dispose等

## Unity固有パターン

### Nullチェック
```csharp
// ❌ 避けるべき（Riderで警告）
if (cardButton != null)

// ✅ 推奨（Unityオブジェクト用）
if (cardButton)
if (!_cardData) return;
```

### SerializeFieldの扱い
- Inspector設定されるべきコンポーネントはnullチェック不要
- 設定忘れはNullReferenceExceptionで検出

### 非同期処理
```csharp
// UniTaskを使用
await SomeAsyncOperation();
SomeAsyncOperation().Forget(); // fire-and-forget
await UniTask.WhenAll(task1, task2, task3); // 並列処理
```

### Reactiveパターン (R3)
```csharp
// ReadOnlyReactiveProperty
public ReadOnlyReactiveProperty<CardView> SelectedCard => handView?.SelectedCard;

// イベント購読
handView.OnCardSelected.Subscribe(OnCardSelected).AddTo(this);
```

### アニメーション (LitMotion)
```csharp
// カードアニメーション例
LMotion.Create(startPos, endPos, duration)
    .WithEase(Ease.OutCubic)
    .BindToPosition(transform);
```

### VContainer DI
```csharp
// コンストラクタインジェクション
public class GameManager
{
    private readonly GameProgressService _gameProgressService;
    
    public GameManager(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }
}
```

## パフォーマンスガイドライン

### メモリ管理
- `new`でのオブジェクト生成を最小限に
- `string`連結は`StringBuilder`か`string interpolation`を使用
- `foreach`の代わりに`for`ループを使用（ガベージコレクション回避）

### 並列処理
```csharp
// UniTask.WhenAllで並列実行
await UniTask.WhenAll(
    DrawCardAsync(),
    UpdateUIAsync(),
    PlaySoundAsync()
);
```

## 設計哲学

### YAGNI (You Aren't Gonna Need It)
- 将来必要になるかもしれない機能を先回りして実装しない
- 現在の要求を満たす最小限の実装に留める
- 実際に必要になった時点で機能を追加

### KISS (Keep It Simple, Stupid)
- 可能な限りシンプルで理解しやすいコードを書く
- 複雑な設計パターンは本当に必要な場合のみ使用
- 1つのクラス・メソッドには1つの責任のみ

### 実装例
```csharp
// ❌ YAGNI/KISS違反：過度な抽象化
public interface ICardEffectProcessor
{
    void ProcessEffect(CardData card, IEffectContext context);
}
public class ComplexCardEffectSystem : ICardEffectProcessor { ... }

// ✅ YAGNI/KISS準拠：シンプルな実装
public void ApplyCardEffect(CardData card)
{
    if (card.HasDamageEffect)
    {
        enemy.TakeDamage(card.DamageAmount);
    }
}
```

## 禁止事項
- SerializeFieldコンポーネントの過度なnullチェック
- 不要なコメント（コードが自己説明的な場合）
- リファクタリング時の後方互換性維持（クリーンな置き換えを優先）
- 絵文字の使用（ユーザーが明示的に要求しない限り）
