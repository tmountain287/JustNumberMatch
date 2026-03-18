//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;
//using UnityEngine;

//public class AndroidToolPathSetter : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        if (report.summary.platform != BuildTarget.Android)
//            return;

//        // 🖥️ macOS / Windows 분기
//#if UNITY_EDITOR_OSX
//        string jdkPath = "/Library/Java/JavaVirtualMachines";
//        string sdkPath = "/Users/hongjinpyo/SDK/SDK_2";
//        string gradlePath = "/Users/hongjinpyo/SDK/gradle-8.2-all/gradle-8.2";
//#elif UNITY_EDITOR_WIN
//        string jdkPath = @"C:\SDK\OpenJDK_2";
//        string sdkPath = @"C:\SDK\SDK_2";
//        string gradlePath = @"C:\SDK\gradle-8.2-all\gradle-8.2";
//#else
//        Debug.LogWarning("플랫폼이 macOS나 Windows가 아님. Android 경로 자동 설정 생략");
//        return;
//#endif

//        Debug.Log($"📦 [AndroidToolPathSetter] 경로 설정 중...\nJDK: {jdkPath}\nSDK: {sdkPath}\nGradle: {gradlePath}");

//        EditorPrefs.SetBool("JdkUseEmbedded", false);
//        EditorPrefs.SetBool("SdkUseEmbedded", false);
//        EditorPrefs.SetBool("GradleUseEmbedded", false);

//        // 🔧 경로 설정 (ProjectSettings.asset이 아닌 EditorPrefs로 설정됨)
//        //EditorPrefs.SetString("JdkPath", jdkPath);
//        //EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
//        //EditorPrefs.SetString("GradlePath", gradlePath);

//        // ✅ 사용자가 직접 확인 가능하게 로그 출력
//        Debug.Log("✅ Android tool paths 설정 완료 (플랫폼별 자동 적용됨)");
//    }
//}
//#endif
