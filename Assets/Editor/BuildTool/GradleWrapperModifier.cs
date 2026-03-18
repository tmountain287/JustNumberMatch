//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEditor.Build;
//using UnityEditor.Build.Reporting;
//using System.IO;

//public class GradleWrapperModifier : IPreprocessBuildWithReport
//{
//    public int callbackOrder => 0;

//    public void OnPreprocessBuild(BuildReport report)
//    {
//        if (report.summary.platform != BuildTarget.Android)
//            return;

//        string wrapperPath = Path.Combine("Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle", "gradle", "wrapper", "gradle-wrapper.properties");

//        if (!File.Exists(wrapperPath))
//        {
//            UnityEngine.Debug.LogWarning("gradle-wrapper.properties 파일을 찾을 수 없습니다.");
//            return;
//        }

//        string[] lines = File.ReadAllLines(wrapperPath);

//        string correctVersion = "8.1.1";
//        string distUrl = $"distributionUrl=https\\://services.gradle.org/distributions/gradle-{correctVersion}-all.zip";

//        bool replaced = false;
//        for (int i = 0; i < lines.Length; i++)
//        {
//            if (lines[i].StartsWith("distributionUrl="))
//            {
//                lines[i] = distUrl;
//                replaced = true;
//                break;
//            }
//        }

//        if (!replaced)
//        {
//            using StreamWriter writer = File.AppendText(wrapperPath);
//            writer.WriteLine(distUrl);
//        }
//        else
//        {
//            File.WriteAllLines(wrapperPath, lines);
//        }

//        UnityEngine.Debug.Log("✅ gradle-wrapper.properties 수정 완료: " + distUrl);
//    }
//}
//#endif
