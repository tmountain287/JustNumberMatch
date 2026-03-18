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
    public class PurchaseCompletePopup : BasePopup
    {     
        [SerializeField] private List<GameObject> itemObjList = null;
        [SerializeField] private Text valueText = null;

        public void Initialize(int _index, string _value)
        {

            for(int i=0; i<itemObjList.Count; i++)
            {
                itemObjList[i].SetActive(i == _index);
            }

            valueText.text = _value;           
        }
    }
}