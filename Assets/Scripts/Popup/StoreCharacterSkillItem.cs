using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

namespace Gostop.UI
{
    public class StoreCharacterSkillItem : MonoBehaviour
    {
        [SerializeField] private Button button = null;

        [SerializeField] private Text skillText = null;
        [SerializeField] private Text valueText = null;
        [SerializeField] private Text countText = null;

        [SerializeField] private List<GameObject> bgList = null;

        [SerializeField] private List<GameObject> gageList = null;
        [SerializeField] private List<GameObject> gageOnList = null;

        [SerializeField] private GameObject lineOn = null;
        [SerializeField] private GameObject lineOff = null;

        private ShopSkillPanel shopSkillPanel = null;
        private SkillTableData data = null;        
        private Action onRefresh = null;

        public void Start()
        {
            button.onClick.AddListener(() =>
            {
                //UserDataManager.CollectSkill(data.id);
                //onRefresh?.Invoke();
            });
        }

        public void SetItemData(ShopSkillPanel _shopSkillPanel, SkillTableData _data, Action _onRefresh = null)
        {
            shopSkillPanel = _shopSkillPanel;
            data = _data;
            onRefresh = _onRefresh;

            bgList.ForEach(x=>x.gameObject.SetActive(false));
            gageList.ForEach(x => x.gameObject.SetActive(false));
            gageOnList.ForEach(x => x.gameObject.SetActive(false));

            if (data.skillType == SkillType.MONEY_UP)
            {
                skillText.text = "머니획득량";
            }
            else if (data.skillType == SkillType.FIRE_UP)
            {
                skillText.text = "불꽃게이지";
            }

            valueText.text = $"{data.value}% 증가";

            List<int> characterList = new();

            data.shopCharacterIDList.ForEach(x =>
            {
                characterList.Add(TableDataManager.Instance.TableShopCharacterData.GetData(x).characterId);
            });

            int characterCount = characterList.Count;
            int hasCount = characterList.Count(x => UserDataManager.UserData.hasCharcterID.Contains(x));

            countText.text = $"{hasCount}/{characterCount}";

            for (int i=0;i<characterCount;i++)
            {
                gageList[i].SetActive(true);
            }

            for (int i = 0; i < hasCount; i++)
            {
                gageOnList[i].SetActive(true);
            }

            //스킬 조건 만족
            if (UserDataManager.hasSkillTableDataList.Contains(data))
            {
                bgList[(int)data.skillType].SetActive(true);
                skillText.color = Color.white;
                valueText.color = Color.white;
                countText.color = Color.white;

                lineOff.SetActive(false);
                lineOn.SetActive(true);
            }
            else if (UserDataManager.newSkillTableDataList.Contains(data))
            {
                transform.parent.SetAsLastSibling();
                shopSkillPanel.OnSkillEffect(data.skillType, transform);

                DOVirtual.DelayedCall(0.4f, () =>
                {
                    transform.DOScale(new Vector3(1.2f, 1.2f, 1.0f), 0.3f).SetEase(Ease.OutBack).From();
                    bgList[(int)data.skillType].SetActive(true);
                    skillText.color = Color.white;
                    valueText.color = Color.white;
                    countText.color = Color.white;

                    lineOff.SetActive(false);
                    lineOn.SetActive(true);

                    UserDataManager.RefreshSkillData();
                });
            }
            else
            {
                skillText.color = Color.gray;
                valueText.color = Color.gray;
                countText.color = Color.gray;

                lineOff.SetActive(true);
                lineOn.SetActive(false);
            }            
        }

        private Vector3 GetPivotOffset(RectTransform rectTransform)
        {
            Vector3 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;
            Vector3 localPivotOffset = new Vector3(
                (0.5f - pivot.x) * size.x,
                (0.5f - pivot.y) * size.y,
                0f
            );

            return rectTransform.localPosition + localPivotOffset;
        }
    }
} 