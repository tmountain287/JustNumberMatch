#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

public static class PostProcessBuild_AutoSigning
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        string targetGuid = proj.GetUnityMainTargetGuid();
#else
        string targetGuid = proj.TargetGuidByName("Unity-iPhone");
#endif

        // ✅ Automatically manage signing
        proj.SetBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Automatic");

        // ✅ Team ID 설정 (Apple Developer ID)
        proj.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", "G5A8HM5UW4"); // ← 당신의 Team ID로 변경

        // ❌ 수동 설정 제거 (자동 서명과 충돌함)
        proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", "");
        proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE", "");
        proj.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", "");

        proj.WriteToFile(projPath);
        Debug.Log("✅ Automatically manage signing 적용 완료");
    }
}
#endif
