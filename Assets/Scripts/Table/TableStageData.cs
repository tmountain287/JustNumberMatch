using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum StageType
{
    Normal,
    Boss,
}

[Serializable]
public class StageTableData
{
    public DifficultyType difficultyType;
    public int id;
    public int stage;
    public int gate;
    public StageType stageType;
    public int equtionDataID;
    public int randomStartID;
    public int randomEndID;
    public Dictionary<ItemType, int> rewardItemDic = new();
    public int starMax;
    public bool isMax;
    public bool isLock;

    //public EquationTableData(string[] row, DifficultyType _difficultyType )
    //{        
    //    if (row.Length >= 6)
    //    {
    //        int.TryParse(row[0], out stage);
    //        start_Equation = row[1].Trim().Replace("'", "").Replace(" ", "");
    //        target_Equation = row[2].Trim().Replace("'", "").Replace(" ", ""); ;

    //        //List<string> strings = SplitWith11(start_Equation);

    //        List<string> strings = row[16].Trim().Replace("'","").Split(";").ToList();

    //        strings.ForEach(x =>
    //        {
    //            equationDataList.Add(ParseStringToIntOrEnum(x));
    //        });

    //        //row[17].Trim().Replace("'", "").Split(";").ToList().ForEach(x =>
    //        //{
    //        //    targetEquationDataList.Add(ParseStringToIntOrEnum(x));
    //        //});

    //        //List<RecognizerInfo> tokens = new();

    //        //targetEquationDataList.ForEach(x =>
    //        //{
    //        //    tokens.Add(new(x.RecognizerType, x.Value));
    //        //});

    //        //var ok = EquationValidator.TryValidateTokens(tokens, out var res1);

    //        //if (res1.Success && res1.AreAllEqual)
    //        //{
    //        //    Debug.Log("성궁");
    //        //}
    //        //else
    //        //{
    //        //    Debug.LogError(res1.ToString());
    //        //}

    //        int.TryParse(row[4], out changedIndex);
    //        changedTo = ParseStringToIntOrEnum(row[6].Trim());

    //        GetHintMatchStick(equationDataList[changedIndex], changedTo);

    //        difficultyType = _difficultyType;

    //        //int.TryParse(row[19], out int rewardHint);
    //        //rewardItemDic.Add(ItemType.Hint, rewardHint);

    //        //int.TryParse(row[20], out int rewardTicket);
    //        //rewardItemDic.Add(ItemType.TimeAttackTicket, rewardTicket);
    //    }
    //}

    //public List<string> SplitWith11(string input)
    //{
    //    List<string> result = new List<string>();
    //    int i = 0;

    //    while (i < input.Length)
    //    {
    //        // "11"을 하나로 묶어서 추가
    //        if (i + 1 < input.Length && input[i] == '1' && input[i + 1] == '1')
    //        {
    //            result.Add("11");
    //            i += 2;
    //        }
    //        else
    //        {
    //            result.Add(input[i].ToString());
    //            i++;
    //        }
    //    }

    //    return result;
    //}       

    public class TableStageData : BaseTableData
    {
        public Dictionary<DifficultyType, List<StageTableData>> StageTableDataDic { get; private set; } = new();


        public StageTableData GetTableData(DifficultyType _type, int _id)
        {
            return StageTableDataDic[_type].Where(x => x.id == _id).FirstOrDefault();
        }

        private List<StageTableData> MakeStage(DifficultyType _type, int _totalCount, int _bossStep, List<ItemInfo> _normalRewards, List<ItemInfo> _bossRewards)
        {
            List<StageTableData> stageTableList = new();

            int gateCount = 1;

            StageTableData MakeBossStage(int _i, int _index, bool _isMax)
            {
                StageTableData data = new();
                data.difficultyType = _type;
                data.id = _index;
                data.stage = _i + 1;
                data.stageType = StageType.Boss;
                data.randomStartID = _i - _bossStep + 1;
                data.randomEndID = _i;
                data.starMax = 3;
                data.isMax = _isMax;
                data.isLock = false;
                data.gate = gateCount;
                gateCount++;
                _bossRewards.ForEach(x => data.rewardItemDic.Add(x.itemType, x.count));
                return data;
            }

            StageTableData MakeNormalStage(int _i, int _index)
            {
                StageTableData data = new();
                data.difficultyType = _type;
                data.id = _index;
                data.stage = _i + 1;
                data.stageType = StageType.Normal;
                data.equtionDataID = _i;
                data.starMax = (int)_type + 1;
                data.isMax = false;
                _normalRewards.ForEach(x => data.rewardItemDic.Add(x.itemType, x.count));
                data.isLock = false;
                return data;
            }

            int index = 1;
            for (int i = 0; i < _totalCount; i++)
            {
                stageTableList.Add(MakeNormalStage(i, index));
                index++;

                if ((i + 1) % _bossStep == 0)
                {
                    stageTableList.Add(MakeBossStage(i, index, i == _totalCount - 1));
                    index++;
                }
            }

            return stageTableList;
        }

        public override void Load()
        {
            // 난이도별 보상 차등: Easy < Normal < Hard
            StageTableDataDic.Add(DifficultyType.Easy, MakeStage(DifficultyType.Easy, 500, ConfigData.BossStageStepDic[DifficultyType.Easy], new List<ItemInfo> { new(ItemType.Gold, 10) }, new List<ItemInfo> { new(ItemType.Gold, 15), new(ItemType.Hint, 1), new(ItemType.Change, 1) }));
            StageTableDataDic.Add(DifficultyType.Normal, MakeStage(DifficultyType.Normal, 1500, ConfigData.BossStageStepDic[DifficultyType.Normal], new List<ItemInfo> { new(ItemType.Gold, 15) }, new List<ItemInfo> { new(ItemType.Gold, 20), new(ItemType.Hint, 1), new(ItemType.Change, 1) }));
            StageTableDataDic.Add(DifficultyType.Hard, MakeStage(DifficultyType.Hard, 3000, ConfigData.BossStageStepDic[DifficultyType.Hard], new List<ItemInfo> { new(ItemType.Gold, 20) }, new List<ItemInfo> { new(ItemType.Gold, 30), new(ItemType.Hint, 2), new(ItemType.Change, 1) }));
        }
    }
}