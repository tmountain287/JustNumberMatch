using Common.Manager;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class ShopInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text nameText = null;
        [SerializeField] private Text levelText = null;
        [SerializeField] private Text mainSkillText = null;
        //[SerializeField] private Text subSkillText = null;
        [SerializeField] private RawImage profile = null;
        [SerializeField] private GameObject buyObj = null;
        [SerializeField] private GameObject equipObj = null;

        [SerializeField] private Button myRecordButton = null;

        [SerializeField] private Button adButton = null;
        [SerializeField] private Text adButtonText = null;
        [SerializeField] private Button buyButton = null;
        [SerializeField] private Text buyButtonText = null;

        [SerializeField] private Button fullShotButton = null;
        [SerializeField] private Button equipButton = null;

        private ShopCharacterTableData data = null;
        private Action onRefresh = null;

        public void Start()
        {
            myRecordButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<MyRecordPopup>().Initialize();
            });

            adButtonText.text = ConfigData.AdGoldReward.FormatComma();

            adButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    _ = NetworkManager.Instance.AdReward(adapter,"GetCharacter");
                    UserDataManager.BuyCharacter(data, false);
                    onRefresh?.Invoke();
                    SetInfoPanel(data);
                });
            });

            buyButton.onClick.AddListener(() =>
            {
                if (data.gold <= UserDataManager.Gold)
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", $"<color=#FF6600>{data.gold}골드</color>로\n구매하시겠습니까?", () =>
                    {
                        UserDataManager.BuyCharacter(data);
                        onRefresh?.Invoke();
                        SetInfoPanel(data);
                    }, true);
                }
                else
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "<color=#FF6600>골드</color>가 부족합니다.");
                }

            });

            equipButton.onClick.AddListener(() =>
            {
                InGameManager.Instance.SendReqProfileChange(data.characterId);
                UserDataManager.SelectIndex = data.characterId;
                UserDataManager.Save(true);
                onRefresh?.Invoke();
                SetInfoPanel(data);
                //AlarmMessgeManager.Instance.OnMessage("변경된 <color=#FF8436>스킬</color>은\n다음판 부터 적용됩니다.");
            });

            fullShotButton.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<FullShotPopup>().Initialize(TableDataManager.Instance.TableCharacterData.GetCharacterTableData(data.characterId));
            });
        }

        public void Initialize(Action _onRefresh)
        {
            onRefresh = _onRefresh;
        }

        public void SetInfoPanel(ShopCharacterTableData _data)
        {
            if (_data == null)
                return;
            if (data == null || (data != null && data.id != _data.id))
            {
                data = _data;

                CharacterTableData chData = TableDataManager.Instance.TableCharacterData.GetCharacterTableData(_data.characterId);
                CharacterResManager.Instance.SetImage(profile, chData.resource, CharacterImage.Type.ShopInfo, UserDataManager.Level < data.openLevel ? CharacterImage.GrayType.MoreGray : CharacterImage.GrayType.None);
                nameText.text = chData.name;
                levelText.text = data.openLevel.ToString();

                if(chData.mainSkillType == MainSkillType.KwangSteal)
                {
                    mainSkillText.text = "광 모두 뺏기";
                }
                if (chData.mainSkillType == MainSkillType.MungSteal)
                {
                    mainSkillText.text = "열끗 모두  뺏기";
                }
                if (chData.mainSkillType == MainSkillType.DDiSteal)
                {
                    mainSkillText.text = "띠 모두 뺏기";
                }
                if (chData.mainSkillType == MainSkillType.PeeSteal)
                {
                    mainSkillText.text = "피 모두 뺏기";
                }
            }
            if (UserDataManager.Level < data.openLevel)
            {
                equipObj.SetActive(false);
                buyObj.gameObject.SetActive(false);
            }
            else
            {
                if (UserDataManager.UserData.selectIndex == data.characterId)
                {
                    equipButton.gameObject.SetActive(false);
                    equipObj.SetActive(true);
                    buyObj.gameObject.SetActive(false);
                }
                else
                {
                    if (UserDataManager.UserData.hasCharcterID.Contains(data.characterId))
                    {
                        equipObj.SetActive(true);
                        equipButton.gameObject.SetActive(true);
                        buyObj.gameObject.SetActive(false);
                    }
                    else
                    {
                        buyButtonText.text = data.gold.FormatComma();
                        buyButton.gameObject.SetActive(true);

                        equipObj.SetActive(false);
                        buyObj.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}