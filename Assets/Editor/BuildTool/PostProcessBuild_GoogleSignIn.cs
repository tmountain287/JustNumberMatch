#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using System.Linq;

public static class PostProcessBuild_GoogleSignIn
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        PlistElementDict rootDict = plist.root;

        string googlePlistPath = Directory
            .GetFiles(Application.dataPath, "GoogleService-Info.plist", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(googlePlistPath) || !File.Exists(googlePlistPath))
        {
            UnityEngine.Debug.LogWarning("GoogleService-Info.plist not found in project.");
            return;
        }

        // GoogleService-Info.plist 파싱
        PlistDocument googlePlist = new PlistDocument();
        googlePlist.ReadFromFile(googlePlistPath);

        string reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"].AsString();

        // ✅ CFBundleURLTypes 추가
        PlistElementArray urlTypesArray = null;
        if (rootDict.values.ContainsKey("CFBundleURLTypes"))
        {
            urlTypesArray = rootDict["CFBundleURLTypes"].AsArray();
        }
        else
        {
            urlTypesArray = rootDict.CreateArray("CFBundleURLTypes");
        }

        bool alreadyAdded = false;
        foreach (var el in urlTypesArray.values)
        {
            var dict = el as PlistElementDict;
            if (dict == null) continue;
            var schemes = dict["CFBundleURLSchemes"] as PlistElementArray;
            if (schemes == null) continue;

            foreach (var scheme in schemes.values)
            {
                if (scheme.AsString() == reversedClientId)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (alreadyAdded) break;
        }

        if (!alreadyAdded)
        {
            var dict = urlTypesArray.AddDict();
            var schemes = dict.CreateArray("CFBundleURLSchemes");
            schemes.AddString(reversedClientId);
        }

        // ✅ LSApplicationQueriesSchemes 추가
        string[] schemesToAdd = new[]
        {
            "google",
            "com.google",
            reversedClientId
        };

        PlistElementArray queriesSchemesArray = null;
        if (rootDict.values.ContainsKey("LSApplicationQueriesSchemes"))
        {
            queriesSchemesArray = rootDict["LSApplicationQueriesSchemes"].AsArray();
        }
        else
        {
            queriesSchemesArray = rootDict.CreateArray("LSApplicationQueriesSchemes");
        }

        foreach (var scheme in schemesToAdd)
        {
            bool exists = false;
            foreach (var s in queriesSchemesArray.values)
            {
                if (s.AsString() == scheme)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                queriesSchemesArray.AddString(scheme);
        }

        // 저장
        plist.WriteToFile(plistPath);
        UnityEngine.Debug.Log($"[PostProcess] Info.plist에 Google URL 스킴 및 LSApplicationQueriesSchemes 추가 완료");
    }
}
#endif
