# よく使われるパターンとベストプラクティス

## VContainer DIパターン

### コンストラクタインジェクション
```csharp
public class GameManager
{
    private readonly GameProgressService _gameProgressService;
    private readonly CardPoolService _cardPoolService;
    
    // VContainerが自動的に依存を解決
    public GameManager(
        GameProgressService gameProgressService,
        CardPoolService cardPoolService)
    {
        _gameProgressService = gameProgressService;
        _cardPoolService = cardPoolService;
    }
}
```

### LifetimeScopeでの登録
```csharp
// RootLifetimeScope.cs - クロスシーン共有サービス
protected override void Configure(IContainerBuilder builder)
{
    builder.Register<SaveDataManager>(Lifetime.Singleton);
    builder.Register<SceneTransitionManager>(Lifetime.Singleton);
    builder.Register<GameProgressService>(Lifetime.Singleton);
}

// BattleLifetimeScope.cs - バトル専用
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterInstance(player);
    builder.RegisterInstance(enemy);
    builder.RegisterEntryPoint<BattlePresenter>();
    builder.RegisterComponentInHierarchy<UIPresenter>();
}
```

### LifetimeScope種類
- **Singleton**: アプリケーション全体で1インスタンス
- **Transient**: 要求ごとに新規インスタンス
- **Scoped**: Lifetime scope単位で1インスタンス

## R3 Reactiveパターン

### ReactiveProperty
```csharp
// モデル層
private readonly ReactiveProperty<int> _mentalPower = new(100);
public ReadOnlyReactiveProperty<int> MentalPower => _mentalPower;

// 値の更新
_mentalPower.Value = newValue;

// 購読（View層）
model.MentalPower
    .Subscribe(value => UpdateDisplay(value))
    .AddTo(this); // MonoBehaviourのライフサイクルに紐付け
```

### Subject（イベント通知）
```csharp
// イベント定義
private readonly Subject<Unit> _onCardSelected = new();
public Observable<Unit> OnCardSelected => _onCardSelected;

// イベント発火
_onCardSelected.OnNext(Unit.Default);

// イベント購読
handView.OnCardSelected
    .Subscribe(_ => HandleCardSelection())
    .AddTo(this);
```

### Observable チェーン
```csharp
// 複数の変換を連結
selectedCard
    .Where(card => card != null)
    .Select(card => card.CardData)
    .Subscribe(cardData => ProcessCard(cardData))
    .AddTo(this);
```

## UniTask 非同期パターン

### async/await基本
```csharp
public async UniTask DrawCardsAsync(int count)
{
    for (int i = 0; i < count; i++)
    {
        var card = _deckModel.DrawCard();
        await _handView.PlayDrawAnimation(card);
    }
}
```

### fire-and-forget
```csharp
// 結果を待たない非同期処理
PlaySoundAsync().Forget();
```

### 並列処理
```csharp
// 複数の非同期処理を並列実行
await UniTask.WhenAll(
    DrawCardAsync(),
    UpdateUIAsync(),
    PlaySoundAsync()
);
```

### 遅延処理
```csharp
// 指定秒数待機
await UniTask.Delay(TimeSpan.FromSeconds(1.0f));

// 1フレーム待機
await UniTask.Yield();

// 次のフレーム待機
await UniTask.NextFrame();
```

## LitMotion アニメーションパターン

### 基本的なトゥイーン
```csharp
LMotion.Create(startValue, endValue, duration)
    .WithEase(Ease.OutCubic)
    .BindToPosition(transform);
```

### カスタムアニメーション
```csharp
public async UniTask PlayDrawAnimation(Vector3 startPos, Vector3 endPos)
{
    await LMotion.Create(startPos, endPos, DRAW_DURATION)
        .WithEase(Ease.OutBack)
        .WithOnComplete(() => OnAnimationComplete())
        .BindToPosition(transform)
        .ToUniTask();
}
```

### 複数プロパティの同時アニメーション
```csharp
// 位置とスケールを同時にアニメーション
var positionMotion = LMotion.Create(startPos, endPos, duration)
    .BindToPosition(transform);
    
var scaleMotion = LMotion.Create(Vector3.one * 0.5f, Vector3.one, duration)
    .BindToLocalScale(transform);

await UniTask.WhenAll(
    positionMotion.ToUniTask(),
    scaleMotion.ToUniTask()
);
```

## UIコンポーネント追加パターン

### 1. View クラス作成
```csharp
public class NewView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    
    private readonly Subject<Unit> _onButtonClicked = new();
    public Observable<Unit> OnButtonClicked => _onButtonClicked;
    
    public void Initialize()
    {
        button.onClick.AddListener(() => _onButtonClicked.OnNext(Unit.Default));
    }
    
    public void UpdateDisplay(string content)
    {
        text.text = content;
    }
}
```

### 2. UIPresenter に登録
```csharp
public class UIPresenter : MonoBehaviour
{
    [SerializeField] private NewView newView;
    
    private void Start()
    {
        newView.Initialize();
        newView.OnButtonClicked
            .Subscribe(_ => HandleButtonClick())
            .AddTo(this);
    }
}
```

## シーン遷移実装パターン

### 新しいシーンタイプの追加

#### 1. SceneType.cs に追加
```csharp
public enum SceneType 
{ 
    Title, 
    Home, 
    Battle, 
    Novel,
    NewScene  // 新規追加
}

public static class SceneTypeExtensions
{
    private static readonly Dictionary<SceneType, string> SceneNames = new()
    {
        { SceneType.Title, "TitleScene" },
        { SceneType.Home, "HomeScene" },
        { SceneType.Battle, "BattleScene" },
        { SceneType.Novel, "NovelScene" },
        { SceneType.NewScene, "NewSceneName" }  // 新規追加
    };
}
```

#### 2. 遷移データクラス作成（必要な場合）
```csharp
public class NewSceneTransitionData : SceneTransitionData
{
    public override SceneType TargetScene => SceneType.NewScene;
    public SomeData CustomData { get; set; }
}
```

#### 3. LifetimeScope 作成
```csharp
public class NewSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<NewSceneUIPresenter>();
        // その他の依存関係登録
    }
}
```

#### 4. UIPresenter 実装
```csharp
public class NewSceneUIPresenter : MonoBehaviour
{
    private readonly SceneTransitionManager _sceneTransitionManager;
    
    // VContainerによるDI
    public NewSceneUIPresenter(SceneTransitionManager sceneTransitionManager)
    {
        _sceneTransitionManager = sceneTransitionManager;
    }
    
    private void Start()
    {
        var data = _sceneTransitionManager.GetTransitionData<NewSceneTransitionData>();
        // データを使用して初期化
    }
}
```

## 進化条件追加パターン

### 新しい進化条件の実装
```csharp
[Serializable]
public class NewEvolutionCondition : EvolutionConditionBase
{
    [SerializeField] private int threshold;
    
    public override bool IsSatisfied(IEvolutionStatsData stats, CardData card)
    {
        // 条件チェックロジック
        return stats.SomeValue >= threshold;
    }
    
    public override string GetDescription()
    {
        return $"条件: {threshold}以上";
    }
    
    public override string GetConditionTypeName()
    {
        return "新しい条件";
    }
}
```

**SubclassSelectorが自動的にInspectorで利用可能にする**

## デバッグパターン

### デバッグコントローラー使用
```csharp
// 任意のGameObjectにDebugControllerコンポーネントを追加
// インスペクターで設定:
// - Enable Fast Mode: チェック
// - Time Scale: 1.0〜10.0で調整

// 実行時でもリアルタイムで速度変更可能
```

### ログ出力パターン
```csharp
// Debug.Logの使用（開発時のみ）
#if UNITY_EDITOR
Debug.Log($"Card evolved: {oldCard.CardName} → {newCard.CardName}");
#endif
```

## パフォーマンス最適化パターン

### メモリアロケーション削減
```csharp
// ❌ 避けるべき
foreach (var item in collection)
{
    // ガベージコレクション発生
}

// ✅ 推奨
for (int i = 0; i < collection.Count; i++)
{
    var item = collection[i];
}
```

### string 連結最適化
```csharp
// ❌ 避けるべき
string result = str1 + str2 + str3;

// ✅ 推奨（少量）
string result = $"{str1}{str2}{str3}";

// ✅ 推奨（大量）
var sb = new StringBuilder();
sb.Append(str1);
sb.Append(str2);
sb.Append(str3);
string result = sb.ToString();
```

## エラーハンドリングパターン

### Unity固有のnullチェック
```csharp
// ✅ Unity推奨
if (!component) return;
if (gameObject) DoSomething();

// ❌ 避けるべき（Rider警告）
if (component != null) return;
if (gameObject == null) DoSomething();
```

### SerializeField の扱い
```csharp
// Inspector設定前提のコンポーネント
[SerializeField] private Button submitButton;

// nullチェック不要 - 設定忘れはNullReferenceExceptionで検出
private void Start()
{
    submitButton.onClick.AddListener(OnSubmit);
}
```

### 安全なセーブ/ロード
```csharp
try
{
    var data = SaveDataManager.Load();
    if (data == null)
    {
        // デフォルトデータで初期化
        data = CreateDefaultData();
    }
}
catch (Exception e)
{
    Debug.LogError($"Load failed: {e.Message}");
    // フォールバック処理
    data = CreateDefaultData();
}
```
