using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private Text levelText = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private NumberIncrement xpText = null;
    [SerializeField] private Text maxText = null;
    [SerializeField] private AudioClip gaugeAudioClip = null;

    [SerializeField] private bool IsCurrent = true;

    private void OnEnable()
    {
        if(IsCurrent)
        {
            UserDataManager.OnValueLevelChanged += SetLevelUI;
            UserDataManager.OnValueXPChanged += SetLevelUI;
            SetLevelUI();
        }
    }

    private void OnDisable()
    {
        if (IsCurrent)
        {
            UserDataManager.OnValueLevelChanged -= SetLevelUI;
            UserDataManager.OnValueXPChanged -= SetLevelUI;
        }
    }

    public void SetLevelUI()
    {
        LevelData data = TableDataManager.Instance.TableLevelData.GetTableData(UserDataManager.Level);
        SetLevelUI(UserDataManager.Level, UserDataManager.XP, data.xp);
    }

    public void SetLevelUI(int _level, int _xp, int _max)
    {
        levelText.text = _level.ToString();
        slider.maxValue = _max;
        slider.value = _xp;

        xpText.SetNumber(_xp, false);
        maxText.text = _max.ToString();
    }

    public void UpdateUI(int _xp, Action _onComplete = null)
    {
        SoundManager.Instance.PlayFX(gaugeAudioClip);
        xpText.SetNumber(_xp);
        slider.DOValue(_xp, 0.5f).OnComplete(()=>
        {
            _onComplete?.Invoke();
        });
    }    
}