using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI
{
    public class BaseUI : MonoBehaviour
    {
        public enum Type
        {
            NONE,
            INTRO,
            LOBBY,
            STAGE,
            TIMEATTACT,
            GAME,
            DEBUG,
        }

        [SerializeField] private Type type = Type.NONE;
        [SerializeField] private TopUI topUI = null;

        public Type UIType { get => type;}     
        
        protected virtual void OnEnable()
        {
            UIManager.Instance.TopUI = topUI;
        }

        public void SetUI(Type _type)
        {
            gameObject.SetActive(type == _type);
        }
    }
}