using Common.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common.Attribute;
using Unity.VisualScripting;

namespace Common.UI
{
    public class BasePopup : MonoBehaviour
    {
        public enum OpenType
        {
            NONE = 0,
            POP = 1,
            LEFT_TO_RIGHT = 2,
            RIGHT_TO_LEFT = 3,
            LEFT_TO_CENTER = 4,
            BOTTOM_TO_CNETER = 5,
            FADE = 6,
            POP_H = 7,
        }

        [SerializeField] private TopUI topUI = null;
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] public PopupType popupType = PopupType.NONE;
        [SerializeField] private bool canBackButton = true;
        [SerializeField] protected Button closeButton = null;
        [SerializeField] protected List<Button> closeButtons = null;
        [SerializeField] protected bool useBack = true;
        [SerializeField] private bool allowDuplicates = false;
        [SerializeField] protected float delay = 0;
        [ConditionalHide("useBack", true, true)]
        [SerializeField] protected float backAlpha = 0.7f;
        [SerializeField] protected OpenType openType = OpenType.NONE;
        [SerializeField] protected Image back = null;
        [SerializeField] protected RectTransform root = null;
        [SerializeField] protected RectTransform rectTransform = null;
              
        public bool CanClose { get; set; } = true;
        public bool CanBackButton { get => canBackButton; }
        public bool AllowDuplicates { get => allowDuplicates; }

        protected Action OnScaleUpdate = null;

        private TopUI currentTopUI = null;

        protected virtual void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => ClosePopup());
            }

            closeButtons?.ForEach(button => button.onClick.AddListener(() => ClosePopup()));
        }

        protected virtual void OnDisable()
        {
            if (topUI != null)
            {               
                UIManager.Instance.TopUI = currentTopUI;
            }
        }

        protected virtual void OnEnable()
        {
            if (topUI != null)
            {
                currentTopUI = UIManager.Instance.TopUI;
                UIManager.Instance.TopUI = topUI;
            }

            SetBlocksRaycasts(false);
            if (useBack)
            {
                back.DOKill();
                back.color = new Color(0, 0, 0, 0);
                back.DOFade(backAlpha, 0.2f).SetDelay(delay);
            }

            root.DOKill();

            if (openType == OpenType.POP)
            {
                root.localScale = Vector3.zero;
                root.DOScale(1f, 0.2f).SetEase(Ease.OutBack).OnUpdate(() =>
                {
                    OnScaleUpdate?.Invoke();
                }).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.POP_H)
            {
                root.localScale = new Vector3(1, 0, 1);
                root.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack).OnUpdate(() =>
                {
                    OnScaleUpdate?.Invoke();
                }).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.LEFT_TO_RIGHT)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;

                root.anchoredPosition = new Vector2(-(parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                Vector2 targetPosition = new Vector2(-(parentWidth - childWidth) * 0.5f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.RIGHT_TO_LEFT)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;

                root.anchoredPosition = new Vector2((parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                Vector2 targetPosition = new Vector2((parentWidth - childWidth) * 0.5f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.LEFT_TO_CENTER)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;

                root.anchoredPosition = new Vector2(-(parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                Vector2 targetPosition = new Vector2(0f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.5f).SetEase(Ease.OutCubic).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.BOTTOM_TO_CNETER)
            {
                float parentHeight = rectTransform.rect.height;
                float childHeight = root.rect.height;

                root.anchoredPosition = new Vector2(root.anchoredPosition.x, -(parentHeight + childHeight) * 0.5f);
                root.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutCubic).OnComplete(() => SetBlocksRaycasts(true));
            }
            else if (openType == OpenType.FADE)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, 1f).OnComplete(() => SetBlocksRaycasts(true));
            }
            else
            {
                if (delay > 0)
                {
                    root.localScale = Vector3.zero;
                    DOVirtual.DelayedCall(delay, () =>
                    {
                        root.localScale = Vector3.one;
                        SetBlocksRaycasts(true);
                    });
                }
                else
                {
                    SetBlocksRaycasts(true);
                }
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public virtual void Close(Action _onClose = null)
        {
            if (useBack)
            {
                back.DOKill();
                back.DOFade(0.0f, 0.2f);
            }
            root.DOKill();
         
            if (openType == OpenType.POP)
            {
                root.DOScale(0.5f, 0.1f).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.POP_H)
            {
                root.DOScaleY(0.5f, 0.1f).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.LEFT_TO_RIGHT)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;
                Vector2 targetPosition = new Vector2(-(parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.2f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.RIGHT_TO_LEFT)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;
                Vector2 targetPosition = new Vector2((parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.2f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.LEFT_TO_CENTER)
            {
                float parentWidth = rectTransform.rect.width;
                float childWidth = root.rect.width;
                Vector2 targetPosition = new Vector2((parentWidth + childWidth) * 0.5f, root.anchoredPosition.y);
                root.DOAnchorPos(targetPosition, 0.3f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.BOTTOM_TO_CNETER)
            {
                float parentHeight = rectTransform.rect.height;
                float childHeight = root.rect.height;
                Vector2 targetPosition = new Vector2(root.anchoredPosition.x, -(parentHeight + childHeight) * 0.5f);
                root.DOAnchorPos(targetPosition, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else if (openType == OpenType.FADE)
            {
                canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
                {
                    _onClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }
            else
            {
                _onClose?.Invoke();
                gameObject.SetActive(false);
            }
        }

        private void SetBlocksRaycasts(bool _flag)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = _flag;
            }
        }

        protected void ClosePopup(Action _onClose = null)
        {
            PopupManager.Instance.ClosePopup(popupType, _onClose);
        }
    }
}