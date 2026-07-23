using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class MultiPlatformBuilderWindow : EditorWindow {
    private bool buildWindows = true;
    private bool buildLinux = true;
    private bool buildMacOS = true;
    private bool cleanBuild = false;

    [MenuItem("Window/Multi-Platform Builder")]
    public static void ShowWindow() { GetWindow<MultiPlatformBuilderWindow>("Platform Builder"); }

    private void OnGUI() {
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);

        cleanBuild = EditorGUILayout.Toggle("Clean Build", cleanBuild);

        EditorGUILayout.Space();
        GUILayout.Label("Platforms to Build", EditorStyles.label);
        buildWindows = EditorGUILayout.Toggle("Windows (64-bit)", buildWindows);
        buildLinux = EditorGUILayout.Toggle("Linux (64-bit)", buildLinux);
        buildMacOS = EditorGUILayout.Toggle("macOS (Universal)", buildMacOS);

        EditorGUILayout.Space();

        if (GUILayout.Button("Run Selected Builds", GUILayout.Height(40))) { ExecuteBuilds(); }
    }

    private void ExecuteBuilds() {
        string[] enabledScenePaths = GetEnabledScenePaths();

        if (enabledScenePaths.Length == 0) {
            Debug.LogError("Build failed: No scenes are enabled in the Build Settings.");
            return;
        }

        Debug.Log("Starting sequential builds...");

        if (buildWindows) {
            BuildForPlatform(enabledScenePaths, $"Builds/Windows/{PlayerSettings.productName}.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }

        if (buildLinux) {
            BuildForPlatform(enabledScenePaths, $"Builds/Linux/{PlayerSettings.productName}.x86_64", BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        }

        if (buildMacOS) {
            BuildForPlatform(enabledScenePaths, $"Builds/macOS/{PlayerSettings.productName}.app", BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
        }

        Debug.Log("Build process completed! Check the 'Builds' folder.");
    }

    private void BuildForPlatform(string[] scenePaths, string buildLocation, BuildTargetGroup targetGroup, BuildTarget targetPlatform) {
        if (cleanBuild) { PrepareCleanDirectory(buildLocation); }

        Debug.Log($"Starting build for {targetPlatform}...");

        BuildOptions options = BuildOptions.None;
        if (cleanBuild) { options |= BuildOptions.CleanBuildCache; }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions {
            scenes = scenePaths,
            locationPathName = buildLocation,
            targetGroup = targetGroup,
            target = targetPlatform,
            options = options,
        };

        BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary buildSummary = buildReport.summary;

        if (buildSummary.result == BuildResult.Succeeded) {
            Debug.Log($"Build for {targetPlatform} succeeded: {buildSummary.totalSize} bytes in {buildSummary.totalTime}.");
            CleanBuildOutput(buildLocation);
        }
        else { Debug.LogError($"Build for {targetPlatform} was not successful! ({buildSummary.result.ToString()})"); }
    }

    private static void CleanBuildOutput(string buildLocation) {
        string outputDir = Path.GetDirectoryName(buildLocation);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir)) { return; }

        string appName = Path.GetFileNameWithoutExtension(buildLocation);
        if (string.IsNullOrEmpty(appName)) { return; }

        string buildRoot = Directory.GetParent(outputDir)!.FullName;
        string platformName = new DirectoryInfo(outputDir).Name;
        string doNotShipDir = Path.Combine(buildRoot, "DoNotShip", platformName);
        Directory.CreateDirectory(doNotShipDir);

        try {
            string[] folderNames = new[] {
                $"{appName}_BurstDebugInformation_DoNotShip",
                $"{appName}_BackUpThisFolder_ButDontShipItWithYourGame"
            };

            foreach (string folderName in folderNames) {
                string sourcePath = Path.Combine(outputDir, folderName);
                if (!Directory.Exists(sourcePath)) { continue; }
                
                string destPath = Path.Combine(doNotShipDir, folderName);
                if (Directory.Exists(destPath)) { Directory.Delete(destPath, true); }
                
                Directory.Move(sourcePath, destPath);
                Debug.Log($"Moved folder to DoNotShip: {destPath}");
            }

            foreach (FileInfo file in new DirectoryInfo(outputDir).GetFiles("*.pdb")) {
                string destPath = Path.Combine(doNotShipDir, file.Name);
                if (File.Exists(destPath)) { File.Delete(destPath); }

                file.MoveTo(destPath);
                Debug.Log($"Moved symbol file to DoNotShip: {destPath}");
            }
        }
        catch (System.Exception e) { Debug.LogWarning($"Build cleanup warning: {e.Message}"); }
    }

    private void PrepareCleanDirectory(string buildLocation) {
        string directoryPath = Path.GetDirectoryName(buildLocation);
        if (string.IsNullOrEmpty(directoryPath)) { return; }
        if (Directory.Exists(directoryPath)) {
            Debug.Log($"Cleaning directory: {directoryPath}");
            Directory.Delete(directoryPath, true);
        }

        Directory.CreateDirectory(directoryPath);
    }

    private string[] GetEnabledScenePaths() {
        List<string> scenePathsList = new List<string>();
        foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes) {
            if (editorScene.enabled) { scenePathsList.Add(editorScene.path); }
        }

        return scenePathsList.ToArray();
    }
}