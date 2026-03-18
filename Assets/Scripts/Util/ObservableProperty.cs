using System;
using UnityEngine.Events;

public class ObservableProperty<T>
{
    private T value;
    public UnityEvent<T> OnValueChanged { get; } = new UnityEvent<T>();

    public T Value
    {
        get => value;
        set
        {
            if (!Equals(this.value, value))
            {
                this.value = value;
                OnValueChanged.Invoke(this.value);
            }
        }
    }

    public ObservableProperty(T initialValue = default)
    {
        value = initialValue;
    }

    public void SetValue(T _value)
    {
        value = _value;
    }
    public void RemoveAllListeners()
    {
        OnValueChanged.RemoveAllListeners();
    }
}