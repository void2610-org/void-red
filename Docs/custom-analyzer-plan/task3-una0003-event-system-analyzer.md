# タスク3: UNA0003 — R3 イベントシステム強制アナライザー

## 背景

プロジェクトでは R3 の `Subject`/`Observable` を使用し、C# の `event`/`Action`/`Func` をフィールド/プロパティとして使用することを禁止している。
現在はサブエージェントのルール7でチェックしているが、Roslyn アナライザーで自動化できる。

## 診断ルール定義

| ID | 説明 | メッセージ | カテゴリ | 重大度 |
|----|------|---------|---------|--------|
| UNA0003 | C#イベント/デリゲートフィールドの使用禁止 | `"'{0}' にはC#のevent/Action/Funcではなく、R3のSubject/Observableを使用してください"` | Style | Warning |

## 検出ロジック

### 検出対象

| パターン | 例 |
|---------|-----|
| `event` キーワード付きフィールド | `public event Action OnDeath;` |
| `Action` 型フィールド | `private Action _callback;` |
| `Action<T>` 型フィールド | `private Action<int> _onDamage;` |
| `Func<T>` 型フィールド | `private Func<bool> _isReady;` |
| `Action`/`Func` 型プロパティ | `public Action OnClick { get; set; }` |

### 検出除外

| パターン | 理由 |
|---------|------|
| メソッドパラメータの `Action`/`Func` | コールバック引数は許可 |
| ローカル変数の `Action`/`Func` | ローカルスコープでの使用は許可 |

## 変更対象・新規ファイル

### 1. 新規作成: `tools/UnityNamingAnalyzer/EventSystemAnalyzer.cs`

```csharp
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityNamingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EventSystemAnalyzer : DiagnosticAnalyzer
    {
        // R3のSubject/Observableの代わりにC#のevent/Action/Funcを使用している場合の警告
        public static readonly DiagnosticDescriptor UNA0003 = new DiagnosticDescriptor(
            "UNA0003",
            "C#のevent/Action/Funcではなく、R3のSubject/Observableを使用してください",
            "'{0}' にはC#のevent/Action/Funcではなく、R3のSubject/Observableを使用してください",
            "Style",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(UNA0003);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        // eventキーワード付きフィールドを検出
        private static void AnalyzeEvent(SymbolAnalysisContext context)
        {
            var eventSymbol = (IEventSymbol)context.Symbol;
            context.ReportDiagnostic(
                Diagnostic.Create(UNA0003, eventSymbol.Locations[0], eventSymbol.Name));
        }

        // Action/Func型のフィールドを検出
        private static void AnalyzeField(SymbolAnalysisContext context)
        {
            var field = (IFieldSymbol)context.Symbol;

            // コンパイラ生成フィールドは除外（eventのバッキングフィールド等）
            if (field.IsImplicitlyDeclared)
                return;

            if (IsActionOrFuncType(field.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(UNA0003, field.Locations[0], field.Name));
            }
        }

        // Action/Func型のプロパティを検出
        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var property = (IPropertySymbol)context.Symbol;

            // インデクサーは除外
            if (property.IsIndexer)
                return;

            if (IsActionOrFuncType(property.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(UNA0003, property.Locations[0], property.Name));
            }
        }

        // 型名がActionまたはFuncで始まるかチェック
        private static bool IsActionOrFuncType(ITypeSymbol type)
        {
            var name = type.Name;
            return name == "Action" || name == "Func";
        }
    }
}
```

### 2. 新規作成: `tools/UnityNamingAnalyzer.Tests/EventSystemAnalyzerTests.cs`

テストケース一覧:

| テスト | 入力 | 期待結果 |
|--------|------|---------|
| `EventAction_UNA0003` | `public event Action OnDeath;` | UNA0003 検出 |
| `ActionField_UNA0003` | `private Action<int> _onDamage;` | UNA0003 検出 |
| `FuncField_UNA0003` | `private Func<bool> _isReady;` | UNA0003 検出 |
| `ActionProperty_UNA0003` | `public Action OnClick { get; set; }` | UNA0003 検出 |
| `SubjectField_NoDiagnostic` | `private Subject<int> _onDamage;` | 検出なし |
| `ActionParameter_NoDiagnostic` | `void Method(Action callback)` | 検出なし |
| `ActionLocalVar_NoDiagnostic` | `Action a = () => {};` | 検出なし |

### 3. 編集: `.editorconfig`

```ini
# 追加（UNA0001/UNA0002 の下に）
dotnet_diagnostic.UNA0003.severity = warning
```

### 4. 編集: `~/.claude/agents/unity-code-quality-checker.md`

ルール7（イベントシステム）を削除:
```
7. **イベントシステム**:
   - C#のAction/eventキーワードを絶対に許可しない
   - すべてのイベントはR3のSubject/Observableを使用する必要がある
   - `event`、`Action<>`、`Func<>`の使用にフラグを立てる
```

## 検証

```bash
# アナライザービルド
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release

# テスト実行
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release

# 既存コードでフォーマットチェック（新ルールで既存コードが通ることを確認）
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```

### 既存コードへの影響

既存コードで `Action`/`Func`/`event` をフィールドとして使用している箇所がある場合、UNA0003 が検出される。
その場合は R3 の `Subject`/`Observable` に置き換えるか、一時的に `#pragma warning disable UNA0003` で抑制する。
