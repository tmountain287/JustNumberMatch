using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class SpaceKeyExample : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSpacePressed();
        }
    }

    void OnSpacePressed()
    {
        StartCoroutine(TakeScreenshotAndShare());
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