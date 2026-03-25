using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Manager
{
    public class ProfileImageManager : MonoSingleton<ProfileImageManager>
    {
        //#region Inspector Fields
        //[SerializeField] private ProfileImageScriptable profileImages = null;
        //#endregion

        //private List<int> profileIndexList = null;

        //public int ProfileImagesCount { get { return profileImages.sprites.Length; } }

        //private void Awake()
        //{
        //    CNetDocument.ProfileIndex.OnValueChanged.AddListener(SetProfileIndexList);
        //}

        //public void SetProfileIndexList(int _index)
        //{
        //    profileIndexList = Enumerable.Range(0, ProfileImagesCount)
        //                          .Where(x => x != _index)  // selectIndex를 제외한 모든 값들
        //                          .Prepend(_index)          // selectIndex를 리스트의 첫 번째 값으로 추가
        //                          .ToList();
        //}

        //public int GetListData(int _index)
        //{
        //    return profileIndexList[_index];
        //}

        //public Sprite GetProfileImage(int _index)
        //{
        //    return profileImages.sprites[_index];
        //}

        //public void GetProfileImage(string url)
        //{

        //}
    }
}