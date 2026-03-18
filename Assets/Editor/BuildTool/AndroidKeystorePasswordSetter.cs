#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class AndroidKeystorePasswordSetter : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        // ✅ 이미 선택된 keystore 경로와 alias 그대로 사용하고,
        // ✅ 비밀번호만 코드로 설정
        PlayerSettings.Android.keystorePass = "348863361#Lee";
        PlayerSettings.Android.keyaliasPass = "348863361#Lee";
    }
}
#endif