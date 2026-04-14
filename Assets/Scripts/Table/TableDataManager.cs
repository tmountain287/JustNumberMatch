using Common.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableDataManager : MonoSingletonDont<TableDataManager>
{
    public TableLocalizationData TableLocalizationData { get; private set; }
    public TableSlangData TableSlangData { get; private set; }
    public TableProductCatalogData TableProductCatalogData { get; private set; }
    public TableShopData TableShopData { get; private set; }
    public TableGPGAchievementData TableGPGAchievementData { get; private set; }
    public TableLevelData TableLevelData { get; private set; }
    public TableCharacterData TableCharacterData { get; private set; }
    public TableMissionData TableMissionData { get; private set; }

    private bool init = false;

    public void LoadAllCSVs()
    {
        if (init) return;

        TableLocalizationData = new();       
        TableShopData = new();
        TableProductCatalogData = new();
        TableGPGAchievementData = new();
        TableLevelData = new();
        TableCharacterData = new();
        TableMissionData = new();
        TableSlangData = new();
        init = true;
    }

    

    public static List<T> Parse<T>(EncryptedCSVData csvData, Func<string[], T> createFunc)
    {
        var list = new List<T>();

        // 1) 복호화
        string decrypted = csvData.GetDecryptedText() ?? string.Empty;

        // 2) BOM 제거 + 개행 정규화
        if (decrypted.Length > 0 && decrypted[0] == '\uFEFF') // BOM
            decrypted = decrypted.Substring(1);
        // \r\n, \r 혼재 → \n 하나로 통일
        decrypted = decrypted.Replace("\r\n", "\n").Replace("\r", "\n");

        // 3) 라인 단위 스트리밍 파싱(메모리 스파이크 방지)
        using (var reader = new System.IO.StringReader(decrypted))
        {
            string? line;
            int lineIndex = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineIndex++;
                if (lineIndex == 1) continue;                 // 헤더 스킵
                if (string.IsNullOrWhiteSpace(line)) continue; // 빈 줄 스킵

                var fields = SplitCsvLineRespectingQuotes(line);

                // 필요 최소 컬럼 체크 (원 코드 유지)
                if (fields == null || fields.Length < 2) continue;

                // 양 끝 공백/제어문자 정리
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] is null) continue;
                    fields[i] = fields[i].Trim()
                                         .Trim('\uFEFF')       // 드물게 필드 앞에 붙는 BOM
                                         .Replace("\u0000", ""); // 널문자 제거
                }

                list.Add(createFunc(fields));
            }
        }

        return list;
    }

    /// <summary>
    /// CSV 한 줄을 따옴표(쌍따옴표 " 와 단따옴표 ')를 고려해 안전하게 분할합니다.
    /// - 콤마는 따옴표 밖에서만 분리
    /// - "문장 내 , 콤마" / '문장 내 , 콤마' 지원
    /// - "" → " / '' → ' 언이스케이프
    /// </summary>
    private static string[] SplitCsvLineRespectingQuotes(string line)
    {
        var result = new List<string>();
        var sb = new System.Text.StringBuilder();

        bool inDouble = false;
        bool inSingle = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"' && !inSingle)
            {
                if (inDouble && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // 이스케이프된 쌍따옴표 ("")
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inDouble = !inDouble;
                }
                continue;
            }

            if (c == '\'' && !inDouble)
            {
                if (inSingle && i + 1 < line.Length && line[i + 1] == '\'')
                {
                    // 이스케이프된 단따옴표 ('')
                    sb.Append('\'');
                    i++;
                }
                else
                {
                    inSingle = !inSingle;
                }
                continue;
            }

            if (c == ',' && !inDouble && !inSingle)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());

        // 양 끝 따옴표 제거
        for (int i = 0; i < result.Count; i++)
        {
            var f = result[i];
            if (string.IsNullOrEmpty(f)) continue;

            if ((f.Length >= 2 && f[0] == '"' && f[^1] == '"') ||
                (f.Length >= 2 && f[0] == '\'' && f[^1] == '\''))
            {
                result[i] = f.Substring(1, f.Length - 2);
            }
        }

        return result.ToArray();
    }
}