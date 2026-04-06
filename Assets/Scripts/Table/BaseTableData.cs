using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class BaseTableData
{
    public BaseTableData()
    {
        Load();
    }

    public List<T> Parse<T>(EncryptedCSVData csvData, Func<string[], T> createFunc,
                                string tableName = "Unknown", bool dumpDecryptedToFile = true)
    {
        var list = new List<T>();
        if (csvData == null)
        {
            Debug.LogError($"[CSV] csvData NULL ({tableName})");
            return list;
        }

        string decrypted = csvData.GetDecryptedText() ?? string.Empty;

        // 기본 정보 로그
        if (Debug.isDebugBuild)
            Debug.Log($"[CSV] {tableName} decrypted len: {decrypted.Length}");

        // 비어있으면 바로 리턴 (여기서 많이 잡힙니다: 암호키/리소스 미포함/타이밍 문제)
        if (string.IsNullOrEmpty(decrypted))
        {
            Debug.LogWarning($"[CSV] {tableName} decrypted is EMPTY.");
            return list;
        }

        // BOM/개행/널 문자 정리
        if (decrypted.Length > 0 && decrypted[0] == '\uFEFF') decrypted = decrypted.Substring(1);
        decrypted = decrypted.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\0", "");

        // (선택) 디바이스에 원문 덤프해 실제 내용을 눈으로 확인
        if (dumpDecryptedToFile && Application.isMobilePlatform)
        {
            try
            {
                var dumpPath = System.IO.Path.Combine(Application.persistentDataPath, $"{tableName}_decrypted.csv");
                System.IO.File.WriteAllText(dumpPath, decrypted);
                if (Debug.isDebugBuild) Debug.Log($"[CSV] Dumped to: {dumpPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CSV] Dump failed: {e.Message}");
            }
        }

        int total = 0, used = 0, skippedEmpty = 0, skippedTooShort = 0, parseErrors = 0;

        using (var reader = new System.IO.StringReader(decrypted))
        {
            string line;
            // 헤더
            line = reader.ReadLine();
            if (line == null)
            {
                Debug.LogWarning($"[CSV] {tableName} has no lines.");
                return list;
            }

            if (Debug.isDebugBuild) Debug.Log($"[CSV] {tableName} header: {line}");

            while ((line = reader.ReadLine()) != null)
            {
                total++;
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    skippedEmpty++;
                    continue;
                }

                var fields = SplitCsvLineRespectingQuotes(trimmed);
                if (fields == null || fields.Length < 2)
                {
                    skippedTooShort++;
                    continue;
                }

                // 필드 트리밍
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i] != null)
                        fields[i] = fields[i].Trim().Trim('\uFEFF').Replace("\0", "");
                }

                try
                {
                    var item = createFunc(fields);
                    list.Add(item);
                    used++;
                }
                catch (Exception ex)
                {
                    parseErrors++;
                    if (parseErrors <= 5) // 과다 로그 방지
                        Debug.LogWarning($"[CSV] {tableName} parse error at line {total + 1}: {ex.Message}\n>> {trimmed}");
                }
            }
        }

        if (Debug.isDebugBuild)
            Debug.Log($"[CSV] {tableName} lines={total}, parsed={used}, empty={skippedEmpty}, short={skippedTooShort}, errors={parseErrors}");

        // 아주 흔한 케이스: 행은 많은데 전부 스킵됨 → 헤더/필드 수 불일치, 따옴표/콤마 이슈.
        if (used == 0)
            Debug.LogWarning($"[CSV] {tableName} parsed 0 items. Check header/quotes/commas and createFunc.");

        return list;
    }

    // 따옴표 보존 CSV 분할 (콤마는 따옴표 밖에서만 분리)
    private string[] SplitCsvLineRespectingQuotes(string line)
    {
        var result = new List<string>();
        var sb = new System.Text.StringBuilder();
        bool inDouble = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            // 큰따옴표만 인용부호 처리
            if (c == '"')
            {
                if (inDouble && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"'); // "" → "
                    i++;
                }
                else
                {
                    inDouble = !inDouble;
                }
                continue;
            }

            // 싱글따옴표 ' 는 그냥 문자로 추가
            // → 인용부호로 인식 안 함
            if (c == ',' && !inDouble)
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

        // 필드 클린업
        for (int i = 0; i < result.Count; i++)
        {
            var f = result[i];

            if (string.IsNullOrEmpty(f))
            {
                result[i] = f;
                continue;
            }

            // "..." 제거
            if (f.Length >= 2 && f[0] == '"' && f[^1] == '"')
                f = f.Substring(1, f.Length - 2);

            // 방정식에 남아있는 앞뒤 ' ... ' 제거 (있으면만 제거)
            if (f.Length >= 2 && f[0] == '\'' && f[^1] == '\'')
                f = f.Substring(1, f.Length - 2);

            result[i] = f;
        }

        return result.ToArray();
    }
    public abstract void Load();
}