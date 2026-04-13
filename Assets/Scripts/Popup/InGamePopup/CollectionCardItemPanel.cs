using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UI.Popup;

namespace UI
{
    public class CollectionCardItemPanel : MonoBehaviour
    {
        [SerializeField] private Text score = null;
        [SerializeField] private Transform layout = null;
        [SerializeField] private List<Image> cardList = null;
        [SerializeField] private CardPositionGroupCount cardPositionGroupCount = null;

        private void OnValidate()
        {
            if (layout == null)
            {
                layout = transform.Find("Layout");
                cardList = layout.GetComponentsInChildren<Image>().ToList();
            }
        }

        public void SetPanel(string _score, int _count, List<int> _snList)
        {
            score.text = _score;
            cardList.ForEach(card => card.gameObject.SetActive(false));

            for (int i = 0; i < _snList.Count; i++)
            {
                cardList[i].sprite = Resources.Load<Sprite>(string.Format("Image/Cards/Card{0:D2}", _snList[i] + 1));
                cardList[i].gameObject.SetActive(true);
            }

            cardPositionGroupCount.SetCount(_count);
            cardPositionGroupCount.SetTransform(_count, _snList.Count > 0 ? cardList[_snList.Count - 1].transform : null);
        }
    }
}
