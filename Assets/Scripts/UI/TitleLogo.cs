using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleLogo : MonoBehaviour
{
    [SerializeField] private Image logo = null;

    private void OnEnable()
    {
        logo.DOFade(1, 0.5f).OnComplete(() =>
        {
            for (int i = 0; i < logo.transform.childCount; i++)
            {
                int index = i;
                DOVirtual.DelayedCall(i * 0.1f, () =>
                {
                    logo.transform.GetChild(index).gameObject.SetActive(true);
                });
            }
        });
    }
}
