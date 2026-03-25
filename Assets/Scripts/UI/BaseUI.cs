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
            GAME,
            DEBUG,
        }

        [SerializeField] private Type type = Type.NONE;

        public Type UIType { get => type;}        

        public void SetUI(Type _type)
        {
            gameObject.SetActive(type == _type);
        }
    }
}