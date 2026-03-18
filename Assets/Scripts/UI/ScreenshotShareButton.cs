using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotShareButton : MonoBehaviour
{
    [SerializeField] private Button button = null;

    private void OnValidate()
    {
        if(button == null)
            button = GetComponent<Button>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            StartCoroutine(TakeScreenshotAndShare());
        });
    }

    private string MakeSampleScreenShotImage()
    {
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = Path.Combine(Application.temporaryCachePath, filename);
        File.WriteAllBytes(filePath, ss.EncodeToPNG());

        Destroy(ss);

        return filePath;
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();

        int w = Screen.width;
        int h = Screen.height;

        Texture2D ss = new Texture2D(w, h, TextureFormat.RGBA32, false);

        var prev = RenderTexture.active;
        RenderTexture.active = null;

        ss.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        ss.Apply();

        RenderTexture.active = prev;

        byte[] pngBytes = ss.EncodeToPNG();
        Destroy(ss);

        string filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

#if UNITY_EDITOR
        // ✅ 에디터: 저장 위치 선택
        string path = EditorUtility.SaveFilePanel(
            "Save Screenshot",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            filename,
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, pngBytes);
            Debug.Log($"[Screenshot] Saved (Editor): {path}");
            EditorUtility.RevealInFinder(path);
        }
#else
        // ✅ 빌드: 캐시에 저장 후 공유
        string path = Path.Combine(Application.temporaryCachePath, filename);
        File.WriteAllBytes(path, pngBytes);

        new NativeShare()
            .AddFile(path)
            .SetSubject(filename)
            .SetCallback((result, shareTarget) =>
                Debug.Log($"Share result: {result}, target: {shareTarget}"))
            .Share();
#endif
    }
}