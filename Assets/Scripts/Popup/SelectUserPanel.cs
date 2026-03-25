using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class SelectUserPanel : MonoBehaviour
    {
        [SerializeField] private Text levelText = null;
        [SerializeField] private Text nickNameText = null;
        [SerializeField] private Text winRateText = null;
        [SerializeField] private Text moneyText = null;
        [SerializeField] private Text goldText = null;

        [SerializeField] private Button selectButton = null;

        private Action onSelectClick = null;

        public void Start()
        {
            selectButton.onClick.AddListener(() =>
            {
                onSelectClick?.Invoke();
            });
        }

        public void SetPanel(UserData _userData, Action _onSelectClick = null)
        {
            onSelectClick = _onSelectClick;
            levelText.text = _userData.level.ToString();
            nickNameText.text = _userData.nickName;
            winRateText.text = _userData.playCount == 0 ? "0.00%" : ((float)_userData.winCount / _userData.playCount * 100f).ToString("F2") + "%";
            moneyText.text = _userData.money.FormatKoreanUnits();
            goldText.text = _userData.gold.FormatComma();
        }
    }
}
