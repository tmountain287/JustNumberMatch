using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSpeed : MonoBehaviour
{
    [SerializeField] private Text value = null;

    private void OnEnable()
    {
        value.text = Time.timeScale.ToString();
    }

    public void UpTimeScale()
    {
        Time.timeScale += 0.1f;
        value.text = Time.timeScale.ToString();
    }

    public void DownTimeScale()
    {
        float ts = Time.timeScale;
        ts -= 0.1f;
        if (ts < 0)
            Time.timeScale = 0;
        Time.timeScale = ts;
        value.text = Time.timeScale.ToString();
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1;
        value.text = Time.timeScale.ToString();
    }
}
