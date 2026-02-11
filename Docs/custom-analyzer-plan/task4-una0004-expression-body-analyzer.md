# タスク4: UNA0004 — 単一文 public メソッド式本体強制アナライザー

## 背景

単一行の public メソッドは `=>` 式本体を使用するプロジェクト規約がある。
editorconfig の `csharp_style_expression_bodied_methods` では public/private の区別や行数の条件を設定できないため、カスタムアナライザーで実装する。

## 診断ルール定義

| ID | 説明 | メッセージ | カテゴリ | 重大度 |
|----|------|---------|---------|--------|
| UNA0004 | 単一文のpublicメソッドは式本体を使用すべき | `"publicメソッド '{0}' は式本体(=>)を使用してください"` | Style | Warning |

## 検出ロジック

### 検出条件（すべて満たす場合に警告）

1. アクセス修飾子が `public`
2. ブロック本体 `{ }` を使用している
3. ステートメントが **1つのみ**
4. コンストラクタ・デストラクタ・演算子でない

### 検出除外

| パターン | 理由 |
|---------|------|
| `private` / `protected` / `internal` メソッド | 規約対象外 |
| ステートメントが2つ以上 | 複数行は式本体に適さない |
| 既に式本体 `=>` を使用 | 既に準拠 |
| コンストラクタ | 規約対象外 |
| デストラクタ | 規約対象外 |
| 演算子オーバーロード | 規約対象外 |

### 具体例

```csharp
// UNA0004 検出 ← public + ブロック本体 + 1ステートメント
public int GetHealth()
{
    return _health;
}

// 検出なし ← 既に式本体
public int GetHealth() => _health;

// 検出なし ← ステートメントが2つ
public void Initialize()
{
    _health = 100;
    _isAlive = true;
}

// 検出なし ← private
private int GetHealthInternal()
{
    return _health;
}

// 検出なし ← コンストラクタ
public Player()
{
    _health = 100;
}
```

## 変更対象・新規ファイル

### 1. 新規作成: `tools/UnityNamingAnalyzer/ExpressionBodyAnalyzer.cs`

```csharp
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityNamingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ExpressionBodyAnalyzer : DiagnosticAnalyzer
    {
        // 単一文のpublicメソッドが式本体を使用していない場合の警告
        public static readonly DiagnosticDescriptor UNA0004 = new DiagnosticDescriptor(
            "UNA0004",
            "単一文のpublicメソッドは式本体(=>)を使用してください",
            "publicメソッド '{0}' は式本体(=>)を使用してください",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(UNA0004);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // 式本体を既に使用している場合はスキップ
            if (method.ExpressionBody != null)
                return;

            // ブロック本体がない場合はスキップ（抽象メソッドなど）
            if (method.Body == null)
                return;

            // publicメソッドのみ対象
            if (!method.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            // ステートメントが1つのみの場合に警告
            if (method.Body.Statements.Count != 1)
                return;

            context.ReportDiagnostic(
                Diagnostic.Create(UNA0004, method.Identifier.GetLocation(), method.Identifier.Text));
        }
    }
}
```

**技術ポイント**: `SyntaxNodeAction` を使用し `MethodDeclarationSyntax` を解析。
`RegisterSymbolAction` ではなく構文ノードレベルで解析する理由は、`Body.Statements.Count` など構文情報にアクセスする必要があるため。

### 2. 新規作成: `tools/UnityNamingAnalyzer.Tests/ExpressionBodyAnalyzerTests.cs`

テストケース一覧:

| テスト | 入力 | 期待結果 |
|--------|------|---------|
| `PublicBlockBodyOneStatement_UNA0004` | `public int Get() { return 1; }` | UNA0004 検出 |
| `PublicVoidBlockBodyOneStatement_UNA0004` | `public void Do() { DoSomething(); }` | UNA0004 検出 |
| `PublicExpressionBody_NoDiagnostic` | `public int Get() => 1;` | 検出なし |
| `PublicBlockBodyTwoStatements_NoDiagnostic` | `public void Do() { a(); b(); }` | 検出なし |
| `PrivateBlockBodyOneStatement_NoDiagnostic` | `private int Get() { return 1; }` | 検出なし |
| `ProtectedBlockBodyOneStatement_NoDiagnostic` | `protected int Get() { return 1; }` | 検出なし |
| `Constructor_NoDiagnostic` | `public TestClass() { _x = 1; }` | 検出なし |

### 3. 編集: `.editorconfig`

```ini
# 追加
dotnet_diagnostic.UNA0004.severity = warning

# タスク2の変更を元に戻す（UNA0004に委任）
csharp_style_expression_bodied_methods = false:silent
```

### 4. 編集: `~/.claude/agents/unity-code-quality-checker.md`

ルール4（メソッド形式）を削除:
```
4. **メソッド形式**:
   - 1行のシンプルなpublicメソッドは=>式本体を使用する必要がある
   - =>に簡略化できるpublicメソッドにフラグを立てる
   - privateメソッドは通常のブロック形式を使用する
```

## タスク2 との関係

```
タスク2: false:silent → when_on_single_line:suggestion  (一時的に方向性を一致)
タスク4: when_on_single_line:suggestion → false:silent   (UNA0004に完全委任)
```

タスク4 でカスタムアナライザーが完成するため、editorconfig の設定は `false:silent` に戻す。
UNA0004 は editorconfig より正確な制御（public のみ、ステートメント数判定）が可能。

## 検証

```bash
# アナライザービルド
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release

# テスト実行
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release

# 既存コードでフォーマットチェック
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```

### 既存コードへの影響

既存コードで単一ステートメントの public メソッドがブロック本体を使用している場合、UNA0004 が検出される。
該当箇所は式本体 `=>` に修正する。
