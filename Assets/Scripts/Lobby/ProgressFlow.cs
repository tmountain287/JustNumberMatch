using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public sealed class ProgressFlow
{
    private readonly Slider _slider;
    private float _value;

    public ProgressFlow(Slider slider)
    {
        _slider = slider;
        _value = Mathf.Clamp01(slider.value);
    }

    public float Value => _value;

    public UniTask AddAsync(float delta01, float animSec = 0.2f)
    {
        _value = Mathf.Clamp01(_value + Mathf.Max(0f, delta01));
        _slider.DOKill();
        var tw = _slider.DOValue(_value, animSec);
        return tw.AsyncWaitForCompletion().AsUniTask();
    }

    public UniTask SetAsync(float value01, float animSec = 0.2f)
    {
        _value = Mathf.Clamp01(value01);
        _slider.DOKill();
        var tw = _slider.DOValue(_value, animSec);
        return tw.AsyncWaitForCompletion().AsUniTask();
    }

    // 기존 동기 호출 필요하면 남겨도 됨
    public void Add(float delta01, float animSec = 0.2f) => _ = AddAsync(delta01, animSec);
    public void Set(float value01, float animSec = 0.2f) => _ = SetAsync(value01, animSec);
}
