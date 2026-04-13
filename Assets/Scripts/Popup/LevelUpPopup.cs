using Common.Manager;
using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class LevelUpPopup : BasePopup
    {
        [SerializeField] private Text levelText1 = null;
        [SerializeField] private Text levelText2 = null;
        [SerializeField] private RewardItemList rewardItemList = null;
        [SerializeField] private Button claimAgainButton = null;

        [SerializeField] private List<RectTransform> rectList = null;

        private Dictionary<ItemType, int> rewardItemDic = null;
        private Action closeAction = null;        

        protected override void Start()
        {
            //base.Start();
            claimAgainButton.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowRewardedAd((adapter) =>
                {
                    rewardItemList.PlayMultiple();
                    UserDataManager.AddItemCount(rewardItemDic);
                    claimAgainButton.gameObject.SetActive(false);
                }, null);
            });

            closeButton.onClick.AddListener(() =>
            {
                ClosePopup(closeAction);
            });
        }

        protected override void OnEnable()
        {
            for (int i = 0; i < rectList.Count; i++)
            {
                int index = i;
                var rt = rectList[index];
                rt.localScale = Vector3.zero;

                var seq = DOTween.Sequence();
                seq.Append(rt.DOScale(1.1f, 0.35f).SetEase(Ease.OutBack))
                   .Append(rt.DOScale(1.0f, 0.12f).SetEase(Ease.OutCubic))
                   .SetDelay(index * 0.1f);
            }
        }

        public void Initialize(int _level, Dictionary<ItemType, int> _rewardItemDic, Action _closeAction)
        {
            rewardItemDic = _rewardItemDic;
            levelText1.text = _level.ToString();
            levelText2.text = _level.ToString();
            rewardItemList.SetRewardItemList(_rewardItemDic);

            int index = ConfigData.UnlockModeLevelList.FindIndex(x => x == _level);            

            if (index == -1)
            {
                closeAction = _closeAction;
            }
            else
            {
                closeAction = ()=> PopupManager.Instance.OpenPopup<UnlockPopup>().Initialize(index, _closeAction);
            }
        }
    }
}