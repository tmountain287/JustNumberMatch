using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AppDefine
{
#if UNITY_ANDROID 
    public static string STORE_APP_URL = $"https://play.google.com/store/apps/details?id={Application.identifier}";
#elif UNITY_IOS
    public static string STORE_APP_URL = "https://apps.apple.com/app/id6749435135";
#else
    public static string STORE_APP_URL = $"https://play.google.com/store/apps/details?id={Application.identifier}";
#endif

    public static string SERVICE_URL = "https://kingdomhub.kr/service";
    public static string PRIVACY_URL = "https://kingdomhub.kr/privacy";
    public static string SUPPORT_URL = "https://cafe.naver.com/f-e/cafes/31534655/articles/3?boardtype=L&menuid=2&referrerAllArticles=false";

#if UNITY_ANDROID 
    public static string Platform = "AOS";
#elif UNITY_IOS
    public static string Platform = "IOS";
#else
    public static string Platform = "AOS";
#endif
} 