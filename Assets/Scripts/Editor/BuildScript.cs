using UnityEditor;
using System.IO;
using System;

/// <summary>
/// 命令行构建脚本 — 供 build_and_deploy.bat 调用
/// 支持 Windows Standalone 构建（团结引擎无 WebGL 模块时的回退方案）
/// </summary>
public static class BuildScript
{
    public static void BuildWebGL()
    {
        Build(BuildTarget.WebGL, "WebGL");
    }

    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "WindowsBuild");
    }

    static void Build(BuildTarget target, string outputDir)
    {
        string[] scenes = new[] { "Assets/Scenes/SampleScene.scene" };

        string outputPath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            outputDir
        );

        try
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone, target);
        }
        catch { /* 忽略切换失败 */ }

        PlayerSettings.productName = "打字逃生游戏";

        var report = BuildPipeline.BuildPlayer(
            scenes,
            outputPath,
            target,
            BuildOptions.None
        );

        Debug.Log($"Build result: {report.summary.result}");
        Debug.Log($"Build output: {outputPath}");
        Debug.Log($"Total errors: {report.summary.totalErrors}");
        Debug.Log($"Total warnings: {report.summary.totalWarnings}");

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(0);
        else
            EditorApplication.Exit(1);
    }
}
