using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : Toggle
{
    protected override void Start()
    {
        base.Start();
        onValueChanged.AddListener(OnToggleValueChanged);
        UpdateGraphicState(isOn);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateGraphicState(value);
    }

    private void UpdateGraphicState(bool value)
    {
        if (graphic != null)
            graphic.gameObject.SetActive(value);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateGraphicState(isOn);
    }
#endif
}
