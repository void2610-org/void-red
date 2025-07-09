#!/bin/bash

# Rider's dotnet SDK を使用
export PATH="/Applications/Rider.app/Contents/lib/ReSharperHost/macos-arm64/dotnet:$HOME/.dotnet/tools:$PATH"

echo "=== ReSharper 命名規則チェック（ローカルテスト） ==="

# inspectcode実行
echo "命名規則チェック実行中..."
jb inspectcode Assembly-CSharp.csproj \
  --output=resharper-inspection-report.xml \
  --format=xml \
  --include="Assets/Scripts/**/*.cs" \
  --exclude="**/Library/**;**/Temp/**;**/obj/**;**/bin/**;**/Packages/**;**/Samples/**" \
  --severity=WARNING \
  --no-build \
  --disable-settings-layers=GlobalAll \
  --properties:Configuration=Debug 2>/dev/null || true

# 結果分析
if [ -f "resharper-inspection-report.xml" ]; then
  echo ""
  echo "検査レポート生成完了"
  
  # SerializeFieldを除外した実際の命名規則違反をカウント
  ALL_VIOLATIONS=$(grep 'InconsistentNaming' resharper-inspection-report.xml | grep 'File="Assets\\Scripts.*\.cs"' || echo "")
  
  if [ -n "$ALL_VIOLATIONS" ]; then
    ACTUAL_VIOLATIONS=$(echo "$ALL_VIOLATIONS" | grep -v 'SerializeField' | wc -l)
    TOTAL_VIOLATIONS=$(echo "$ALL_VIOLATIONS" | wc -l)
    SERIALIZED_VIOLATIONS=$((TOTAL_VIOLATIONS - ACTUAL_VIOLATIONS))
    
    echo ""
    echo "📊 命名規則違反統計:"
    echo "  全体: $TOTAL_VIOLATIONS 件"
    echo "  SerializeField: $SERIALIZED_VIOLATIONS 件（Unity慣習により除外）"
    echo "  修正すべき違反: $ACTUAL_VIOLATIONS 件"
    
    if [ $ACTUAL_VIOLATIONS -gt 0 ]; then
      echo ""
      echo "❌ 修正すべき命名規則違反:"
      echo "$ALL_VIOLATIONS" | grep -v 'SerializeField' | \
        sed 's/.*File="\([^"]*\)".*Line="\([^"]*\)".*Message="\([^"]*\)".*/\1:\2 → \3/' | head -10
    else
      echo ""
      echo "✅ 修正すべき命名規則違反なし"
    fi
  else
    echo ""
    echo "✅ 命名規則違反なし"
  fi
else
  echo ""
  echo "⚠️ 検査レポートが生成されませんでした"
fi