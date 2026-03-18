using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Common
{
    public class AutoDisableTimer : MonoBehaviour
    {
        [SerializeField] private float time = 5.0f;

        [SerializeField] private UnityEvent OnCompleteEvent = null;

        private void OnEnable()
        {
            StartCoroutine(StartTimer());
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }

        IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(time);
            OnCompleteEvent?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
