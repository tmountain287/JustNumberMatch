using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProfileImages : MonoBehaviour
{
    [SerializeField] private bool isMy = false;
    [SerializeField] private List<Image> images = null;

    public List<Image> Images { get => images; }

    private void OnValidate()
    {
        images ??= transform.GetComponentsInChildren<Image>(true).ToList();
    }

    private void OnEnable()
    {
        if(isMy)
        {
            SetProfile(UserDataManager.ProfileIndex);
            UserDataManager.OnValueProfileIndexChanged += SetProfile;
        }
    }

    public void SetProfile(int _index)
    {
        for (int i = 0; i < Images.Count; i++)
        {
            Images[i].gameObject.SetActive(i == _index);
        }
    }
}
