using Common.Manager;
using Common.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class GamblerScoreBoard : MonoBehaviour
    {
        [SerializeField] private GameObject firstMark = null;
        [SerializeField] private NumberIncrement moneyValue = null;
        
        [SerializeField] private Text nickName = null;

        [SerializeField] private Transform go = null;
        [SerializeField] private Transform bbuk = null;
        [SerializeField] private Transform shake = null;

        [SerializeField] private GameObject score = null;
        [SerializeField] private Text scoreText = null;

        //[SerializeField] private CharImage charImage = null;
        [SerializeField] private GameObject fire = null;
        [SerializeField] private RectTransform gauge = null;

        [SerializeField] private Button button = null;
        [SerializeField] private GameObject newObj = null;
        [SerializeField] private GameObject workdObj = null;

        [SerializeField] private Transform pasanPivot = null;
        

        private void Start()
        {
            if (button != null)
            {
                //button.onClick.AddListener(() =>
                //{
                //    PopupManager.Instance.OpenPopup<MyCharacterPopup>().Initialize();
                //});
            }
        }

        private void OnEnable()
        {
            //if (newObj != null)
            //{
            //    UserDataManager.OnCheckNewCharacter.AddListener(CheckNewIcon);
            //    CheckNewIcon();
            //}

            //if(workdObj!=null)
            //{
            //    UserDataManager.OnCheckNewCharacter.AddListener(CheckWorkdChampion);
            //    CheckWorkdChampion();
            //}
        }

        private void OnDisable()
        {
            //if (newObj != null)
            //{
            //    UserDataManager.OnCheckNewCharacter.RemoveListener(CheckNewIcon);
            //}

            //if (workdObj != null)
            //{
            //    UserDataManager.OnCheckNewCharacter.RemoveListener(CheckWorkdChampion);
            //}
        }

        public void SetProfile(int _characterID)
        {
            //charImage.SetImage(_characterID);
        }

        public void SetFire(bool _flag)
        {
            fire.SetActive(_flag);
        }

        public void SetFirstMark(bool _flag)
        {
            firstMark.SetActive(_flag);
        }

        public void SetName(string _name)
        {
            nickName.text = _name;
        }

        public void OnPasan(GameObject _pasanObj)
        {
            _pasanObj.transform.position = pasanPivot.transform.position;
            _pasanObj.SetActive(true);
        }

        public void SetMoney(long _value, bool _isAni = true)
        {
            moneyValue.SetNumber(_value, _isAni);
        }

        public void Clear()
        {
            SetBBuk(0);
            SetGo(0);
            SetShake(0);

            score.gameObject.SetActive(false);
        }

        public void SetGo(int _count)
        {            
            for (int i = 0; i < go.childCount; i++)
            {
                go.GetChild(i).gameObject.SetActive(false);
            }

            go.GetChild(_count).gameObject.SetActive(true);
        }

        public void SetBBuk(int _count)
        {          
            for (int i = 0; i < bbuk.childCount; i++)
            {
                bbuk.GetChild(i).gameObject.SetActive(false);
            }

            bbuk.GetChild(_count).gameObject.SetActive(true);
        }

        public void SetShake(int _count)
        {          
            for (int i = 0; i < shake.childCount; i++)
            {
                shake.GetChild(i).gameObject.SetActive(false);
            }

            shake.GetChild(_count).gameObject.SetActive(true);
        }

        public void SetScore(int _score)
        {
            if (_score == 0)
            {
                score.SetActive(false);
                return;
            }
                
            scoreText.text = _score.ToString();
            score.SetActive(true);
        }

        public void SetGauge(float _value, bool _isAni = true)
        {
            if (gauge == null)
                return;

            float targetWidth = 198.0f * _value;
            float currentWidth = gauge.sizeDelta.x;

            if (_isAni)
            {
                DOTween.To(() => currentWidth, x =>
                {
                    currentWidth = x;
                    gauge.sizeDelta = new Vector2(currentWidth, gauge.sizeDelta.y);
                }, targetWidth, 0.1f);
            }
            else
            {
                gauge.sizeDelta = new Vector2(targetWidth, gauge.sizeDelta.y);
            }
        }
    }
}