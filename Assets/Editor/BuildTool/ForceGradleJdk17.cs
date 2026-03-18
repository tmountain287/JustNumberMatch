#if UNITY_EDITOR && UNITY_EDITOR_OSX
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

[InitializeOnLoad]
public static class ForceJdk17Early
{
    private const string Jdk17Home = "/Library/Java/JavaVirtualMachines/temurin-17.jdk/Contents/Home";

    static ForceJdk17Early()
    {
        try
        {
            if (!Directory.Exists(Jdk17Home)) { Debug.LogWarning("JDK 17 not found: " + Jdk17Home); return; }

            SetEnv("JAVA_HOME", Jdk17Home);
            SetEnv("JDK_HOME", Jdk17Home);
            SetEnv("SKIP_JDK_VERSION_CHECK", "true");

            var bin = Path.Combine(Jdk17Home, "bin");
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            if (!path.StartsWith(bin, StringComparison.Ordinal))
                SetEnv("PATH", bin + Path.PathSeparator + path);

            Debug.Log("[ForceJdk17Early] JAVA_HOME -> " + Jdk17Home);
        }
        catch (Exception e)
        {
            Debug.LogError("[ForceJdk17Early] Failed to set JDK env: " + e);
        }
    }

    private static void SetEnv(string key, string value)
        => Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process);

    [MenuItem("Tools/Android/Print Java Env")]
    private static void PrintJavaEnv()
    {
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        var jdkHome = Environment.GetEnvironmentVariable("JDK_HOME");
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        var head = path.Split(Path.PathSeparator)[0];

        Debug.Log("JAVA_HOME=" + javaHome);
        Debug.Log("JDK_HOME=" + jdkHome);
        Debug.Log("PATH head=" + head);
    }
}
#endif