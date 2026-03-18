using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour
{
    [SerializeField] private Image backImage = null;
    [SerializeField] private List<Sprite> sprites = null; 

    [SerializeField] private Text valueText = null;
    [SerializeField] private List<GameObject> iconList = null;
    [SerializeField] private Text multipleText = null;

    private Sequence seq = null;
    private int value = 0;

    public void SetRewardItem(ItemType _itemType, int _value, bool _getReady = false)
    {
        value = _value;
        for(int i = 0; i < iconList.Count; i++)
        {
            iconList[i].SetActive(i == (int)_itemType);
            valueText.text = _value.ToString();
        }

        backImage.sprite = _getReady ? sprites[1] : sprites[0];
    }
    
    /// <param name="_multiplier">배수 (예: 2 = 2배, 5 = 5배)</param>
    public void Play(int _multiplier = 2)
    {
        if (multipleText != null)
        {
            multipleText.text = "x" + _multiplier;
            multipleText.transform.localScale = Vector3.one * 3f;
            var c = multipleText.color;
            c.a = 1f;
            multipleText.color = c;
            multipleText.gameObject.SetActive(true);
        }

        valueText.transform.localScale = Vector3.one;

        seq?.Kill();
        seq = DOTween.Sequence();

        seq.Append(multipleText.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack, 1.7f));
        seq.Join(valueText.transform.DOScale(1.3f, 0.15f).SetDelay(0.2f).SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        valueText.transform.DOScale(1f, 0.15f).SetEase(Ease.InSine);
                        valueText.text = (value * _multiplier).ToString();
                    }));

        seq.Append(multipleText.transform.DOScale(1.6f, 0.35f).SetEase(Ease.InQuad));
        seq.Join(multipleText.DOFade(0f, 0.35f));

        seq.OnComplete(() =>
        {
            multipleText.gameObject.SetActive(false);
        });
    }
}