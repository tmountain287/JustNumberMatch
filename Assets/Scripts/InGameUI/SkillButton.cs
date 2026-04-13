using Common.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class SkillButton : MonoBehaviour
    {
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject countObj = null;
        [SerializeField] private Text countText = null;

        //private void Start()
        //{
        //    button.onClick.AddListener(() =>
        //    {
        //        PopupManager.Instance.OpenPopup<CollectionCharacterPopup>().Initialize();
        //    });
        //}

        //private void OnEnable()
        //{
        //    SetCount(UserDataManager.UserData.pieceCount);
        //    UserDataManager.OnValuePieceCountChanged.AddListener(SetCount);
        //}

        //private void OnDisable()
        //{
        //    UserDataManager.OnValuePieceCountChanged.RemoveListener(SetCount);
        //}

        //private void SetCount(int _count)
        //{
        //    countText.text = _count.ToString();
        //    countObj.SetActive(_count > 0);
        //}
    }
}