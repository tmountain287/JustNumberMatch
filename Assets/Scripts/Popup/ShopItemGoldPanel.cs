using Common.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ShopItemGoldPanel : MonoBehaviour
    {        
        [SerializeField] private Button button = null;
        [SerializeField] private Text goldText = null;
        [SerializeField] private Text priceText = null;
        [SerializeField] private Text saleText = null;

        [SerializeField] private GameObject saleBase = null;
        [SerializeField] private GameObject saleOn = null;

        private Action onClick = null;

        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                onClick?.Invoke();
            });
        }

        public void SetPanel(ProductCatalogData _data, Action _onClick)
        {
            onClick = _onClick;

            goldText.text = $"{_data.gold.FormatComma()} 골드";
            priceText.text = $"{_data.price.FormatComma()} 원";

            saleBase.SetActive(_data.sale == 0);
            saleOn.SetActive(_data.sale > 0);

            if (_data.sale > 0)
            {
                saleText.text = $"{_data.sale}% 할인";
            }
        }
    }
}
