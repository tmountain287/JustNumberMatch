using System;
using UnityEngine;
using System.Linq;

namespace Common.Manager
{
    public enum LocalType
    {
        EN = 0,
        KO = 1,
        JP = 2,
        CH = 3,
        CHT = 4,
        FR = 5,
        SP = 6,
        IT = 7,
        DE = 8,
        PT = 9,
        RU = 10,
        ID = 11,        
        VI = 12,
        TH = 13,
        TR = 14,
    }

    public enum LocalUIType
    {
        Normal = 0,
        Mission = 1,
        Push = 2,
    }

    public class LocalizationManager : MonoSingletonDont<LocalizationManager>
    {
        private int currentLanguageIndex = -1;

        public LocalType CurrentLocalType
        {
            get
            {
                if(currentLanguageIndex == -1)
                {
                    currentLanguageIndex = (int)GetCurrentLanguage();
                    PlayerPrefsManager.Instance.SetPlayerPrefsInfo(PrefsKey.Language, currentLanguageIndex);
                }
                return (LocalType)PlayerPrefsManager.Instance.GetPlayerPrefsValue(PrefsKey.Language, currentLanguageIndex);
            }
        }

        public string GetText(string _entryKey, LocalUIType _type = LocalUIType.Normal)
        {            
            return TableDataManager.Instance.TableLocalizationData.GetLocalString(_type, _entryKey, CurrentLocalType);
        }

        public LocalType GetCurrentLanguage()
        {
            SystemLanguage currentLanguage = Application.systemLanguage;
            Debug.Log("현재 시스템 언어: " + currentLanguage);

            // 예시: 언어별 분기 처리
            switch (currentLanguage)
            {
                case SystemLanguage.Korean:
                    return LocalType.KO; // KO
                case SystemLanguage.Japanese:
                    return LocalType.JP; // JP
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return LocalType.CH;
                case SystemLanguage.ChineseTraditional:
                    return LocalType.CHT;    // zh-TW
                case SystemLanguage.French:
                    return LocalType.FR;
                case SystemLanguage.Spanish:
                    return LocalType.SP;
                case SystemLanguage.Italian:
                    return LocalType.IT;
                case SystemLanguage.German:
                    return LocalType.DE;
                case SystemLanguage.Portuguese:
                    return LocalType.PT;
                case SystemLanguage.Russian:
                    return LocalType.RU;
                case SystemLanguage.Indonesian:
                    return LocalType.ID;
                case SystemLanguage.Vietnamese:
                    return LocalType.VI;
                case SystemLanguage.Thai:
                    return LocalType.TH;
                case SystemLanguage.Turkish:
                    return LocalType.TR;
                default:
                    return LocalType.EN;
            }
        }

        //public string GetErrorString(string _entryKey)
        //{
        //    LocalizationData data = localizationErrorScriptable.localizationDataList.Where(x => x.EntryKey == _entryKey).FirstOrDefault();
        //    if (data != null)
        //    {
        //        return data.Values[(int)CurrentLocalType];
        //    }
        //    return null;
        //}

        //public string LocalCurrencyNumber<T>(T _value, int _count, bool _useDot = false) where T : struct, IConvertible
        //{
        //    return CurrentLocalType == LocalType.KO ? _value.FormatKoreanUnits(_useDot) :
        //            _value.FormatKMBT(_count, _useDot);
        //}
    }
}