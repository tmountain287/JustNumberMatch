using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CEventHadler<T>
{
    private Dictionary<T, UnityEvent<bool>> eventHandlers = new();

    public void AddEventHandler(T _eventType, UnityAction<bool> _handler)
    {
        if (eventHandlers.ContainsKey(_eventType))
        {
            eventHandlers[_eventType].AddListener(_handler);
        }
        else
        {
            var unityEvent = new UnityEvent<bool>();
            unityEvent.AddListener(_handler);
            eventHandlers.Add(_eventType, unityEvent);
        }
    }

    public void RemoveEventHandler(T _eventType, UnityAction<bool> _handler)
    {
        if(eventHandlers!=null)
        {
            if (eventHandlers.ContainsKey(_eventType))
            {
                eventHandlers[_eventType].RemoveListener(_handler);
            }
        }
    }

    public void RemoveAllEventHandlers()
    {
        foreach (var eventType in eventHandlers.Keys)
        {
            eventHandlers[eventType].RemoveAllListeners();
        }

        eventHandlers.Clear();
        eventHandlers = null;
    }

    public void TriggerEvent(T _eventType)
    {
        if (eventHandlers.ContainsKey(_eventType))
        {
            eventHandlers[_eventType].Invoke(true);
        }
    }
}