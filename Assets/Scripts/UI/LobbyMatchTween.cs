using DG.Tweening;
using UnityEngine;

public class LobbyMatchTween : MonoBehaviour
{
    private void OnEnable()
    {
        //gameObject.SetActive(false);



        Vector3 randomPos = new Vector3(
                    UnityEngine.Random.Range(0, 1) == 0 ? -1000 : 1000,
                    UnityEngine.Random.Range(-300f, 300f),
                    0f
                );

        Quaternion randomRot = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-180f, 180f));

        transform.localPosition = randomPos;
        transform.localRotation = randomRot;
        //return;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(Random.Range(0, 0.3f)); // 0.5초 기다림
        //seq.AppendCallback(() => gameObject.SetActive(true));

        // 1) 이동 + 회전 + 스케일 연출
        seq.Append(
            transform.DOLocalMove(Vector3.zero, 0.9f)
            .SetEase(Ease.OutBack)
        );

        seq.Join(
            transform.DOLocalRotate(Vector3.forward * 360f, 0.9f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutBack)
        );

        seq.Join(
            transform.DOScale(1.1f, 0.45f).SetLoops(2, LoopType.Yoyo)
        );

        // 2) 착지 느낌
        seq.Append(
            transform.DOPunchPosition(Vector3.up * 10f, 0.25f, 8, 0.5f)
        );

        seq.Play();
        transform.localScale = Vector3.one;
    }
}
