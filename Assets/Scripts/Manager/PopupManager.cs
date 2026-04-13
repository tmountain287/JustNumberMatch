using Common.UI;
using UI.Popup;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;

namespace Common.Manager
{
    public enum PopupType
    {
        NONE,
        INGAME,
    }

    public class PopupManager : MonoSingleton<PopupManager>
    {
        #region Inspector Fields
        [SerializeField] private Transform popups = null;
        [SerializeField] private Transform ingamePopups = null;

        [SerializeField] private ObjectPool messageBoxPool = null;
        #endregion

        private List<BasePopup> popupList = new();
        private Dictionary<PopupType, List<BasePopup>> popupStackDic = new();

        public int OpenPopupCount { get { return popupStackDic.Values.Sum(list => list.Count); } }
        
        private void Awake()
        {
            messageBoxPool.CreateObjectPool();
            popupList = new List<BasePopup>(popups.GetComponentsInChildren<BasePopup>(true));
        }

        private bool IsOpenPopup(BasePopup popup)
        {
            if(popupStackDic.ContainsKey(popup.popupType))
            {
                return popupStackDic[popup.popupType].Contains(popup);
            }
            return false;
        }

        public T OpenPopup<T>(Action<T> _initialize = null) where T : BasePopup
        {
            T popup = popupList.Find(x => x is T) as T;

            if (popup != null)
            {
                if(!popup.AllowDuplicates && IsOpenPopup(popup))
                {                    
                    return popup;
                }

                _initialize?.Invoke(popup);

                if(popup.popupType == PopupType.INGAME)
                {
                    popup.transform.SetParent(ingamePopups);
                }
                
                popup.transform.SetAsLastSibling();                
                popup.Show();
                if(popupStackDic.ContainsKey(popup.popupType))
                {
                    popupStackDic[popup.popupType].Add(popup);
                }
                else
                {
                    popupStackDic.Add(popup.popupType, new List<BasePopup> { popup });
                }                
            }
          
            return popup;
        }

        public bool ClosePopup(PopupType _popupType = PopupType.NONE, Action _action = null)
        {
            if (!popupStackDic.ContainsKey(_popupType))
            {
                Debug.Log($"{_popupType}이 없습니다.");
                return false;
            }

            List<BasePopup> basePopups = popupStackDic[_popupType];
            if (basePopups.Count == 0)
                return false;
            if (!basePopups[^1].CanClose)
                return false;

            basePopups[^1].Close(() =>
            {
                basePopups.RemoveAt(basePopups.Count - 1);
                _action?.Invoke();
            });

            return true;
        }

        public void ClosePopup<T>(Action _action = null) where T : BasePopup
        {
            List<BasePopup> allLists = popupStackDic.Values.SelectMany(list => list).ToList();

            T popup = (T)allLists.FirstOrDefault(x => x is T);

            if(popup != null)
            {
                popup.Close(_action);
                popupStackDic = popupStackDic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Where(x => x is not T).ToList());
            }
        }

        public T FindOpenPopup<T>() where T : BasePopup
        {
            foreach (List<BasePopup> list in popupStackDic.Values)
            {
                foreach (BasePopup p in list)
                {
                    if (p is T t)
                        return t;
                }
            }
            return null;
        }

        public void AllClosePopup(PopupType _popupType)
        {
            if (!popupStackDic.ContainsKey(_popupType))
            {
                return;
            }          
              
             
            List<BasePopup> basePopups = popupStackDic[_popupType];

            basePopups.ForEach(popup =>  
            {
                if(popup.popupType == _popupType)
                {
                    popup.Close();
                }
            });

            popupStackDic.Remove(_popupType);
        }


        public void OpenMessageBoxPopup(string _title, string _message, Action _onComplete = null, bool _useCancleButton = false, Action _onCancle = null)
        {
            MessageBoxPopup popup = messageBoxPool.GetObjectFromPool<MessageBoxPopup>();
            popup.transform.SetParent(popups);
            RectTransform popupRect = popup.GetComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            popup.transform.localPosition = Vector3.zero;
            popup.transform.localScale = Vector3.one;
            popup.transform.SetAsLastSibling();
            popup.Show();
            if (popupStackDic.ContainsKey(popup.popupType))
            {
                popupStackDic[popup.popupType].Add(popup);
            }
            else
            {
                popupStackDic.Add(popup.popupType, new List<BasePopup> { popup });
            }
            Action cancleAction = null;
            if (_useCancleButton)
            {
                cancleAction = () =>
                {
                    ClosePopup(PopupType.NONE, () =>
                    {
                        messageBoxPool.ReturnObjectToPool(popup.gameObject);
                        _onCancle?.Invoke();
                    });
                };
            }

            popup.Initialize(_title, _message,
                _onOkClick: () =>
                {
                    ClosePopup(PopupType.NONE, () =>
                    {
                        messageBoxPool.ReturnObjectToPool(popup.gameObject);
                        _onComplete?.Invoke();
                    });
                },
                _onCancelClick: cancleAction);
        }

#if (UNITY_ANDROID || UNITY_IOS) // && !UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIManager.Instance.IsLoading)
                    return;

                if (!popupStackDic.ContainsKey(PopupType.NONE))
                {
                    OpenPopup<ExitPopup>();
                }
                else
                {
                    List<BasePopup> basePopups = popupStackDic[PopupType.NONE];
                    if (basePopups.Count == 0)
                    {
                        OpenPopup<ExitPopup>();
                    } 
                    else if (basePopups[^1].CanBackButton && basePopups[^1].CanClose)
                    {
                        basePopups[^1].Close(()=>
                        {
                            basePopups.RemoveAt(basePopups.Count - 1);
                        });
                        
                    }

                }                
            }
        }
#endif
    }
}