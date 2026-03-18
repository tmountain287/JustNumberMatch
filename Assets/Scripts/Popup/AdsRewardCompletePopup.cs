using Common.Manager;
using Common.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class AdsRewardCompletePopup : BasePopup
    {   
        [SerializeField] private Text valueText = null;

        protected override void Start()
        {
            base.Start();
            valueText.text = ConfigData.AdsRewardGold.ToString();
        }
    }
}