using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinStatusTween : MonoBehaviour
{
    [SerializeField] private RectTransform text1Rect;
    [SerializeField] private RectTransform text2Rect;
    [SerializeField] private RectTransform maskArea;

    [SerializeField] private Text text1;
    [SerializeField] private Text text2;


    [SerializeField] private float scrollDuration = 1.5f;    
    [SerializeField] private float stayDuration = 1f;

    private Vector2 topPos;
    private Vector2 centerPos;
    private Vector2 bottomPos;

    private bool isText1Active = true;

    private void Start()
    {
        SetText();
        float height = maskArea.rect.height;
        topPos = new Vector2(0, height);      // 위 (시작 위치)
        centerPos = Vector2.zero;             // 중앙
        bottomPos = new Vector2(0, -height);  // 아래 (사라짐)

        text1Rect.anchoredPosition = new(text1Rect.anchoredPosition.x, centerPos.y);
        text2Rect.anchoredPosition = new(text2Rect.anchoredPosition.x, topPos.y);

        StartScroll();
    }

    private void OnEnable()
    {
        UserDataManager.OnValueResultChanged.AddListener(SetText);
    }

    private void OnDisable()
    {
        UserDataManager.OnValueResultChanged.RemoveListener(SetText);
    }

    void SetText()
    {
        text1.text = $"현재 {UserDataManager.UserData.winningStreak}연승 중";
        text2.text = $"승률 {UserDataManager.GetWinRate()}";
    }

    void StartScroll()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(stayDuration);

        seq.AppendCallback(() => AnimateSwap());
        seq.AppendInterval(scrollDuration);

        seq.SetLoops(-1);
    }

    void AnimateSwap()
    {
        RectTransform current = isText1Active ? text1Rect : text2Rect;
        RectTransform next = isText1Active ? text2Rect : text1Rect;

        current.DOAnchorPosY(bottomPos.y, scrollDuration).SetEase(Ease.InOutQuad);
        next.anchoredPosition = new(next.anchoredPosition.x, topPos.y); // 위에 미리 세팅
        next.DOAnchorPosY(centerPos.y, scrollDuration).SetEase(Ease.InOutQuad);

        isText1Active = !isText1Active;
    }
}
