#!/bin/bash

# デバッグ用のテストスクリプト
echo "=== ReSharper Debug Test ==="

# 1. プロジェクトファイルの存在チェック
echo "1. Project file check:"
ls -la Assembly-CSharp.csproj

# 2. C#ファイルの存在チェック
echo -e "\n2. C# files check:"
find Assets/Scripts -name "*.cs" | wc -l
echo "CS files found: $(find Assets/Scripts -name "*.cs" | wc -l)"

# 3. プロジェクトファイルのCompileセクションをチェック
echo -e "\n3. Project file compile entries (first 10):"
grep -m 10 'Compile Include' Assembly-CSharp.csproj || echo "No Compile Include found"

# 4. 除外パターンが問題かテスト - 除外なしで実行
echo -e "\n4. Testing cleanupcode without exclude patterns..."
if command -v jb &> /dev/null; then
    echo "ReSharper CLI found, testing..."
    jb cleanupcode Assembly-CSharp.csproj \
      --profile="Built-in: Reformat Code" \
      --no-build \
      --properties:Configuration=Debug \
      --verbosity=INFO || echo "Test failed"
else
    echo "ReSharper CLI not found (jb command not available)"
fi

# 5. 現在の除外パターンでテスト
echo -e "\n5. Testing with current exclude patterns..."
if command -v jb &> /dev/null; then
    jb cleanupcode Assembly-CSharp.csproj \
      --exclude="**/*.csproj;**/*.xml;**/*.txt;**/*.md;**/*.json" \
      --profile="Built-in: Reformat Code" \
      --no-build \
      --properties:Configuration=Debug \
      --verbosity=INFO || echo "Test with exclude patterns failed"
else
    echo "ReSharper CLI not found"
fi

# 6. MSBuildプロジェクトの検証
echo -e "\n6. MSBuild project validation..."
if command -v dotnet &> /dev/null; then
    dotnet build Assembly-CSharp.csproj --verbosity minimal --nologo || echo "MSBuild validation failed"
else
    echo "dotnet CLI not found"
fi

echo -e "\n=== Debug Test Complete ==="