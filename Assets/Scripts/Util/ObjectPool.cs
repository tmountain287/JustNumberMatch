using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class ObjectPool : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private GameObject prefab = null;
        [SerializeField] private int initialPoolSize = 10; // 초기 풀 크기
        #endregion

        private Queue<GameObject> pool = new(); // 오브젝트를 저장하는 큐
        private List<GameObject> poolList = new();

        public void CreateObjectPool(GameObject _prefab = null, int? _size = null)
        {
            if(_prefab != null)
            {
                prefab = _prefab;
            }

            if(_size !=null)
            {
                initialPoolSize = (int)_size;
            }

            // 초기 풀 크기만큼 오브젝트를 생성하여 풀에 추가
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false); // 비활성화하여 사용하지 않음
                pool.Enqueue(obj); // 큐에 추가
            }
        }

        // 오브젝트를 풀에서 가져오기
        public GameObject GetObjectFromPool()
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true); // 활성화하여 사용할 준비
                poolList.Add(obj);
                return obj;
            }
            else
            {
                // 풀에 남아있는 오브젝트가 없으면 새로운 오브젝트를 생성하여 반환
                GameObject obj = Instantiate(prefab, transform);
                poolList.Add(obj);
                return obj;
            }
        }

        public T GetObjectFromPool<T>() where T : Component
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true); // 활성화하여 사용할 준비
                poolList.Add(obj);
                return obj.GetComponent<T>();
            }
            else
            {
                // 풀에 남아있는 오브젝트가 없으면 새로운 오브젝트를 생성하여 반환
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(true); // 활성화하여 사용할 준비
                poolList.Add(obj);
                return obj.GetComponent<T>();
            }
        }

        // 오브젝트를 풀에 반환하기
        public void ReturnObjectToPool(GameObject _obj)
        {
            _obj.transform.SetParent(transform);
            _obj.SetActive(false); // 비활성화하여 사용하지 않음
            poolList.Remove(_obj);
            pool.Enqueue(_obj); // 큐에 다시 추가하여 재사용
        }

        public void Clear()
        {
            for (int i = poolList.Count - 1; i >= 0; i--)
            {
                ReturnObjectToPool(poolList[i]);
            }
            poolList.Clear();
        }

        private void OnDestroy()
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                Destroy(obj); // 큐에서 꺼내서 파괴
            }
        }

        //public void AllReturnObjectToPool()
        //{
        //    poolList.ForEach(item =>
        //    {
        //        ReturnObjectToPool(item);
        //    });

        //    poolList.Clear();
        //}
    }
}