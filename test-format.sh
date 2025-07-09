#!/bin/bash

# Rider's dotnet SDK を使用
export PATH="/Applications/Rider.app/Contents/lib/ReSharperHost/macos-arm64/dotnet:$HOME/.dotnet/tools:$PATH"

echo "=== ReSharper フォーマットチェック（ローカルテスト） ==="

# フォーマット実行
echo "フォーマット実行中..."
jb cleanupcode Assembly-CSharp.csproj \
  --include="Assets/Scripts/**/*.cs" \
  --exclude="**/Library/**;**/Temp/**;**/obj/**;**/bin/**;**/Packages/**;**/Samples/**" \
  --profile="Built-in: Reformat Code" \
  --no-build \
  --properties:Configuration=Debug

# 変更確認
echo ""
echo "変更された可能性のあるファイル:"
git status --porcelain

# 差分表示
if ! git diff --quiet; then
  echo ""
  echo "フォーマット変更の詳細:"
  git diff --name-only
  echo ""
  echo "❌ フォーマット違反が検出されました"
  
  echo ""
  echo "具体的な変更内容（最初の20行）:"
  git diff | head -20
else
  echo ""
  echo "✅ フォーマット違反なし"
fi