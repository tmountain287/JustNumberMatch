using Common.Manager;
using Common.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI.Popup;
using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;


public class IntroUI : BaseUI
{
    [SerializeField] private GameObject logo;
    [SerializeField] private Image charImage;
    [SerializeField] private Slider progressbar;
    [SerializeField] private LocalChangeTextEvent progressText;

    private ProgressFlow flow;

    /// <summary>인트로 플로우는 앱에서 한 번만. 씬에 IntroUI가 둘 이상이거나 Start가 두 번 오면 EnterStep.RunAsync도 중복 실행됨.</summary>
    private static int s_introFlowRunGate;

    private void Start()
    {
        progressbar.gameObject.SetActive(true);
        flow = new ProgressFlow(progressbar);

        Run().Forget();
    }

    private async UniTaskVoid Run()
    {
        if (Interlocked.CompareExchange(ref s_introFlowRunGate, 1, 0) != 0)
        {
            Debug.LogWarning("[IntroUI] Run 이미 진행 중이거나 완료됨 — 중복 진입 무시(EnterStep 이중 호출 방지). 씬에 IntroUI가 두 개 없는지 확인하세요.");
            return;
        }

        try
        {
            await RunIntroFlowCoreAsync();
        }
        finally
        {
            Interlocked.Exchange(ref s_introFlowRunGate, 0);
        }
    }

    private async UniTask RunIntroFlowCoreAsync()
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

        var ctx = new IntroFlowContext(
            ui: this,
            progress: flow,
            setTextKey: key => progressText.EntryKey = key
        );

        // 오프라인이면 Enter만
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            await new EnterStep().RunAsync(ctx);
            Debug.Log("dasfasdf");
            return;
        }

        // Steps 조립
        var runner = new IntroFlowRunner(new IIntroFlowStep[]
        {
            //new VersionCheckStep(),
            new FirebaseInitStep(),
            new SaveStep(),
            new EnterStep(),
        });

        await runner.RunAsync(ctx);
    }
}
