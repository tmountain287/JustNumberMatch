
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CNetDocument
{    
    public static CInGame InGame = null;

    /// <summary>서버 공지 등(로컬 빌드에서는 비어 있음). GostopLocal 흐름과 맞추기 위한 큐.</summary>
    public static readonly Queue<object> NoticeInfoQueue = new();

    public static void UpdateInGame()
    {
        InGame?.ProcessEvent();
    }
    
    public static void ClearInGameEvent()
    {
        InGame?.ClearEvent();
    }
}