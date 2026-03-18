using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StagePageItem : MonoBehaviour
{
    [SerializeField] private List<StageButton> elemList = null;

    public List<StageButton> ElemList { get => elemList; }

    private void OnValidate()
    {
        if(elemList == null)
        {
            elemList = GetComponentsInChildren<StageButton>().ToList();
        }
    }



}
