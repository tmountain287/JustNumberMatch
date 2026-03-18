using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Common.UI
{
    public class SpriteAnimation : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Image imageSc = null;
        [SerializeField] private SpriteAtlas atlas = null;
        [SerializeField] private float fAniSpeed = 0.08f;
        [SerializeField] private bool bLoop = false;
        [SerializeField] private bool bAutoStart = false;
        [SerializeField] private string spriteName = string.Empty;
        [SerializeField] private bool isAutoDisable = false;
        [SerializeField] private float fDisableDelay = 0;
        [SerializeField] private GameObject disableObject = null;
        [SerializeField] private UnityEvent unityEvent = null;
        #endregion

        private Coroutine coProcess = null;
        private bool bPlaying = false;

        // Start is called before the first frame update
        private void OnEnable()
        {
            if (true == bAutoStart)
                PlayAnimation();
        }

        //========================================================================================================================================//
        public void StopAnimation()
        {
            if (true == bPlaying)
            {
                StopCoroutine(coProcess);
                bPlaying = false;
            }
        }
        public void PlayAnimation()
        {
            int iCount = 0;
            string strSpriteName = spriteName + iCount.ToString();
            imageSc.sprite = atlas.GetSprite(strSpriteName);

            coProcess = StartCoroutine(PlayAnimtion());
        }

        //========================================================================================================================================//
        // 루프 카운트?????
        IEnumerator PlayAnimtion()
        {
            imageSc.enabled = true;

            int iMaxCount = atlas.spriteCount;
            int iCount = 0;
            bPlaying = true;
            //int iAniLoopCount = 0;

            while (true)
            {
                if (true == bLoop)
                {
                    if (iMaxCount - 1 <= iCount)
                        iCount = 0;
                }
                else if (false == bLoop)
                {
                    if (iMaxCount - 1 <= iCount)
                        break;
                }
                iCount++;
                string strSpriteName = spriteName + iCount.ToString();
                imageSc.sprite = atlas.GetSprite(strSpriteName);
                yield return new WaitForSeconds(fAniSpeed);
            }
            bPlaying = false;
            unityEvent?.Invoke();
            if (isAutoDisable)
            {
                yield return new WaitForSeconds(fDisableDelay);
                disableObject.SetActive(false);
            }
        }
    }
}