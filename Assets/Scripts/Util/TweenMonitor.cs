using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

#if UNITY_EDITOR
[System.Serializable]
public class TweenInfo
{
    public string id;
    public Object target;
}

public class TweenMonitor : MonoBehaviour
{
    public List<TweenInfo> playingTweenInfos = new();

    public void Refresh()
    {
        playingTweenInfos.Clear();

        var tweens = DOTween.PlayingTweens();
        if (tweens != null)
        {
            foreach (var tween in tweens)
            {
                Object unityTarget = tween.target as Object;

                playingTweenInfos.Add(new TweenInfo()
                {
                    id = tween.id != null ? tween.id.ToString() : "None",
                    target = unityTarget
                });
            }
        }
    }
}
#endif
