# タスク完了時のワークフロー

## タスク完了チェックリスト

### 1. コード品質確認

#### コンパイルエラーチェック
```bash
# コンパイル状態確認
./unity-tools/unity-compile.sh check .

# エラーがある場合はトリガーしてから再確認
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
```

**期待される出力**:
```
✅ No recent compilation errors detected
```

#### コーディング規約確認
- [ ] 命名規則に従っているか（PascalCase, camelCase, _camelCase, UPPER_SNAKE_CASE）
- [ ] クラスメンバーの宣言順序は正しいか
- [ ] コメントはすべて日本語か
- [ ] Unity固有のnullチェックパターンを使用しているか（`if (obj)` not `if (obj != null)`）
- [ ] YAGNI/KISS原則に従ったシンプルな実装か

### 2. Unityエディターでの動作確認

#### Play Mode テスト
- [ ] 関連シーンをPlayモードで実行
- [ ] 変更した機能が正常に動作するか
- [ ] UIが正しく表示されるか
- [ ] エラーログがConsoleに出ていないか

#### Inspector確認
- [ ] SerializeFieldが適切に設定されているか
- [ ] ScriptableObjectの参照が正しいか
- [ ] VContainer LifetimeScopeの設定が正しいか

### 3. リファクタリング時の追加確認

#### 統合テスト
- [ ] 変更が他のシステムに影響していないか
- [ ] 依存関係が正しく解決されるか（VContainer）
- [ ] R3のObservable購読が正しく機能するか
- [ ] UniTaskの非同期処理が正常に完了するか

#### シーン遷移テスト（該当する場合）
- [ ] SceneTransitionManagerが正しく動作するか
- [ ] データが正しく受け渡されるか
- [ ] 戻るシーンが正しく設定されているか

### 4. パフォーマンス確認（大規模変更時）

- [ ] 不要な`new`によるメモリアロケーションがないか
- [ ] `foreach`を`for`ループに置き換えられないか
- [ ] UniTask.WhenAll()で並列化できる処理はないか
- [ ] Profilerでフレームレート低下がないか確認

### 5. ドキュメント更新（必要に応じて）

- [ ] CLAUDE.mdの更新が必要か確認
- [ ] 新しいシステムや重要なパターンを追加した場合は記録

### 6. Git コミット

#### コミット前
```bash
# 変更ファイル確認
git status

# 差分確認
git diff

# 意図しない変更がないか確認
```

#### コミットメッセージ
```bash
git add .
git commit -m "明確で説明的なコミットメッセージ

- 変更内容の詳細
- 変更理由
- 影響範囲"
```

**良いコミットメッセージ例**:
```
カード進化システムに即時進化チェック機能を追加

- PlayerPresenterにCheckCardEvolution呼び出しを追加
- カードプレイ時に進化条件を即座にチェック
- 進化時はUIに通知を表示

影響範囲: PlayerPresenter.cs, BattlePresenter.cs
```

### 7. プッシュ前の最終確認

```bash
# 最新のmainブランチとの差分確認（必要に応じて）
git fetch origin
git diff origin/main

# プッシュ
git push
```

## タスク完了の基準

以下がすべて満たされた場合、タスク完了とみなす：

1. ✅ コンパイルエラーがない
2. ✅ Unityエディターで動作確認済み
3. ✅ コーディング規約に準拠
4. ✅ 意図した機能が正常に動作
5. ✅ 既存機能への影響がない（または意図した影響のみ）
6. ✅ コンソールにエラー/警告がない
7. ✅ Gitコミットが完了

## 注意事項

### 避けるべき行動
- ❌ コンパイルエラーがある状態でのコミット
- ❌ Unityエディターでの動作確認なしでのコミット
- ❌ 過度な抽象化や将来の拡張を見越した実装（YAGNI違反）
- ❌ 後方互換性のための複雑なコード（クリーンな置き換えを優先）

### 推奨される行動
- ✅ シンプルで理解しやすいコードを書く（KISS原則）
- ✅ 必要最小限の変更で目的を達成
- ✅ 明確なコミットメッセージで変更内容を記録
- ✅ SerializeFieldの設定忘れはエラーで検出する設計

## 緊急時の対応

### コンパイルエラーが解決しない場合
1. エラーメッセージを注意深く読む
2. Unity Console で詳細を確認
3. 最近の変更を段階的にrevertして原因を特定
4. 必要に応じてUnityエディターを再起動

### Gitコンフリクトが発生した場合
```bash
# 最新の状態を取得
git fetch origin

# マージまたはリベース
git merge origin/main
# または
git rebase origin/main

# コンフリクト解決後
git add .
git commit
```
