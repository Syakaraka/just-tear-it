using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildTools
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroidAPK()
    {
        // 设置产品名称
        PlayerSettings.productName = "撕就对了";

        // 设置Bundle Identifier
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.teargame.justtearit");

        // 设置Android目标架构为ARM64 + ARMv7
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

        // 设置最低API版本
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

        string[] scenes = {
            "Assets/Scenes/BootScene.unity",
            "Assets/Scenes/MainScene.unity",
            "Assets/Scenes/LevelSelectScene.unity",
            "Assets/Scenes/GameScene.unity"
        };

        string outputPath = Path.Combine(Application.dataPath, "../Build/JustTearIt.apk");

        // 确保输出目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("开始构建Android APK...");
        var result = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("构建结果: " + result);
    }
}
