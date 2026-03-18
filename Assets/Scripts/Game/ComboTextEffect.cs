using Common.Manager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboTextEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text comboText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("색상 설정")]
    [SerializeField] private List<Color> colorList = null;

    [Header("애니메이션 설정")]
    [SerializeField] private float showDuration = 0.15f;
    [SerializeField] private float holdDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.22f;

    [SerializeField] private List<GameObject> effectList = null;
    [SerializeField] private List<AudioClip> audioClips = null;

    private Tween _currentTween;
   

    private void Awake()
    {
        if (comboText == null)
            comboText = GetComponentInChildren<Text>();
        if (canvasGroup == null)
            canvasGroup = comboText.GetComponent<CanvasGroup>();
        

        canvasGroup.alpha = 0f;
        comboText.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 0~1 범위의 게이지 값으로 콤보 텍스트 출력. 연출이 끝나면 onComplete 호출(풀 반환용).
    /// </summary>
    public void PlayCombo(float gauge01, Action onComplete = null)
    {
        gauge01 = Mathf.Clamp01(gauge01);

        string id = GetComboIdByGauge(gauge01);
        string text = LocalizationManager.Instance.GetText(id);

        //SoundManager.Instance.PlayFX(GetAudioClipByGauge(gauge01), Random.Range(1.08f, 1.14f));

        GetEffectByGauge(gauge01).SetActive(true);

        comboText.text = text;
        comboText.color = GetColorByGauge(gauge01);

        // 이전 트윈 정리
        _currentTween?.Kill(true);

        Transform t = comboText.transform;
        t.localScale = Vector3.one * 0.6f;
        t.localRotation = Quaternion.identity;
        canvasGroup.alpha = 0f;

        // 기본 연출 시퀀스
        Sequence seq = DOTween.Sequence();

        // 1) 등장 (스케일 업 + 알파 업)
        seq.Append(
            DOTween.To(
                () => canvasGroup.alpha,
                a => canvasGroup.alpha = a,
                1f,
                showDuration
            )
        );
        seq.Join(t.DOScale(1.2f, showDuration).SetEase(Ease.OutBack));

        // 2) 살짝 줄어들면서 안정
        seq.Append(t.DOScale(1.0f, 0.1f).SetEase(Ease.OutQuad));

        // 3) 상위 단계일수록 추가 연출
        int level = GetLevelByGauge(gauge01);
        if (level >= 4 && level < 6)
        {
            // 4~5단계 : 약한 흔들림
            seq.Join(t.DOShakeRotation(0.25f, new Vector3(0, 0, 8f)));
        }
        else if (level >= 6)
        {
            // FEVER : 더 강한 흔들림 + 살짝 위로 이동
            seq.Join(t.DOShakeRotation(0.35f, new Vector3(0, 0, 18f)));
            seq.Join(t.DOLocalMoveY(t.localPosition.y + 15f, 0.35f)
                .SetRelative(false)
                .SetEase(Ease.OutSine));
        }

        // 4) 잠시 유지
        seq.AppendInterval(holdDuration);

        // 5) 페이드 아웃
        seq.Append(
            DOTween.To(
                () => canvasGroup.alpha,
                a => canvasGroup.alpha = a,
                0f,
                fadeDuration
            )
        );

        seq.OnComplete(() =>
        {
            _currentTween = null;
            onComplete?.Invoke();
        });
        _currentTween = seq;
    }

    private string GetComboIdByGauge(float gauge01)
    {
        if (gauge01 >= 0.80f) return "Combo_4_Amazing";
        if (gauge01 >= 0.60f) return "Combo_3_Awesome";
        if (gauge01 >= 0.30f) return "Combo_2_Great";
        return "Combo_1_Good";
    }

    private int GetLevelByGauge(float gauge01)
    {
        if (gauge01 >= 0.80f) return 4;
        if (gauge01 >= 0.60f) return 3;
        if (gauge01 >= 0.30f) return 2;
        return 1;
    }

    private GameObject GetEffectByGauge(float gauge01)
    {
        if (gauge01 >= 0.80f) return effectList[3];
        if (gauge01 >= 0.60f) return effectList[2];
        if (gauge01 >= 0.30f) return effectList[1];
        return effectList[0];
    }

    private Color GetColorByGauge(float gauge01)
    {
        if (gauge01 >= 0.80f) return colorList[3];
        if (gauge01 >= 0.60f) return colorList[2];
        if (gauge01 >= 0.30f) return colorList[1];
        return colorList[0];
    }

    private AudioClip GetAudioClipByGauge(float gauge01)
    {
        if (gauge01 >= 0.80f) return audioClips[3];
        if (gauge01 >= 0.60f) return audioClips[2];
        if (gauge01 >= 0.30f) return audioClips[1];
        return audioClips[0];
    }
}
