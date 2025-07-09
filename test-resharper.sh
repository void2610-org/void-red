#!/bin/bash

echo "========================================="
echo "ReSharper ローカルテスト"
echo "========================================="

# Rider's dotnet SDK を使用
export PATH="/Applications/Rider.app/Contents/lib/ReSharperHost/macos-arm64/dotnet:$HOME/.dotnet/tools:$PATH"

# 前提条件チェック
echo "前提条件チェック中..."

if ! command -v dotnet >/dev/null 2>&1; then
    echo "❌ .NET SDK が見つかりません"
    echo "Riderがインストールされていることを確認してください"
    exit 1
fi

if ! command -v jb >/dev/null 2>&1; then
    echo "❌ ReSharper Command Line Tools が見つかりません"
    echo "以下のコマンドでインストールしてください:"
    echo "dotnet tool install -g JetBrains.ReSharper.GlobalTools"
    exit 1
fi

echo "✅ 前提条件OK"
echo ""

# プロジェクトファイル確認
if [ ! -f "Assembly-CSharp.csproj" ]; then
    echo "❌ Assembly-CSharp.csproj が見つかりません"
    echo "Unityプロジェクトのルートディレクトリで実行してください"
    exit 1
fi

echo "✅ プロジェクトファイル確認完了"
echo ""

# 現在の状態を保存
echo "現在のGit状態を保存中..."
git stash push -m "ReSharper test backup" 2>/dev/null || true

# フォーマットテスト
echo "========================================="
echo "1. フォーマットチェックテスト"
echo "========================================="

./test-format.sh

echo ""

# 命名規則テスト  
echo "========================================="
echo "2. 命名規則チェックテスト"
echo "========================================="

./test-naming.sh

echo ""

# 結果まとめ
echo "========================================="
echo "テスト完了"
echo "========================================="

# 変更を元に戻すか確認
echo ""
echo "フォーマット変更を元に戻しますか？ (y/N)"
read -r response
if [[ "$response" =~ ^[Yy]$ ]]; then
    git checkout . 2>/dev/null || true
    echo "✅ 変更を元に戻しました"
else
    echo "変更を保持しました"
fi

# 生成されたファイルをクリーンアップ
if [ -f "resharper-inspection-report.xml" ]; then
    rm -f resharper-inspection-report.xml
    echo "✅ テンポラリファイルをクリーンアップしました"
fi