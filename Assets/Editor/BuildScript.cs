using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build entry points for batchmode: -executeMethod BuildScript.BuildAndroid / BuildScript.BuildWebGL
public static class BuildScript
{
    const string AndroidOutputPath = "Builds/Android/BlastGame.apk";
    const string WebGLOutputPath = "Builds/WebGL";

    public static void BuildAndroid()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AndroidOutputPath));
        Build(AndroidOutputPath, BuildTarget.Android);
    }

    public static void BuildWebGL()
    {
        Directory.CreateDirectory(WebGLOutputPath);
        Build(WebGLOutputPath, BuildTarget.WebGL);
    }

    static void Build(string outputPath, BuildTarget target)
    {
        var options = new BuildPlayerOptions
        {
            scenes = Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.None,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"Build result: {summary.result}, size: {summary.totalSize} bytes, errors: {summary.totalErrors}");
        EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
