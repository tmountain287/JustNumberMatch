using Common.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Common.Manager
{
    public class AlarmMessgeManager : MonoSingleton<AlarmMessgeManager>
    {
        [SerializeField] private ObjectPool alarmMessageBoxPool = null;
        [SerializeField] private Transform pivot = null;

        private List<AlarmMessageBox> alarmMessageBoxList = new();

        private void Awake()
        {
            alarmMessageBoxPool.CreateObjectPool();
        }

        public void OnMessage(string _message)
        {
            if(alarmMessageBoxList.Count > 0)
            {
                alarmMessageBoxList[^1].OnNextAlarm();
                for (int i = 0; i < alarmMessageBoxList.Count - 1; i++)
                {
                    alarmMessageBoxList[i].OnComplete();
                }
            }

            AlarmMessageBox messageBox = alarmMessageBoxPool.GetObjectFromPool<AlarmMessageBox>();

            alarmMessageBoxList.Add(messageBox);

            messageBox.transform.SetParent(pivot);
            messageBox.transform.localPosition = new Vector3(0, 300, 0);
            messageBox.transform.SetAsFirstSibling();
            messageBox.OnAlarm(_message, () =>
            {
                alarmMessageBoxList.Remove(messageBox);
                alarmMessageBoxPool.ReturnObjectToPool(messageBox.gameObject);
            });
        }
    }
}