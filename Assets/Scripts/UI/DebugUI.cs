using Common.Manager;
using Common.UI;
using Gostop.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugUI : BaseUI
{
    [SerializeField] private Button level1Button = null;
    [SerializeField] private Button level100Button = null;
    [SerializeField] private Button goldUpButton = null;
    //[SerializeField] private Button goldDownButton = null;

    [SerializeField] private Button moneyUpButton = null;

    [SerializeField] private Button suddaResetButton = null;

    [SerializeField] private Button clearButton = null;

    [SerializeField] private Button closeButton = null;

    [SerializeField] private Transform levelUpPopupButton = null;
    [SerializeField] private Transform resultPopupButton = null;

    [SerializeField] private Transform FirePopupButton1 = null;
    [SerializeField] private Transform FirePopupButton2 = null;

    [SerializeField] private Button fireOpenButton = null;

    [SerializeField] private Button newGameButton = null;
    [SerializeField] private Button restartButton = null;

    [SerializeField] private Button peeButton = null;

    [SerializeField] private Text tokenText = null;
    [SerializeField] private Button tokenSendButton = null;


    private int fireIndex1 = 1;
    private int fireIndex2 = 1;

    private void OnEnable()
    {
        tokenText.text = FirebasePushReceiver.Instance.PushToken;
    }

    void Start()
    {
        level1Button.onClick.AddListener(() =>
        {
            UserDataManager.LevelUp();
        });

        level100Button.onClick.AddListener(() =>
        {
            for (int i = 0; i < 100; i++)
                UserDataManager.LevelUp();
        });

        goldUpButton.onClick.AddListener(() =>
        {
            UserDataManager.AddGold(10000);
            UserDataManager.Save();
        });

        moneyUpButton.onClick.AddListener(() =>
        {
            UserDataManager.AddMoney(100000000);
        });

        peeButton.onClick.AddListener(() =>
        {
            UserDataManager.PeeStealCount++;
            UserDataManager.Save();
        });

        clearButton.onClick.AddListener(() =>
        {
            //PlatformLoginReceiver.Instance.LogOut();
            //UserDataManager.ClearData();
            //NetworkManager.Instance.ClearData();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });

        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OnUI(Type.DEBUG, false);
        });

        newGameButton.onClick.AddListener(() =>
        {
            InGameManager.Instance.GameClear();
        });

        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });

        suddaResetButton.onClick.AddListener(() =>
        {
            UserDataManager.UserData.suddaData.playCount = 0;
            UserDataManager.OnValueSuddaPlayCountChanged.Invoke(0);
            UserDataManager.Save();
        });

        tokenSendButton.onClick.AddListener(() =>
        {
            new NativeShare()
                   .SetSubject("최고수맞고 푸쉬 토큰")
                   .SetText(FirebasePushReceiver.Instance.PushToken)
                   .SetTitle("앱 공유")
                   .Share();
        });

        for (int i = 0; i < levelUpPopupButton.childCount; i++)
        {
            int index = i + 1;
            Button button = levelUpPopupButton.GetChild(i).GetComponent<Button>();
            button.transform.GetChild(0).GetComponent<Text>().text = index.ToString();          

            button.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<InGameLevelupPopup>((popup) =>
                {
                    popup.popupType = PopupType.NONE;
                }).Initialize(index, TableDataManager.Instance.TableCharacterData.GetCharacterTableData(index), true);
            });
        }

        for (int i = 0; i < resultPopupButton.childCount; i++)
        {
            int index = i + 1;
            Button button = resultPopupButton.GetChild(i).GetComponent<Button>();
            button.transform.GetChild(0).GetComponent<Text>().text = index.ToString();

            button.onClick.AddListener(() =>
            {
                PopupManager.Instance.OpenPopup<InGameResultPopup>((popup) =>
                {
                    popup.popupType = PopupType.NONE;
                }).Initialize(TableDataManager.Instance.TableCharacterData.GetCharacterTableData(index));
            });
        }

        for (int i = 0; i < FirePopupButton1.childCount; i++)
        {
            int index = i + 1;
            Transform testT = FirePopupButton1.GetChild(i);
            testT.GetChild(0).GetComponent<Text>().text = index.ToString();
            Button b = testT.GetComponent<Button>();
            testT.GetComponent<Button>().onClick.AddListener(() =>
            {
                fireIndex1 = index;

                for (int j = 0; j < FirePopupButton1.childCount; j++)
                {
                    FirePopupButton1.GetChild(j).GetComponent<Image>().color = Color.white;
                }

                testT.GetComponent<Image>().color = Color.gray;

            });
        }

        for (int i = 0; i < FirePopupButton2.childCount; i++)
        {
            int index = i + 1;
            Transform testT = FirePopupButton2.GetChild(i);
            testT.GetChild(0).GetComponent<Text>().text = index.ToString();
            Button b = testT.GetComponent<Button>();
            testT.GetComponent<Button>().onClick.AddListener(() =>
            {
                fireIndex2 = index;

                for (int j = 0; j < FirePopupButton2.childCount; j++)
                {
                    FirePopupButton2.GetChild(j).GetComponent<Image>().color = Color.white;
                }

                testT.GetComponent<Image>().color = Color.gray;

            });
        }

        fireOpenButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.OpenPopup<InGameFireMatchPopup>((popup) =>
            {
                popup.popupType = PopupType.NONE;
            }).Initialize(TableDataManager.Instance.TableCharacterData.GetCharacterTableData(fireIndex1), TableDataManager.Instance.TableCharacterData.GetCharacterTableData(fireIndex2), false, true);
        });
    }   
}
