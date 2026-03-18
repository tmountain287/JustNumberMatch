using Common.Manager;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoMatchCanvasScaler : MonoBehaviour
{
    private CanvasScaler scaler;
  
    void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
       
        UpdateMatchMode();
    }

    private void OnEnable()
    {
        ResolutionManager.Instance.OnChangeResolution.AddListener(UpdateMatchMode);
    }

    private void OnDisable()
    {
        if (ResolutionManager.Instance != null)
            ResolutionManager.Instance.OnChangeResolution.RemoveListener(UpdateMatchMode);
    }

    void UpdateMatchMode()
    {
        Vector2 refResolution = scaler.referenceResolution;
        float targetAspect = refResolution.x / refResolution.y;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            // 현재 화면이 더 가로로 넓음 → 너비 기준
            scaler.matchWidthOrHeight = 1f;
        }
        else
        {
            // 현재 화면이 더 세로로 김 → 높이 기준
            scaler.matchWidthOrHeight = 0f;
        }
    }
}
