# C# コードフォーマットチェック

dotnet-format とカスタム Roslyn アナライザーを使用した C# コードのフォーマット・スタイル・命名規約チェックシステム。

## 概要

このプロジェクトでは、以下の3層構造でコード品質を担保する:

1. **dotnet format whitespace** - 空白・インデント・改行のフォーマット
2. **dotnet format style** - コードスタイル規約（var使用、式本体など）
3. **dotnet format analyzers** - カスタムアナライザーによる命名規約チェック

### アーキテクチャ

```
┌─────────────────────────────────────────────────────────────────┐
│                      dotnet format                              │
│  ┌───────────────┐  ┌───────────────┐  ┌─────────────────────┐ │
│  │  whitespace   │  │    style      │  │     analyzers       │ │
│  │ (空白/改行)   │  │(コードスタイル)│  │    (静的解析)       │ │
│  └───────┬───────┘  └───────┬───────┘  └──────────┬──────────┘ │
│          │                  │                     │            │
│          └──────────────────┴─────────────────────┘            │
│                             │                                  │
│              ┌──────────────┴──────────────┐                   │
│              │        .editorconfig        │                   │
│              │        (ルール定義)          │                   │
│              └──────────────┬──────────────┘                   │
│                             │                                  │
│    ┌────────────────────────┼────────────────────────┐         │
│    │                        │                        │         │
│    ▼                        ▼                        ▼         │
│ ┌──────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│ │FormatCheck   │  │ IDE 標準規則    │  │UnityNamingAnalyzer│   │
│ │.csproj       │  │ (IDE0001等)     │  │(UNA0001-UNA0005) │   │
│ │(対象定義)    │  │                 │  │                  │   │
│ └──────────────┘  └─────────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## ファイル構成

```
void-red/
├── .editorconfig                    # コーディング規約定義
├── FormatCheck.csproj               # dotnet-format対象ファイル定義
├── .github/workflows/
│   └── format-check.yml             # CIワークフロー
└── tools/
    ├── UnityNamingAnalyzer/
    │   ├── UnityNamingAnalyzer.csproj
    │   ├── SerializeFieldNamingAnalyzer.cs   # UNA0001/UNA0002: フィールド命名規約
    │   ├── EventSystemAnalyzer.cs            # UNA0003: R3イベントシステム強制
    │   ├── ExpressionBodyAnalyzer.cs         # UNA0004: 単一文public式本体強制
    │   └── MemberOrderAnalyzer.cs            # UNA0005: クラスメンバー宣言順序
    └── UnityNamingAnalyzer.Tests/
        ├── UnityNamingAnalyzer.Tests.csproj
        ├── SerializeFieldNamingAnalyzerTests.cs
        ├── EventSystemAnalyzerTests.cs
        ├── ExpressionBodyAnalyzerTests.cs
        └── MemberOrderAnalyzerTests.cs
```

### 各ファイルの役割

| ファイル | 役割 |
|----------|------|
| `.editorconfig` | フォーマット・スタイル・命名規則の定義 |
| `FormatCheck.csproj` | チェック対象ファイルの指定、カスタムアナライザーの参照 |
| `format-check.yml` | CI実行ステップの定義 |
| `UnityNamingAnalyzer` | Unity固有の命名規約を検証するRoslynアナライザー |

## カスタムアナライザー: UnityNamingAnalyzer

Unity プロジェクト特有の命名規約を検証するカスタム Roslyn アナライザー。

### 診断ルール

| ID | 説明 | 対象 |
|----|------|------|
| **UNA0001** | privateフィールドには `_` プレフィックスが必要 | `[SerializeField]` なしの private フィールド |
| **UNA0002** | `[SerializeField]` フィールドには `_` プレフィックスを付けない | `[SerializeField]` 付きの private フィールド |
| **UNA0003** | イベントにはR3の `Subject<T>` を使用 | `event` キーワード、`Action`/`Func` 型のフィールド・プロパティ |
| **UNA0004** | 単一文の public メソッドには式本体を使用 | ブロック本体で1ステートメントの public メソッド |
| **UNA0005** | クラスメンバーの宣言順序が不正 | クラス・構造体のメンバー宣言順序 |

### 命名規約の理由

```csharp
public class Player : MonoBehaviour
{
    // [SerializeField] → Inspectorに表示されるためプレフィックスなし
    [SerializeField] private int maxHealth;
    [SerializeField] private float moveSpeed;

    // 通常のprivateフィールド → _プレフィックス必須
    private int _currentHealth;
    private bool _isMoving;
}
```

- **`[SerializeField]`**: Unity Inspector に表示されるため、見やすさを優先して `_` なし
- **通常の private フィールド**: ローカル変数と区別するため `_` プレフィックス必須

### イベントシステム規約 (UNA0003)

C# 標準の `event`、`Action`、`Func` の代わりに R3 の `Subject<T>` を使用する:

```csharp
// NG: C#標準のイベント・デリゲート
public event EventHandler OnDamaged;        // UNA0003
private Action _onHealthChanged;            // UNA0003
public Func<int> GetValue { get; set; }     // UNA0003

// OK: R3のSubjectを使用
private readonly Subject<int> _onDamaged = new();
public Observable<int> OnDamaged => _onDamaged;
```

- メソッドパラメータやローカル変数の `Action`/`Func` は除外（コールバック受け取りは許可）

### 式本体規約 (UNA0004)

単一ステートメントの public メソッドは式本体 (`=>`) で記述する:

```csharp
// NG: 単一文のブロック本体
public int GetValue()
{
    return 42;       // UNA0004
}

// OK: 式本体
public int GetValue() => 42;

// OK: 複数文はブロック本体のまま
public int Calculate()
{
    var x = 42;
    return x * 2;
}
```

- private/protected/internal メソッドは対象外
- コンストラクタは対象外

### メンバー宣言順序規約 (UNA0005)

クラス・構造体のメンバーは以下の順序で宣言する:

| 順序 | カテゴリ | 説明 |
|------|----------|------|
| 0 | Enum | ネストされた enum |
| 1 | SerializeField | `[SerializeField]` 付き private フィールド |
| 2 | public properties | public プロパティ |
| 3 | constants | `const`、`static readonly` フィールド |
| 4 | private fields | その他の private フィールド（readonly 含む） |
| 5 | constructors | コンストラクタ |
| 6 | public methods (one line) | 式本体 (`=>`) の public メソッド |
| 7 | public methods (multi line) | ブロック本体の public メソッド |
| 8 | private methods | private/protected/internal メソッド（Unity event、cleanup 以外） |
| 9 | Unity events | `Awake`, `Start`, `Update`, `OnEnable`, `OnDisable` 等 |
| 10 | cleanup | `OnDestroy`, `Dispose` |

```csharp
public class Player : MonoBehaviour
{
    // 0. Enum
    public enum State { Idle, Running }

    // 1. SerializeField
    [SerializeField] private int maxHealth;

    // 2. public properties
    public int CurrentHealth { get; private set; }

    // 3. constants
    public const int MaxLevel = 100;
    private static readonly int DefaultHealth = 10;

    // 4. private fields
    private int _level;

    // 5. constructors（MonoBehaviourでは通常不要）

    // 6. public methods (one line)
    public int GetLevel() => _level;

    // 7. public methods (multi line)
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    // 8. private methods
    private void UpdateUI()
    {
        // UI更新処理
    }

    // 9. Unity events
    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // 10. cleanup
    private void OnDestroy()
    {
        // クリーンアップ処理
    }
}
```

- ネストされたクラス/構造体/インターフェースは順序チェック対象外（enum は対象）
- `static` メソッドはアクセス修飾子に従い通常メソッドと同カテゴリ
- `protected`/`internal` プロパティは対象外

### IDE1006 との競合回避

標準の命名規則 (IDE1006) はカスタムアナライザーと競合するため、`.editorconfig` で severity を `suggestion` に下げている:

```ini
# IDE1006（標準命名規則）はカスタムアナライザーと競合するためsuggestに下げる
dotnet_diagnostic.IDE1006.severity = suggestion

# カスタムアナライザー: UnityNamingAnalyzer
dotnet_diagnostic.UNA0001.severity = warning
dotnet_diagnostic.UNA0002.severity = warning
dotnet_diagnostic.UNA0003.severity = warning
dotnet_diagnostic.UNA0004.severity = warning
dotnet_diagnostic.UNA0005.severity = warning
```

## ローカル実行

### 前提条件

- .NET 8 SDK

```bash
dotnet --version  # 8.0.x
```

### コマンド一覧

```bash
# 1. カスタムアナライザーをビルド（初回 or 変更時）
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release

# 2. アナライザーのテスト実行
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release

# 3. フォーマットチェック（差分確認のみ）
dotnet format whitespace FormatCheck.csproj --verify-no-changes
dotnet format style FormatCheck.csproj --verify-no-changes --severity warn
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn

# 4. 自動修正（whitespace/styleのみ。analyzersは手動修正が必要）
dotnet format whitespace FormatCheck.csproj
dotnet format style FormatCheck.csproj --severity warn
```

### 一括チェックスクリプト例

```bash
#!/bin/bash
set -e

echo "Building analyzer..."
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release -v q

echo "Running analyzer tests..."
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release -v q

echo "Checking whitespace..."
dotnet format whitespace FormatCheck.csproj --verify-no-changes

echo "Checking style..."
dotnet format style FormatCheck.csproj --verify-no-changes --severity warn

echo "Checking analyzers..."
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn

echo "All checks passed!"
```

## CI パイプライン

### トリガー条件

- `main` / `develop` ブランチへのプッシュ
- プルリクエスト

以下のファイルが変更された場合のみ実行:
- `Assets/Scripts/**/*.cs`
- `.editorconfig`
- `.github/workflows/format-check.yml`
- `tools/UnityNamingAnalyzer/**`

### 実行ステップ

1. リポジトリのチェックアウト（サブモジュール含む）
2. .NET 8 SDK のセットアップ
3. カスタムアナライザーのビルド
4. アナライザーのテスト実行
5. `dotnet format whitespace --verify-no-changes`
6. `dotnet format style --verify-no-changes --severity warn`
7. `dotnet format analyzers --verify-no-changes --severity warn`

## FormatCheck.csproj 詳細

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>10</LangVersion>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Assets/Scripts/**/*.cs" />
    <!-- サブモジュール（シンボリックリンク先）を除外 -->
    <Compile Remove="Assets/Scripts/SettingsSystem/**/*.cs" />
    <Compile Remove="Assets/Scripts/Utils/**/*.cs" />
  </ItemGroup>
  <ItemGroup>
    <Analyzer Include="tools/UnityNamingAnalyzer/bin/Release/netstandard2.0/UnityNamingAnalyzer.dll"
              Condition="Exists('...')" />
  </ItemGroup>
</Project>
```

### 設計ポイント

- **サブモジュール除外**: `SettingsSystem` と `Utils` は別リポジトリのサブモジュールのため除外
- **カスタムアナライザー参照**: ビルド済み DLL を `Condition` 付きで参照（存在しない場合はスキップ）

## .editorconfig ルール詳細

### 基本設定

| 設定 | 値 | 説明 |
|------|-----|------|
| `indent_style` | `space` | スペースでインデント |
| `indent_size` | `4` | 4 スペース |
| `end_of_line` | `lf` | LF 改行 |
| `charset` | `utf-8` | UTF-8 エンコーディング |

### ブレーススタイル（Allman）

```csharp
public class Example
{
    public void Method()
    {
        if (condition)
        {
        }
    }
}
```

### 命名規則サマリー

| 対象 | 規則 | 例 | 重大度 |
|------|------|-----|--------|
| 通常 private フィールド | `_camelCase` | `_playerHealth` | warning (UNA0001) |
| `[SerializeField]` フィールド | `camelCase` | `maxHealth` | warning (UNA0002) |
| イベント・デリゲート | R3 `Subject<T>` を使用 | `Subject<int>` | warning (UNA0003) |
| 単一文 public メソッド | 式本体 (`=>`) で記述 | `=> 42` | warning (UNA0004) |
| メンバー宣言順序 | 規定の順序で宣言 | Enum → ... → cleanup | warning (UNA0005) |
| パブリックメンバー | `PascalCase` | `GetPlayer()` | warning |
| 型 | `PascalCase` | `PlayerController` | warning |
| インターフェース | `IPascalCase` | `IDisposable` | warning |
| ローカル変数 | `camelCase` | `localVar` | warning |

## アナライザー拡張方法

### 新しい診断ルールの追加

1. `tools/UnityNamingAnalyzer/` に新しいアナライザークラスを作成（既存の `EventSystemAnalyzer.cs` 等を参考）:

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NewAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UNA0005 = new DiagnosticDescriptor(
        "UNA0005", "タイトル", "メッセージ '{0}'",
        "Category", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(UNA0005);

    public override void Initialize(AnalysisContext context) { /* ... */ }
}
```

2. `tools/UnityNamingAnalyzer.Tests/` にテストクラスを追加

3. `.editorconfig` に severity を設定:

```ini
dotnet_diagnostic.UNA0005.severity = warning
```

### 外部アナライザーパッケージの追加

`FormatCheck.csproj` に PackageReference を追加:

```xml
<ItemGroup>
  <PackageReference Include="StyleCop.Analyzers" Version="1.1.118">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

`.editorconfig` でルールを設定:

```ini
# StyleCop ルール例
dotnet_diagnostic.SA1101.severity = none      # this. 接頭辞不要
dotnet_diagnostic.SA1633.severity = none      # ファイルヘッダー不要
```

## トラブルシューティング

### アナライザーが適用されない

```bash
# アナライザーDLLが存在するか確認
ls -la tools/UnityNamingAnalyzer/bin/Release/netstandard2.0/

# ない場合はビルド
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release
```

### CI で失敗するがローカルでは成功

1. 改行コードの違い（CRLF vs LF）
   - `.editorconfig` の `end_of_line = lf` を確認
   - Git設定: `git config --global core.autocrlf input`

2. アナライザービルドの不整合
   - ローカルで `dotnet clean` してから再ビルド

### 特定ファイルを除外したい

`FormatCheck.csproj` で除外:

```xml
<ItemGroup>
  <Compile Include="Assets/Scripts/**/*.cs" />
  <Compile Remove="Assets/Scripts/Generated/**/*.cs" />
</ItemGroup>
```

## 参考リンク

- [dotnet format 公式ドキュメント](https://learn.microsoft.com/dotnet/core/tools/dotnet-format)
- [.editorconfig 設定リファレンス](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/code-style-rule-options)
- [Roslyn アナライザー作成ガイド](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
- [Microsoft.CodeAnalysis.Testing](https://github.com/dotnet/roslyn-sdk/tree/main/src/Microsoft.CodeAnalysis.Testing)
