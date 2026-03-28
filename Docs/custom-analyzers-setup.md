# カスタムアナライザー＆フォーマット設定ガイド

## ファイル配置

| 相対パス | 役割 |
|---|---|
| `.editorconfig` | `unity-coding-standards/config/.editorconfig` への symlink |
| `FormatCheck.csproj` | `unity-coding-standards/config/FormatCheck.csproj` への symlink |
| `Directory.Build.props` | `unity-coding-standards/config/Directory.Build.props` への symlink |
| `unity-coding-standards/` | 共有コーディング規約 submodule |

```text
プロジェクトルート/
├── .editorconfig -> unity-coding-standards/config/.editorconfig
├── Directory.Build.props -> unity-coding-standards/config/Directory.Build.props
├── FormatCheck.csproj -> unity-coding-standards/config/FormatCheck.csproj
└── unity-coding-standards/
    ├── config/
    ├── scripts/
    └── src/Void2610.Unity.Analyzers/
```

## カスタムアナライザー一覧

| ID | ルール名 | チェック内容 |
|---|---|---|
| VUA1001 | SerializeFieldNullCheck | `[SerializeField]`フィールドへの防御的nullチェックを禁止 |
| VUA1002 | EventSystem | C#標準の`event`や`Action`/`Func`フィールドを禁止 |
| VUA1003 | MotionHandleTryCancel | LitMotionのキャンセル処理を`TryCancel()`に統一 |
| VUA1004 | MemberOrder | クラスメンバーの宣言順序を強制 |
| VUA2001 | SerializeFieldNaming | `[SerializeField]`フィールドに`_`プレフィックスを付けない |
| VUA2002 | PrivateFieldNaming | 通常のprivateフィールドに`_`プレフィックス必須 |
| VUA3001 | ExpressionBody | 単一文のpublicメソッドに式本体を推奨 |
| VUA3002 | ConstantsNaming | `const`フィールドに`ALL_UPPER`を要求 |
| VUA3003 | FileScopedNamespace | file-scoped namespace を要求 |
| VUA4001 | EnumSummary | トップレベルenumメンバーに`/// <summary>`コメントを必須化 |

## フォーマットチェックの実行

```bash
# 自動修正
./unity-coding-standards/scripts/run-format.sh

# 差分確認のみ
./unity-coding-standards/scripts/run-format.sh --verify-no-changes
```

## 他のUnityプロジェクトで再現する手順

### 1. submodule を追加する

```bash
git submodule add https://github.com/void2610/unity-coding-standards.git unity-coding-standards
```

### 2. 共有設定ファイルを symlink にする

```bash
ln -s unity-coding-standards/config/.editorconfig .editorconfig
ln -s unity-coding-standards/config/Directory.Build.props Directory.Build.props
ln -s unity-coding-standards/config/FormatCheck.csproj FormatCheck.csproj
```

### 3. アナライザーをビルドする

```bash
dotnet build unity-coding-standards/src/Void2610.Unity.Analyzers/Void2610.Unity.Analyzers.csproj -c Release
```

### 前提条件

- .NET SDK がインストールされていること
- アナライザー DLL は Release ビルド済みであること
