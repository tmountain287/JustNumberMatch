using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectWarning : MonoBehaviour
{
    [SerializeField] private Image mark = null;
    [SerializeField] private Image markBlur = null;
    [SerializeField] private Image title = null;
    [SerializeField] private Image titleBlur = null;

    private void OnEnable()
    {
        Sequence seq = DOTween.Sequence();
        
        seq.Append(titleBlur.rectTransform.DOScale(Vector3.one, 0.15f).OnComplete(() =>
            {
                title.gameObject.SetActive(true);
            }))
           .Append(titleBlur.DOFade(0, 0.1f));
        
        seq.AppendInterval(0.5f)
           .Append(title.rectTransform.DOScaleX(0, 0.15f).OnStart(()=>
           {
               markBlur.SetAlpha(0.7f);
           }).OnComplete(()=>
           {
               //mark.gameObject.SetActive(true);
           }))
           .Append(markBlur.DOFade(0, 0.15f));

        seq.SetId(this);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
        titleBlur.rectTransform.localScale = new Vector3(5, 5, 5);
        titleBlur.SetAlpha(1);

        title.rectTransform.localScale = new Vector3(1, 1, 1);
        title.gameObject.SetActive(false);

        mark.gameObject.SetActive(false);
        markBlur.SetAlpha(0);
    }
}