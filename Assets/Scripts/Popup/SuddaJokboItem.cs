using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SuddaJokboItem : MonoBehaviour
{
    [SerializeField] private Text nameText = null;
    [SerializeField] private Text rewardText = null;    

    [SerializeField] private GameObject check = null;

    public SuddaJokboType Type { get; set; }

    public void SetItem(string _name, string _reward, SuddaJokboType _type)
    {
        nameText.text = _name;
        rewardText.text = _reward;
        Type = _type;
    }

    public void SetReward(string _reward)
    {
        rewardText.text = _reward;
    }

    public void SetCheck(bool _flag)
    {
        if(_flag)
        {
            check.transform.DOScale(1.2f, 0.05f).SetEase(Ease.InOutSine);
            nameText.fontSize = 46;
            rewardText.fontSize = 46;
            
            transform.SetAsLastSibling();
        }
        else
        {
            check.transform.localScale = Vector3.one;
            nameText.fontSize = 36;
            rewardText.fontSize = 36;
        }

        check.SetActive(_flag);
    }
}
