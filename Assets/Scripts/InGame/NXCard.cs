
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NxCard
{
    public int Sn { get; set; } = -1;
    public int SubIndex { get; set; } = -1;
    public bool IsLock { get; set; } = false;
    public CardData CardData { get; set; } = null;

    public NxCard(int _sn, int _slot = -1)
    {
        Sn = _sn;
        SubIndex = _slot == -1 ? _sn / 4 : _slot;
        CardData = Sn == -1 ? null : new(CardDataInfo.CardDataInfoList[Sn]);
    }

    public void ChangeToDoublePee()
    {
        CardData.MainType = CardMainType.PEE;
        CardData.SubType = CardSubType.DOUBLE_PEE;
    }
}

public class NxCardList
{
    public List<NxCard> Cards { get; set; } = new();

    public int Count { get => Cards.Count; }

    public NxCardList() { }
    public NxCardList(List<NxCard> cards)
    {
        Cards = cards;
    }

    public List<int> SnList
    {
        get => Cards.Select(card => card.Sn).ToList();
    }

    public void SetCards(List<int> snList)
    {
        Cards.Clear();
        if (snList == null)
            return;
        snList.ForEach(sn => Add(sn));
    }

    public NxCard GetCard(int _sn)
    {
        NxCard card = Cards.Where(x => x.Sn == _sn).FirstOrDefault();

        if (card == null)
        {
            Log();
            Debug.LogError($"Card가 없습니다. : {_sn}");
        }

        return card;
    }

    public void Add(int _sn)
    {
        Cards.Add(new(_sn));
    }

    public void Add(NxCard _nxCard)
    {
        if (_nxCard == null)
            return;

        if (Cards.Contains(_nxCard))
            return;

        Cards.Add(_nxCard);
    }

    public void Add(List<NxCard> _nxCards)
    {
        if (_nxCards == null)
            return;
        _nxCards.ForEach(x => Add(x));
    }

    public void Remove(int _sn)
    {
        int index = Cards.FindIndex(x => x.Sn == _sn);
        if (index >= 0)
        {
            Cards.RemoveAt(index);
        }
    }

    public void Remove(NxCard _nxCard)
    {
        if (_nxCard == null)
            return;
        Cards = Cards.Where(x => x.Sn != _nxCard.Sn).ToList();
    }

    public void Remove(List<NxCard> _nxCards)
    {
        if (_nxCards == null)
            return;
        _nxCards.ForEach(card => Remove(card));
    }

    public virtual void Clear()
    {
        Cards.Clear();
    }

    public int GetCardsTotalScore(int _mainIndex)
    {
        List<NxCard> cards = Cards.Where(x => x.CardData.MainIndex == _mainIndex).ToList();
        int sum = 0;
        cards.ForEach(x =>
        {
            if (x.CardData.SubType == CardSubType.DOUBLE_PEE)
            {
                sum += 2;
            }
            else if (x.CardData.SubType == CardSubType.THREE_PEE)
            {
                sum += 3;
            }
            else
            {
                sum += 1;
            }
        });
        return sum;
    }

    public NxCard GetCard(CardMainType _mainType)
    {
        return Cards.Where(x => x.CardData.MainType == _mainType).FirstOrDefault();
    }

    public List<NxCard> GetCards(CardMainType _mainType)
    {
        return Cards.Where(x => x.CardData.MainType == _mainType).ToList();
    }

    public List<NxCard> GetLastElements(CardMainType _mainType, int count)
    {
        List<NxCard> cardList = GetCards(_mainType);

        if (cardList.Count == 0)
            return null;

        int startIndex = Mathf.Max(0, cardList.Count - count);
        return cardList.GetRange(startIndex, cardList.Count - startIndex);
    }

    public NxCard GetCard(CardSubType _subType)
    {
        return Cards.Where(x => x.CardData.SubType == _subType).FirstOrDefault();
    }

    public List<NxCard> GetCards(int _subIndex)
    {
        if (_subIndex == -1)
            return null;
        return Cards.Where(x => x.SubIndex == _subIndex && x.SubIndex != -1).ToList();
    }

    public List<NxCard> GetCardsEx(int _subIndex, CardMainType _maintType)
    {
        if (_subIndex == -1)
            return null;
        return Cards.Where(x => x.SubIndex == _subIndex && x.SubIndex != -1 && x.CardData.MainType != _maintType).ToList();
    }

    public List<NxCard> GetWithSameSubIndex(int _count = 4)
    {
        var grouped = Cards
            .Where(card => card.SubIndex != -1)
            .GroupBy(card => card.SubIndex)
            .FirstOrDefault(g => g.Count() == _count);

        return grouped?.Take(4).ToList();
    }

    public void Log()
    {
        string k = "";
        Cards.ForEach(x => k = k + " " + x.Sn.ToString());

        //Debug.Log(k);
    }
}
