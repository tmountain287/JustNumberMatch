using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

public enum EquationPosType
{
    None,
    Left_1,
    Left_2,
    Operation_1,
    Right_1,
    Right_2,
    Operation_2,
    Result_1,
    Result_2,
}

public class EquationData
{
    public RecognizerType RecognizerType { get; set; }
    public object Value { get; set; }

    public EquationData(RecognizerType recognizerType,  object value)
    {  RecognizerType = recognizerType; Value = value; }
}

[Serializable]
public class EquationTableData
{
    public int stage;
    public string start_Equation;
    public string target_Equation;

    public int changedIndex;
    public EquationData changedTo;

    public List<EquationData> equationDataList = new();

    public List<EquationData> targetEquationDataList = new();

    public Dictionary<EquationPosType, EquationData> startNumberDataDic = new();
    public Dictionary<EquationPosType, EquationData> targetNumberDataDic = new();
    public Dictionary<ItemType, int> rewardItemDic = new();

    public DifficultyType  difficultyType;

    public EquationTableData(string[] row, DifficultyType _difficultyType )
    {        
        if (row.Length >= 17)
        {
            int.TryParse(row[0], out stage);
            start_Equation = row[1].Trim().Replace("'", "").Replace(" ", "");
            target_Equation = row[2].Trim().Replace("'", "").Replace(" ", ""); ;

            //List<string> strings = SplitWith11(start_Equation);

            List<string> strings = row[16].Trim().Replace("'","").Split(";").ToList();

            strings.ForEach(x =>
            {
                equationDataList.Add(ParseStringToIntOrEnum(x));
            });

            //row[17].Trim().Replace("'", "").Split(";").ToList().ForEach(x =>
            //{
            //    targetEquationDataList.Add(ParseStringToIntOrEnum(x));
            //});

            //List<RecognizerInfo> tokens = new();

            //targetEquationDataList.ForEach(x =>
            //{
            //    tokens.Add(new(x.RecognizerType, x.Value));
            //});

            //var ok = EquationValidator.TryValidateTokens(tokens, out var res1);

            //if (res1.Success && res1.AreAllEqual)
            //{
            //    Debug.Log("성궁");
            //}
            //else
            //{
            //    Debug.LogError(res1.ToString());
            //}

            int.TryParse(row[4], out changedIndex);
            changedTo = ParseStringToIntOrEnum(row[6].Trim());

            GetHintMatchStick(equationDataList[changedIndex], changedTo);

            difficultyType = _difficultyType;

            //int.TryParse(row[19], out int rewardHint);
            //rewardItemDic.Add(ItemType.Hint, rewardHint);

            //int.TryParse(row[20], out int rewardTicket);
            //rewardItemDic.Add(ItemType.TimeAttackTicket, rewardTicket);
        }
    }

    public List<string> SplitWith11(string input)
    {
        List<string> result = new List<string>();
        int i = 0;

        while (i < input.Length)
        {
            // "11"을 하나로 묶어서 추가
            if (i + 1 < input.Length && input[i] == '1' && input[i + 1] == '1')
            {
                result.Add("11");
                i += 2;
            }
            else
            {
                result.Add(input[i].ToString());
                i++;
            }
        }

        return result;
    }

    public EquationData ParseStringToIntOrEnum(string input)
    {
        if (int.TryParse(input, out int number))
            return new(RecognizerType.Digit, number);

        if (input == "-")
            return new(RecognizerType.Operator, OperatorType.Minus);
        else if (input == "+")
            return new(RecognizerType.Operator, OperatorType.Plus);
        else if (input == "=")
            return new(RecognizerType.Operator, OperatorType.Equals);
        else if(input =="None")
            return new(RecognizerType.None, null);        
        
        return new(RecognizerType.None, null);
    }

    public void GetHintMatchStick(EquationData start, EquationData target)
    {
        if (target.Value == null)
        {
            target.Value = OperatorType.None;
        }
        var startValue = start.Value;
        var targetValue = target.Value;

        bool[] from;
        bool[] to;

        var SegmentTableList = SegmentTable.NumberSegmentTableList[start.RecognizerType];

        if (startValue is int intValue)
        {
            from = SegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)startValue).Flags;
            to = SegmentTableList.FirstOrDefault(kv => (int)kv.Value == (int)targetValue).Flags;
        }
        else
        {
            from = SegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)startValue).Flags;
            to = SegmentTableList.FirstOrDefault(kv => (OperatorType)kv.Value == (OperatorType)targetValue).Flags;
        }

        // true -> false 로 변한 인덱스들 찾기
        var changedIndices = from
            .Select((value, index) => new { value, index })
            .Where(x => x.value == true && to[x.index] == false)
            .Select(x => x.index)
            .ToList();

        //if (changedIndices.Count > 1 && changedIndices.Count == 0)
        //{
        //    Debug.LogError("문제 풀이가 잘못되었습니다.");
        //    return null;
        //}
        if (changedIndices.Count != 1)
        {
            Debug.Log(startValue);
            Debug.Log(targetValue);
            Debug.LogError($"{stage} 수식 잘못됨");
        }
    }
}

public enum DifficultyType
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
    None = 3,
}

public class TableEquationData : BaseTableData
{   
    public Dictionary<DifficultyType, List<EquationTableData>> EquationTableDataDic { get; private set; } = new();


    public EquationTableData GetTableData(DifficultyType _type, int _index)
    {
        return EquationTableDataDic[_type][_index];
    }

    public void MakeEquationData(string userId)
    {
        EquationTableDataDic.Clear();

        List<EquationTableData> basicEquationTableDataList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_equationtablebasic"), row => new EquationTableData(row, DifficultyType.Easy));

        List<EquationTableData> easyList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_equationtableeasy"), row => new EquationTableData(row, DifficultyType.Easy));
        List<EquationTableData> normalList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_equationtablenormal"), row => new EquationTableData(row, DifficultyType.Normal));
        List<EquationTableData> hardList = Parse(Resources.Load<EncryptedCSVData>("EncryptedCSVs/Encrypted_equationtablehard"), row => new EquationTableData(row, DifficultyType.Hard));

        EquationTableDataDic.Add(DifficultyType.Easy, easyList);
        EquationTableDataDic.Add(DifficultyType.Normal, normalList);
        EquationTableDataDic.Add(DifficultyType.Hard, hardList);

        ShuffleForUserInPlace(userId, DifficultyType.Easy);
        ShuffleForUserInPlace(userId, DifficultyType.Normal);
        ShuffleForUserInPlace(userId, DifficultyType.Hard);

        EquationTableDataDic[DifficultyType.Easy].InsertRange(0, basicEquationTableDataList);
    }

    public override void Load()
    {
        

       
    }

    public void ShuffleForUserInPlace(string userId, DifficultyType difficultyType, string salt = "LevelOrderSaltV1")
    {
        if (EquationTableDataDic[difficultyType] == null || EquationTableDataDic[difficultyType].Count <= 1) return;
        var rng = new SplitMix64(StableSeed.Seed64(userId, salt));
        ShuffleUtil.ShuffleInPlace(EquationTableDataDic[difficultyType], rng);
    }

    // 2) 유저별 고정 순서로 "복사본을 반환" (원본 유지)
    public List<EquationTableData> GetUserShuffledCopy(string userId, DifficultyType difficultyType, string salt = "LevelOrderSaltV1")
    {
        if (EquationTableDataDic[difficultyType] == null || EquationTableDataDic[difficultyType].Count <= 1)
            return null;

        var rng = new SplitMix64(StableSeed.Seed64(userId, salt));
        return ShuffleUtil.ShuffledCopy(EquationTableDataDic[difficultyType], rng);
    }

    public EquationTableData GetRandomEquation(DifficultyType difficulty)
    {
        return GetRandomEquation(difficulty, null);
    }

    /// <summary>
    /// 최근 N개 문제를 제외하고 랜덤 선택 (중복 출제 방지)
    /// </summary>
    public EquationTableData GetRandomEquation(DifficultyType difficulty, IList<EquationTableData> excludeRecent)
    {
        var table = EquationTableDataDic[difficulty];
        if (table == null || table.Count == 0) return null;

        List<EquationTableData> available = table;
        if (excludeRecent != null && excludeRecent.Count > 0)
        {
            var excludeSet = new HashSet<EquationTableData>(excludeRecent);
            available = table.Where(x => !excludeSet.Contains(x)).ToList();
        }

        if (available.Count == 0)
            available = table;

        int idx = UnityEngine.Random.Range(0, available.Count);
        return available[idx];
    }

    //public List<EquationTableData> GetRandomDataList(DifficultyType _type, int _count)
    //{
    //    return GetRandomItems(EquationTableDataDic[_type], _count);
    //}

    //public int GetMaxStage(DifficultyType _type)
    //{
    //    return EquationTableDataDic[_type].Max(x => x.stage);
    //}

    //public List<T> GetRandomItems<T>(List<T> source, int count)
    //{
    //    List<T> temp = new List<T>(source);
    //    System.Random rng = new();

    //    for (int i = 0; i < count; i++)
    //    {
    //        int swapIndex = rng.Next(i, temp.Count);
    //        (temp[i], temp[swapIndex]) = (temp[swapIndex], temp[i]);
    //    }

    //    return temp.Take(count).ToList();
    //}

    //public static T GetRandomExcept<T>(List<T> listA, List<T> listB)
    //{
    //    if (listA == null || listA.Count == 0)
    //        return default;

    //    System.Random rng = new();
    //    // listB에 없는 요소만 필터링
    //    var available = listA.Where(item => !listB.Contains(item)).ToList();

    //    if (available.Count == 0)
    //        return default; // 선택할 게 없으면 기본값 반환

    //    int index = rng.Next(available.Count);
    //    return available[index];
    //}
}