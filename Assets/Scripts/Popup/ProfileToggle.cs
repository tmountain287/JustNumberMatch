using UnityEngine;
using UnityEngine.UI;

public class ProfileToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle = null;
    [SerializeField] private ProfileImages profileImages = null;
    
    void Start()
    {
        int index = transform.GetSiblingIndex();

        toggle.targetGraphic = profileImages.Images[index];
        profileImages.SetProfile(index);

        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
                UserDataManager.ProfileIndex = index;
        });
    }

    private void OnEnable()
    {
        toggle.isOn = transform.GetSiblingIndex() == UserDataManager.ProfileIndex;
    }
}