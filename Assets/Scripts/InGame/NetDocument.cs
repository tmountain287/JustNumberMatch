using backend_cli.BackEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CNetDocument
{    
    public static CInGame InGame = null;
    public static Queue<AppPopUpInfo> NoticeInfoQueue { get; set; } = new();

    public static ShowNoticeInfoList ShowNoticeInfoList { get; set; } = null;

    public static void LoadShowNoticeInfoList()
    {
        string s = PlayerPrefs.GetString("ShowNoticeInfoList", "");
        if (!string.IsNullOrEmpty(s))
        {
            ShowNoticeInfoList = JsonUtility.FromJson<ShowNoticeInfoList>(s);
        }
        else
        {
            ShowNoticeInfoList = new();
        }

        string today = DateTime.Now.ToString("yyyyMMdd");

        ShowNoticeInfoList.showNoticeInfos = ShowNoticeInfoList.showNoticeInfos.Where(x => x.today == today).ToList(); //오늘과 값이 다르면 다 지우자
        PlayerPrefs.SetString("ShowNoticeInfoList", JsonUtility.ToJson(ShowNoticeInfoList));
        PlayerPrefs.Save();
    }

    public static void AddShowNoticeInfoList(ShowNoticeInfo _info)
    {
        ShowNoticeInfoList.showNoticeInfos.Add(_info);
        PlayerPrefs.SetString("ShowNoticeInfoList", JsonUtility.ToJson(ShowNoticeInfoList));
        PlayerPrefs.Save();
    }

    public static void UpdateInGame()
    {
        InGame?.ProcessEvent();
    }
    
    public static void ClearInGameEvent()
    {
        InGame?.ClearEvent();
    }
}