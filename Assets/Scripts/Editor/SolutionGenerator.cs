#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// CI用ソリューションファイル生成ヘルパー
/// dotnet-formatがUnity生成の.csprojを使用してスタイルチェックを行うために必要
/// </summary>
public static class SolutionGenerator
{
    /// <summary>
    /// ソリューションファイルを生成してUnityを終了する
    /// CIから -executeMethod SolutionGenerator.Generate で呼び出される
    /// </summary>
    public static void Generate()
    {
        // IDE統合パッケージ（com.unity.ide.visualstudio等）を使用してソリューションを生成
        Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
        UnityEngine.Debug.Log("[SolutionGenerator] ソリューションファイルの生成が完了しました");
        EditorApplication.Exit(0);
    }
}
#endif
