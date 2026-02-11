# サブエージェントルール → フォーマットチェック移行計画

## 目的

`unity-code-quality-checker` サブエージェントが8ルールをチェックしているが、`.editorconfig` + カスタム Roslyn アナライザーと重複・矛盾するルールがある。
冗長ルールの削除、矛盾の解決、新規アナライザー（UNA0003/UNA0004）の追加により、サブエージェントの負荷を **8→4 項目に半減**させる。

## タスク一覧

| タスク | 概要 | 詳細プラン |
|--------|------|------------|
| タスク1 | サブエージェントから冗長ルール削除 | [task1-remove-redundant-rules.md](./task1-remove-redundant-rules.md) |
| タスク2 | editorconfig の矛盾解決 | [task2-fix-editorconfig-conflict.md](./task2-fix-editorconfig-conflict.md) |
| タスク3 | UNA0003 — R3 イベントシステム強制アナライザー | [task3-una0003-event-system-analyzer.md](./task3-una0003-event-system-analyzer.md) |
| タスク4 | UNA0004 — 単一文 public メソッド式本体強制アナライザー | [task4-una0004-expression-body-analyzer.md](./task4-una0004-expression-body-analyzer.md) |
| タスク5 | サブエージェント最終整理 + ドキュメント更新 | [task5-final-cleanup.md](./task5-final-cleanup.md) |

## 依存関係

```
タスク1 ─────────────────────────────────┐
タスク2 → タスク4（タスク4完了後に設定を戻す）├→ タスク5
タスク3 ─────────────────────────────────┤
タスク4 ─────────────────────────────────┘
```

- タスク1, 2, 3 は独立して並行実行可能
- タスク4 はタスク2 の editorconfig 変更を最終的に上書きする
- タスク5 は全タスク完了後に実行

## 対象ファイルまとめ

| ファイル | 操作 |
|---------|------|
| `~/.claude/agents/unity-code-quality-checker.md` | 編集（ルール削除・整理） |
| `.editorconfig` | 編集（矛盾解決 + UNA0003/UNA0004 severity 追加） |
| `tools/UnityNamingAnalyzer/EventSystemAnalyzer.cs` | 新規作成 |
| `tools/UnityNamingAnalyzer/ExpressionBodyAnalyzer.cs` | 新規作成 |
| `tools/UnityNamingAnalyzer.Tests/EventSystemAnalyzerTests.cs` | 新規作成 |
| `tools/UnityNamingAnalyzer.Tests/ExpressionBodyAnalyzerTests.cs` | 新規作成 |
| `Docs/format-check.md` | 編集（新ルールドキュメント追加） |

## 検証方法（全タスク共通）

```bash
# アナライザービルド
dotnet build tools/UnityNamingAnalyzer/UnityNamingAnalyzer.csproj -c Release

# テスト実行
dotnet test tools/UnityNamingAnalyzer.Tests/UnityNamingAnalyzer.Tests.csproj -c Release

# 既存コードへのフォーマットチェック
dotnet format analyzers FormatCheck.csproj --verify-no-changes --severity warn
```

## サブエージェント最終状態（4ルール）

| # | ルール | アナライザー化しない理由 |
|---|--------|------------------------|
| 1 | クラスメンバー宣言順序 | 10カテゴリの順序判定はROIが低い |
| 2 | エラーハンドリングポリシー | セマンティック判断が必要で静的解析不可 |
| 3 | コメント規約（日本語） | 言語検出が困難 |
| 4 | LitMotion 使用規約 | 「簡単なアニメーション」の判断が主観的 |
