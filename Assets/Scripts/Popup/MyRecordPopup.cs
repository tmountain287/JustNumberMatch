using Common.Manager;
using Common.UI;
using DG.Tweening;
using InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class MyRecordPopup : BasePopup
    {
        [SerializeField] private Text fireTicket = null;

        [SerializeField] private Text moneyUp = null;
        [SerializeField] private Text fireUp = null;

        [SerializeField] private Text playCount = null;
        [SerializeField] private Text playStatus = null;
        [SerializeField] private Text winRate = null;

        [SerializeField] private Text todayPlayStatus = null;
        [SerializeField] private Text todayWinRate = null;
        [SerializeField] private Text todayMoney = null;

        [SerializeField] private Text topScore = null;
        [SerializeField] private Text topWinMoney = null;
        [SerializeField] private Text topWinnigStreak = null;
        [SerializeField] private Text topGocount = null;

        public void Initialize()
        {
            fireTicket.text = $"{UserDataManager.FireTicket.FormatComma()}개";

            moneyUp.text = $"+{UserDataManager.SkillDataDic[SkillType.MONEY_UP]}%";
            fireUp.text = $"+{UserDataManager.SkillDataDic[SkillType.FIRE_UP]}%";

            playCount.text = $"총 {UserDataManager.UserData.playCount}전";
            playStatus.text = $"{UserDataManager.UserData.winCount}승 / {UserDataManager.UserData.playCount - UserDataManager.UserData.winCount}패";
            winRate.text = UserDataManager.GetWinRate();

            todayPlayStatus.text = $"{UserDataManager.UserData.todayPlayCount}전 ({UserDataManager.UserData.todayWinCount}승 / {UserDataManager.UserData.todayPlayCount - UserDataManager.UserData.todayWinCount}패)";

            string tw;
            if (UserDataManager.UserData.todayPlayCount == 0)
                tw = "0.00%";
            else
            {
                float rate = (float)UserDataManager.UserData.todayWinCount / UserDataManager.UserData.todayPlayCount;
                tw = (rate * 100f).ToString("F2") + "%";
            }

            todayWinRate.text = tw;

            long tm = System.Math.Abs(UserDataManager.UserData.todayMoney);
            string mark;

            if (UserDataManager.UserData.todayMoney < 0)
                mark = "-";
            else if (UserDataManager.UserData.todayMoney > 0)
                mark = "+";
            else mark = "";

            todayMoney.text = $"{mark}{tm.FormatKoreanUnits()}";

            topScore.text = $"{UserDataManager.UserData.topScore.FormatComma()}점";
            topWinMoney.text = UserDataManager.UserData.topWinMoney.FormatKoreanUnits();
            topWinnigStreak.text = $"{UserDataManager.UserData.topWinnigStreak}연승";
            topGocount.text = $"{UserDataManager.UserData.topGocount}고";
        }
    }
}