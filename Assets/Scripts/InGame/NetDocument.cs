
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CNetDocument
{    
    public static CInGame InGame = null;   

    public static void UpdateInGame()
    {
        InGame?.ProcessEvent();
    }
    
    public static void ClearInGameEvent()
    {
        InGame?.ClearEvent();
    }
}