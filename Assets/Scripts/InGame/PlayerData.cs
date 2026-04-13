using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerData
{
    public string name;
    public int characterID;
    public bool isFirst;
    public int slotIndex;
    public ObservableProperty<long> Money = new(0);
    public ObservableProperty<int> GaugeValue = new(0);
    public NxCardList HandCards { get; set; } = new();
    public NxCardList CollectCards { get; set; } = new();

    public List<int> BbukList { get; set; } = new();
    public List<int> PresidentList { get; set; } = new();

    public List<CollectionType> CollectionTypeList = new();

    public int shakeCount = 0;
    public int bbukCount = 0;
    public int goCount = 0;

    public bool IsKwangBak { get => KwangCount == 0; }
    public bool IsPeeBak { get => PeeCount < 8 && PeeCount > 0; }
    public bool IsGoBak { get => goCount > 0; }

    public bool IsChungdan { get=>CollectionTypeList.Contains(CollectionType.CHUNGDAN); }
    public bool IsHongdan { get => CollectionTypeList.Contains(CollectionType.HONGDAN); }
    public bool IsChodan { get => CollectionTypeList.Contains(CollectionType.CHODAN); }
    public bool IsGodori { get => CollectionTypeList.Contains(CollectionType.GODORI); }

    public bool IsRealPeeBak
    {
        get
        {
            if (!IsPeeBak)
            {
                return false;
            }
            else
            {
                if (PeeCount == 6 || PeeCount == 7)
                {
                    NxCard card = CollectCards.GetCard(CardSubType.GUKJUN);
                    if (card != null && !card.IsLock)
                    {
                        return false;
                    }
                }
                    
                return true;
            }
        }
    }

    public int KwangCount { get => CollectCards.GetCardsTotalScore(0); }
    public int MungCount { get => CollectCards.GetCardsTotalScore(1); }
    public int DdiCount { get => CollectCards.GetCardsTotalScore(2); }
    public int PeeCount { get => CollectCards.GetCardsTotalScore(3); }

    public int MungMultiple { get => MungCount >= 7 ? 2 : 1; }
    public int ShakeMultiple { get => (int)Mathf.Pow(2, shakeCount); }
    public int GoMultiple { get => (int)Mathf.Pow(2, Mathf.Max(0, goCount - 2)); }
    public int MissionMultiple = 1;

    public int MungScore
    {
        get => Mathf.Max(0, MungCount - 4);
    }

    public int DdiScore
    {
        get => Mathf.Max(0, DdiCount - 4);
    }

    public int PeeScore
    {
        get => Mathf.Max(0, PeeCount - 9);
    }

    public int CollectionScore
    {
        get
        {
            int sum = MungScore + DdiScore + PeeScore + goCount;

            CollectionTypeList.ForEach(ct =>
            {
                CollectionData cd = CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == ct).FirstOrDefault();
                if (cd != null)
                    sum += cd.score;
            });

            return sum;
        }
    }

    public bool CanGukjunToGo
    {
        get
        {
            NxCard card = CollectCards.GetCard(CardSubType.GUKJUN);
            if (card != null && !card.IsLock)
            {
                return ScoreGukjunToGo >= 7;
            }

            return false;
        }
    }

    public int ScoreGukjunToGo
    {
        get
        {
            NxCard card = CollectCards.GetCard(CardSubType.GUKJUN);
            if (card != null && !card.IsLock)
            {
                //점수를 다시 계산
                int sum = Mathf.Max(0, MungCount - 1 - 4) + DdiScore + Mathf.Max(0, PeeCount + 2 - 9);

                CollectionTypeList.ForEach(ct =>
                {
                    CollectionData cd = CollectionDataInfo.CollectionDataInfoList.Where(x => x.collectionType == ct).FirstOrDefault();
                    if (cd != null)
                        sum += cd.score;
                });

                return sum;
            }

            return CollectionScore;
        }
    }


    public bool CanGo
    {
        get => CollectionScore >= 7 && CollectionScore > MaxScore;
    }

    public int MaxScore = 0;

    public PlayerData() { }
    public PlayerData(string name, int characterID, bool isFirst, int slotIndex, long money, int gauge = 0)
    {
        this.name = name;
        this.characterID = characterID;
        this.isFirst = isFirst;
        this.slotIndex = slotIndex;
        Money.Value = money;
        GaugeValue.Value = gauge;
    }

    public void Refresh(string name, int characterID, bool isFirst, int slotIndex, long money)
    {
        this.name = name;
        this.characterID = characterID;
        this.isFirst = isFirst;
        this.slotIndex = slotIndex;
        Money.SetValue(money);
    }

    //public void AddCollectionType(CollectionType _collectionType)
    //{
    //    // 해당 타입의 데이터 가져오기
    //    var data = CollectionDataInfo.CollectionDataInfoList.FirstOrDefault(c => c.collectionType == _collectionType);
    //    if (data == null) return;

    //    // 1. score가 0이면 제외
    //    if (data.score == 0) return;

    //    // 2. group이 -1이면 그냥 추가 (중복이면 무시)
    //    if (data.group == -1)
    //    {
    //        if (!CollectionTypeList.Contains(_collectionType))
    //        {
    //            CollectionTypeList.Add(_collectionType);
    //        }
    //        return;
    //    }

    //    // 3. 같은 group이 이미 있는지 확인
    //    var existingType = CollectionTypeList
    //        .Select(ct => CollectionDataInfo.CollectionDataInfoList.FirstOrDefault(c => c.collectionType == ct))
    //        .FirstOrDefault(c => c != null && c.group == data.group);

    //    if (existingType != null)
    //    {
    //        if ((int)_collectionType > (int)existingType.collectionType)
    //        {
    //            // 새 enum 값이 더 크면 기존 거 제거하고 새로 추가
    //            CollectionTypeList.Remove(existingType.collectionType);
    //            CollectionTypeList.Add(_collectionType);
    //        }
    //        // 작으면 아무것도 안 함
    //    }
    //    else
    //    {
    //        // 같은 그룹이 없으면 추가
    //        CollectionTypeList.Add(_collectionType);
    //    }
    //}

    public void Clear()
    {
        shakeCount = 0;
        bbukCount = 0;
        goCount = 0;
        MissionMultiple = 1;
        MaxScore = 0;
        HandCards.Clear();
        CollectCards.Clear();
        BbukList.Clear();
        CollectionTypeList.Clear();
    }
}