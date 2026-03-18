//#if UNITY_EDITOR
//using System.IO;
//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;
//using UnityEditor.Callbacks;
//using UnityEngine;

//public class GradlePropertiesModifier : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        if (report.summary.platform != BuildTarget.Android)
//            return;

//        // 🔍 gradle.properties 가능한 경로들
//        string[] possiblePaths = new string[]
//        {
//            Path.Combine("Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle", "gradle.properties"),
//            Path.Combine("Temp", "gradleOut", "gradle.properties")
//        };

//        string gradlePropertiesPath = null;
//        foreach (string path in possiblePaths)
//        {
//            if (File.Exists(path))
//            {
//                gradlePropertiesPath = path;
//                break;
//            }
//        }

//        if (string.IsNullOrEmpty(gradlePropertiesPath))
//        {
//            Debug.LogWarning("⚠️ gradle.properties 파일을 찾을 수 없습니다.");
//            return;
//        }

//        string[] lines = File.ReadAllLines(gradlePropertiesPath);

//        // 💻 OS에 따라 분기
//        string jdkPath;
//#if UNITY_EDITOR_OSX
//        jdkPath = "/Users/hongjinpyo/SDK/jdk-17.0.14+7/Contents/Home";
//#else
//        jdkPath = "C:/SDK/microsoft-jdk-17.0.14-windows-x64/jdk-17.0.14+7";
//#endif

//        bool found = false;
//        for (int i = 0; i < lines.Length; i++)
//        {
//            if (lines[i].StartsWith("org.gradle.java.home="))
//            {
//                lines[i] = $"org.gradle.java.home={jdkPath}";
//                found = true;
//                break;
//            }
//        }

//        if (!found)
//        {
//            using StreamWriter writer = File.AppendText(gradlePropertiesPath);
//            writer.WriteLine($"org.gradle.java.home={jdkPath}");
//        }
//        else
//        {
//            File.WriteAllLines(gradlePropertiesPath, lines);
//        }

//        Debug.Log($"✅ gradle.properties JDK 경로 설정 완료: {jdkPath}");
//    }
//}
//#endif
