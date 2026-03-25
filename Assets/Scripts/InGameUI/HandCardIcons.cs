using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

namespace Gostop.UI
{
    public class HandCardIcons : MonoBehaviour
    {
        [SerializeField] private List<HandCardIcon> iconList = null;
        [SerializeField] private Transform root = null;

        [SerializeField] float moveDistance = -10f;
        [SerializeField] float duration = 0.5f;
        [SerializeField] float delay = 0.2f;

        private Sequence sequence = null;

        private void OnValidate()
        {
            if (root == null)
            {
                root = transform.Find("Root");
            }

            if (iconList == null)
            {
                iconList = root.GetComponentsInChildren<HandCardIcon>().ToList();
            }
        }

        private void OnDisable()
        {
            if(sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }
            root.localPosition = Vector3.zero;
            gameObject.SetActive(false);

        }

        public void SetHandCardIcons(List<HandCardIcon.Type> _iconTypeList)
        {
            root.localPosition = Vector3.zero;

            gameObject.SetActive(true);

            iconList.ForEach(icon => icon.OffIcon());

            for (int i = 0; i < _iconTypeList.Count; i++)
            {
                iconList[i].SetIcon(_iconTypeList[i]);
            }

            if (sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }

            sequence = DOTween.Sequence();
            sequence.Append(root.DOLocalMoveY(moveDistance, duration).SetEase(Ease.InOutSine)); // 위로 이동
            sequence.Append(root.DOLocalMoveY(0, duration).SetEase(Ease.InOutSine)); // 아래로 이동
            sequence.AppendInterval(delay); // 0.1초 멈춤
            sequence.SetLoops(-1); // 무한 루프

            sequence.Play(); // 트윈 실행
        }
    }
}