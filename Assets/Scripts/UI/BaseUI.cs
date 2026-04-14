using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    public class BaseUI : MonoBehaviour
    {
        public enum Type
        {
            INTRO,
            LOBBY,
            STAGE,
            TIMEATTACT,
            GAME,
            DEBUG,
            MAX,
        }

        [SerializeField] private Type type = Type.INTRO;        

        public Type UIType { get => type;}     
        
        protected virtual void OnEnable()
        {
            
        }

        public void SetUI(Type _type)
        {
            Debug.Log("ddddddddddddd");
            gameObject.SetActive(type == _type);
        }
    }
}