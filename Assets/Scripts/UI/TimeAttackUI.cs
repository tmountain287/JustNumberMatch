using Common.UI;
using UnityEngine;
using UnityEngine.UI;


public class TimeAttackUI : BaseUI
{
    [SerializeField] private Button exitButton = null;

    private void Start()
    {
        exitButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowUI(Type.STAGE);           
        });
    }

}