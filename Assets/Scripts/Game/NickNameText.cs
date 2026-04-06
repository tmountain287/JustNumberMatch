using UnityEngine;
using UnityEngine.UI;

public class NickNameText : MonoBehaviour
{
    [SerializeField] private Text nickNameText = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        UserDataManager.OnValueNickNameChanged += SetNickName;
        SetNickName();
    }

    private void OnDisable()
    {
        UserDataManager.OnValueNickNameChanged -= SetNickName;
    }

    private void SetNickName()
    {
        nickNameText.text = UserDataManager.NickName;
    }
}
