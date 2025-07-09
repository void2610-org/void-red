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
    # SerializeFieldフィールドを除外するための関数
    filter_serialize_field_violations() {
      local violations="$1"
      local filtered_violations=""
      
      while IFS= read -r line; do
        if [[ -n "$line" ]]; then
          # ファイルパスと行番号を抽出
          local file=$(echo "$line" | sed 's/.*File="\([^"]*\)".*/\1/' | sed 's/\\\\/\//g')
          local line_num=$(echo "$line" | sed 's/.*Line="\([^"]*\)".*/\1/')
          
          # WindowsパスをUnixパスに変換
          local unix_file=$(echo "$file" | sed 's/Assets\\Scripts/Assets\/Scripts/g' | sed 's/\\\\/\//g')
          
          # 該当行にSerializeFieldがあるかチェック
          if [[ -f "$unix_file" ]]; then
            # 該当行とその前後の行をチェック（SerializeFieldは通常フィールドの直前の行にある）
            local has_serialize_field=$(sed -n "$((line_num-2)),$((line_num+2))p" "$unix_file" 2>/dev/null | grep -q "SerializeField"; echo $?)
            if [[ $has_serialize_field -ne 0 ]]; then
              # SerializeFieldが見つからない場合のみ追加
              filtered_violations="$filtered_violations$line"$'\n'
            fi
          else
            # ファイルが見つからない場合は保持
            filtered_violations="$filtered_violations$line"$'\n'
          fi
        fi
      done <<< "$violations"
      
      echo "$filtered_violations"
    }
    
    FILTERED_VIOLATIONS=$(filter_serialize_field_violations "$ALL_VIOLATIONS")
    ACTUAL_VIOLATIONS=$(echo "$FILTERED_VIOLATIONS" | grep -c "InconsistentNaming" || echo "0")
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
      echo "$FILTERED_VIOLATIONS" | grep "InconsistentNaming" | \
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