using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class SpinePlayer : MonoBehaviour
    {
        [Serializable]
        public class SpineAniInfo
        {
            public string aniationName = "";
            public int loopCount = 1;
        }

        #region Inspector Fields
        [SerializeField] private SkeletonGraphic skeletonGraphic = null;
        [SerializeField] private List<SpineAniInfo> spineAniInfoList = null;
        [SerializeField] private bool isAutoDisable = true;
        #endregion

        private string currentAnimationName = "";
        private int currentAnimationIndex = 0;
        private int currentLoop = 0;
        private bool isEnd = false;

        private void OnValidate()
        {
            if (skeletonGraphic == null)
            {
                skeletonGraphic = GetComponent<SkeletonGraphic>();
            }
        }

        private void OnDisable()
        {
            isEnd = false;
            currentAnimationIndex = 0;
            if(isAutoDisable) gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            PlayNextAnimation();
        }

        public void PlayAnimationSequence(bool _isAni = true)
        {
            if (spineAniInfoList.Count == 0)
            {
                return;
            }

            if(_isAni)
            {
                PlayNextAnimation();
            }
            else
            {
                currentAnimationName = "Idle";
                skeletonGraphic.AnimationState.SetAnimation(0, "Idle", false);
            }
            
            gameObject.SetActive(true);
        }

        private void PlayNextAnimation()
        {
            if (currentAnimationIndex >= spineAniInfoList.Count)
            {
                return;
            }

            var animationInfo = spineAniInfoList[currentAnimationIndex];
            currentLoop = 0;

            PlayAnimation(animationInfo);
        }

        private void PlayAnimation(SpineAniInfo animationInfo)
        {
            if(isEnd)
            {
                return;
            }
            currentAnimationName = animationInfo.aniationName;
            skeletonGraphic.AnimationState.SetAnimation(0, animationInfo.aniationName, false).Complete += entry =>
            {
                currentLoop++;

                if(animationInfo.loopCount == -1)
                {
                    PlayAnimation(animationInfo);
                }
                else
                {
                    if (currentLoop < animationInfo.loopCount)
                    {
                        PlayAnimation(animationInfo);
                    }
                    else
                    {
                        currentAnimationIndex++;
                        PlayNextAnimation();
                    }
                }
            };
        }

        public void PlayEnd()
        {
            if(gameObject.activeSelf && currentAnimationName != "End")
            {
                isEnd = true;
                currentAnimationName = "End";
                skeletonGraphic.AnimationState.SetAnimation(0, currentAnimationName, false).Complete -= null;
                skeletonGraphic.AnimationState.SetAnimation(0, currentAnimationName, false).Complete += entry =>
                {
                    if(isAutoDisable) gameObject.SetActive(false);
                };
            }
        }
    }
}