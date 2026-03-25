using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoSingleton<UnityMainThreadDispatcher>
{
    private readonly Queue<Action> _executionQueue = new Queue<Action>();

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                try
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Dispatcher] 예외 발생: {ex}");
                }
            }
        }
    }

    // 기본 Action 사용
    public void Enqueue(Action action)
    {
        if (action == null)
        {
            return;
        }
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(()=>action?.Invoke());
        }
    }

    // Action<T> 지원
    public void Enqueue<T>(Action<T> action, T param)
    {
        if (action == null) return;
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() => action(param));
        }
    }
}