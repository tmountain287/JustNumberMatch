#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

public static class PostProcessBuild_Provisioning
{
    [PostProcessBuild(1000)]
    public static void OnPostProcess(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) return;

        var settings = AssetDatabase.LoadAssetAtPath<ProvisioningSettings>(
            "Assets/Editor/BuildTool/ProvisioningSettings.asset"
        );

        if (settings == null)
        {
            Debug.LogWarning("ProvisioningSettings.asset not found!");
            return;
        }

        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
#endif

        // 🟡 수동 서명 설정
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_STYLE", "Manual");
        proj.SetBuildProperty(mainTargetGuid, "PROVISIONING_PROFILE_SPECIFIER", settings.provisioningProfileSpecifier);
        proj.SetBuildProperty(mainTargetGuid, "DEVELOPMENT_TEAM", settings.developmentTeam);
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_IDENTITY", "Apple Development"); // 확실히 설정

        // 🟡 자동 서명 비활성화 (유니티에서 자동 서명 체크되었을 수도 있음)
        proj.SetBuildProperty(mainTargetGuid, "ENABLE_BITCODE", "NO"); // Bitcode 비활성화도 같이 하면 안정적

        proj.WriteToFile(projPath);
        Debug.Log("✅ Provisioning 설정 적용 완료 (ScriptableObject)");
    }
}
#endif
