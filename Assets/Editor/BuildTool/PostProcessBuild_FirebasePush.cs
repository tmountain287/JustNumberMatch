#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Reflection;

public static class PostProcessBuild_FirebasePush
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS) return;

        SetInfoPlist(path);
        SetEntitlementsAndCapabilities(path);
        AddFrameworks(path);               // 🔁 두 타깃 모두에 추가
        SetDeploymentTarget(path, "13.0"); // (권장) 최소 iOS 13
        // EnableObjCExceptions(path, true); // (선택) @try/@catch 쓰려면 켜기
    }

    private static void AddFrameworks(string path)
    {
        var projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
        string unityFrameworkGuid = proj.GetUnityFrameworkTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
        string unityFrameworkGuid = mainTargetGuid;
#endif
        void Add(string target, string fw, bool weak = false)
        {
            proj.AddFrameworkToProject(target, fw, weak);
        }

        // ✅ 두 타깃 모두에 추가
        Add(mainTargetGuid,       "AuthenticationServices.framework");
        Add(unityFrameworkGuid,   "AuthenticationServices.framework");
        Add(mainTargetGuid,       "UserNotifications.framework");
        Add(unityFrameworkGuid,   "UserNotifications.framework");

        proj.WriteToFile(projPath);
        Debug.Log("✅ Linked frameworks to BOTH targets (Unity-iPhone & UnityFramework)");
    }

    private static void SetInfoPlist(string path)
    {
        string plistPath = Path.Combine(path, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var root = plist.root;

        root.SetBoolean("FirebaseAppDelegateProxyEnabled", true);
        root.SetString("NSUserNotificationUsageDescription", "앱에서 알림을 보내기 위해 권한이 필요합니다.");

        var bg = root.values.ContainsKey("UIBackgroundModes")
            ? root["UIBackgroundModes"].AsArray()
            : root.CreateArray("UIBackgroundModes");
        bool has = false;
        foreach (var v in bg.values) if (v.AsString() == "remote-notification") { has = true; break; }
        if (!has) bg.AddString("remote-notification");

        plist.WriteToFile(plistPath);
        Debug.Log("✅ Info.plist 설정 완료");
    }

    private static void SetEntitlementsAndCapabilities(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
#endif

        string entName = "Unity.entitlements";
        string entRel  = Path.Combine("Unity-iPhone", entName);
        string entFull = Path.Combine(path, entRel);

        var ent = new PlistDocument();
        if (File.Exists(entFull)) ent.ReadFromFile(entFull); else ent.Create();

#if SERVICE
        ent.root.SetString("aps-environment", "production");
#else
        ent.root.SetString("aps-environment", "development");
#endif
        const string key = "com.apple.developer.applesignin";
        if (!ent.root.values.ContainsKey(key))
        {
            var arr = ent.root.CreateArray(key);
            arr.AddString("Default");
        }
        else
        {
            var arr = ent.root[key].AsArray();
            bool hasDefault = false;
            foreach (var v in arr.values) if (v.AsString() == "Default") { hasDefault = true; break; }
            if (!hasDefault) arr.AddString("Default");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(entFull));
        ent.WriteToFile(entFull);
        Debug.Log("✅ Unity.entitlements 설정 완료");

        var cap = new ProjectCapabilityManager(projPath, entRel, "Unity-iPhone");
        cap.AddInAppPurchase();
        cap.AddPushNotifications(true);
        cap.AddGameCenter();
        cap.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        TryAddSignInWithAppleCapability(cap);
        cap.WriteToFile();

        proj.AddFile(entRel, entRel);
        proj.SetBuildProperty(mainTargetGuid, "CODE_SIGN_ENTITLEMENTS", entRel);
        proj.WriteToFile(projPath);
        Debug.Log("✅ Capabilities 설정 완료");
    }

    private static void TryAddSignInWithAppleCapability(ProjectCapabilityManager cap)
    {
        try
        {
            var mi = typeof(ProjectCapabilityManager).GetMethod("AddSignInWithApple",
                BindingFlags.Public | BindingFlags.Instance);
            if (mi != null) { mi.Invoke(cap, null); Debug.Log("✅ AddSignInWithApple() 적용"); }
            else { Debug.Log("ℹ️ AddSignInWithApple() 없음 — entitlements로 대체 완료"); }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("⚠️ AddSignInWithApple 실패: " + e.Message);
        }
    }

    private static void SetDeploymentTarget(string path, string minVersion)
    {
        var projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);
#if UNITY_2019_3_OR_NEWER
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
        string unityFrameworkGuid = proj.GetUnityFrameworkTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
        string unityFrameworkGuid = mainTargetGuid;
#endif
        proj.SetBuildProperty(mainTargetGuid,     "IPHONEOS_DEPLOYMENT_TARGET", minVersion);
        proj.SetBuildProperty(unityFrameworkGuid, "IPHONEOS_DEPLOYMENT_TARGET", minVersion);
        proj.WriteToFile(projPath);
        Debug.Log($"✅ Deployment Target = iOS {minVersion}");
    }

    // (선택) @try/@catch 쓰려면 켜기
    private static void EnableObjCExceptions(string path, bool enable)
    {
        var projPath = PBXProject.GetPBXProjectPath(path);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);
#if UNITY_2019_3_OR_NEWER
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
        string unityFrameworkGuid = proj.GetUnityFrameworkTargetGuid();
#else
        string mainTargetGuid = proj.TargetGuidByName("Unity-iPhone");
        string unityFrameworkGuid = mainTargetGuid;
#endif
        string val = enable ? "YES" : "NO";
        proj.SetBuildProperty(mainTargetGuid,     "GCC_ENABLE_OBJC_EXCEPTIONS", val);
        proj.SetBuildProperty(unityFrameworkGuid, "GCC_ENABLE_OBJC_EXCEPTIONS", val);
        proj.WriteToFile(projPath);
        Debug.Log($"ℹ️ Objective-C Exceptions = {val}");
    }
}
#endif
