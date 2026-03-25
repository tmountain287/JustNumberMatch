using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class NoticePanel : MonoBehaviour
    {
        public enum NoticeType
        {            
            GoStop = 1,            
            SelectCard = 2,
            SelectDoublePee = 3,
            SelectPresident = 4,
        }

        [SerializeField] private Text message = null;

        public void OnNotice(NoticeType _noticeType)
        {

            if (_noticeType == NoticeType.GoStop)
            {
                message.text = "<color=#FFD200>고, 스톱</color>을 선택중입니다.";
            }
            if (_noticeType == NoticeType.SelectCard)
            {
                message.text = "<color=#FFD200>먹을 패</color>를 선택 중 입니다.";
            }
            if (_noticeType == NoticeType.SelectDoublePee)
            {
                message.text = "<color=#FFD200>국진 열끚</color>\n위치를 선택 중 입니다.";
            }
            if (_noticeType == NoticeType.SelectPresident)
            {
                message.text = "<color=#FFD200>총통</color>을 선택 중 입니다.";
            }
            gameObject.SetActive(true);       
        }

        public void Off()
        {
            gameObject.SetActive(false);
        }
    }
}
