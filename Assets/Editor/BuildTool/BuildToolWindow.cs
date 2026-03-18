#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public class BuildToolWindow : EditorWindow
{
    private enum BuildPlatform { Android, iOS }
    private BuildPlatform selectedPlatform = BuildPlatform.Android;

    // --- Android/iOS 버전 입력값 ---
    private string androidBundleVersion = "1.0";  // Android: versionName == PlayerSettings.bundleVersion
    private int androidVersionCode = 1;        // Android: VersionCode(번들 넘버)
    private string iosBundleVersion = "1.0";  // iOS: CFBundleShortVersionString == PlayerSettings.bundleVersion
    private string iosBuildNumber = "1";      // iOS: CFBundleVersion

    private bool buildAsAppBundle = false; // Android AAB 빌드 여부
    private string buildName = "MyApp";
    private bool openAfterBuild = true;  // 빌드 후 파인더/탐색기 열기
    private bool cleanBuild = false; // 클린 빌드 토글

    // --- 마지막 빌드 정보 파일 경로 ---
    private static readonly string LastDir = "Assets/Editor/BuildTool/";
    private static readonly string LastAndroidBundleVersionPath = Path.Combine(LastDir, "LastAndroidBundleVersion.txt");
    private static readonly string LastAndroidVersionCodePath = Path.Combine(LastDir, "LastAndroidVersionCode.txt");
    private static readonly string LastIOSBundleVersionPath = Path.Combine(LastDir, "LastIOSBundleVersion.txt");
    private static readonly string LastIOSBuildNumberPath = Path.Combine(LastDir, "LastIOSBuildNumber.txt");

    // --- 설정 저장 경로 ---
    private static readonly string ConfigPath = Path.Combine(LastDir, "BuildToolConfig.json");

    [Serializable] private class DefineSymbolEntry { public string symbol; public bool enabled = true; }

    [Serializable]
    private class BuildToolConfig
    {
        public string buildName = "MyApp";

        public string androidBundleVersion = "1.0.0";
        public int androidVersionCode = 1;
        public bool buildAsAppBundle = false;

        public string iosBundleVersion = "1.0.0";
        public string iosBuildNumber = "1";

        public bool openAfterBuild = true;
        public bool cleanBuild = false;

        public List<DefineSymbolEntry> defineSymbols = new List<DefineSymbolEntry>();
    }

    private List<DefineSymbolEntry> defineSymbols = new List<DefineSymbolEntry>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Build Tool")]
    public static void ShowWindow() => GetWindow<BuildToolWindow>("Build Tool");

    private void OnEnable()
    {
        Directory.CreateDirectory(LastDir);
        LoadSettings();

        if (string.IsNullOrWhiteSpace(buildName) || buildName == "MyApp")
            buildName = GetDefaultBuildNameFromBundleId();

        if (!defineSymbols.Any())
            defineSymbols.Add(new DefineSymbolEntry { symbol = string.Empty, enabled = true });
    }

    private void OnDisable() => SaveSettings();

    private void OnGUI()
    {
        GUILayout.Label("📦 플랫폼 선택", EditorStyles.boldLabel);
        selectedPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("플랫폼", selectedPlatform);

        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        buildName = EditorGUILayout.TextField("📛 빌드 이름", buildName);
        if (GUILayout.Button("← bundle id", GUILayout.Width(110)))
            buildName = GetDefaultBuildNameFromBundleId();
        EditorGUILayout.EndHorizontal();

        // --- 플랫폼별 버전 입력 ---
        GUILayout.Space(10);
        GUILayout.Label("🔖 버전 설정", EditorStyles.boldLabel);

        if (selectedPlatform == BuildPlatform.Android)
        {
            androidBundleVersion = EditorGUILayout.TextField("Android Bundle Version (versionName)", androidBundleVersion);
            androidVersionCode = EditorGUILayout.IntField("Android VersionCode (번들 넘버)", androidVersionCode);
            buildAsAppBundle = EditorGUILayout.Toggle("AAB(App Bundle) 빌드", buildAsAppBundle);
        }
        else
        {
            iosBundleVersion = EditorGUILayout.TextField("iOS Bundle Version", iosBundleVersion);
            iosBuildNumber = EditorGUILayout.TextField("iOS Build Number", iosBuildNumber);
        }

        // --- 심볼 관리 ---
        GUILayout.Space(12);
        GUILayout.Label("🧩 Scripting Define Symbols", EditorStyles.boldLabel);

        // ScrollView 짝 보장
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(140));
        try
        {
            for (int i = 0; i < defineSymbols.Count; i++)
            {
                var entry = defineSymbols[i];
                EditorGUILayout.BeginHorizontal();
                entry.enabled = EditorGUILayout.Toggle(entry.enabled, GUILayout.Width(20));
                entry.symbol = EditorGUILayout.TextField(entry.symbol);
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    defineSymbols.RemoveAt(i); i--;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }

        if (GUILayout.Button("+ 심볼 추가"))
            defineSymbols.Add(new DefineSymbolEntry { symbol = string.Empty, enabled = true });

        GUILayout.Space(12);

        // 라벨 잘림 방지: 넓은 라벨 폭 (짝 보장)
        float old = EditorGUIUtility.labelWidth;
        try
        {
            EditorGUIUtility.labelWidth = position.width - 60f;
            openAfterBuild = EditorGUILayout.Toggle("✅ 빌드 후 파인더/탐색기에서 열기", openAfterBuild);
            cleanBuild = EditorGUILayout.Toggle("🧹 클린 빌드(캐시/산출물 삭제 후 빌드)", cleanBuild);
        }
        finally
        {
            EditorGUIUtility.labelWidth = old;
        }

        GUILayout.Space(12);
        if (GUILayout.Button("🔥 빌드 시작", GUILayout.Height(40)))
        {
            SaveSettings();

            // OnGUI 프레임에서 무거운 작업을 실행하지 않음 (레이아웃 에러 방지)
            var platformSnapshot = selectedPlatform;
            var needClean = cleanBuild;

            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (needClean) DoClean(platformSnapshot);
                    Build();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };
        }

        // --- 마지막 빌드 정보 표시 ---
        GUILayout.Space(18);
        GUILayout.Label("📚 마지막 빌드 정보", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Android Bundle Version", GetLastValue(LastAndroidBundleVersionPath, "(없음)"));
            EditorGUILayout.LabelField("Android VersionCode", GetLastValue(LastAndroidVersionCodePath, "(없음)"));
            GUILayout.Space(8);
            EditorGUILayout.LabelField("iOS Bundle Version", GetLastValue(LastIOSBundleVersionPath, "(없음)"));
            EditorGUILayout.LabelField("iOS Build Number", GetLastValue(LastIOSBuildNumberPath, "(없음)"));
        }
    }

    private void Build()
    {
        string buildFolder = "Builds/" + selectedPlatform;
        Directory.CreateDirectory(buildFolder);

        var group = selectedPlatform == BuildPlatform.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        var target = selectedPlatform == BuildPlatform.Android ? BuildTarget.Android : BuildTarget.iOS;

        // 플랫폼 별 PlayerSettings 적용
        if (selectedPlatform == BuildPlatform.Android)
        {
            PlayerSettings.bundleVersion = androidBundleVersion;                 // versionName
            PlayerSettings.Android.bundleVersionCode = androidVersionCode;       // VersionCode
            EditorUserBuildSettings.buildAppBundle = buildAsAppBundle;
        }
        else // iOS
        {
            PlayerSettings.bundleVersion = iosBundleVersion;                    // CFBundleShortVersionString
            PlayerSettings.iOS.buildNumber = string.IsNullOrWhiteSpace(iosBuildNumber) ? "1" : iosBuildNumber.Trim();
        }

        // 심볼 적용
        ApplyDefineSymbols(group);

        // 출력 파일/폴더 이름
        string extension, fileName;
        if (selectedPlatform == BuildPlatform.Android)
        {
            extension = buildAsAppBundle ? ".aab" : ".apk";
            string date = DateTime.Now.ToString("yyyyMMdd");
            fileName = $"{buildName}_{date}{extension}";
        }
        else
        {
            extension = string.Empty; // Xcode 프로젝트 폴더
            fileName = buildName;
        }

        string fullPath = Path.Combine(buildFolder, SanitizeFileName(fileName));

        var buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = fullPath,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            // 마지막 값 저장(성공 시에만)
            if (selectedPlatform == BuildPlatform.Android)
            {
                WriteText(LastAndroidBundleVersionPath, androidBundleVersion);
                WriteText(LastAndroidVersionCodePath, androidVersionCode.ToString());
            }
            else
            {
                WriteText(LastIOSBundleVersionPath, iosBundleVersion);
                WriteText(LastIOSBuildNumberPath, PlayerSettings.iOS.buildNumber);
            }

            if (openAfterBuild)
            {
                string outPath = summary.outputPath;
                if (File.Exists(outPath) || Directory.Exists(outPath))
                    EditorUtility.RevealInFinder(outPath);
                else
                    EditorUtility.RevealInFinder(Path.GetDirectoryName(fullPath));
            }
            EditorUtility.DisplayDialog("✅ 빌드 성공", $"파일 위치:\n{summary.outputPath}", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("❌ 빌드 실패", summary.result.ToString(), "확인");
        }
    }

    // ---------- Clean ----------
    private void DoClean(BuildPlatform platform)
    {
        try
        {
            EditorUtility.DisplayProgressBar("Clean Build", "정리 중...", 0.1f);

            // 1) (Android) 기존 Gradle 프로젝트가 있으면 gradlew clean 먼저
            if (platform == BuildPlatform.Android)
            {
                string gradleDir = Path.Combine("Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle");
                string gradlew = Path.Combine(gradleDir, Application.platform == RuntimePlatform.WindowsEditor ? "gradlew.bat" : "gradlew");
                if (File.Exists(gradlew))
                {
                    TryRun(gradlew, "clean", gradleDir, 30_000);
                }
            }

            EditorUtility.DisplayProgressBar("Clean Build", "캐시/산출물 삭제 중...", 0.6f);

            // 2) 프로젝트 로컬 캐시/산출물 삭제
            SafeDeleteDir("Library/Bee");
            SafeDeleteDir("Library/Il2cppBuildCache");
            SafeDeleteDir("Library/Gradle"); // 구버전 잔재
            SafeDeleteDir("Temp");
            SafeDeleteDir("Builds/" + platform);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BuildTool] Clean 실패: " + e.Message);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    private static void SafeDeleteDir(string path)
    {
        try { if (Directory.Exists(path)) FileUtil.DeleteFileOrDirectory(path); }
        catch (Exception e) { Debug.LogWarning($"[BuildTool] 디렉토리 삭제 실패: {path} => {e.Message}"); }
    }

    private static void TryRun(string file, string args, string workdir, int timeoutMs)
    {
        try
        {
            var p = new Process();
            p.StartInfo.FileName = file;
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = workdir;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                try { new Process { StartInfo = new ProcessStartInfo("chmod", $"+x \"{file}\"") { UseShellExecute = false } }.Start(); } catch { }
            }

            p.Start();
            string so = p.StandardOutput.ReadToEnd();
            string se = p.StandardError.ReadToEnd();
            p.WaitForExit(timeoutMs);

            Debug.Log($"[BuildTool] run: {file} {args}\n{so}\n{se}");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BuildTool] 외부 명령 실행 실패: " + e.Message);
        }
    }

    // ---------- Defines ----------
    private void ApplyDefineSymbols(BuildTargetGroup group)
    {
        string current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        var existing = new HashSet<string>(
            current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()),
            StringComparer.Ordinal);

        var toAdd = new HashSet<string>(
            defineSymbols.Where(e => e.enabled)
                         .Select(e => e.symbol?.Trim())
                         .Where(s => !string.IsNullOrEmpty(s)),
            StringComparer.Ordinal);

        var toRemove = new HashSet<string>(
            defineSymbols.Where(e => !e.enabled)
                         .Select(e => e.symbol?.Trim())
                         .Where(s => !string.IsNullOrEmpty(s)),
            StringComparer.Ordinal);

        existing.ExceptWith(toRemove);
        existing.UnionWith(toAdd);

        string merged = string.Join(";", existing.OrderBy(s => s, StringComparer.Ordinal));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, merged);
        Debug.Log("[BuildTool] Defines => " + merged);
    }

    private string[] GetEnabledScenes() =>
        EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    // ---------- Settings save/load ----------
    private void SaveSettings()
    {
        var cfg = new BuildToolConfig
        {
            buildName = buildName,

            androidBundleVersion = androidBundleVersion,
            androidVersionCode = androidVersionCode,
            buildAsAppBundle = buildAsAppBundle,

            iosBundleVersion = iosBundleVersion,
            iosBuildNumber = iosBuildNumber,

            openAfterBuild = openAfterBuild,
            cleanBuild = cleanBuild,

            defineSymbols = defineSymbols.Where(s => s != null)
                                               .Select(s => new DefineSymbolEntry { symbol = s.symbol?.Trim(), enabled = s.enabled })
                                               .ToList()
        };
        WriteText(ConfigPath, JsonUtility.ToJson(cfg, true));
        AssetDatabase.Refresh();
    }

    private void LoadSettings()
    {
        if (!File.Exists(ConfigPath)) return;

        try
        {
            var cfg = JsonUtility.FromJson<BuildToolConfig>(File.ReadAllText(ConfigPath));

            buildName = cfg.buildName;

            androidBundleVersion = cfg.androidBundleVersion;
            androidVersionCode = cfg.androidVersionCode;
            buildAsAppBundle = cfg.buildAsAppBundle;

            iosBundleVersion = cfg.iosBundleVersion;
            iosBuildNumber = cfg.iosBuildNumber;

            openAfterBuild = cfg.openAfterBuild;
            cleanBuild = cfg.cleanBuild;

            defineSymbols = cfg.defineSymbols ?? new List<DefineSymbolEntry>();
        }
        catch (Exception e)
        {
            Debug.LogWarning("BuildTool 설정 불러오기 실패: " + e.Message);
        }
    }

    // ---------- Helpers ----------
    private static void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, text ?? string.Empty);
    }

    private static string GetLastValue(string path, string fallback) =>
        File.Exists(path) ? File.ReadAllText(path).Trim() : fallback;

    private string GetDefaultBuildNameFromBundleId()
    {
        var group = selectedPlatform == BuildPlatform.Android ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        var bundleId = PlayerSettings.GetApplicationIdentifier(group);
        if (string.IsNullOrWhiteSpace(bundleId)) return SanitizeFileName(Application.productName);

        var last = bundleId.Split('.').Where(s => !string.IsNullOrWhiteSpace(s)).LastOrDefault();
        return SanitizeFileName(string.IsNullOrWhiteSpace(last) ? Application.productName : last);
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "App";
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name.Trim();
    }
}
#endif
