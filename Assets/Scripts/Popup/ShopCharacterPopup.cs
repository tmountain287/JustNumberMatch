using Common.Manager;
using Common.UI;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Gostop.UI
{
    public class ShopCharacterPopup : BasePopup
    {
        [SerializeField] private ShopSkillPanel shopSkillPanel = null;
        [SerializeField] private ShopInfoPanel shopInfoPanel = null;
        [SerializeField] private RectTransform chidRoot = null;
        [SerializeField] private NumberIncrement goldValue = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            goldValue.SetNumber(UserDataManager.Gold, false);
            UserDataManager.OnValueGoldChanged.AddListener(SetGold);
        }

        private void OnDisable()
        {
            UserDataManager.OnValueGoldChanged.RemoveListener(SetGold);
        }

        public void SetGold(int _value, bool _isAni)
        {
            goldValue.SetNumber(_value, _isAni);
        }

        public void Initialize()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(chidRoot);
            ShopCharacterTableData selectedData = TableDataManager.Instance.TableShopCharacterData.GetDataByCharacterId(UserDataManager.SelectIndex);

            shopInfoPanel.SetInfoPanel(selectedData);

            shopInfoPanel.Initialize(shopSkillPanel.RefreshPanel);
            shopSkillPanel.Initialize((data) =>
            {
                shopInfoPanel.SetInfoPanel(data);
                
            });
        }
    }
}