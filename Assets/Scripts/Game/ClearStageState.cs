using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ClearStageState : MonoBehaviour
{
    [SerializeField] private Slider slider = null;
    [SerializeField] private List<ClearStageStateIcon> iconList = null;

    private void OnValidate()
    {
        if(slider == null)
            slider = GetComponentInChildren<Slider>();

        if (iconList == null)
            iconList = GetComponentsInChildren<ClearStageStateIcon>().ToList();
    }

    public void SetTimeAttackState(int _count)
    {
        slider.value = 0;
        slider.maxValue = (_count - 1) * 10;

        for (int i= 0; i < iconList.Count; i++)
        {
            if(i < _count)
            {
                iconList[i].SetIcon(i);
            }
            else
            {
                iconList[i].gameObject.SetActive(false);
            }
        }
    }
    

    public void OnIcon(int _value)
    {
        if (_value == 0)
        {
            iconList[_value].SetOn(true);
        }
        else
        {
            slider.DOValue(_value * 10, 0.1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                iconList[_value].SetOn(true);
            });
        }
    }

    public void BindCountdown(CountdownSolveRunTimer timer)
    {
        // 별도 참조만 유지하면 되고, OnTickMs는 컨트롤러에서 SetRemain 호출 중
    }

   
}
