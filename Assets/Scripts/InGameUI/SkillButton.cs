using Common.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SkillButton : MonoBehaviour
    {
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject countObj = null;
        [SerializeField] private Text countText = null;

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<ShopCharacterPopup>().Initialize();
            });
        }

        private void OnEnable()
        {
            SetCount();
            UserDataManager.OnCheckNewCharacter.AddListener(SetCount);
        }

        private void OnDisable()
        {            
            UserDataManager.OnCheckNewCharacter.RemoveListener(SetCount);
        }

        private void SetCount()
        {
            List<int> idList = TableDataManager.Instance.TableShopCharacterData.ShopCharacterTableDataList.Where(x => x.openLevel <= UserDataManager.Level).Select(x => x.characterId).ToList();

            idList.RemoveAll(x => UserDataManager.UserData.hasCharcterID.Contains(x));
            idList.RemoveAll(x => UserDataManager.ViewCharacterData.CharacterIDList.Contains(x));

            int freeTotal = idList.Count;

            if(freeTotal > 0)
            {
                countText.text = freeTotal.ToString();
                countObj.SetActive(true);
            }
            else
            {
                countObj.SetActive(false);       
            }
        }
    }
}