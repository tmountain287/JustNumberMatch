using Common.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnStart()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneStart()
    {
        //CharacterResManager.Instance.LoadAllCharacterVisuals();
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void TrackObjects()
    //{
    //    SceneManager.sceneUnloaded += scene =>
    //    {
    //        foreach (var go in GameObject.FindObjectsOfType<GameObject>())
    //        {
    //            if (go.scene.name == scene.name)
    //                Debug.LogWarning($"Still exists after scene unload: {go.name}", go);
    //        }
    //    };
    //}
}