using UnityEditor;
using System.IO;

/// <summary>
/// 命令行构建脚本 — 供 build_and_deploy.bat 调用
/// </summary>
public static class BuildScript
{
    public static void BuildWebGL()
    {
        // 设置场景
        string[] scenes = new[]
        {
            "Assets/Scenes/SampleScene.scene"
        };

        // 输出路径
        string outputPath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            "WebGL"
        );

        // Player Settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.memorySize = 256;
        PlayerSettings.productName = "打字逃生游戏";

        // 执行构建
        var report = BuildPipeline.BuildPlayer(
            scenes,
            outputPath,
            BuildTarget.WebGL,
            BuildOptions.None
        );

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }
}
