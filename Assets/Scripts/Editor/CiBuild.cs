using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class CiBuild
{
    private const string BUILD_NAME = "void-red";

    public static void BuildWindows() => Build(
        BuildTarget.StandaloneWindows64,
        GetBuildOutputPath(Path.Combine("builds", "StandaloneWindows64", $"{BUILD_NAME}.exe")));

    public static void BuildMac() => Build(
        BuildTarget.StandaloneOSX,
        GetBuildOutputPath(Path.Combine("builds", "StandaloneOSX", $"{BUILD_NAME}.app")));

    public static void BuildWebGL()
    {
        // セルフホストランナーで Library が古い WebGL 圧縮設定を保持していると
        // ProjectSettings 側の Brotli 指定が反映されず非圧縮成果物が出るため、
        // ビルド直前に API 経由で強制上書きする。
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;

        // さらに保険として PlayerDataCache は消しておく。
        DeleteIfExists("Library/PlayerDataCache");

        Build(
            BuildTarget.WebGL,
            GetBuildOutputPath(Path.Combine("CIBuilds", "WebGL", "build")));
    }

    private static void DeleteIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void Build(BuildTarget target, string locationPathName)
    {
        ApplyBuildVersion();

        var outputDirectory = Path.GetDirectoryName(locationPathName);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray(),
            locationPathName = locationPathName,
            target = target,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException($"Build failed: {report.summary.result}");
    }

    private static void ApplyBuildVersion()
    {
        var buildVersion = Environment.GetEnvironmentVariable("BUILD_VERSION");
        if (!string.IsNullOrEmpty(buildVersion))
        {
            PlayerSettings.bundleVersion = buildVersion;
        }
    }

    private static string GetBuildOutputPath(string defaultPath)
    {
        var buildOutputPath = Environment.GetEnvironmentVariable("UNITY_BUILD_OUTPUT_PATH");
        return string.IsNullOrEmpty(buildOutputPath) ? defaultPath : buildOutputPath;
    }
}
