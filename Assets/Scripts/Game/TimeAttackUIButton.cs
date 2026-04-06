using Common.Manager;
using JustOneMatch.UI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeAttackUIButton : MonoBehaviour
{
    [SerializeField] private DifficultyType difficultyType;
    [SerializeField] private Button button = null;

    [SerializeField] private Text bestLabText = null;
    [SerializeField] private Text ticketCountText = null; 
    
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            int needCount = ConfigData.NeedTimeAttckTicketCountDic[difficultyType];
            if (UserDataManager.GetItemCount(ItemType.TimeAttackTicket) < needCount)
            {
                PopupManager.Instance.OpenMessageBoxPopup("", LocalizationManager.Instance.GetText("NotEnoughTicket"));
            }
            else
            {
                UserDataManager.SubItemCount(ItemType.TimeAttackTicket, needCount);
                UserDataManager.Save(_onComplete: ()=>
                {
                    GameMgr.Instance.StartTimeAttack(difficultyType);
                });                
            }
        });
    }

    private void OnEnable()
    {
        long lab = 0;

        if(UserDataManager.UserData.timeAttackInfoDic.ContainsKey(difficultyType))
        {
            lab = UserDataManager.UserData.timeAttackInfoDic[difficultyType];
        }

        long bestLab = lab;
        bestLabText.text = bestLab.FormatFromMs();

        bestLabText.color = bestLab == 0 ? Color.gray : Color.green;

        ticketCountText.text = $"x{ConfigData.NeedTimeAttckTicketCountDic[difficultyType]}";
    }
} 