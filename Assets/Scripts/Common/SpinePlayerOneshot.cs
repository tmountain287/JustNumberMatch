using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.Events;

namespace Common.UI
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class SpinePlayerOneshot : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private SkeletonGraphic skeletonGraphic = null;
        [SerializeField] private string animationName = "";
        [SerializeField] private bool IsAutoPlay = true;
        [SerializeField] private bool IsAutoDisable = false;

        [SerializeField] private UnityEvent OnCompleteEvent = null;
        #endregion

        private void OnValidate()
        {
            if (skeletonGraphic == null)
            {
                skeletonGraphic = GetComponent<SkeletonGraphic>();
            }
        }

        private void OnDisable()
        {
            
        }

        private void OnEnable()
        {
            skeletonGraphic.Initialize(true);
            if(IsAutoPlay)
                PlayAnimation();
        }

        public void PlayAnimation(System.Action onComplete = null)
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.AnimationState.SetEmptyAnimations(0.1f);
            var entry = skeletonGraphic.AnimationState.SetAnimation(0, animationName, false);
            
            entry.Complete += _ =>
            {
                OnCompleteEvent?.Invoke();
                onComplete?.Invoke();
                if (IsAutoDisable)
                {
                    gameObject.SetActive(false);
                }
            };            
        }

        public void CancelAnimation()
        {
            if (skeletonGraphic == null || skeletonGraphic.AnimationState == null)
                return;

            skeletonGraphic.AnimationState.ClearTrack(0); // 실제 애니메이션 중단
        }
    }
}
