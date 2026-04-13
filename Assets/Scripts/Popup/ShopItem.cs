using Common.Manager;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Text valueText = null;
    [SerializeField] private Text priceText = null;

    [SerializeField] private Button button = null;

    [SerializeField] private GameObject saleObj = null;
    [SerializeField] private Text saleValueText = null;

    private Action onClick = null;

    private void OnValidate()
    {
        if(button == null)
            button = GetComponent<Button>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private int saleValue = 0;

    public void SetItem(string _price, string _value, int _saleValue, Action _onClick)
    {
        onClick = _onClick;

        if (saleObj != null)
        {
            saleObj.SetActive(_saleValue > 0);
            saleValueText.text = string.Format(LocalizationManager.Instance.GetText("ValueSale"), _saleValue);
            saleValue = _saleValue;
        }

        valueText.text = _value;
        priceText.text = _price;
    }

    private void OnEnable()
    {
        if (saleObj != null)
        {
            saleObj.SetActive(saleValue > 0);
            saleValueText.text = string.Format(LocalizationManager.Instance.GetText("ValueSale"), saleValue);
        }
    }
}