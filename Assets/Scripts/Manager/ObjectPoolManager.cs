using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace Common.Manager
{
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        [SerializeField] private ObjectPool matchStickPool = null;
        [SerializeField] private ObjectPool digitRecognizerPool = null;
        [SerializeField] private ObjectPool operationRecognizerPool = null;
        [SerializeField] private ObjectPool addOperationRecognizerPool = null;

        public List<MatchStick> MatchStickPoolList { get; set; } = new();
        public ObjectPool MatchStickPool { get => matchStickPool; }

        private List<BaseRecognizer> digitRecognizerPoolList = new();
        private List<BaseRecognizer> operationRecognizerPoolList = new();
        private List<BaseRecognizer> addOperationRecognizerPoolList = new();

        void Start()
        {
            matchStickPool.CreateObjectPool();
            digitRecognizerPool.CreateObjectPool();
            operationRecognizerPool.CreateObjectPool();
            addOperationRecognizerPool.CreateObjectPool();
        }

        public void Clear()
        {
            MatchStickPoolList.ForEach(x => matchStickPool.ReturnObjectToPool(x.gameObject));
            MatchStickPoolList.Clear();
          
            digitRecognizerPoolList.ForEach(x => digitRecognizerPool.ReturnObjectToPool(x.gameObject));
            digitRecognizerPoolList.Clear();

            operationRecognizerPoolList.ForEach(x => operationRecognizerPool.ReturnObjectToPool(x.gameObject));
            operationRecognizerPoolList.Clear();

            addOperationRecognizerPoolList.ForEach(x => addOperationRecognizerPool.ReturnObjectToPool(x.gameObject));
            addOperationRecognizerPoolList.Clear();
        }

        public MatchStick GetMatchStick()
        {
            MatchStick objPool = matchStickPool.GetObjectFromPool<MatchStick>();
            MatchStickPoolList.Add(objPool);
            return objPool;
        }

        public BaseRecognizer GetDigitRecognizer()
        {
            BaseRecognizer objPool = digitRecognizerPool.GetObjectFromPool<BaseRecognizer>();
            digitRecognizerPoolList.Add(objPool);
            return objPool;
        }

        public OperatorRecognizer GetOerationRecognizer()
        {
            OperatorRecognizer objPool = operationRecognizerPool.GetObjectFromPool<OperatorRecognizer>();
            operationRecognizerPoolList.Add(objPool);
            return objPool;
        }

        public AddOperatorRecognizer GetAddOerationRecognizer()
        {
            AddOperatorRecognizer objPool = addOperationRecognizerPool.GetObjectFromPool<AddOperatorRecognizer>();
            addOperationRecognizerPoolList.Add(objPool);
            return objPool;
        }

        public List<BaseRecognizer> ClearAddOpearation(bool _isAll = false)
        {
            List<BaseRecognizer> temp = new();

            for (int i = addOperationRecognizerPoolList.Count - 1; i >= 0; i--)
            {
                BaseRecognizer addOperatorRecognizer = addOperationRecognizerPoolList[i];
                if (!_isAll)
                {
                    if (addOperatorRecognizer.MatchSlotRectList.Any(x => x.childCount > 2)) continue;
                }
                temp.Add(addOperatorRecognizer);
                addOperationRecognizerPool.ReturnObjectToPool(addOperatorRecognizer.gameObject);
                addOperationRecognizerPoolList.Remove(addOperatorRecognizer);
            }

            return temp;
        }
    }
}