#!/bin/bash

# デバッグ用のSerializeFieldフィルタリングテスト
export PATH="/Applications/Rider.app/Contents/lib/ReSharperHost/macos-arm64/dotnet:$HOME/.dotnet/tools:$PATH"

echo "=== SerializeFieldフィルタリングテスト ==="

# 違反例を取得
ALL_VIOLATIONS=$(grep 'InconsistentNaming' resharper-inspection-report.xml | grep 'File="Assets\\Scripts.*\.cs"' | head -5)

echo "テスト対象の違反（最初の5件）:"
echo "$ALL_VIOLATIONS"
echo ""

# 各違反をテスト
while IFS= read -r line; do
  if [[ -n "$line" ]]; then
    echo "処理中: $line"
    
    # ファイルパスと行番号を抽出
    file=$(echo "$line" | sed 's/.*File="\([^"]*\)".*/\1/')
    line_num=$(echo "$line" | sed 's/.*Line="\([^"]*\)".*/\1/')
    
    # WindowsパスをUnixパスに変換
    unix_file=$(echo "$file" | sed 's/\\\\/\//g')
    
    echo "  ファイル: $unix_file"
    echo "  行番号: $line_num"
    
    if [[ -f "$unix_file" ]]; then
      echo "  該当行周辺の内容:"
      sed -n "$((line_num-2)),$((line_num+2))p" "$unix_file" 2>/dev/null | nl -v$((line_num-2))
      
      # SerializeFieldチェック
      has_serialize_field=$(sed -n "$((line_num-2)),$((line_num+2))p" "$unix_file" 2>/dev/null | grep -q "SerializeField"; echo $?)
      if [[ $has_serialize_field -eq 0 ]]; then
        echo "  → SerializeFieldが検出されました（除外対象）"
      else
        echo "  → SerializeFieldなし（違反として保持）"
      fi
    else
      echo "  → ファイルが見つかりません"
    fi
    echo ""
  fi
done <<< "$ALL_VIOLATIONS"