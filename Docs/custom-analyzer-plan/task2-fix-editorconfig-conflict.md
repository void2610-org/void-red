# タスク2: editorconfig の矛盾解決

## 背景

`csharp_style_expression_bodied_methods = false:silent` が設定されているが:

1. severity `silent` のため実際にはチェックされていない
2. サブエージェントは「単一行 public メソッドは `=>` を使用」と**逆方向**のルールを持つ
3. 実際のコードはサブエージェントのルールに従っている

つまり editorconfig は「式本体を使うな（ただし silent）」、サブエージェントは「式本体を使え」と矛盾している。

## 変更対象

- `.editorconfig` (52行目)

## 変更内容

```diff
# Before
-csharp_style_expression_bodied_methods = false:silent
# After
+csharp_style_expression_bodied_methods = when_on_single_line:suggestion
```

### 設定値の意味

| 設定 | 意味 |
|------|------|
| `when_on_single_line` | 単一行の場合に式本体を推奨 |
| `suggestion` | IDE でヒントとして表示（CI `--severity warn` では検出されない） |

### タスク4 との関係

- この変更は**一時的**なもの
- タスク4 で UNA0004（カスタムアナライザー）を実装後、`false:silent` に戻す
- UNA0004 は public メソッドのみを対象とし、editorconfig よりも正確な制御が可能

```
タスク2: false:silent → when_on_single_line:suggestion  (一時的に方向性を一致させる)
タスク4: when_on_single_line:suggestion → false:silent   (UNA0004に完全委任)
```

## 検証

```bash
# editorconfig変更後、既存コードがstyleチェックを通ることを確認
dotnet format style FormatCheck.csproj --verify-no-changes --severity warn
```

- `suggestion` レベルのため `--severity warn` では検出されず、既存CIは影響を受けない
