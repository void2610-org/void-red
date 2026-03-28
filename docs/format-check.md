# C# コードフォーマットチェック

このプロジェクトの C# コーディング規約は `unity-coding-standards` submodule を基盤にしている。  
ルートの `.editorconfig`、`Directory.Build.props`、`FormatCheck.csproj` は shared config への symlink で管理する。

## 構成

```text
void-red/
├── .editorconfig -> unity-coding-standards/config/.editorconfig
├── Directory.Build.props -> unity-coding-standards/config/Directory.Build.props
├── FormatCheck.csproj -> unity-coding-standards/config/FormatCheck.csproj
├── .github/workflows/format-check.yml
└── unity-coding-standards/
    ├── config/
    ├── scripts/
    └── src/Void2610.Unity.Analyzers/
```

## 実行コマンド

```bash
# 自動修正
./unity-coding-standards/scripts/run-format.sh

# 差分確認のみ
./unity-coding-standards/scripts/run-format.sh --verify-no-changes

# アナライザー単体ビルド
dotnet build unity-coding-standards/src/Void2610.Unity.Analyzers/Void2610.Unity.Analyzers.csproj -c Release
```

## チェック内容

- `dotnet format whitespace`
- `dotnet format style`
- `dotnet format analyzers`
- `Void2610.Unity.Analyzers` による Unity 向け規約チェック

代表的な diagnostic ID:

- `VUA1001`: `[SerializeField]` フィールドへの防御的 null チェック禁止
- `VUA1002`: `event` / `Action` / `Func` フィールド禁止
- `VUA1003`: LitMotion キャンセル処理を `TryCancel()` に統一
- `VUA1004`: メンバー宣言順序の統一
- `VUA2001`: `[SerializeField]` フィールドに `_` プレフィックス禁止
- `VUA2002`: 通常の private フィールドに `_` プレフィックス必須
- `VUA3001`: 単一文 public メソッドに式本体を推奨
- `VUA3002`: `const` フィールドは `ALL_UPPER`
- `VUA3003`: file-scoped namespace を要求
- `VUA4001`: enum メンバーの `summary` 必須

## CI

`.github/workflows/format-check.yml` は shared の reusable workflow を呼ぶ caller のみを持つ。  
ローカルに CI ロジックを複製せず、更新は `unity-coding-standards` 側に集約する。
