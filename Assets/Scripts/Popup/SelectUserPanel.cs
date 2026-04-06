using Common.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JustOneMatch.UI
{
    public class SelectUserPanel : MonoBehaviour
    {
        [SerializeField] private List<Color> colorList = new();
        [SerializeField] private Image bg = null;
        [SerializeField] private Text title = null;
        [SerializeField] private ProfileImages profileImages = null;
        [SerializeField] private Text nickName = null;

        [SerializeField] private LevelUI levelUI = null;

        [SerializeField] private List<Slider> sliderList = null;
        [SerializeField] private List<Text> sliderValueList = null;
        [SerializeField] private List<Text> itemCountText = null;

        [SerializeField] private GameObject removeAdComplete = null;
        [SerializeField] private GameObject removeAdNot = null;

        [SerializeField] private Button selectButton = null;

        private Action onSelectClick = null;

        public void Start()
        {
            selectButton.onClick.AddListener(() =>
            {
                onSelectClick?.Invoke();
            });
        }

        public void SetPanel(bool _isCurrent, UserData _userData, Action _onSelectClick = null)
        {
            bg.color = _isCurrent ? colorList[1] : colorList[0];
            title.text = LocalizationManager.Instance.GetText(_isCurrent ? "Current play record" : "Previous play record");

            profileImages.SetProfile(_userData.profileIndex);
            nickName.text = _userData.nickName;

            onSelectClick = _onSelectClick;

            levelUI.SetLevelUI(_userData.level, _userData.xp, TableDataManager.Instance.TableLevelData.GetTableData(_userData.level).xp);

            for(int i=0; i<sliderList.Count; i++)
            {
                int maxStage = TableDataManager.Instance.TableStageData.StageTableDataDic[(DifficultyType)i].Count;

                int clearStage = _userData.clearStageInfoDic[(DifficultyType)i];

                sliderValueList[i].text = $"{clearStage}/{maxStage}";
                sliderList[i].value = (float)clearStage / maxStage;
            }

            for(int i = 0; i < itemCountText.Count; i++)
            {
                itemCountText[i].text = _userData.itemInfoDic[(ItemType)i].FormatComma();
            }

            removeAdComplete.SetActive(_userData.isAdsFree);
            removeAdNot.SetActive(!_userData.isAdsFree);
        }
    }
}