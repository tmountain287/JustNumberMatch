using Common.Manager;
using Common.UI;
using JustOneMatch.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public enum GameModeType
{
    STAGE,
    BOSS_STAGE,
    TIME_ATTACK,
    SURVIVAL_MODE,
}

public class GameMgr : MonoSingleton<GameMgr>
{
    [SerializeField] private GameUI gameUI = null;
    [SerializeField] private Equation equation = null;

    [SerializeField] private GameSessionController stageSessionController = null;
    [SerializeField] private GameSessionController bossStageSessionController = null;
    [SerializeField] private GameSessionController timeAttackSessionController = null;
    [SerializeField] private GameSessionController infiniteModeSessionController = null;

    public GameUI GameUI { get => gameUI; }
    public Equation Equation { get => equation; }
    
    public EquationTableData CurrentTableData { get; set; }
    /// <summary>현재 플레이 중인 스테이지 테이블(실패 팝업 등에서 분석용)</summary>
    public StageTableData CurrentStageTableData => session?.StageTableData;

    public event Action OnStageCleared = null;

    private GameSessionController session;

    public IStageSequence BuildSingle(EquationTableData _data)
    {       
        return new SingleStageSequence(_data);
    }

    public IStageSequence BuildRangeFrom(DifficultyType diff, int startStage, int count)
    {
        var list = TableDataManager.Instance.TableEquationData.EquationTableDataDic[diff];
        int startIdx = Mathf.Max(0, list.FindIndex(x => x.stage == startStage));
        var slice = list.Skip(startIdx).Take(count);
        return new ListStageSequence(slice);
    }

    public IStageSequence BuildRandomRangeFrom(DifficultyType diff, int startStage, int endStage, int count)
    {
        var list = TableDataManager.Instance.TableEquationData.EquationTableDataDic[diff];
        var rangeList = list.GetRange(startStage, endStage - startStage + 1);

        if (rangeList.Count <= 0)
            return null;
        //    return new ListStageSequence(new List<EquationData>());
        
        var shuffled = rangeList.OrderBy(_ => UnityEngine.Random.value).ToList();
        
        var slice = shuffled.Take(count);

        return new ListStageSequence(slice);
    }

    public IStageSequence BuildRandomRangeFrom(
    DifficultyType diff,
    int startStage,
    int endStage,
    int count,
    IList<EquationTableData> excludeList) // <= 추가
    {
        var list = TableDataManager.Instance.TableEquationData.EquationTableDataDic[diff];

        // startStage, endStage 가 인덱스라고 가정
        var rangeList = list.GetRange(startStage, endStage - startStage + 1);

        if (excludeList != null && excludeList.Count > 0)
        {
            rangeList = rangeList
                .Where(x => !excludeList.Contains(x))
                .ToList();
        }

        if (rangeList.Count <= 0)
            return null;

        var shuffled = rangeList.OrderBy(_ => UnityEngine.Random.value).ToList();
        var slice = shuffled.Take(count).ToList();

        return new ListStageSequence(slice);
    }

    public IStageSequence BuildRandomRangeFrom(
   DifficultyType diff,  
   int count,
   IList<EquationTableData> excludeList) // <= 추가
    {
        var rangeList = TableDataManager.Instance.TableEquationData.EquationTableDataDic[diff];

        // startStage, endStage 가 인덱스라고 가정
      

        if (excludeList != null && excludeList.Count > 0)
        {
            rangeList = rangeList
                .Where(x => !excludeList.Contains(x))
                .ToList();
        }

        if (rangeList.Count <= 0)
            return null;

        var shuffled = rangeList.OrderBy(_ => UnityEngine.Random.value).ToList();
        var slice = shuffled.Take(count).ToList();

        return new ListStageSequence(slice);
    }


    public IStageSequence BuildRandomFrom(DifficultyType diff, int count)
    {
        var list = TableDataManager.Instance.TableEquationData.EquationTableDataDic[diff];

        if (list == null || list.Count == 0)
            return null;

        // 리스트 섞기
        var shuffled = list.OrderBy(_ => UnityEngine.Random.value).ToList();

        // count개만 추출 (list.Count보다 크면 최대치까지만)
        var slice = shuffled.Take(Mathf.Min(count, shuffled.Count));

        return new ListStageSequence(slice);
    }

    public IStageSequence BuildExplicitList(IEnumerable<(DifficultyType diff, int stage)> plan)
    {
        var datas = plan.Select(p => TableDataManager.Instance.TableEquationData.GetTableData(p.diff, p.stage));
        return new ListStageSequence(datas);
    }


    public void StartStageMode(StageTableData _data)
    {
        UserDataManager.PlayStage();
        StopAnySession();
        ObjectPoolManager.Instance.Clear();
        if (_data.stageType == StageType.Normal)
        {
            GameAnalyticsHelper.LogGameModeStart("stage", _data.difficultyType.ToString().ToLower(), _data.id);
            session = stageSessionController;

            Debug.Log(_data.equtionDataID);

            var data = TableDataManager.Instance.TableEquationData.GetTableData(_data.difficultyType, _data.equtionDataID);
            session.StageTableData = _data;            
            gameUI.GameTopUI.SetGameMode(GameModeType.STAGE);
            GameUI.SetStage(GameModeType.STAGE, _data.difficultyType);

            gameUI.StageTextUI.ShowStage(_data.stage, _data.stageType, ()=>
            {
                session.ReadySession(BuildSingle(data));
            });
            
        }
        else if(_data.stageType == StageType.Boss)
        {
            GameAnalyticsHelper.LogGameModeStart("boss_stage", _data.difficultyType.ToString().ToLower(), _data.id);
            session = bossStageSessionController;
            session.StageTableData = _data;

            session.ReadySession(BuildRandomRangeFrom(_data.difficultyType, _data.randomStartID, _data.randomEndID, 3));
            gameUI.GameTopUI.SetGameMode(GameModeType.BOSS_STAGE);
            GameUI.SetStage(GameModeType.BOSS_STAGE, _data.difficultyType);

            Action action = () =>
            {
                gameUI.StageTextUI.ShowStage(_data.gate, _data.stageType, () =>
                {
                    session.StartSession();
                });
            };

            if (UserDataManager.UserData.tutorialInfoDic[TutorialType.Boss])
            {
                action?.Invoke();
            }
            else
            {
                PopupManager.Instance.OpenPopup<TutorialPopup>().Initialize("Tutorial1", () =>
                {
                    UserDataManager.UserData.tutorialInfoDic[TutorialType.Boss] = true;
                    UserDataManager.Save();
                    action?.Invoke();
                });
            }
        }
        UIManager.Instance.ShowUI(BaseUI.Type.GAME);
    }

    public void StartInfiniteMode()
    {
        GameAnalyticsHelper.LogGameModeStart("survival");
        StopAnySession();
        session = infiniteModeSessionController;
        session.ReadySession(null);
        session.StartSession();
        gameUI.GameTopUI.SetGameMode(GameModeType.SURVIVAL_MODE);
        GameUI.SetStage(GameModeType.SURVIVAL_MODE, DifficultyType.None);
        UIManager.Instance.ShowUI(BaseUI.Type.GAME);

    }

    public void StartTimeAttack(DifficultyType diff)
    {
        GameAnalyticsHelper.LogGameModeStart("time_attack", diff.ToString().ToLower());
        StopAnySession();
        session = timeAttackSessionController;
        session.ReadySession(BuildRandomFrom(diff, 6));
        gameUI.GameTopUI.SetGameMode(GameModeType.TIME_ATTACK);
        GameUI.SetStage(GameModeType.TIME_ATTACK, diff);
        UIManager.Instance.ShowUI(BaseUI.Type.GAME);
    }

    public void StopAnySession() => session?.StopSession();
    
    public void ReStage()
    {
        ReStoreMatchStick();
        gameUI.GameTopUI.ReStage();
        SetEquation(CurrentTableData);
    }

    public void ChangeStage()
    {
       
    }

    public void SetEquation(EquationTableData _data)
    {
        ObjectPoolManager.Instance.Clear();
        CurrentTableData = _data;

        UIManager.Instance.ActivateForSeconds(1.7f);
        equation.SetStage(_data);

        GameUI.SetCharacter(ValidationResultType.NONE);
        //GameUI.GameTopUI.RefreshButtons();
        HideSlots();
    }

    public void ReStoreMatchStick()
    {
        ObjectPoolManager.Instance.MatchStickPoolList.ForEach(x => x.ForceCancelAndReturn());
    }
    

    public void ShowSlots() => equation.ShowSlots();
    public void HideSlots(Transform _firstSlot = null) => equation.HideSlots(_firstSlot);

    public void UseChangeItem(bool _isAd)
    {
        GameAnalyticsHelper.LogSkillUsed("change", session?.StageTableData?.id ?? -1);
        ReStoreMatchStick();
        session.ChangeSequence();
        
        if (!_isAd)
        {
            if (UserDataManager.GetItemCount(ItemType.Change) > 0)
            {
                UserDataManager.PlayDailyMission(MissionType.ItemUse);
                UserDataManager.SubItemCount(ItemType.Change, 1);
            }
            else
                UserDataManager.SubItemCount(ItemType.Gold, TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == ItemType.Change && x.value == 1).FirstOrDefault().needValue);

            UserDataManager.Save();
        }
    }

    public void UseHintItem(bool _isAd)
    {
        GameAnalyticsHelper.LogHintUsed(session?.StageTableData?.id ?? -1);
        equation.OnHint();
        if (!_isAd)
        {
            if (UserDataManager.GetItemCount(ItemType.Hint) > 0)
            {
                UserDataManager.PlayDailyMission(MissionType.ItemUse);
                UserDataManager.SubItemCount(ItemType.Hint, 1);
            }
            else
                UserDataManager.SubItemCount(ItemType.Gold, TableDataManager.Instance.TableShopData.ShopDataList.Where(x => x.itemType == ItemType.Hint && x.value == 1).FirstOrDefault().needValue);
                
            UserDataManager.Save();
        }
    }
   
    public void SetMatchLock(MatchStick _matchStick)
    {
        ObjectPoolManager.Instance.MatchStickPoolList.ForEach(x => x.SetBlockRaycasts(_matchStick == x));
    }

    public void AllMatchUnlock(bool _flag)
    {
        ObjectPoolManager.Instance.MatchStickPoolList.ForEach(x => x.SetBlockRaycasts(_flag));
    }

    public void OnAddOperation()
    {
        if (CurrentTableData.difficultyType == DifficultyType.Easy || CurrentTableData.difficultyType == DifficultyType.Normal)
            return;

        List<BaseRecognizer> digitRecognizerList = Equation.UnitRecognizerList.Where(x => x.RecognizerType == RecognizerType.Digit).ToList();

        digitRecognizerList.ForEach(x =>
        {
            int index = x.transform.GetSiblingIndex();
            AddOperatorRecognizer addOprationRecognizer = ObjectPoolManager.Instance.GetAddOerationRecognizer();

            addOprationRecognizer.transform.SetParent(equation.transform);
            addOprationRecognizer.transform.SetSiblingIndex(index);
            addOprationRecognizer.transform.localScale = Vector3.one;

            addOprationRecognizer.GetComponent<RectTransform>().sizeDelta = new(-equation.Spacing, 0);

            addOprationRecognizer.SetValue(OperatorType.None);
            addOprationRecognizer.ShowSlots();

            Equation.UnitRecognizerList.Insert(index, addOprationRecognizer);
        });
    }

    public void ClearAddOpearation()
    {
        if (CurrentTableData.difficultyType == DifficultyType.Easy || CurrentTableData.difficultyType == DifficultyType.Normal)
            return;

        ObjectPoolManager.Instance.ClearAddOpearation().ForEach(x=>
        {
            Equation.UnitRecognizerList.Remove(x);
        });
    }

    public void SortEqution()
    {
        Equation.UnitRecognizerList.ForEach(x =>
        {
            if(x is AddOperatorRecognizer)
            {

            }
        });
    }

    public void ValidateEquation()
    {
        var result = equation.ValidateEquation();

        GameAnalyticsHelper.LogEquationSubmit(result.Item2.ToString(), result.Item2 == ValidationResultType.OK);
        GameUI.SetCharacter(result.Item2, result.Item1);

        if (result.Item2 == ValidationResultType.OK)
        {           
            ObjectPoolManager.Instance.MatchStickPoolList.ForEach(x => x.Clear());
            Equation.ChangeAddOperation();
            HideSlots();
            if (PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.VIBRATION, 0) > 0)
                Handheld.Vibrate();
            OnStageCleared?.Invoke();
        }
    }
















    // ─────────────────────────────────────────────────────────────
    // 프리뷰 오퍼레이터 스폰/파괴 (토글 스팸 방지: 같은 대상이면 무시)
    //public void SpawnOperator(BaseRecognizer unit)
    //{
    //    if (currentOverlappingUnit == unit) return; // 같은 대상이면 중복 스폰 금지

    //    // Debug.Log("Spawn");
    //    DestroyOperator(); // 이전 프리뷰 정리

    //    currentOverlappingUnit = unit;

    //    currentOperatorRecognizer = GetoOerationRecognizer();

    //    currentOperatorRecognizer.transform.SetParent(equation.transform);
    //    currentOperatorRecognizer.transform.SetSiblingIndex(currentOverlappingUnit.transform.GetSiblingIndex());
    //    currentOperatorRecognizer.SpawnType = SpawnType.Spawn;
    //    currentOperatorRecognizer.SetValue(OperatorType.None);       
    //    currentOperatorRecognizer.ShowSlots();

    //    // ⚠️ 드래그 중 빈번한 리빌드는 깜빡임 유발 → 최소화
    //    // 필요시 드래그 종료 시점(확정)에서 한 번 호출하는 것을 권장
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(equation.GetComponent<RectTransform>());

    //    currentOperatorRecognizer.onRemoveNone = () =>
    //    {
    //        operationRecognizerPool.ReturnObjectToPool(currentOperatorRecognizer.gameObject);
    //        operationRecognizerPoolList.Remove(currentOperatorRecognizer);
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(equation.GetComponent<RectTransform>());
    //        currentOverlappingUnit = null;
    //        currentOperatorRecognizer.onRemoveNone = null;
    //        equation.UnitRecognizerList.Remove(currentOperatorRecognizer);
    //        currentOperatorRecognizer = null;
    //    };

    //    equation.UnitRecognizerList.Add(currentOperatorRecognizer);
    //}

    //public void DestroyOperator()
    //{
    //    if (currentOverlappingUnit == null || currentOperatorRecognizer == null) return;

    //    // Debug.Log("Destroy");

    //    var m = currentOperatorRecognizer.GetComponentsInChildren<MatchStick>().ToList();

    //    if (m.Count == 0)
    //    {
    //        operationRecognizerPool.ReturnObjectToPool(currentOperatorRecognizer.gameObject);
    //        operationRecognizerPoolList.Remove(currentOperatorRecognizer);
    //        LayoutRebuilder.ForceRebuildLayoutImmediate(equation.GetComponent<RectTransform>());
    //        currentOperatorRecognizer.onRemoveNone = null;
    //        equation.UnitRecognizerList.Remove(currentOperatorRecognizer);
    //        currentOperatorRecognizer = null;
    //    }
    //    else
    //    {
    //        // 실제 스틱이 들어가 있으면 비활성화하지 않고 대상만 해제
    //        currentOverlappingUnit = null;
    //    }
    //}


}