#nullable enable
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RFL.Editor
{
    /// <summary>Player builds. Addressables (the string tables) are built first for the target.</summary>
    public static class BuildScript
    {
        [MenuItem("RFL/Build/Windows 64-bit")]
        public static void Windows() =>
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/RobotFightingLeague.exe");

        [MenuItem("RFL/Build/macOS")]
        public static void Mac() =>
            Build(BuildTarget.StandaloneOSX, "Builds/macOS/RobotFightingLeague.app");

        static void Build(BuildTarget target, string path)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone,
                    target
                );
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult content);
            if (!string.IsNullOrEmpty(content.Error))
            {
                Fail("Addressables build failed: " + content.Error);
                return;
            }
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings
                    .scenes.Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray(),
                locationPathName = path,
                target = target,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Fail($"Player build {report.summary.result}: {report.summary.totalErrors} errors");
                return;
            }
            Debug.Log($"Built {target} at {path} ({report.summary.totalSize / (1024 * 1024)} MB).");
        }

        static void Fail(string message)
        {
            Debug.LogError(message);
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
