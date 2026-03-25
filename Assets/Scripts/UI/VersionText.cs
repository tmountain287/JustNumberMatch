using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour
{
    [SerializeField] private Text version = null;
    [SerializeField] private string prefix = null;

    private void OnValidate()
    {
        if(version == null)
        {
            version = GetComponent<Text>();
        }
    }

    private void Start()
    {
        string r = "";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        r = "_D";
#endif
        string rawVersion = Application.version;

        if (float.TryParse(rawVersion, out float versionFloat))
        {
            string formatted = versionFloat.ToString("F2"); // 소수점 둘째 자리까지
            version.text = $"{prefix}v{formatted}{r}";
        }
        else
        {
            version.text = $"{prefix}v0.00{r}"; // 파싱 실패 시 fallback
        }
    }
}
