//#if UNITY_IOS
//using UnityEditor;
//using UnityEditor.Callbacks;
//using UnityEditor.iOS.Xcode;
//using System.IO;

//public static class PostBuild_AddPodInstallScript
//{
//    [PostProcessBuild(1000)]
//    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
//    {
//        if (target != BuildTarget.iOS)
//            return;

//        string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
//        PBXProject proj = new PBXProject();
//        proj.ReadFromFile(pbxPath);

//#if UNITY_2019_3_OR_NEWER
//        string targetGuid = proj.GetUnityMainTargetGuid();
//#else
//        string targetGuid = proj.TargetGuidByName("Unity-iPhone");
//#endif

//        // Run Script Phase 추가 (무조건 덮어쓰기)
//        proj.AddShellScriptBuildPhase(
//            targetGuid,
//            "Run Pod Install",       // 표시 이름
//            "/bin/sh",               // 쉘 타입
//            "cd \"${SRCROOT}\"\npod install --repo-update" // 실제 실행할 스크립트
//        );

//        proj.WriteToFile(pbxPath);
//        UnityEngine.Debug.Log("✅ [PostBuild] Xcode에 pod install Run Script 추가 완료");
//    }
//}
//#endif
