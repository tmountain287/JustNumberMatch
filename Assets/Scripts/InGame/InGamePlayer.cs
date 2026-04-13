using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CInGamePlayer
{
    //public List<CardData> HandCards { get; set; } = new();
    //public List<CardData> CollectionCardList { get; set; } = new();
    
    public PlayerData PlayerData { get; set; } = null;

    public CInGamePlayer(PlayerData _playerData)
    {
        PlayerData = new();
        SetPlayerData(_playerData);
    }

    public void SetPlayerData(PlayerData _playerData)
    {
        PlayerData.name = _playerData.name;
        PlayerData.characterID = _playerData.characterID;
        PlayerData.isFirst = _playerData.isFirst;
        PlayerData.slotIndex = _playerData.slotIndex;
        PlayerData.GaugeValue.Value = _playerData.GaugeValue.Value;
        PlayerData.Money.Value = _playerData.Money.Value;
        PlayerData.HandCards = _playerData.HandCards;
        PlayerData.CollectCards = _playerData.CollectCards;
        PlayerData.BbukList = _playerData.BbukList;
        PlayerData.PresidentList = _playerData.PresidentList;
        PlayerData.CollectionTypeList = _playerData.CollectionTypeList;
        PlayerData.shakeCount = _playerData.shakeCount;
        PlayerData.bbukCount = _playerData.bbukCount;
        PlayerData.goCount = _playerData.goCount;
        PlayerData.MissionMultiple = _playerData.MissionMultiple;
    }
}
