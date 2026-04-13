using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class HandCardIcon : MonoBehaviour
    {
        public enum Type
        {
            Gray = 0,
            Blue = 1,
            Red = 2,
            Bell = 3,
            Bomb = 4,
            None = 5,
        }

        [SerializeField] private Image icon = null;        

        public void OffIcon()
        {
            for(int i=0;i<transform.childCount;i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            icon.gameObject.SetActive(false);
        }

        public void SetIcon(Type _type)
        {
            if(_type == Type.None)            
                return;

            transform.GetChild((int)_type).gameObject.SetActive(true);
        }
    }
}