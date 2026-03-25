using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ProloguePopupPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup back = null;
        [SerializeField, TextArea] private string strDialogue = null;
        [SerializeField] private Text dialogueText = null;

        [SerializeField] private List<Image> images = null;
        [SerializeField] private List<BreathingIdle> breathingIdles = null;
        [SerializeField] private Text skipMessage = null;
        [SerializeField] private string strSkipMessage = "터치하여 넘기기";

        private Coroutine typingCoroutine;
        private int touchCount = 0;
        private bool isTyping = false;
        private Tween fadeTween;
        private Action nextAction = null;        

        private void OnEnable()
        {
            touchCount = 0;
            isTyping = false;

            back.alpha = 0f;

            dialogueText.text = "";

            fadeTween = back.DOFade(1f, 2.0f);
            StartTyping();

            breathingIdles?.ForEach(b => b.enabled = false);

            if (images != null)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    int index = i;
                    images[i].color = Color.black;
                    images[i].gameObject.SetActive(true);
                    images[i].DOColor(Color.white, 0.5f).SetDelay(0.5f * i + 0.3f).OnComplete(() =>
                    {                      
                        breathingIdles[index].enabled = true;
                    });
                }
            }

            skipMessage.text = "터치하여 빨리 넘기기";
        }

        private void OnDisable()
        {
            dialogueText.text = "";
            gameObject.SetActive(false);
        }

        public void SetNextAction(Action _action)
        {
            nextAction = _action;
        }


        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (touchCount == 0)
                {
                    // 페이드 도중 클릭 → 즉시 완료 처리
                    if (fadeTween != null && fadeTween.IsActive())
                    {
                        fadeTween.Kill(); // 페이드 즉시 중단
                        back.alpha = 1f;
                    }

                    if (isTyping)
                    {
                        CompleteTypingInstantly();
                    }

                    if (images != null)
                    {
                        for (int i = 0; i < images.Count; i++)
                        {
                            int index = i;
                            images[i].color = Color.white;
                            if(breathingIdles[i].enabled != false)
                            {
                                DOVirtual.DelayedCall(0.05f * i, () =>
                                {
                                    breathingIdles[index].enabled = true;
                                });
                            }
                        }
                    }

                    skipMessage.text = strSkipMessage;
                }
                else if (touchCount == 1)
                {
                    nextAction?.Invoke();
                }
            }
        }

        private void StartTyping()
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypingCoroutine());
        }

        private IEnumerator TypingCoroutine()
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char c in strDialogue)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.1f);
            }

            isTyping = false;
            touchCount = 1;
            skipMessage.text = strSkipMessage;
        }

        private void CompleteTypingInstantly()
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = strDialogue;
            isTyping = false;
            touchCount = 1;
            skipMessage.text = strSkipMessage;
        }
    }
}