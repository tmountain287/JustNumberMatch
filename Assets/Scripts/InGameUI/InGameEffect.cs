using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class InGameEffect : MonoBehaviour
    {
        [SerializeField] private RectTransform target = null;
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private Image image = null;

        private Action onDisableAction = null;
        private Sequence seq;
        public void OnComplete()
        {
            onDisableAction?.Invoke();
        }

        //public void OnEnable()
        //{
        //    PlayEffect();
        //}

        public void OnEffect(int _characterID, Action _onDisableAction)
        {
            onDisableAction = _onDisableAction;

            if (_characterID != -1 && image != null)
            {
                CharacterResManager.Instance.SetImage(image, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(_characterID).resource, CharacterImage.Type.Effect);
                PlayEffect();
            }
        }

        public void PlayEffect()
        {
            if (seq != null && seq.IsActive()) seq.Kill();
            Vector2 offScreenPos = new Vector2(200f, 0);
            Vector2 centerPos = Vector2.zero;

            target.anchoredPosition = offScreenPos;
            canvasGroup.alpha = 0f;

            seq = DOTween.Sequence();

            seq.Append(target.DOAnchorPos(centerPos, 0.5f).SetEase(Ease.OutBack)) // 팟 하고 이동
               .Join(canvasGroup.DOFade(1f, 0.5f)) // 알파 0 -> 1
               .AppendInterval(1.5f) // 1초간 유지
               .Append(canvasGroup.DOFade(0f, 0.8f)); // 알파 0으로

            seq.Play();
        }
    }
}