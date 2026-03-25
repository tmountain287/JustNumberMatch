using Common.Manager;
using System.Collections;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    private float delayTime = 0.01f;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        GoogleAdManager.Instance.Initialize(() =>
        {
            SoundManager.Instance.MuteSound();
            UIManager.Instance.SetBackAds(true);
        }, () =>
        {
            SoundManager.Instance.RestoreSound();
            UIManager.Instance.SetBackAds(false);
        });
        yield return new WaitForSeconds(delayTime);
        TableDataManager.Instance.LoadAllCSVs();
        yield return new WaitForSeconds(delayTime);
        IAPManager.Instance.Initialize();
        yield return new WaitForSeconds(delayTime);
        FirebaseManager.Instance.Initialize();
        yield return new WaitForSeconds(delayTime);
        PlatformLoginReceiver.Instance.Initialize();
        yield return new WaitForSeconds(delayTime);
        PlatformSocialManager.Instance.Initialize();
        yield return new WaitForSeconds(delayTime);
        UIManager.Instance.ShowUI(Common.UI.BaseUI.Type.LOBBY);
    }
}