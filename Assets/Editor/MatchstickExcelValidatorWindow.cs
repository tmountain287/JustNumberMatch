// Assets/Editor/MatchstickExcelValidatorWindow.cs
//#define ENABLE_EXCEL_READER

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MatchstickExcelValidatorWindow : EditorWindow
{
    [MenuItem("Tools/Matchstick/Validate Excel/CSV")]
    public static void Open()
    {
        var win = GetWindow<MatchstickExcelValidatorWindow>("Matchstick Validator");
        win.minSize = new Vector2(520, 360);
        win.Show();
    }

    [SerializeField] private string filePath = "";
    [SerializeField] private string sheetName = "";            // xlsx 전용
    [SerializeField] private string startHeader = "Start_Equation";
    [SerializeField] private string targetHeader = "Target_Equation";
    [SerializeField] private bool fileHasHeader = true;

    [Header("Token Column Headers (옵션)")]
    [SerializeField] private string startTokensHeader = "Start_Tokens";
    [SerializeField] private string targetTokensHeader = "Target_Tokens";
    [SerializeField] private bool validateTokensIfColumnsExist = true;

    [Header("Parse Options")]
    [SerializeField] private bool removeSpaces = true;
    [SerializeField] private bool trimSingleQuotes = true;     // '9+1=0' → 9+1=0
    [SerializeField] private bool trySemicolonTokens = true;   // 5;2;+;2;8;=;8 → 52+28=8
    [SerializeField] private char csvDelimiter = ',';

    [Header("Rule Options")]
    [SerializeField] private bool allowSplitMerge11_4 = true;

    private Vector2 scroll;

    private void OnGUI()
    {
        using var sv = new EditorGUILayout.ScrollViewScope(scroll);
        scroll = sv.scrollPosition;

        EditorGUILayout.LabelField("Excel/CSV Matchstick Validator", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            filePath = EditorGUILayout.TextField("File Path", filePath);
            if (GUILayout.Button("Select...", GUILayout.Width(90)))
            {
                var fp = EditorUtility.OpenFilePanel("Select Excel/CSV", Application.dataPath, "xlsx,csv");
                if (!string.IsNullOrEmpty(fp)) filePath = fp;
            }
        }

        if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            sheetName = EditorGUILayout.TextField(new GUIContent("Sheet Name (xlsx)", "비우면 첫 시트"), sheetName);
#if !ENABLE_EXCEL_READER
            EditorGUILayout.HelpBox("ExcelDataReader 비활성화 상태입니다. .xlsx를 읽으려면 상단 define 주석 해제 + 패키지 설치.", MessageType.Info);
#endif
        }
        else
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField("Sheet Name (xlsx)", "(CSV에서는 미사용)");
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Column Headers", EditorStyles.boldLabel);
        startHeader = EditorGUILayout.TextField("Start Header", startHeader);
        targetHeader = EditorGUILayout.TextField("Target Header", targetHeader);
        fileHasHeader = EditorGUILayout.Toggle("File Has Header", fileHasHeader);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Token Headers (optional)", EditorStyles.boldLabel);
        startTokensHeader = EditorGUILayout.TextField(new GUIContent("Start Tokens Header", "예: Start_Tokens"), startTokensHeader);
        targetTokensHeader = EditorGUILayout.TextField(new GUIContent("Target Tokens Header", "예: Target_Tokens"), targetTokensHeader);
        validateTokensIfColumnsExist = EditorGUILayout.Toggle(new GUIContent("Validate Tokens (if columns exist)", "토큰 컬럼이 있으면 스마트 토큰 규칙으로 검사"), validateTokensIfColumnsExist);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Parse Options", EditorStyles.boldLabel);
        removeSpaces = EditorGUILayout.Toggle("Remove Spaces", removeSpaces);
        trimSingleQuotes = EditorGUILayout.Toggle("Trim Single Quotes", trimSingleQuotes);
        trySemicolonTokens = EditorGUILayout.Toggle("Join Semicolon Tokens", trySemicolonTokens);
        csvDelimiter = EditorGUILayout.DelayedTextField("CSV Delimiter", csvDelimiter.ToString()).FirstOrDefault();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Rule Options", EditorStyles.boldLabel);
        allowSplitMerge11_4 = EditorGUILayout.Toggle(new GUIContent("Allow 11↔4 (counts as one move)", "특수치환 자체를 1회 이동으로 인정"), allowSplitMerge11_4);

        EditorGUILayout.Space(10);
        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(filePath)))
        {
            if (GUILayout.Button("Validate", GUILayout.Height(36)))
            {
                try { ValidateNow(); }
                catch (Exception ex) { Debug.LogError($"[MatchstickValidator] Exception: {ex}"); }
            }
        }
    }

    private void ValidateNow()
    {
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("Error", "파일이 존재하지 않습니다.", "OK");
            return;
        }

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        List<Dictionary<string, string>> rows;

        if (ext == ".csv")
            rows = LoadCsv(filePath, csvDelimiter, fileHasHeader);
        else if (ext == ".xlsx")
        {
#if ENABLE_EXCEL_READER
            rows = LoadXlsx(filePath, sheetName, fileHasHeader);
#else
            EditorUtility.DisplayDialog("Excel Reader Disabled",
                ".xlsx는 현재 비활성화. CSV를 사용하거나 ExcelDataReader 설치 후 define 활성화하세요.", "OK");
            return;
#endif
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "지원 확장자: .csv, .xlsx", "OK");
            return;
        }

        if (rows == null || rows.Count == 0)
        {
            EditorUtility.DisplayDialog("No Data", "데이터 행이 없습니다.", "OK");
            return;
        }

        // 헤더 존재 확인
        var headerSet = new HashSet<string>(rows[0].Keys, StringComparer.OrdinalIgnoreCase);
        if (!headerSet.Contains(startHeader) || !headerSet.Contains(targetHeader))
        {
            EditorUtility.DisplayDialog("Header Missing",
                $"필요 헤더: '{startHeader}', '{targetHeader}'\n실제: {string.Join(", ", headerSet)}", "OK");
            return;
        }

        bool hasStartTokens = !string.IsNullOrEmpty(startTokensHeader) && headerSet.Contains(startTokensHeader);
        bool hasTargetTokens = !string.IsNullOrEmpty(targetTokensHeader) && headerSet.Contains(targetTokensHeader);
        bool canValidateTokens = validateTokensIfColumnsExist && hasStartTokens && hasTargetTokens;

        if (validateTokensIfColumnsExist && !canValidateTokens)
        {
            Debug.Log("[MatchstickValidator] Token 검증 옵션은 켜져 있지만, 토큰 헤더가 없어서 토큰 검사는 건너뜁니다.");
        }

        var results = new List<ValidationResult>();
        int pass = 0, fail = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            string start = GetValue(row, startHeader);
            string target = GetValue(row, targetHeader);
            string ns = NormalizeEquation(start);
            string nt = NormalizeEquation(target);

            // 1) 스마트 토큰 생성 (17 ↔ 11 자리수 비교 규칙 들어 있는 부분)
            BuildSmartTokens(ns, nt, out var smartStartTokens, out var smartTargetTokens);
            string smartStartTokenStr = string.Join(";", smartStartTokens);
            string smartTargetTokenStr = string.Join(";", smartTargetTokens);

            bool tokensOk = true;
            string tokenReason = "";

            if (canValidateTokens)
            {
                var csvStartTokens = ParseTokenColumn(GetValue(row, startTokensHeader));
                var csvTargetTokens = ParseTokenColumn(GetValue(row, targetTokensHeader));

                string csvStartTokenStr = string.Join(";", csvStartTokens);
                string csvTargetTokenStr = string.Join(";", csvTargetTokens);

                bool sameStart = csvStartTokens.SequenceEqual(smartStartTokens);
                bool sameTarget = csvTargetTokens.SequenceEqual(smartTargetTokens);

                if (!sameStart || !sameTarget)
                {
                    tokensOk = false;
                    tokenReason =
                        $"Token mismatch. " +
                        $"Start expected: {smartStartTokenStr}, got: {csvStartTokenStr} | " +
                        $"Target expected: {smartTargetTokenStr}, got: {csvTargetTokenStr}";
                }
            }

            // 2) 성냥개비 1회 이동 규칙 검사
            bool moveOk = MatchstickRuleValidator2.IsOneMoveTransform(ns, nt, allowSplitMerge11_4, out var moveReason);

            if (moveOk && tokensOk)
            {
                pass++;
            }
            else
            {
                fail++;
                string reason;
                if (!moveOk && tokensOk)
                {
                    // 순수하게 성냥 규칙만 실패
                    reason = moveReason;
                }
                else if (moveOk && !tokensOk)
                {
                    // 이동 규칙은 맞는데 토큰이 잘못된 경우 (너가 말한 17→11인데 11을 한 토큰으로 둔 케이스)
                    reason = "Move rule OK, but " + tokenReason;
                }
                else
                {
                    // 둘 다 실패
                    reason = tokenReason + " | MoveRule: " + moveReason;
                }

                results.Add(new ValidationResult(i, false, reason, start, target, ns, nt));
            }
        }

        if (results.Count > 0)
        {
            // 리포트 저장 (FAIL만)
            string dir = "Assets/ValidationReports";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string reportPath = Path.Combine(dir, $"matchstick_report_{stamp}.csv");
            WriteReport(reportPath, results);
            AssetDatabase.Refresh();

            Debug.Log($"[MatchstickValidator] Done. PASS={pass}, FAIL={fail}, TotalRows={rows.Count}, FailRows={results.Count}\nReport: {reportPath}");
            EditorUtility.DisplayDialog("Validation Complete", $"PASS={pass}, FAIL={fail}\nReport saved:\n{reportPath}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Complete", $"PASS={pass}, FAIL={fail}", "OK");
        }
    }

    // -------- Helpers --------

    private string NormalizeEquation(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Trim();
        if (trimSingleQuotes && s.Length >= 2 && s[0] == '\'' && s[^1] == '\'')
            s = s.Substring(1, s.Length - 2);
        if (removeSpaces)
            s = new string(s.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        if (trySemicolonTokens && s.Contains(';') && !s.Contains(','))
        {
            var tokens = s.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(t => t.Trim('\'', ' '));
            s = string.Concat(tokens);
        }
        return s;
    }

    private static string GetValue(Dictionary<string, string> row, string key)
    {
        foreach (var kv in row)
            if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
                return kv.Value ?? "";
        return "";
    }

    private List<Dictionary<string, string>> LoadCsv(string path, char delimiter, bool hasHeader)
    {
        var lines = File.ReadAllLines(path);
        var rows = new List<Dictionary<string, string>>();
        if (lines.Length == 0) return rows;

        string[] header;
        int startIdx = 0;

        if (hasHeader)
        {
            header = SplitCsvLine(lines[0], delimiter);
            startIdx = 1;
        }
        else
        {
            int colCount = SplitCsvLine(lines[0], delimiter).Length;
            header = Enumerable.Range(0, colCount).Select(i => $"Col{i}").ToArray();
        }

        for (int i = startIdx; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cells = SplitCsvLine(lines[i], delimiter);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < header.Length; c++)
            {
                string key = header[c];
                string val = c < cells.Length ? cells[c] : "";
                dict[key] = val;
            }
            rows.Add(dict);
        }
        return rows;
    }

    private string[] SplitCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var cur = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    cur.Append('\"'); i++;
                }
                else inQuotes = !inQuotes;
            }
            else if (ch == delimiter && !inQuotes)
            {
                result.Add(cur.ToString()); cur.Length = 0;
            }
            else cur.Append(ch);
        }
        result.Add(cur.ToString());
        return result.ToArray();
    }

#if ENABLE_EXCEL_READER
    private List<Dictionary<string, string>> LoadXlsx(string path, string sheet, bool hasHeader)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var rows = new List<Dictionary<string, string>>();

        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
        var conf = new ExcelDataReader.ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataReader.ExcelDataTableConfiguration { UseHeaderRow = hasHeader }
        };
        var dataSet = reader.AsDataSet(conf);
        DataTable table = null;

        if (!string.IsNullOrEmpty(sheet))
            table = dataSet.Tables.Cast<DataTable>().FirstOrDefault(t => string.Equals(t.TableName, sheet, StringComparison.OrdinalIgnoreCase)) ?? dataSet.Tables[0];
        else
            table = dataSet.Tables[0];

        var headers = new List<string>();
        if (hasHeader) foreach (DataColumn col in table.Columns) headers.Add(col.ColumnName);
        else for (int i = 0; i < table.Columns.Count; i++) headers.Add($"Col{i}");

        foreach (DataRow dr in table.Rows)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < headers.Count; c++)
                dict[headers[c]] = dr[c]?.ToString() ?? "";
            rows.Add(dict);
        }
        return rows;
    }
#endif

    private void WriteReport(string path, List<ValidationResult> results)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var sw = new StreamWriter(fs, new System.Text.UTF8Encoding(true));
        sw.WriteLine("Row,Passed,Reason,Start,Target,NormalizedStart,NormalizedTarget");
        foreach (var r in results)
        {
            sw.WriteLine(string.Join(",",
                CsvEsc((r.FileRowIndex + (fileHasHeader ? 2 : 1)).ToString()), // 파일상 줄번호
                CsvEsc(r.Passed ? "TRUE" : "FALSE"),
                CsvEsc(r.Reason),
                CsvEsc(r.Start),
                CsvEsc(r.Target),
                CsvEsc(r.NormStart),
                CsvEsc(r.NormTarget)
            ));
        }
    }

    private string CsvEsc(string s)
    {
        if (s == null) return "";
        bool needQuote = s.Contains(',') || s.Contains('\"') || s.Contains('\n') || s.Contains('\r');
        if (needQuote) { s = s.Replace("\"", "\"\""); return $"\"{s}\""; }
        return s;
    }

    private class ValidationResult
    {
        public int FileRowIndex;
        public bool Passed;
        public string Reason;
        public string Start;
        public string Target;
        public string NormStart;
        public string NormTarget;

        public ValidationResult(int fileRowIndex, bool passed, string reason, string start, string target, string ns, string nt)
        {
            FileRowIndex = fileRowIndex;
            Passed = passed;
            Reason = reason;
            Start = start;
            Target = target;
            NormStart = ns;
            NormTarget = nt;
        }
    }

    // 숫자/연산자 덩어리로 먼저 나누기
    private static List<(bool isNumber, string value)> SplitEquationPieces(string eq)
    {
        var list = new List<(bool isNumber, string value)>();
        if (string.IsNullOrEmpty(eq))
            return list;

        int i = 0;
        while (i < eq.Length)
        {
            char c = eq[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsDigit(c))
            {
                int start = i;
                while (i < eq.Length && char.IsDigit(eq[i]))
                    i++;

                string num = eq.Substring(start, i - start);
                list.Add((true, num));
            }
            else
            {
                // +, -, = 같은 연산자/기타 문자
                list.Add((false, c.ToString()));
                i++;
            }
        }

        return list;
    }

    // 숫자 덩어리(연속된 숫자) 쌍을 토큰화
    private static void TokenizeNumberPair(
        string numS, string numT,
        List<string> startTokens,
        List<string> targetTokens)
    {
        if (numS == numT)
        {
            // 숫자 덩어리가 변하지 않은 경우
            var tok = TokenizeNumber_Unchanged(numS);
            startTokens.AddRange(tok);
            targetTokens.AddRange(tok);
        }
        else
        {
            // 숫자 덩어리가 바뀐 경우
            TokenizeNumber_Changed(numS, numT, startTokens, targetTokens);
        }
    }

    /// <summary>
    /// 숫자 덩어리가 "변하지 않았을 때"의 토큰화
    /// - 11이 포함되면:
    ///   * 끝이 11이면: 앞자리들 + "11"  (예: 111 -> 1,11 / 711 -> 7,11)
    ///   * 앞이 11이면: "11" + 뒷자리들 (예: 113 -> 11,3)
    ///   * 중간에 11이면: 앞자리들 + "11" + 뒷자리들
    /// - 11이 없으면: 한 자리씩
    /// </summary>
    private static List<string> TokenizeNumber_Unchanged(string num)
    {
        var tokens = new List<string>();

        if (num.Contains("11"))
        {
            if (num.EndsWith("11"))
            {
                string prefix = num.Substring(0, num.Length - 2);
                foreach (char ch in prefix)
                    tokens.Add(ch.ToString());
                tokens.Add("11");
            }
            else if (num.StartsWith("11"))
            {
                tokens.Add("11");
                string suffix = num.Substring(2);
                foreach (char ch in suffix)
                    tokens.Add(ch.ToString());
            }
            else
            {
                int pos = num.IndexOf("11", StringComparison.Ordinal);
                string prefix = num.Substring(0, pos);
                foreach (char ch in prefix)
                    tokens.Add(ch.ToString());

                tokens.Add("11");

                string suffix = num.Substring(pos + 2);
                foreach (char ch in suffix)
                    tokens.Add(ch.ToString());
            }
        }
        else
        {
            foreach (char ch in num)
                tokens.Add(ch.ToString());
        }

        return tokens;
    }

    /// <summary>
    /// 숫자 덩어리가 "변했을 때"의 토큰화
    /// - 11 ↔ 4 는 항상 한 토큰
    /// - 711 ↔ 74 같은 "맨 뒤 11 ↔ 4" 패턴 지원
    /// - 48 ↔ 118 같은 "맨 앞 4 ↔ 11" 패턴 지원
    /// - 119 ↔ 115 처럼 숫자 안에 11이 그대로 남아 있는 경우:
    ///     → 공통 11 은 "11" 한 토큰, 나머지는 자리수 단위
    /// - 그 외는 모두 자리수 단위
    /// </summary>
    private static void TokenizeNumber_Changed(
        string numS,
        string numT,
        List<string> startTokens,
        List<string> targetTokens)
    {
        // 1) 전체가 11 ↔ 4
        if ((numS == "11" && numT == "4") ||
            (numS == "4" && numT == "11"))
        {
            startTokens.Add(numS);
            targetTokens.Add(numT);
            return;
        }

        // 2) 뒤에서 11 ↔ 4 (예: 711 ↔ 74)
        if (numS.EndsWith("11") && numT.EndsWith("4")
            && numS.Length == numT.Length + 1
            && numS.Substring(0, numS.Length - 2) == numT.Substring(0, numT.Length - 1))
        {
            string prefix = numS.Substring(0, numS.Length - 2);
            foreach (char ch in prefix)
            {
                startTokens.Add(ch.ToString());
                targetTokens.Add(ch.ToString());
            }
            startTokens.Add("11");
            targetTokens.Add("4");
            return;
        }

        if (numT.EndsWith("11") && numS.EndsWith("4")
            && numT.Length == numS.Length + 1
            && numT.Substring(0, numT.Length - 2) == numS.Substring(0, numS.Length - 1))
        {
            string prefix = numS.Substring(0, numS.Length - 1);
            foreach (char ch in prefix)
            {
                startTokens.Add(ch.ToString());
                targetTokens.Add(ch.ToString());
            }
            startTokens.Add("4");
            targetTokens.Add("11");
            return;
        }

        // 3) 앞에서 4 ↔ 11 (예: 48 ↔ 118)
        if (numS.Length + 1 == numT.Length &&
            numS.StartsWith("4") &&
            numT.StartsWith("11") &&
            numS.Substring(1) == numT.Substring(2))
        {
            startTokens.Add("4");
            targetTokens.Add("11");

            string suffix = numS.Substring(1);
            foreach (char ch in suffix)
            {
                startTokens.Add(ch.ToString());
                targetTokens.Add(ch.ToString());
            }
            return;
        }

        if (numT.Length + 1 == numS.Length &&
            numT.StartsWith("4") &&
            numS.StartsWith("11") &&
            numT.Substring(1) == numS.Substring(2))
        {
            startTokens.Add("11");
            targetTokens.Add("4");

            string suffix = numT.Substring(1);
            foreach (char ch in suffix)
            {
                startTokens.Add(ch.ToString());
                targetTokens.Add(ch.ToString());
            }
            return;
        }

        // 4) 양쪽 다 앞이 11로 시작하는 경우 (예: 119 ↔ 115)
        if (numS.StartsWith("11") && numT.StartsWith("11"))
        {
            startTokens.Add("11");
            targetTokens.Add("11");

            string restS = numS.Substring(2);
            string restT = numT.Substring(2);

            foreach (char ch in restS)
                startTokens.Add(ch.ToString());
            foreach (char ch in restT)
                targetTokens.Add(ch.ToString());

            return;
        }

        // 5) 양쪽 다 뒤가 11로 끝나는 경우 (필요하면)
        if (numS.EndsWith("11") && numT.EndsWith("11"))
        {
            string prefixS = numS.Substring(0, numS.Length - 2);
            string prefixT = numT.Substring(0, numT.Length - 2);

            foreach (char ch in prefixS)
                startTokens.Add(ch.ToString());
            foreach (char ch in prefixT)
                targetTokens.Add(ch.ToString());

            startTokens.Add("11");
            targetTokens.Add("11");
            return;
        }

        // 6) 그 외에는 모두 자리수 단위
        foreach (char ch in numS)
            startTokens.Add(ch.ToString());
        foreach (char ch in numT)
            targetTokens.Add(ch.ToString());
    }

    /// <summary>
    /// 스마트 토큰 생성:
    /// - 숫자는 기본적으로 자리수별 토큰
    /// - 단, 위의 규칙으로 11/4, 111, 711, 119 같은 케이스를 처리
    /// </summary>
    private static void BuildSmartTokens(string start, string target,
        out List<string> startTokens, out List<string> targetTokens)
    {
        startTokens = new List<string>();
        targetTokens = new List<string>();

        var s = start ?? string.Empty;
        var t = target ?? string.Empty;
        int i = 0, j = 0;

        while (i < s.Length && j < t.Length)
        {
            char cs = s[i];
            char ct = t[j];

            // 공백 스킵
            if (char.IsWhiteSpace(cs)) { i++; continue; }
            if (char.IsWhiteSpace(ct)) { j++; continue; }

            bool ds = char.IsDigit(cs);
            bool dt = char.IsDigit(ct);

            if (ds && dt)
            {
                // 숫자 덩어리 뽑기
                int i0 = i;
                while (i < s.Length && char.IsDigit(s[i])) i++;
                string numS = s.Substring(i0, i - i0);

                int j0 = j;
                while (j < t.Length && char.IsDigit(t[j])) j++;
                string numT = t.Substring(j0, j - j0);

                if (numS == numT)
                {
                    // 숫자 덩어리가 완전히 동일할 때
                    var tokens = TokenizeNumber_Unchanged(numS);
                    startTokens.AddRange(tokens);
                    targetTokens.AddRange(tokens);
                }
                else
                {
                    // 숫자 덩어리가 다를 때
                    TokenizeNumber_Changed(numS, numT, startTokens, targetTokens);
                }
            }
            else
            {
                // 연산자나 기타 문자: 그대로 1글자씩
                startTokens.Add(cs.ToString());
                targetTokens.Add(ct.ToString());
                i++; j++;
            }
        }

        // 남은 문자 처리
        while (i < s.Length)
        {
            if (!char.IsWhiteSpace(s[i]))
                startTokens.Add(s[i].ToString());
            i++;
        }

        while (j < t.Length)
        {
            if (!char.IsWhiteSpace(t[j]))
                targetTokens.Add(t[j].ToString());
            j++;
        }
    }



    // 예외 케이스용: 그냥 공백 제외하고 한 글자씩 토큰
    private static void SimpleCharTokenize(string eq, List<string> tokens)
    {
        if (string.IsNullOrEmpty(eq))
            return;

        foreach (char c in eq)
        {
            if (!char.IsWhiteSpace(c))
                tokens.Add(c.ToString());
        }
    }



    /// <summary>
    /// CSV에 저장된 토큰 문자열(예: "'-;4;+;11;=;7'")을 파싱해서 리스트로 변환
    /// </summary>
    /// <summary>
    /// CSV에 저장된 토큰 문자열(예: "'-;4;+;11;=;7'")을 파싱해서 리스트로 변환.
    /// - 전체 문자열 양끝의 ' 제거
    /// - 각 토큰 양끝에 남아 있는 ' 도 모두 제거
    /// </summary>
    private static List<string> ParseTokenColumn(string raw)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(raw))
            return list;

        raw = raw.Trim();

        // 전체를 감싸고 있는 홑따옴표 제거 ('...') 형태
        if (raw.Length >= 2 && raw[0] == '\'' && raw[^1] == '\'')
            raw = raw.Substring(1, raw.Length - 2);

        // 세미콜론 기준 분리
        var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            var token = p.Trim();

            // 각 토큰 앞뒤에 남아 있는 홑따옴표 제거 (예: "1'" -> "1")
            token = token.Trim('\'', ' ');

            if (string.IsNullOrEmpty(token))
                continue;

            list.Add(token);
        }

        return list;
    }

}

// ====== 규칙 로직 v2 ======
public static class MatchstickRuleValidator2
{
    // 7-seg 점등 세그먼트 (a,b,c,d,e,f,g)
    private static readonly Dictionary<char, string> Segs = new()
    {
        ['0'] = "abcdef",
        ['1'] = "cf",
        ['2'] = "acdeg",
        ['3'] = "acdfg",
        ['4'] = "bcfg",
        ['5'] = "abdfg",
        ['6'] = "abdefg",
        ['7'] = "acf",
        ['8'] = "abcdefg",
        ['9'] = "abcdfg",
    };

    // 연산자 성냥 개수(대략)
    private static readonly Dictionary<char, int> OpCount = new()
    {
        ['+'] = 2,
        ['-'] = 1,
        ['='] = 2,
    };

    // ---- 공개 API ----
    public static bool IsOneMoveTransform(string start, string target, bool allowSplitMerge11_4, out string reason)
    {
        // ① 글로벌 1회 이동 (전역 세그먼트 diff로 add=1, remove=1)
        if (IsOneMoveDirect(start, target))
        {
            reason = "OK: direct one-stick move.";
            return true;
        }

        // ② 한 자리 숫자 내부에서 1 off + 1 on
        if (IsSingleDigitMorphOneMove(start, target))
        {
            reason = "OK: single-digit morph by one-stick move.";
            return true;
        }

        // ③ 숫자 한 자리에서 성냥 1개를 빼서 다른 숫자로 만들고,
        //    그 성냥으로 '-' 연산자를 새로 만든 경우
        if (IsDigitToMinusOneMove(start, target))
        {
            reason = "OK: digit loses 1 segment and new '-' is created (one-stick move).";
            return true;
        }

        // ④ '+' 하나에서 성냥 1개를 빼서 '-' 두 개가 되는 경우
        if (IsPlusSplitToTwoMinusesOneMove(start, target))
        {
            reason = "OK: '+' splits into two '-' (one-stick move).";
            return true;
        }

        if (allowSplitMerge11_4)
        {
            // ⑤ 11→4 치환만으로 동일 or 치환 + 1회 이동
            foreach (var s2 in Enumerate11to4Once(start))
            {
                if (s2 == target)
                {
                    reason = "OK: 11→4 special transform (counts as one move).";
                    return true;
                }
                if (IsOneMoveDirect(s2, target)
                    || IsSingleDigitMorphOneMove(s2, target)
                    || IsDigitToMinusOneMove(s2, target)
                    || IsPlusSplitToTwoMinusesOneMove(s2, target))
                {
                    reason = "OK: 11→4 transform + one-stick move.";
                    return true;
                }
            }

            // ⑥ 4→11 치환만으로 동일 or 치환 + 1회 이동
            foreach (var s3 in Enumerate4to11Once(start))
            {
                if (s3 == target)
                {
                    reason = "OK: 4→11 special transform (counts as one move).";
                    return true;
                }
                if (IsOneMoveDirect(s3, target)
                    || IsSingleDigitMorphOneMove(s3, target)
                    || IsDigitToMinusOneMove(s3, target)
                    || IsPlusSplitToTwoMinusesOneMove(s3, target))
                {
                    reason = "OK: 4→11 transform + one-stick move.";
                    return true;
                }
            }
        }

        reason = BuildFailureReason(start, target, allowSplitMerge11_4);
        return false;
    }

    public static string Explain(string start, string target, bool allowSplitMerge11_4 = true)
    {
        return IsOneMoveTransform(start, target, allowSplitMerge11_4, out var why) ? why : why;
    }

    // ---- 내부 로직 ----

    /// 전역 세그먼트 diff 기반 1회 이동 체크 (add=1, remove=1)
    private static bool IsOneMoveDirect(string a, string b)
    {
        var (add, remove, valid, _) = DiffAsMoves(a, b);
        return valid && add == 1 && remove == 1;
    }

    /// 한 자리 숫자 내부에서 세그먼트 1 off + 1 on
    private static bool IsSingleDigitMorphOneMove(string s, string t)
    {
        if (s.Length != t.Length) return false;
        int diffIdx = -1;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] != t[i])
            {
                if (diffIdx != -1) return false; // 두 곳 이상 다르면 불가
                diffIdx = i;
            }
        }
        if (diffIdx == -1) return false;       // 동일 문자열
        char from = s[diffIdx], to = t[diffIdx];
        if (!char.IsDigit(from) || !char.IsDigit(to)) return false;

        if (!Segs.TryGetValue(from, out var segFrom) ||
            !Segs.TryGetValue(to, out var segTo))
            return false;

        var A = new HashSet<char>(segFrom);
        var B = new HashSet<char>(segTo);
        var off = new HashSet<char>(A); off.ExceptWith(B);
        var on = new HashSet<char>(B); on.ExceptWith(A);
        return off.Count == 1 && on.Count == 1; // 1개 끄고 1개 켠 경우
    }

    /// 숫자 한 자리에서 성냥 1개를 빼서 다른 숫자로 만들고,
    /// 그 성냥으로 '-' 연산자를 새로 생성한 경우를 1회 이동으로 인정.
    /// (예: 32+24=29 → 3-2+24=25, 9→5 + 새 '-')
    private static bool IsDigitToMinusOneMove(string start, string target)
    {
        int CountTotal(string s)
        {
            int total = 0;
            foreach (var ch in s)
            {
                if (char.IsDigit(ch))
                {
                    if (!Segs.TryGetValue(ch, out var segs)) return -1;
                    total += segs.Length;
                }
                else if (OpCount.TryGetValue(ch, out var cnt)) total += cnt;
                else if (!char.IsWhiteSpace(ch)) return -1;
            }
            return total;
        }

        int ta = CountTotal(start);
        int tb = CountTotal(target);
        if (ta < 0 || tb < 0 || ta != tb)
            return false;

        var fs = new Dictionary<char, int>();
        var ft = new Dictionary<char, int>();

        void AddFreq(Dictionary<char, int> dict, char ch)
        {
            if (!dict.ContainsKey(ch)) dict[ch] = 0;
            dict[ch]++;
        }

        foreach (var ch in start)
        {
            if (char.IsWhiteSpace(ch)) continue;
            AddFreq(fs, ch);
        }

        foreach (var ch in target)
        {
            if (char.IsWhiteSpace(ch)) continue;
            AddFreq(ft, ch);
        }

        var delta = new Dictionary<char, int>();
        void AddDelta(char ch, int d)
        {
            if (!delta.ContainsKey(ch)) delta[ch] = 0;
            delta[ch] += d;
        }

        foreach (var kv in fs)
            AddDelta(kv.Key, -kv.Value);
        foreach (var kv in ft)
            AddDelta(kv.Key, kv.Value);

        // '-' 는 정확히 1개 늘어남
        if (!delta.TryGetValue('-', out var minusDelta) || minusDelta != 1)
            return false;

        char dFrom = '\0', dTo = '\0';
        int dFromCount = 0, dToCount = 0;

        foreach (var kv in delta)
        {
            char ch = kv.Key;
            int d = kv.Value;

            if (ch == '-') continue;

            if (char.IsDigit(ch))
            {
                if (d == -1)
                {
                    dFrom = ch;
                    dFromCount++;
                }
                else if (d == +1)
                {
                    dTo = ch;
                    dToCount++;
                }
                else if (d != 0)
                {
                    return false;
                }
            }
            else
            {
                if (d != 0) return false;
            }
        }

        if (dFromCount != 1 || dToCount != 1)
            return false;

        if (!Segs.TryGetValue(dFrom, out var segFrom) ||
            !Segs.TryGetValue(dTo, out var segTo))
            return false;

        int lenFrom = segFrom.Length;
        int lenTo = segTo.Length;

        // dFrom 이 dTo 보다 세그먼트 1개 더 많아야 함 (1개 off)
        return lenFrom == lenTo + 1;
    }

    /// '+' 하나에서 성냥 1개를 빼서 '-' 두 개가 되는 경우를 1회 이동으로 인정.
    /// (예: 32+21=29 → 32-2-1=29)
    private static bool IsPlusSplitToTwoMinusesOneMove(string start, string target)
    {
        int CountTotal(string s)
        {
            int total = 0;
            foreach (var ch in s)
            {
                if (char.IsDigit(ch))
                {
                    if (!Segs.TryGetValue(ch, out var segs)) return -1;
                    total += segs.Length;
                }
                else if (OpCount.TryGetValue(ch, out var cnt)) total += cnt;
                else if (!char.IsWhiteSpace(ch)) return -1;
            }
            return total;
        }

        int ta = CountTotal(start);
        int tb = CountTotal(target);
        if (ta < 0 || tb < 0 || ta != tb)
            return false;

        var fs = new Dictionary<char, int>();
        var ft = new Dictionary<char, int>();

        void AddFreq(Dictionary<char, int> dict, char ch)
        {
            if (!dict.ContainsKey(ch)) dict[ch] = 0;
            dict[ch]++;
        }

        foreach (var ch in start)
        {
            if (char.IsWhiteSpace(ch)) continue;
            AddFreq(fs, ch);
        }

        foreach (var ch in target)
        {
            if (char.IsWhiteSpace(ch)) continue;
            AddFreq(ft, ch);
        }

        var delta = new Dictionary<char, int>();
        void AddDelta(char ch, int d)
        {
            if (!delta.ContainsKey(ch)) delta[ch] = 0;
            delta[ch] += d;
        }

        foreach (var kv in fs)
            AddDelta(kv.Key, -kv.Value);
        foreach (var kv in ft)
            AddDelta(kv.Key, kv.Value);

        // '+' 는 정확히 1개 줄어들고, '-' 는 정확히 2개 늘어나야 함
        if (!delta.TryGetValue('+', out var plusDelta) || plusDelta != -1)
            return false;
        if (!delta.TryGetValue('-', out var minusDelta) || minusDelta != 2)
            return false;

        // 나머지 문자들(delta)은 전부 0이어야 함
        foreach (var kv in delta)
        {
            char ch = kv.Key;
            int d = kv.Value;
            if (ch == '+' || ch == '-') continue;
            if (d != 0) return false;
        }

        return true;
    }

    private static (int add, int remove, bool valid, string msg) DiffAsMoves(string a, string b)
    {
        int CountTotal(string s)
        {
            int total = 0;
            foreach (var ch in s)
            {
                if (char.IsDigit(ch))
                {
                    if (!Segs.ContainsKey(ch)) return -1;
                    total += Segs[ch].Length;
                }
                else if (OpCount.TryGetValue(ch, out var cnt)) total += cnt;
                else if (!char.IsWhiteSpace(ch)) return -1;
            }
            return total;
        }

        int ta = CountTotal(a), tb = CountTotal(b);
        if (ta < 0 || tb < 0)
            return (0, 0, false, "Invalid character in equation.");

        if (tb - ta == 0)
        {
            (int pos, int neg) = RoughSegmentDelta(a, b);
            return (pos, Math.Abs(neg), true, "");
        }
        else
        {
            return (0, 0, false, "Total matchstick count differs (add/remove needed).");
        }
    }

    private static (int pos, int neg) RoughSegmentDelta(string a, string b)
    {
        var aa = a.ToCharArray();
        var bb = b.ToCharArray();
        int n = Math.Min(aa.Length, bb.Length), pos = 0, neg = 0;

        for (int i = 0; i < n; i++)
        {
            int ca = CountChar(aa[i]), cb = CountChar(bb[i]);
            int d = cb - ca;
            if (d >= 0) pos += d; else neg += d;
        }

        if (aa.Length > n)
            for (int i = n; i < aa.Length; i++) neg -= CountChar(aa[i]);
        else if (bb.Length > n)
            for (int i = n; i < bb.Length; i++) pos += CountChar(bb[i]);

        return (pos, neg);
    }

    private static int CountChar(char ch)
    {
        if (char.IsDigit(ch))
            return Segs.TryGetValue(ch, out var segs) ? segs.Length : 0;
        if (OpCount.TryGetValue(ch, out var cnt))
            return cnt;
        return 0;
    }

    private static IEnumerable<string> Enumerate11to4Once(string s)
    {
        for (int i = 0; i < s.Length - 1; i++)
            if (s[i] == '1' && s[i + 1] == '1')
                yield return s.Substring(0, i) + "4" + s.Substring(i + 2);
    }

    private static IEnumerable<string> Enumerate4to11Once(string s)
    {
        for (int i = 0; i < s.Length; i++)
            if (s[i] == '4')
                yield return s.Substring(0, i) + "11" + s.Substring(i + 1);
    }

    private static string BuildFailureReason(string start, string target, bool allowSplitMerge11_4)
    {
        var (add, remove, valid, msg) = DiffAsMoves(start, target);
        if (!valid) return $"FAIL: {msg}";

        if (add == 1 && remove == 1)
            return "Unexpected: should have passed as one move.";

        if (IsSingleDigitMorphOneMove(start, target))
            return "Unexpected: should have passed (single-digit one-stick morph).";

        if (IsDigitToMinusOneMove(start, target))
            return "Unexpected: should have passed (digit→digit + new '-' by one-stick move).";

        if (IsPlusSplitToTwoMinusesOneMove(start, target))
            return "Unexpected: should have passed ('+' → two '-' by one-stick move).";

        if (allowSplitMerge11_4)
        {
            foreach (var s2 in Enumerate11to4Once(start))
                if (s2 == target
                    || IsOneMoveDirect(s2, target)
                    || IsSingleDigitMorphOneMove(s2, target)
                    || IsDigitToMinusOneMove(s2, target)
                    || IsPlusSplitToTwoMinusesOneMove(s2, target))
                    return "Unexpected: should have passed via 11→4 rule.";

            foreach (var s3 in Enumerate4to11Once(start))
                if (s3 == target
                    || IsOneMoveDirect(s3, target)
                    || IsSingleDigitMorphOneMove(s3, target)
                    || IsDigitToMinusOneMove(s3, target)
                    || IsPlusSplitToTwoMinusesOneMove(s3, target))
                    return "Unexpected: should have passed via 4→11 rule.";
        }

        return $"FAIL: needs {add} additions and {Math.Abs(remove)} removals (≈{Math.Max(add, Math.Abs(remove))} moves), not one.";
    }
}
