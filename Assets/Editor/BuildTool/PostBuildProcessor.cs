#if UNITY_IOS && UNITY_EDITOR_OSX
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public class PostBuildProcessor
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS) return;

        string podfilePath = Path.Combine(path, "Podfile");
        string podfileContent = @"
source 'https://cdn.cocoapods.org/'

platform :ios, '13.0'
use_frameworks! :linkage => :static

target 'UnityFramework' do
  pod 'Firebase/Auth', '10.17.0'
  pod 'Firebase/Core', '10.17.0'
  pod 'Firebase/Messaging', '10.17.0'
  pod 'Firebase/Analytics', '10.17.0'
  pod 'GoogleSignIn', '6.2.4'
  pod 'Google-Mobile-Ads-SDK', '~> 12.12.0'
  pod 'GoogleUserMessagingPlatform', '3.0.0'

  //# Unity Ads Mediation Adapter (includes UnityAds automatically)
  pod 'GoogleMobileAdsMediationUnity', '~> 4.16'
end

target 'Unity-iPhone' do
end
";

        File.WriteAllText(podfilePath, podfileContent);
        UnityEngine.Debug.Log("✅ [PostBuild] Podfile written.");

        RunCommand("/opt/homebrew/lib/ruby/gems/3.4.0/bin/pod", "install", path);
    }

    private static void RunCommand(string cmd, string args, string workingDir)
    {
        var process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"cd '{workingDir}' && '{cmd}' {args}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.StartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
        process.StartInfo.EnvironmentVariables["COCOAPODS_DISABLE_STATS"] = "true";

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.Log("🔧 pod: " + e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogWarning("⚠️ pod error: " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
            UnityEngine.Debug.LogError("❌ [PostBuild] pod install failed with exit code " + process.ExitCode);
        else
            UnityEngine.Debug.Log("✅ [PostBuild] pod install 성공!");
    }
}
#endif
