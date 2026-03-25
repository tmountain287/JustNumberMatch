using UnityEngine;
using UnityEngine.UI;
using System;

namespace Gostop.UI
{
    public class StoreCharacterItem : MonoBehaviour
    {
        [SerializeField] private Button button = null;

        [SerializeField] private Text numberText = null;
        [SerializeField] private Text levelText = null;
        [SerializeField] private Text levelTitle = null;

        [SerializeField] private RawImage profile = null;
        [SerializeField] private GameObject dontHasObj = null;        
        
        [SerializeField] private GameObject selectObj = null;
        [SerializeField] private GameObject newIcon = null;
        [SerializeField] private GameObject equipObj = null;
        
        private ShopCharacterTableData data = null;        
        private Action<ShopCharacterTableData> onSelect = null;

        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                if (newIcon.activeSelf)
                {
                    UserDataManager.AddViewCharacter(data.characterId);
                    newIcon.SetActive(false);
                }
                onSelect?.Invoke(data);
            });
        }

        public void SetItemData(ShopCharacterTableData _data, int _selectIndex, Action<ShopCharacterTableData> _onSelect = null)
        {
            data = _data;
            onSelect = _onSelect;

            CharacterResManager.Instance.SetImage(profile, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(data.characterId).resource, CharacterImage.Type.ShopItem, UserDataManager.Level < data.openLevel ? CharacterImage.GrayType.Gray : CharacterImage.GrayType.None);

            //lockText.text = $"{data.openLevel}레벨\n잠금해제";
            //lockObj.SetActive(UserDataManager.Level < data.openLevel);

            selectObj.SetActive(data.characterId == _selectIndex);
            button.enabled = data.characterId != _selectIndex;

            numberText.text = (data.id + 1).ToString();
            levelText.text = data.openLevel.ToString();

            numberText.color = data.characterId != _selectIndex ? Color.black : Color.white;
            levelText.color = data.characterId != _selectIndex ? Color.black : Color.white;
            levelTitle.color = data.characterId != _selectIndex ? Color.black : Color.white;

            equipObj.SetActive(data.characterId == UserDataManager.SelectIndex);

            bool bHas = UserDataManager.Level >= data.openLevel && !UserDataManager.UserData.hasCharcterID.Contains(data.characterId);

            dontHasObj.SetActive(bHas);

            newIcon.SetActive(bHas && !UserDataManager.ViewCharacterData.CharacterIDList.Contains(data.characterId));            
        }
    }
} 