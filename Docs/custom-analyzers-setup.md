# カスタムアナライザー＆フォーマット設定ガイド

## ファイル配置

| 相対パス | 役割 |
|---|---|
| `.editorconfig` | C#フォーマット・命名規則・アナライザーseverity設定 |
| `FormatCheck.csproj` | `dotnet format`専用のプロジェクトファイル |
| `Directory.Build.props` | 全体ビルド設定＋アナライザーDLL参照 |
| `unity-analyzers/` | カスタムRoslynアナライザープロジェクト一式 |

```
プロジェクトルート/
├── .editorconfig
├── Directory.Build.props
├── FormatCheck.csproj
└── unity-analyzers/
    ├── Directory.Build.props
    └── src/Void2610.Unity.Analyzers/
        ├── *.cs (アナライザーソースコード)
        ├── Void2610.Unity.Analyzers.csproj
        └── bin/Release/netstandard2.0/
            └── Void2610.Unity.Analyzers.dll
```

## カスタムアナライザー一覧（VUA0001〜VUA0008）

| ID | ルール名 | チェック内容 |
|---|---|---|
| VUA0001 | SerializeFieldNullCheck | `[SerializeField]`フィールドへの防御的nullチェック（`== null`, `?.`, `??`, `is null`）を禁止 |
| VUA0002 | SerializeFieldNaming | `[SerializeField]`フィールドに`_`プレフィックスを付けないことを強制 |
| VUA0003 | EventSystem | C#標準の`event`や`Action`/`Func`フィールドを禁止（R3の`Subject<T>`推奨） |
| VUA0004 | ExpressionBody | 単一文のpublicメソッドに式本体（`=>`）の使用を推奨 |
| VUA0005 | MemberOrder | クラスメンバーの宣言順序を強制（enum→SerializeField→Property→const→private field→ctor→public method→private method→Unity event→Cleanup） |
| VUA0006 | EnumSummary | トップレベルenumのメンバーに`/// <summary>`コメントを必須化 |
| VUA0007 | MotionHandleTryCancel | LitMotionの`if(handle.IsActive()) handle.Cancel()`を`handle.TryCancel()`に統一 |
| VUA0008 | PrivateFieldNaming | 通常のprivateフィールドに`_`プレフィックス必須 |

## フォーマットチェックの実行

```bash
# 1. ホワイトスペース整形
dotnet format whitespace FormatCheck.csproj

# 2. コードスタイル整形
dotnet format style FormatCheck.csproj --severity warn

# 3. カスタムアナライザー検証
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```

## 他のUnityプロジェクトで再現する手順

### 1. アナライザープロジェクトをコピー＆ビルド

`unity-analyzers/` ディレクトリをそのまま新プロジェクトに配置してビルドする。

```bash
cd unity-analyzers/src/Void2610.Unity.Analyzers
dotnet build -c Release
```

### 2. `Directory.Build.props` をプロジェクトルートに配置

```xml
<Project>
  <PropertyGroup>
    <LangVersion>preview</LangVersion>
    <BaseIntermediateOutputPath>$(MSBuildThisFileDirectory)../Temp/obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
    <BaseOutputPath>$(MSBuildThisFileDirectory)../Temp/bin/$(MSBuildProjectName)/</BaseOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <Analyzer Include="$(MSBuildThisFileDirectory)unity-analyzers/src/Void2610.Unity.Analyzers/bin/Release/netstandard2.0/Void2610.Unity.Analyzers.dll" />
  </ItemGroup>
</Project>
```

- `BaseIntermediateOutputPath`/`BaseOutputPath`でobj/binをUnityの`Temp/`配下に退避し、Unityのインポート対象から除外する
- `Analyzer`でカスタムアナライザーDLLを参照する

### 3. `FormatCheck.csproj` をプロジェクトルートに配置

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Assets/Scripts/**/*.cs"
             Exclude="Assets/Scripts/除外したいパス/**/*.cs" />
  </ItemGroup>
</Project>
```

- `EnableDefaultCompileItems=false`にして、チェック対象のスクリプトだけを明示的に指定する

### 4. `.editorconfig` をプロジェクトルートに配置

プロジェクトの`.editorconfig`をコピーし、末尾でカスタムアナライザーのseverityを有効化する。

```ini
# カスタムアナライザー
dotnet_diagnostic.VUA0001.severity = warning
dotnet_diagnostic.VUA0002.severity = warning
dotnet_diagnostic.VUA0003.severity = warning
dotnet_diagnostic.VUA0004.severity = warning
dotnet_diagnostic.VUA0005.severity = warning
dotnet_diagnostic.VUA0006.severity = warning
dotnet_diagnostic.VUA0007.severity = warning
dotnet_diagnostic.VUA0008.severity = warning
```

### 5. `unity-analyzers/Directory.Build.props` を配置

アナライザープロジェクト自体のビルド設定を分離するため、`unity-analyzers/`直下にも配置する。

```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

### 前提条件

- .NET SDK がインストールされていること（`dotnet format`コマンド用）
- アナライザーDLLはReleaseビルド済みであること
