using Common.Manager;
using Common.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gostop.UI
{
    public class AgreementPopup : BasePopup
    {
        [SerializeField] private Button startButton = null;
        [SerializeField] private Text startButtonText = null;

        [SerializeField] private Button agreeShowButton1 = null;
        [SerializeField] private Button agreeShowButton2 = null;

        [SerializeField] private Toggle agreeToggle1 = null;
        [SerializeField] private Toggle agreeToggle2 = null;
        [SerializeField] private Toggle agreeToggle3 = null;

        [SerializeField] private Toggle totalToggle = null;

        private Action startAction = null;

        protected override void Start()
        {
            base.Start();

            void SetStartButtonText()
            {
                startButtonText.color = startButton.interactable ? new(1,1,1,1) : new(128/255f, 128 / 255f, 128 / 255f, 70 / 255f);
            }

            startButton.onClick.AddListener(() =>
            {
                //PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.PUSH, agreeToggle3.isOn ? 1 : 0);
                string message = $"{DateTime.Now:yyyy년 M월 d일} 광고성 정보가\n<color=#FF8436>수신 {(agreeToggle3.isOn ? "동의" : "거부")}</color> 처리되었습니다.";
                AlarmMessgeManager.Instance.OnMessage(message);
                ClosePopup(startAction);
            });

            agreeShowButton1.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.SERVICE_URL);

            });

            agreeShowButton2.onClick.AddListener(() =>
            {
                Application.OpenURL(AppDefine.PRIVACY_URL);

            });

            agreeToggle1.onValueChanged.AddListener((isOn) =>
            {
                totalToggle.onValueChanged.RemoveAllListeners();
                if (isOn)
                {
                    if(agreeToggle2.isOn && agreeToggle3.isOn)
                    {
                        totalToggle.isOn = true;
                        startButton.interactable = true;
                        SetStartButtonText();


                    }
                    else if (agreeToggle2.isOn)
                    {
                        startButton.interactable = true;
                        SetStartButtonText();
                    }
                }
                else
                {
                    totalToggle.isOn = false;
                    startButton.interactable = false;
                }
                totalToggle.onValueChanged.AddListener((isOn) =>
                {
                    agreeToggle1.isOn = isOn;
                    agreeToggle2.isOn = isOn;
                    agreeToggle3.isOn = isOn;

                });
            });

            agreeToggle2.onValueChanged.AddListener((isOn) =>
            {
                totalToggle.onValueChanged.RemoveAllListeners();
                if (isOn)
                {
                    if (agreeToggle1.isOn && agreeToggle3.isOn)
                    {                        
                        totalToggle.isOn = true;
                        startButton.interactable = true;
                        SetStartButtonText();
                    }
                    else if(agreeToggle1.isOn)
                    {
                        startButton.interactable = true;
                        SetStartButtonText();
                    }
                }
                else
                {
                    totalToggle.isOn = false;
                    startButton.interactable = false;
                    SetStartButtonText();
                }
                totalToggle.onValueChanged.AddListener((isOn) =>
                {
                    agreeToggle1.isOn = isOn;
                    agreeToggle2.isOn = isOn;
                    agreeToggle3.isOn = isOn;

                });

            });

            agreeToggle3.onValueChanged.AddListener((isOn) =>
            {
                totalToggle.onValueChanged.RemoveAllListeners();
                if (isOn)
                {
                    if (agreeToggle1.isOn && agreeToggle2.isOn)
                    {
                        totalToggle.isOn = true;
                        //startButton.interactable = true;
                    }
                }
                else
                {
                    totalToggle.isOn = false;
                    //startButton.interactable = false;
                }
                totalToggle.onValueChanged.AddListener((isOn) =>
                {
                    agreeToggle1.isOn = isOn;
                    agreeToggle2.isOn = isOn;
                    agreeToggle3.isOn = isOn;

                });

            });

            totalToggle.onValueChanged.AddListener((isOn) =>
            {
                agreeToggle1.isOn = isOn;
                agreeToggle2.isOn = isOn;
                agreeToggle3.isOn = isOn;
                startButton.interactable = isOn;
                SetStartButtonText();

            });
        }
        public void Initialize(Action<bool> _startAction)
        {
            startAction = () => _startAction.Invoke(agreeToggle3.isOn);
        }
    }
}