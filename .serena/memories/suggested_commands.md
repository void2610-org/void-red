# 推奨コマンド

## コンパイルチェックコマンド

### unity-compile.sh の使用方法

プロジェクトルートに配置された`unity-tools/unity-compile.sh`を使用してUnityのコンパイル状態を確認・トリガーできます。

#### コンパイルエラーチェック
```bash
./unity-tools/unity-compile.sh check .
```
- Unity Editor.logを解析して最新のコンパイルエラーを表示
- エディター実行中でなくても使用可能
- エラーがない場合は"✅ No recent compilation errors detected"を表示

#### コンパイルトリガー + チェック（推奨ワークフロー）
```bash
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .
```
- Unityエディターに対してCmd+Rを送信してコンパイルをトリガー
- 3秒待機してコンパイル完了を待つ
- コンパイル結果をチェック

**注意**: `trigger`コマンドはUnityエディターが実行中でmacOS (Darwin)環境でのみ動作します（AppleScript使用）。

### 使用タイミング
- コード変更後の確認
- プルリクエスト作成前
- コミット前の最終確認

## Git コマンド

### 基本的なGitワークフロー
```bash
# 現在の状態確認
git status

# 変更をステージング
git add .

# コミット
git commit -m "コミットメッセージ"

# プッシュ
git push
```

## Darwin (macOS) システムコマンド

### ファイル操作
```bash
# ファイル一覧
ls -la

# ディレクトリ移動
cd path/to/directory

# ファイル検索
find . -name "*.cs"

# ファイル内容検索
grep -r "pattern" Assets/Scripts/
```

### プロセス管理
```bash
# Unityプロセス確認
pgrep -f "Unity"

# プロセス終了（必要な場合のみ）
pkill -f "Unity"
```

## 開発ワークフロー推奨コマンド

### 1. 変更後の検証
```bash
# コンパイルチェック
./unity-tools/unity-compile.sh check .
```

### 2. コミット前の確認
```bash
# コンパイルエラーを再確認
./unity-tools/unity-compile.sh check .

# 変更ファイル確認
git status
git diff
```

### 3. タスク完了時
```bash
# 最終コンパイルチェック
./unity-tools/unity-compile.sh trigger . && sleep 3 && ./unity-tools/unity-compile.sh check .

# Unityエディターでの動作確認（手動）
# - Playモードで動作確認
# - 関連シーンでのテスト

# コミット
git add .
git commit -m "説明的なコミットメッセージ"
```

## テスト・ビルド

### Unityエディター内での操作
- **Play Mode**: エディター上部の再生ボタンでゲーム実行
- **Build**: File > Build Settings > Build (WebGL/Windows/macOS選択)

### ログファイル確認
```bash
# Unity Editor.log の場所
tail -f ~/Library/Logs/Unity/Editor.log
```

## その他便利なコマンド

### ファイル数カウント
```bash
# C#ファイル数
find Assets/Scripts -name "*.cs" | wc -l

# 行数カウント
find Assets/Scripts -name "*.cs" -exec wc -l {} + | tail -1
```

### ディレクトリ構造表示
```bash
# tree コマンド（Homebrewでインストール必要）
tree -L 3 Assets/Scripts

# または ls を使った簡易表示
ls -R Assets/Scripts
```

## 注意事項
- Unityエディターが実行中の場合、フォーカス時に自動コンパイルが実行される
- unity-compile.sh trigger はmacOS専用（AppleScript使用）
- コンパイルエラーがある状態でのコミットは避ける
- WebGLビルドは時間がかかるため、頻繁な実行は推奨しない
