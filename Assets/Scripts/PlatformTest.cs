using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlatformTest : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] Button loginButton;

    [SerializeField] Button saveButton;
    [SerializeField] Button loadButton;
    // Start is called before the first frame update
    void Start()
    {
        loginButton.onClick.AddListener(() =>
        {
            PlatformLoginReceiver.Instance.StartLogin(async ()=>
            {
                text.text = PlatformLoginReceiver.Instance.Token;
                //var res1 = await BackendApi.PostJson("http://127.0.0.1:5000/api/verifyGoogleIdToken", $"{{\"idToken\":\"{PlatformLoginReceiver.Instance.Token}\"}}");

                var res1 = await NetworkManager.Instance.LinkAccountAsync();

                if(res1.success)
                {
                    var result = await NetworkManager.Instance.SaveUserDataAsync();

                    
                }
                else
                {
                    //에러
                }

                Debug.Log(res1.result.GoogleUser.Uid);



            },(error)=>
            {
                text.text = error;
            });
        });

        saveButton.onClick.AddListener(async () =>
        {
            UserDataManager.Load();

            Debug.Log(JsonUtility.ToJson(UserDataManager.UserData));

            string record = SecurePlayerPrefs.Encrypt(UserDataManager.UserData);

            var result = await FirebaseFirestoreManager.Instance.SaveData(SystemInfo.deviceUniqueIdentifier, record);
            text.text = result;
        });

        loadButton.onClick.AddListener(async () =>
        {
            var result = await FirebaseFirestoreManager.Instance.GetRecord(SystemInfo.deviceUniqueIdentifier);
            Debug.Log(result);
            text.text = result;
        });
    }

//    // 일회성
//    string payload = $"{{\"packageName\":\"{Application.identifier}\",\"productId\":\"gold_1000\",\"purchaseToken\":\"{purchaseToken}\",\"acknowledge\":true}}";
//    string resp = await BackendApi.PostJson("https://us-central1-justonematch-d2cfe.cloudfunctions.net/verifyPurchase", payload);
//    Debug.Log(resp);

//// 구독
//string payload2 = $"{{\"packageName\":\"{Application.identifier}\",\"subscriptionId\":\"vip_monthly\",\"purchaseToken\":\"{purchaseToken}\",\"acknowledge\":true}}";
//    string resp2 = await BackendApi.PostJson("https://us-central1-justonematch-d2cfe.cloudfunctions.net/verifyPurchase", payload2);
//    Debug.Log(resp2);

   
}
