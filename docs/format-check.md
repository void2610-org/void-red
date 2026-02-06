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
│ │.csproj       │  │ (IDE0001等)     │  │(UNA0001/UNA0002) │   │
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
    │   └── SerializeFieldNamingAnalyzer.cs   # カスタムアナライザー
    └── UnityNamingAnalyzer.Tests/
        ├── UnityNamingAnalyzer.Tests.csproj
        └── SerializeFieldNamingAnalyzerTests.cs
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

### IDE1006 との競合回避

標準の命名規則 (IDE1006) はカスタムアナライザーと競合するため、`.editorconfig` で severity を `suggestion` に下げている:

```ini
# IDE1006（標準命名規則）はカスタムアナライザーと競合するためsuggestに下げる
dotnet_diagnostic.IDE1006.severity = suggestion

# カスタムアナライザー: SerializeFieldの命名規約
dotnet_diagnostic.UNA0001.severity = warning
dotnet_diagnostic.UNA0002.severity = warning
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
| パブリックメンバー | `PascalCase` | `GetPlayer()` | warning |
| 型 | `PascalCase` | `PlayerController` | warning |
| インターフェース | `IPascalCase` | `IDisposable` | warning |
| ローカル変数 | `camelCase` | `localVar` | warning |

## アナライザー拡張方法

### 新しい診断ルールの追加

1. `SerializeFieldNamingAnalyzer.cs` に新しい `DiagnosticDescriptor` を追加:

```csharp
public static readonly DiagnosticDescriptor UNA0003 = new DiagnosticDescriptor(
    "UNA0003",
    "タイトル",
    "メッセージ '{0}'",
    "Naming",
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true);
```

2. `SupportedDiagnostics` に追加:

```csharp
public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    ImmutableArray.Create(UNA0001, UNA0002, UNA0003);
```

3. `AnalyzeField` または新しい分析メソッドで診断を報告

4. テストを追加

5. `.editorconfig` に severity を設定:

```ini
dotnet_diagnostic.UNA0003.severity = warning
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
