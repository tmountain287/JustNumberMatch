using InGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Common.Manager;

namespace Gostop.UI
{
    public class MissionUI : MonoBehaviour
    {
        //[SerializeField] private Text title = null;
        [SerializeField] private RectTransform root = null;
        [SerializeField] private List<GameObject> mulNumList = null;
        [SerializeField] private List<Image> missionCardList = null;
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private GameObject failObj = null;

        [SerializeField] private List<GameObject> otherList = null;
        [SerializeField] private List<GameObject> myList = null;

        [SerializeField] private GameObject mySuccessObj = null;
        [SerializeField] private GameObject otherSuccessObj = null;        

        private void OnDisable()
        {
            //title.text = "";
            Clear();
        }

        public void SetMissionUI(netRoundMission _netRoundMission)
        {
            //if (_netRoundMission == null) return;
            //title.text = _netRoundMission.strMissionName;
            gameObject.SetActive(false);

            if (_netRoundMission == null) return;

            missionCardList.ForEach(card => card.transform.parent.gameObject.SetActive(false));

            mulNumList.ForEach(num => num.SetActive(false));

            if (_netRoundMission.iMultiple > 0)
            {
                mulNumList[_netRoundMission.iMultiple - 2].SetActive(true);
            }

            for (int i = 0; i < _netRoundMission.aMissionCard.Count; i++)
            {
                missionCardList[i].sprite = Resources.Load<Sprite>(string.Format("Image/Cards/Card{0:D2}", _netRoundMission.aMissionCard[i] + 1));
                missionCardList[i].color = Color.white;
                missionCardList[i].transform.parent.gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            root.DOAnchorPosX(0, 0.5f);
        }

        public void SetMissionSuccess(bool _my)
        {
            otherList.ForEach(x => x.SetActive(false));
            myList.ForEach(x => x.SetActive(false));

            missionCardList.ForEach(card=>card.color = Color.gray);

            (_my ? mySuccessObj : otherSuccessObj).SetActive(true);
            (_my ? otherSuccessObj : mySuccessObj).SetActive(false);
            //if(_my)
            //    SoundManager.Instance.PlayFX(successSound);
        }

        public void SetMissionFail(Action _onComplete = null)
        {
            otherList.ForEach(x => x.SetActive(false));
            myList.ForEach(x => x.SetActive(false));
            missionCardList.ForEach(card => card.color = Color.gray);

            mySuccessObj.SetActive(false);
            otherSuccessObj.SetActive(false);

            failObj.SetActive(true);
            root.DOAnchorPosX(400, 0.3f).SetDelay(0.5f).OnComplete(() =>
            {
                _onComplete?.Invoke();
                Clear();
            });
            //SoundManager.Instance.PlayFX(failSound);
        }

        public void SetCheck(int _i, bool _my)
        {
            (_my ? myList[_i] : otherList[_i]).SetActive(true);
            (_my ? otherList[_i] : myList[_i]).SetActive(false);
            missionCardList[_i].color = Color.gray;
        }

        public void Clear()
        {
            otherList.ForEach(x => x.SetActive(false));
            myList.ForEach(x => x.SetActive(false));
            mySuccessObj.SetActive(false);
            otherSuccessObj.SetActive(false);
            failObj.SetActive(false);
            root.anchoredPosition = new Vector2(400, 0);
            gameObject.SetActive(false);
        }
    }
}