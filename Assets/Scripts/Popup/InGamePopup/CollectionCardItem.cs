using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace UI.Popup
{
    public class CollectionCardItem : MonoBehaviour
    {
        [SerializeField] private CollectionCardItemPanel kwang = null;
        [SerializeField] private CollectionCardItemPanel mung = null;
        [SerializeField] private CollectionCardItemPanel ddi = null;
        [SerializeField] private CollectionCardItemPanel pee = null;

        public void SetItemData(PlayerData _playerData)
        {
            List<CollectionData> collectionDatas = new List<CollectionData>();

            _playerData.CollectionTypeList.ForEach(c =>
            {
                CollectionData data = CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == c).FirstOrDefault();
                if (data != null)
                {
                    if (data.score > 0)
                    {
                        collectionDatas.Add(data);
                    }
                }
            });

            CollectionData kwangData = collectionDatas.Where(x => x.mainType == CardMainType.KWANG).FirstOrDefault();

            string strKwang = "0점";

            List<int> snKwangList = _playerData.CollectCards.GetCards(CardMainType.KWANG).Select(x => x.Sn).ToList();

            if (kwangData != null)
            {
                strKwang = $"{kwangData.score}점({kwangData.name}{kwangData.score}점)";                
            }

            kwang.SetPanel(strKwang, snKwangList.Count, snKwangList);

            List<CollectionData> mungDataList = collectionDatas.Where(x => x.mainType == CardMainType.MUNG).ToList();

            int mungCollectionSum = mungDataList.Sum(x => x.score);
            int mungTotal = _playerData.MungScore + mungCollectionSum;

            StringBuilder strMung = new StringBuilder();

            strMung.Append($"{mungTotal}점");

            if (mungTotal > 0)
            {
                if (_playerData.MungScore > 0)
                    strMung.Append($"(열끗{_playerData.MungScore}점");

                for(int i = 0; i < mungDataList.Count; i++)
                {
                    if(i==0 && _playerData.MungScore > 0)
                    {
                        strMung.Append(",");
                    }
                    else if (i == 0 && _playerData.MungScore == 0)
                    {
                        strMung.Append("(");
                    }
                    else
                    {
                        strMung.Append(",");
                    }

                    strMung.Append($"{mungDataList[i].name}{mungDataList[i].score}점");
                }

                strMung.Append(")");
            }

            List<int> snMungList = _playerData.CollectCards.GetCards(CardMainType.MUNG).Select(x => x.Sn).ToList();

            mung.SetPanel(strMung.ToString(), snMungList.Count, snMungList);


            List<CollectionData> ddiDataList = collectionDatas.Where(x => x.mainType == CardMainType.DDI).ToList();

            int ddiCollectionSum = ddiDataList.Sum(x => x.score);
            int ddiTotal = _playerData.DdiScore + ddiCollectionSum;

            StringBuilder strDdi = new StringBuilder();

            strDdi.Append($"{ddiTotal}점");

            if (ddiTotal > 0)
            {
                if (_playerData.DdiScore > 0)
                    strDdi.Append($"(띠{_playerData.DdiScore}점");

                for (int i = 0; i < ddiDataList.Count; i++)
                {
                    if (i == 0 && _playerData.DdiScore > 0)
                    {
                        strDdi.Append(",");
                    }
                    else if (i == 0 && _playerData.DdiScore == 0)
                    {
                        strMung.Append("(");
                    }
                    else
                    {
                        strDdi.Append(",");
                    }

                    strDdi.Append($"{ddiDataList[i].name}{ddiDataList[i].score}점");
                }

                strDdi.Append(")");
            }

            List<int> snDdiList = _playerData.CollectCards.GetCards(CardMainType.DDI).Select(x => x.Sn).ToList();

            ddi.SetPanel(strDdi.ToString(), snDdiList.Count, snDdiList);


            _playerData.CollectCards.Log();

            List<int> snPeeList = _playerData.CollectCards.Cards.Where(x=>x.CardData.MainType == CardMainType.PEE || x.CardData.MainType == CardMainType.JOCKER).ToList().Select(x=>x.Sn).ToList();            

            pee.SetPanel($"{_playerData.PeeScore}점", _playerData.PeeCount, snPeeList);
        }
    }
}