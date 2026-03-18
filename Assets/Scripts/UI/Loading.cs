using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [SerializeField] private Image back = null;
    [SerializeField] private GameObject root = null;

    private void OnEnable()
    {
        Invoke(nameof(PlaySequence), 0.2f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(PlaySequence));
        root.SetActive(false);
        back.DOKill();
        back.color = new Color(0, 0, 0, 0);
    }

    public void PlaySequence()
    {
        root.SetActive(true);

        back.DOFade(0.7f, 0.1f);
    }
}