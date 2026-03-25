using Common.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gostop.UI
{
    public class ProloguePopup : BasePopup
    {
        [SerializeField] List<ProloguePopupPanel> prologuePopupPanels = new List<ProloguePopupPanel>();

        public void Initialize(Action _lastAction, Action _closeAction)
        {
            for (int i = 0; i < prologuePopupPanels.Count; i++)
            {
                int index = i;

                if(i < prologuePopupPanels.Count - 1)
                {
                    prologuePopupPanels[i].SetNextAction(() =>
                    {
                        prologuePopupPanels[index].gameObject.SetActive(false);
                        prologuePopupPanels[index + 1].gameObject.SetActive(true);
                    });
                }
                else
                {
                    prologuePopupPanels[i].SetNextAction(() =>
                    {
                        _lastAction?.Invoke();
                        ClosePopup(()=>
                        {
                            _closeAction?.Invoke();
                        });                        
                    });
                }
            }

            DOVirtual.DelayedCall(1f, () =>
            {
                prologuePopupPanels[0].gameObject.SetActive(true);
            });
        }
    }
}