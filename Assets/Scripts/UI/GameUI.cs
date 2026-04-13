using Common.UI;
using UI.Popup;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : BaseUI
{
    [SerializeField] private InGameUI inGameUI = null;

    public InGameUI InGameUI { get => inGameUI; }
}