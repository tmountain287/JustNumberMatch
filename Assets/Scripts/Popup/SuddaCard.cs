using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuddaCard : MonoBehaviour
{
    [SerializeField] private Transform pivot = null;
    [SerializeField] private Image image = null;

    [SerializeField] private Image shadow = null;

    private float flipDuration = 0.3f;
    private bool isFront = false;

    public Tween FlipCard(int _sn = -1)
    {
        if (_sn == -1 && !isFront)
        {
            return null; // 아무 동작도 하지 않을 경우 `null` 반환
        }

        // 카드가 반으로 접히는 효과
        return pivot.DOScaleX(0.2f, flipDuration * 0.2f).OnUpdate(() =>
            {
                UpdateShadowPos();
            })
            .OnComplete(() =>
            {
                UpdateShadowPos();
                if (isFront)
                {
                    SetBackside();
                }
                else
                {
                    SetCard(_sn);
                }
                pivot.DOScaleX(1, flipDuration * 0.8f).OnUpdate(() =>
                {
                    UpdateShadowPos();
                }).SetAutoKill(true);
            });
    }

    public void SetCard(int _sn)
    {
        image.sprite = Resources.Load<Sprite>(string.Format("Image/SuddaCards/Card{0:D2}", _sn + 1));
        isFront = true;
        UpdateShadowPos();
    }

    public void SetBackside()
    {
        pivot.localScale = Vector3.one;
        image.sprite = Resources.Load<Sprite>("Image/SuddaCards/Card52");
        isFront = false;
        UpdateShadowPos();
    }

    private void UpdateShadowPos()
    {
        shadow.transform.localPosition = new(pivot.localScale.x * 3.2f, -3.2f, 0);
    }

    public void SetShadow(bool _flag)
    {
        shadow.gameObject.SetActive(_flag);
    }
}
