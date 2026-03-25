using Common.Manager;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class CardPositionGroupCount : MonoBehaviour
    {
        [SerializeField] private Text countText = null;

        private Transform target = null;

        private int currentCount = 0;

        private void OnEnable()
        {
            ResolutionManager.Instance.OnChangeResolution?.AddListener(SetResolution);
        }

        private void OnDisable()
        {
            currentCount = 0;
            target = null;
            if (ResolutionManager.Instance != null)
                ResolutionManager.Instance.OnChangeResolution?.RemoveListener(SetResolution);
        }

        public void SetCount(int _count)
        {
            countText.text = _count.ToString();
            currentCount = _count;
            SetTransform(target);
        }

        public void SetTransform(int _count, Transform _parent)
        {
            gameObject.SetActive(_count > 0);
            if (_count > 0)
            {
                transform.SetParent(_parent);
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.SetAsLastSibling();
            }
        }

        public void SetTransform(Transform _parent)
        {
            target = _parent;
            gameObject.SetActive(_parent != null && currentCount != 0);
            if (_parent != null)
            {
                //transform.SetParent(_parent);
                transform.position = _parent.position;
                //transform.SetAsLastSibling();
            }
        }

        private void SetResolution()
        {
            StartCoroutine(SetResolutionCoroutine());
        }

        IEnumerator SetResolutionCoroutine()
        {
            yield return new WaitForEndOfFrame();
            SetTransform(target);
        }
    }
}