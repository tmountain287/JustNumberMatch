using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public class AutoReturnPool : MonoBehaviour
    {
        [SerializeField] private float time = 5.0f;
        [SerializeField] private UnityEvent unityEvent = null;
        private void OnEnable()
        {
            StartCoroutine(StartTimer());
        }

        IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(time);
            unityEvent?.Invoke();
        }
    }
}
