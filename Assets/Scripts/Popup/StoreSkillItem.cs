using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Gostop.UI
{
    public class StoreSkillItem : MonoBehaviour
    {
        [SerializeField] private List<StoreCharacterItem> storeCharcaterItemList = null;
        [SerializeField] private StoreCharacterSkillItem storeSkillItem = null;
        

        public void SetItemData(ShopSkillPanel _shopSkillPanel, int _selectIndex, SkillTableData _data, Action<ShopCharacterTableData> _onSelect, Action _onRefresh)
        {
            for (int i = 0; i < storeCharcaterItemList.Count; i++)
            {
                storeCharcaterItemList[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < _data.shopCharacterIDList.Count; i++)
            {
                ShopCharacterTableData data = TableDataManager.Instance.TableShopCharacterData.GetData(_data.shopCharacterIDList[i]);
                if (data != null)
                {
                    storeCharcaterItemList[i].SetItemData(data, _selectIndex, (_data) => _onSelect?.Invoke(_data));
                    storeCharcaterItemList[i].gameObject.SetActive(true);
                }
            }

            storeSkillItem.SetItemData(_shopSkillPanel, _data, _onRefresh);

            //List<List<int>> splitList = SplitBySizes(_data.shopCharacterIDList, 5);

            //for(int i = 0; i < layoutList.Count; i++)
            //{
            //    layoutList[i].gameObject.SetActive(false);
            //}

            //for(int i=0;i< splitList.Count;i++)
            //{
            //    layoutList[i].SetItem(splitList[i]);
            //}

            //LayoutRebuilder.ForceRebuildLayoutImmediate(chRectTransform);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            //CharacterResManager.Instance.SetImage(profile, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(data.characterId).resource, CharacterImage.Type.Profile);

            //lockText.text = $"{data.openLevel}레벨 잠금해제";
            //buyButtonText.text = data.gold.FormatComma();
            //RefreshItem();
        }       

        //public static List<int> SplitIntoRows(int value, int maxPerRow)
        //{
        //    List<int> rows = new List<int>();

        //    if (value <= 0 || maxPerRow <= 0)
        //        return rows;

        //    int rowCount = Mathf.CeilToInt((float)value / maxPerRow); // 필요한 줄 수
        //    int baseCount = value / rowCount;                         // 기본 개수
        //    int remainder = value % rowCount;                         // 몇 개에 +1

        //    for (int i = 0; i < rowCount; i++)
        //    {
        //        int count = baseCount + (i < remainder ? 1 : 0); // 앞에서부터 +1
        //        rows.Add(count);
        //    }

        //    return rows;
        //}

        //public List<List<int>> SplitBySizes(List<int> source, int maxPerRow)
        //{
        //    List<int> rowSizes = SplitIntoRows(source.Count, maxPerRow);

        //    List<List<int>> result = new();
        //    int index = 0;

        //    foreach (int size in rowSizes)
        //    {
        //        if (index + size > source.Count)
        //        {
        //            Debug.LogWarning("요청한 크기가 source 길이를 초과합니다.");
        //            break;
        //        }

        //        result.Add(source.GetRange(index, size));
        //        index += size;
        //    }

        //    return result;
        //}
    }
} 