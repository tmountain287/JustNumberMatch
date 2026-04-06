using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlatformAdID
{
    public string Platform;
    public string InterstitialAdUnitId;
    public string RewardedAdUnitId;
    public string BannerAdUnitId;
}

[CreateAssetMenu(fileName = "GoogleAdsConfig", menuName = "Google/GoogleAdsConfig", order = 0)]
public class GoogleAdsConfig : ScriptableObject
{
    [Header("안드로이드 Ad Unit IDs (실서비스 광고 ID 사용)")]
    [SerializeField] private List<PlatformAdID> adsIDList;

    public PlatformAdID GetAdsID()
    {
        return adsIDList.Where(x => x.Platform == AppDefine.Platform).FirstOrDefault();
    }
}
