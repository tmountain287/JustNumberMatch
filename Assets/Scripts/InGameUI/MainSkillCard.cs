using Common.Manager;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class MainSkillCard : MonoBehaviour
    {
        [SerializeField] private Button button = null;
        [SerializeField] private Text valueText = null;
        [SerializeField] private GameObject adObj = null;
        [SerializeField] private GameObject block = null;

        private Action onClick = null;

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                if (InGameManager.Instance.CanStealCard(CardMainType.PEE))
                {
                    Refrsh();

                    if (UserDataManager.PeeStealCount > 0)
                    {
                        UserDataManager.PeeStealCount--;
                        _ = NetworkManager.Instance.SendItemLog("PeeStealTicketUse", -1);
                        UserDataManager.Save();
                        onClick?.Invoke();
                    }
                    else
                    {
                        if (UserDataManager.UserData.adPeeStealData.playCount >= ConfigData.AdPeeStealMaxCount)
                        {
                            PopupManager.Instance.OpenPopup<StealPeeShopPopup>();
                        }
                        else
                        {
                            UIManager.Instance.ShowRewardedAdOnFail((adapter) =>
                            {
                                _ = NetworkManager.Instance.AdReward(adapter, "STEAL_PEE");
                                UserDataManager.PlayAdPeeSteal();
                                UserDataManager.Save();
                                onClick?.Invoke();
                            },
                            () =>
                            {
                                PopupManager.Instance.OpenPopup<StealPeeGoldPopup>().Initialize(() =>
                                {
                                    UserDataManager.SubGold(ConfigData.StealPeeGold);
                                    _ = NetworkManager.Instance.SendItemLog("GoldStealPee", -ConfigData.StealPeeGold);
                                    UserDataManager.Save();
                                    onClick?.Invoke();
                                });
                            });
                        }
                    }
                }
                else
                {
                    PopupManager.Instance.OpenMessageBoxPopup("알림", "뺏을 수 있는 카드가\n없습니다");
                }
            });
        }

        private void OnEnable()
        {
            UserDataManager.OnValuePeeStealCountChanged.AddListener(SetCard);
            UserDataManager.OnValueAdPeeStealPlayCountChanged.AddListener(SetCard);
            Refrsh();
            SetCard();
            InvokeRepeating(nameof(Refrsh), 0f, 10f);
        }

        private void OnDisable()
        {
            UserDataManager.OnValuePeeStealCountChanged.RemoveListener(SetCard);
            UserDataManager.OnValueAdPeeStealPlayCountChanged.RemoveListener(SetCard);
        }

        public void SetMainSkillCard(Action _onClick)
        {
            onClick = _onClick;
            gameObject.SetActive(true);
        }

        public void SetBlock(bool _flag)
        {
            block.SetActive(_flag);
        }

        private void SetCard()
        {
           // gameObject.SetActive(UserDataManager.PeeStealCount > 0);
            if(UserDataManager.PeeStealCount > 0)
            {
                adObj.SetActive(false);
                valueText.text = UserDataManager.PeeStealCount.ToString();
            }
            else
            {
                adObj.SetActive(true);
                valueText.text = (ConfigData.AdPeeStealMaxCount - UserDataManager.UserData.adPeeStealData.playCount).ToString();
            }
        }

        private void Refrsh()
        {
            UserDataManager.RefreshAdPeeSteal();
        }
    }
}