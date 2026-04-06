using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;

public sealed class ProgressFlow
{
    private const float DefaultStepAnimSec = 0.72f;
    /// <summary>Out 계열은 구간 시작에서 가속이 커져 초반이 급하게 느껴질 수 있어 InOut 사용.</summary>
    private static readonly Ease StepEase = Ease.InOutQuad;

    private readonly Slider _slider;
    private float _value;

    public ProgressFlow(Slider slider)
    {
        _slider = slider;
        _value = Mathf.Clamp01(slider.value);
    }

    public float Value => _value;

    /// <summary>
    /// 로비 단계 가중치(0~1)에 따른 채우기 시간. 작은 구간(초반)은 더 길게 재생해 끊김·급한 느낌을 줄입니다.
    /// </summary>
    public static float GetStepAnimDuration(float stepWeight)
    {
        float w = Mathf.Clamp(stepWeight, 0.05f, 0.7f);
        float sec = 0.58f + w * 0.52f;
        // 초반 스텝(가중치 낮음)일수록 더 길게 — 체감 속도 완화
        if (w < 0.28f)
            sec += (0.28f - w) * 2.55f;
        return Mathf.Clamp(sec, 0.78f, 1.38f);
    }

    public UniTask AddAsync(float delta01, float animSec = DefaultStepAnimSec)
    {
        _value = Mathf.Clamp01(_value + Mathf.Max(0f, delta01));
        _slider.DOKill();
        var tw = _slider.DOValue(_value, animSec).SetEase(StepEase);
        return tw.AsyncWaitForCompletion().AsUniTask();
    }

    public UniTask SetAsync(float value01, float animSec = DefaultStepAnimSec)
    {
        _value = Mathf.Clamp01(value01);
        _slider.DOKill();
        var tw = _slider.DOValue(_value, animSec).SetEase(StepEase);
        return tw.AsyncWaitForCompletion().AsUniTask();
    }

    // 기존 동기 호출 필요하면 남겨도 됨
    public void Add(float delta01, float animSec = DefaultStepAnimSec) => _ = AddAsync(delta01, animSec);
    public void Set(float value01, float animSec = DefaultStepAnimSec) => _ = SetAsync(value01, animSec);
}
