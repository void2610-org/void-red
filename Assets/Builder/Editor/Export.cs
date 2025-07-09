using Packages.Rider.Editor.ProjectGeneration;
using UnityEditor;

public static class Export
{
    [MenuItem("Tools/Generate Solution Files")]
    public static void VSSolution()
    {
        AssetDatabase.Refresh();
        var generator = new ProjectGeneration();
        generator.Sync();
        UnityEngine.Debug.Log("Solution files generated successfully!");
    }
}