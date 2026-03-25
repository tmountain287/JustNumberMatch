using Common.Manager;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Gostop.UI
{    

    public class InGameEffectPool : MonoBehaviour
    {
        [SerializeField] private ObjectPool effectPool = null;
        [SerializeField] private Transform parent = null;

        private void Awake()
        {
            effectPool.CreateObjectPool();
        }

        private void OnValidate()
        {
            if (effectPool == null)
            {
                effectPool = GetComponent<ObjectPool>();
            }
        }

        public void OnEffect(Transform _target, int _characterID = -1)
        {
            if (effectPool != null)
            {
                InGameEffect effect = effectPool.GetObjectFromPool<InGameEffect>();
                
                effect.OnEffect(_characterID, () =>
                {
                    effectPool.ReturnObjectToPool(effect.gameObject);
                });

                effect.transform.SetParent(parent);
                if (_target != null)
                    effect.transform.position = _target.transform.position;
            }
        }
    }
}