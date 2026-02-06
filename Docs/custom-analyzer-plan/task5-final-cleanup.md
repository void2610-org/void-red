# タスク5: サブエージェント最終整理 + ドキュメント更新

## 背景

タスク1〜4 完了後、サブエージェントのルール定義を最終整理し、ドキュメントを更新する。

## サブエージェント最終状態（4ルール）

| # | ルール | アナライザー化しない理由 |
|---|--------|------------------------|
| 1 | クラスメンバー宣言順序 | 10カテゴリの順序判定はROIが低い |
| 2 | エラーハンドリングポリシー | セマンティック判断が必要で静的解析不可 |
| 3 | コメント規約（日本語） | 言語検出が困難 |
| 4 | LitMotion 使用規約 | 「簡単なアニメーション」の判断が主観的 |

## 変更内容

### 1. `~/.claude/agents/unity-code-quality-checker.md` — 最終版に整理

以下のルールを削除済みであることを確認し、ルール番号を1〜4に振り直す:

- ~~ルール3: アクセス修飾子~~ (タスク1で削除)
- ~~ルール4: メソッド形式~~ (タスク4で削除)
- ~~ルール6: 型推論/var使用~~ (タスク1で削除)
- ~~ルール7: イベントシステム~~ (タスク3で削除)

最終的なサブエージェント定義:

```markdown
**強制すべき重要原則**:

1. **エラーハンドリング方針（最重要）**:
   - 開発者の設定ミスに対するnullチェックを絶対に許可しない
   - コードは設定エラー時に即座にクラッシュすべき
   - 防御的なnullチェックを重大な違反として報告する

2. **コメント規約**:
   - すべてのコメントは日本語である必要がある
   - 自明で冗長コメントにフラグを立てる

3. **LitMotion**:
   - 単純なアニメーションは直接LitMotionを使用せず、LitMotionExtensions.csの拡張メソッドを使用
   - キャンセル時はif文ではなくTryCancelメソッドを使用
```

### 2. `Docs/format-check.md` — UNA0003/UNA0004 のドキュメント追加

#### 診断ルールテーブルに追加

```markdown
| ID | 説明 | 対象 |
|----|------|------|
| **UNA0001** | privateフィールドには `_` プレフィックスが必要 | `[SerializeField]` なしの private フィールド |
| **UNA0002** | `[SerializeField]` フィールドには `_` プレフィックスを付けない | `[SerializeField]` 付きの private フィールド |
| **UNA0003** | C#のevent/Action/Funcではなく、R3のSubject/Observableを使用 | event宣言、Action/Func型のフィールド・プロパティ |
| **UNA0004** | 単一文のpublicメソッドは式本体(=>)を使用 | ブロック本体を持つ単一文のpublicメソッド |
```

#### アーキテクチャ図の更新

`(UNA0001/UNA0002)` → `(UNA0001-UNA0004)` に変更。

#### ファイル構成の更新

```
tools/
├── UnityNamingAnalyzer/
│   ├── UnityNamingAnalyzer.csproj
│   ├── SerializeFieldNamingAnalyzer.cs   # UNA0001/UNA0002
│   ├── EventSystemAnalyzer.cs            # UNA0003 (NEW)
│   └── ExpressionBodyAnalyzer.cs         # UNA0004 (NEW)
└── UnityNamingAnalyzer.Tests/
    ├── UnityNamingAnalyzer.Tests.csproj
    ├── SerializeFieldNamingAnalyzerTests.cs
    ├── EventSystemAnalyzerTests.cs       # (NEW)
    └── ExpressionBodyAnalyzerTests.cs    # (NEW)
```

#### 命名規約の理由セクションに追記

**イベントシステム (UNA0003)**:
```csharp
// NG: C#のevent/Action/Func
public event Action OnDeath;
private Action<int> _onDamage;

// OK: R3のSubject/Observable
private readonly Subject<Unit> _onDeath = new();
public Observable<Unit> OnDeath => _onDeath;
```

**式本体 (UNA0004)**:
```csharp
// NG: ブロック本体（publicメソッド、1ステートメント）
public int GetHealth()
{
    return _health;
}

// OK: 式本体
public int GetHealth() => _health;

// OK: privateメソッドはブロック本体でよい
private int GetHealthInternal()
{
    return _health;
}
```

#### `.editorconfig` 設定例の追記

```ini
# カスタムアナライザー
dotnet_diagnostic.UNA0001.severity = warning
dotnet_diagnostic.UNA0002.severity = warning
dotnet_diagnostic.UNA0003.severity = warning
dotnet_diagnostic.UNA0004.severity = warning
```

## 検証

- ドキュメントが正しくフォーマットされていること（手動確認）
- サブエージェントのルール定義が4項目になっていること
- 全テストが通ること
- 既存コードのフォーマットチェックが通ること

```bash
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```
