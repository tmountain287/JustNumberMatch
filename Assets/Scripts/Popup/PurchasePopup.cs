using Common.Manager;
using Common.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class PurchasePopup : BasePopup
    {
        [SerializeField] private Button okButton = null;
        [SerializeField] private List<GameObject> itemObjList = null;

        [SerializeField] private Text valueText = null;
        [SerializeField] private Text priceText = null;

        private Action onOK = null;

        protected override void Start()
        {
            base.Start();

            okButton.onClick.AddListener(() =>
            {
                ClosePopup();
                onOK?.Invoke();
            });            
        }

        public void Initialize(ItemType _itemType, string _price, string _value, Action _onOK)
        {
            onOK = _onOK;

            for(int i=0; i<itemObjList.Count; i++)
            {
                itemObjList[i].SetActive(i == (int)_itemType);
            }

            valueText.text = _value;
            priceText.text = _price;
        }
    }
}