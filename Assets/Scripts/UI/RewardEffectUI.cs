using System;
using UnityEngine;
using Util;

public class RewardEffectUI : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;

    public void Start()
    {
        objectPool.CreateObjectPool();
    }

    public void OnEffect(ItemType _itemType, int _amount, Vector3 _startPosition, Transform _targetPoint,
        Action _firsArrivedAction = null, Action _arrivedAction = null)
    {
        DropEffect dropEffect = objectPool.GetObjectFromPool<DropEffect>();

        dropEffect.SpawnItem(_itemType, _amount, transform, _startPosition, _targetPoint,
        _firsArrivedAction, _arrivedAction, ()=>
        {
            objectPool.ReturnObjectToPool(dropEffect.gameObject);
        });
    }
}