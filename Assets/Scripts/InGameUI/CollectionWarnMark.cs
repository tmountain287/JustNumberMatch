using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Common.Manager;

namespace Gostop.UI
{
    public class CollectionWarnMark : MonoBehaviour
    {
        [SerializeField] private List<CollectionType> collectionTypeList = new();
        [SerializeField] private List<GameObject> markList = null;

        private Transform target = null;

        private void OnEnable()
        {
            ResolutionManager.Instance.OnChangeResolution?.AddListener(SetResolution);
        }

        private void OnDisable()
        {
            Clear();
            if(ResolutionManager.Instance!=null)
                ResolutionManager.Instance.OnChangeResolution?.RemoveListener(SetResolution);
        }

        public void Clear()
        {
            markList.ForEach(x => x.SetActive(false));
        }

        public void SetMark(CollectionType _collectionType, bool _on)
        {
            if(collectionTypeList.Any(x=> x== _collectionType))
            {
                GameObject mark = markList[(int)_collectionType - 8];
                mark.transform.SetAsLastSibling();
                mark.SetActive(_on);
            }
        }

        public void SetTransform(int _count, Transform _parent)
        {            
            if (_count > 0)
            {
                transform.SetParent(_parent);
                transform.localPosition = Vector3.zero;
                transform.SetAsLastSibling();
            }
        }

        public void SetTransform(Transform _parent)
        {
            target = _parent;
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