using Common.Manager;
using Common.UI;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Gostop.UI
{
    public class SuddaInfoPopup : BasePopup
    {
        [SerializeField] private ScrollRect scrollRect = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetScrollRect();
            ResolutionManager.Instance.OnChangeResolution.AddListener(SetScrollRect);
            scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0f);
        }

        private void OnDisable()
        {
            ResolutionManager.Instance.OnChangeResolution.RemoveListener(SetScrollRect);
        }

        private void SetScrollRect()
        {
            float contentHeight = scrollRect.content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;

            scrollRect.vertical = contentHeight > viewportHeight;

            if(!scrollRect.vertical)
            {
                scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, 0f);
            }
        }
    }
}