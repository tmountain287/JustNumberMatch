using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JustOneMatch.UI;
using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUI : BaseUI
{
    [SerializeField] private GameObject logo;
    [SerializeField] private Image charImage;
    [SerializeField] private Slider progressbar;
    [SerializeField] private LocalChangeTextEvent progressText;
    [SerializeField] private Button startButton;

    private ProgressFlow flow;

    private void Start()
    {
        //UIManager.Instance.ShowUI(UIType);

        //startButton.onClick.AddListener(() => UIManager.Instance.ShowUI(Type.STAGE));

        //progressbar.gameObject.SetActive(true);
        //flow = new ProgressFlow(progressbar);

        //Run().Forget();
    }

    private async UniTaskVoid Run()
    {
        UserDataManager.Load();

        logo.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

        Color c = Color.black;
        c.a = 0;
        charImage.color = c;
        charImage.gameObject.SetActive(true);        
        charImage.DOFade(1, 0.5f);
        charImage.DOColor(Color.white, 0.3f).SetDelay(0.8f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        progressbar.gameObject.SetActive(true);

        var ctx = new LobbyFlowContext(
            ui: this,
            progress: flow,
            setTextKey: key => progressText.EntryKey = key
        );

        // 오프라인이면 Enter만
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            await new EnterStep().RunAsync(ctx);
            return;
        }

        // Steps 조립
        var runner = new LobbyFlowRunner(new ILobbyFlowStep[]
        {
            new VersionCheckStep(),
            new FirebaseInitStep(),
            new SaveStep(),
            new EnterStep(),
        });

        await runner.RunAsync(ctx);
    }
}
