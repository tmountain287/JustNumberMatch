using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

#region Token/Result Types (프로젝트 정의에 맞게 교체 가능)

public enum ValidationResultType
{
    NONE = 0,
    OK = 1,     // 성공
    FALSE = 2,  //수식은 완성되었지만 수식이 틀렸음
    NULL = 3,   //인식안되는 값이 있음
    NONE_EQUAL = 4, //등호가 없음
    ERROR = 5, //그 외 에러
}

public sealed class ValidationResult
{
    // 공통
    public bool Success { get; set; }
    public string Error { get; set; }

    // 등식(=)일 때: a = b = c ...
    public bool IsEquation { get; set; }
    public List<int> SegmentValues { get; set; } = new();
    public List<string> SegmentNormalized { get; set; } = new();
    public bool AreAllEqual { get; set; }

    // 호환 출력(등호가 1개이고, 한쪽 단일 숫자 vs 다른 쪽 '숫자 op 숫자' 1개일 때만 의미 있게 채움)
    public int Left { get; set; }
    public int Right { get; set; }
    public int Result { get; set; }
    public OperatorType Op { get; set; } = OperatorType.None;

    public ValidationResultType ValidationResultType { get; set; } = ValidationResultType.NONE;

    public override string ToString()
    {
        if (!Success) return $"Invalid: {Error}";
        if (IsEquation)
            return $"Eq OK: {string.Join(" = ", SegmentNormalized)} | AllEqual={AreAllEqual}";
        return "OK";
    }
}
#endregion

public static class EquationValidator
{
    /// <summary>
    /// 토큰을 "아무거나" 던져도 판단/검증.
    /// - '='(Equals) 토큰이 하나도 없으면 즉시 실패: "등호(=)가 없습니다."
    /// - '='가 하나 이상이면: a = b = c ... (연쇄 등식; 각 Expr 값 계산 후 모두 같은지 체크)
    /// 추가로, (한쪽 단일 숫자) vs (다른쪽 '숫자 op 숫자' 1개) 패턴이면 호환 출력(Left/Right/Result/Op) 채움.
    /// </summary>
    public static bool TryValidateTokens(List<RecognizerInfo> tokens, out ValidationResult res)
    {
        res = new ValidationResult();

        if (tokens == null || tokens.Count == 0)
        {
            res.Success = false;
            res.Error = "토큰이 비어 있습니다.";
            return false;
        }

        // '=' 위치 수집
        var equalIdx = new List<int>();
        for (int k = 0; k < tokens.Count; k++)
        {
            if (IsOperator(tokens[k], OperatorType.Equals))
                equalIdx.Add(k);
        }

        // '=' 없으면 즉시 실패
        if (equalIdx.Count == 0)
        {
            res.Success = false;
            res.Error = "등호(=)가 없습니다.";
            res.ValidationResultType = ValidationResultType.NONE_EQUAL;
            return false;
        }

        // '=' 하나 이상 → 연쇄 등식: segments = Expr ('=' Expr)*
        int start = 0;
        var segments = new List<(int s, int e)>(); // [s, e) 구간
        foreach (int eqPos in equalIdx)
        {
            segments.Add((start, eqPos));
            start = eqPos + 1;
        }
        segments.Add((start, tokens.Count));

        foreach (var (s, e) in segments)
        {
            if (s >= e)
            {
                res.Success = false;
                res.Error = $"등호 주변에 비어 있는 식이 있습니다. (구간 {s}~{e})";
                res.ValidationResultType = ValidationResultType.ERROR;
                return false;
            }

            int i = s;
            if (!TryParseExpr(tokens, ref i, out int val, out string norm, out string err))
            {
                res.Success = false;
                res.Error = PrefixIdx(err, i);
                return false;
            }
            if (i != e)
            {
                res.Success = false;
                res.Error = $"구간 내에 불필요한 토큰이 있습니다. (index {i}, 구간 {s}~{e})";
                res.ValidationResultType = ValidationResultType.ERROR;
                return false;
            }

            res.SegmentValues.Add(val);
            res.SegmentNormalized.Add(norm);
        }

        res.IsEquation = true;
        res.Success = true;
        res.AreAllEqual = AllEqual(res.SegmentValues);

        // 호환 출력 채우기(등호가 정확히 1개일 때만 후보)
        if (res.SegmentValues.Count == 2)
        {
            string L = res.SegmentNormalized[0];
            string R = res.SegmentNormalized[1];

            if (TryDecomposeSingleOpExpr(L, out int a, out OperatorType o, out int b) &&
                int.TryParse(R, out int c))
            {
                res.Left = a; res.Right = b; res.Result = c; res.Op = o;
            }
            else if (int.TryParse(L, out int c2) &&
                     TryDecomposeSingleOpExpr(R, out int a2, out OperatorType o2, out int b2))
            {
                res.Left = a2; res.Right = b2; res.Result = c2; res.Op = o2;
            }
        }

        return true;
    }

    #region Core Parsing

    // Expr = [ ('+'|'-') ] Number { ('+'|'-') [ ('+'|'-') ] Number }*
    //  - 첫 항: 단항부호 허용
    //  - 반복부: 이항 연산자(+|-) 뒤에 "단항 부호(+|-)"를 1회 허용 → 예: "97 - - 4" → "97 - (-4)"
    private static bool TryParseExpr(
        List<RecognizerInfo> tokens, ref int i,
        out int value, out string normalized, out string error)
    {
        value = 0;
        normalized = null;
        error = null;

        var parts = new List<string>();

        // 첫 항: 단항부호 허용
        int sign = +1;
        if (i < tokens.Count &&
            tokens[i].RecognizerType == RecognizerType.Operator &&
            tokens[i].Value is OperatorType ot1 &&
            (ot1 == OperatorType.Plus || ot1 == OperatorType.Minus || ot1 == OperatorType.Minus2))
        {
            sign = (ot1 == OperatorType.Minus || ot1 == OperatorType.Minus2) ? -1 : +1;
            parts.Add(ot1 == OperatorType.Minus || ot1 == OperatorType.Minus2 ? "-" : "+");
            i++;
        }

        if (!TryParseNumber(tokens, ref i, out int firstNum, out error))
            return false;

        long acc = (long)sign * firstNum;
        parts.Add(Math.Abs(firstNum).ToString()); // 부호는 parts[0]에 반영됨

        // 후속: (+|-) [ (+|-) ] Number 반복  ✅ 단항부호 허용 추가
        while (i < tokens.Count &&
               tokens[i].RecognizerType == RecognizerType.Operator &&
               tokens[i].Value is OperatorType ot &&
               (ot == OperatorType.Plus || ot == OperatorType.Minus || ot == OperatorType.Minus2))
        {
            // 1) 이항 연산자 소비
            i++;
            bool isPlus = (ot == OperatorType.Plus);
            parts.Add(isPlus ? "+" : "-");

            // 2) (선택) 단항 부호 허용
            int unarySign = +1;
            if (i < tokens.Count &&
                tokens[i].RecognizerType == RecognizerType.Operator &&
                tokens[i].Value is OperatorType uop &&
                (uop == OperatorType.Plus || uop == OperatorType.Minus || uop == OperatorType.Minus2))
            {
                unarySign = (uop == OperatorType.Minus || uop == OperatorType.Minus2) ? -1 : +1;
                i++;
            }

            // 3) 숫자 파싱(연속 Digit 연결: 11은 "11" 두 자리)
            if (!TryParseNumber(tokens, ref i, out int nextNum, out error))
                return false;

            // 4) 누적 계산
            nextNum *= unarySign;
            acc = isPlus ? acc + nextNum : acc - nextNum;

            // 5) 정규화 출력(부호가 반영된 수를 그대로 넣음 → "+ -4" 같은 형태를 만들지 않기 위해)
            parts.Add(nextNum.ToString());
        }

        // 정규화 문자열: 맨 앞 '+ '는 제거
        normalized = string.Join(" ", parts).Trim();
        if (normalized.StartsWith("+ ")) normalized = normalized.Substring(2);

        // 값 검증
        if (acc > int.MaxValue || acc < int.MinValue)
        {
            error = "계산 결과가 int 범위를 벗어났습니다.";
            
            return false;
        }
        value = (int)acc;
        return true;
    }

    // Number: 연속 Digit 붙이기(11은 "11"로 두 자리)
    private static bool TryParseNumber(
        List<RecognizerInfo> tokens, ref int i, out int number, out string error)
    {
        number = 0;
        error = null;

        // 최소 한 개 이상의 Digit 필요
        if (i >= tokens.Count || tokens[i].RecognizerType != RecognizerType.Digit)
        {
            error = "숫자가 필요합니다.";
            return false;
        }

        var buf = new StringBuilder();
        while (i < tokens.Count && tokens[i].RecognizerType == RecognizerType.Digit)
        {
            if (!(tokens[i].Value is int d))
            {
                error = "Digit Value가 int가 아닙니다.";
                return false;
            }

            if (d == 11)
            {
                buf.Append("11"); // 특수 케이스(두 자리)
            }
            else if (0 <= d && d <= 9)
            {
                buf.Append((char)('0' + d));
            }
            else
            {
                error = $"지원하지 않는 Digit 값: {d}";
                return false;
            }
            i++;
        }

        if (buf.Length == 0)
        {
            error = "유효한 숫자가 없습니다.";
            return false;
        }

        if (!int.TryParse(buf.ToString(), out number))
        {
            error = $"숫자 파싱 실패: {buf}";
            return false;
        }
        return true;
    }
    #endregion

    #region Utils
    private static bool IsOperator(RecognizerInfo t, OperatorType type) =>
        t.RecognizerType == RecognizerType.Operator &&
        t.Value is OperatorType ot && ot == type;

    private static string PrefixIdx(string msg, int idx) =>
        string.IsNullOrEmpty(msg) ? $"파싱 오류 (index {idx})" : $"{msg} (index {idx})";

    private static bool AllEqual(List<int> vals)
    {
        if (vals.Count <= 1) return true;
        int x = vals[0];
        for (int k = 1; k < vals.Count; k++)
            if (vals[k] != x) return false;
        return true;
    }

    // "a [+|-] b" 꼴인지 간단 분해(공백 기준) - 호환 출력용
    private static bool TryDecomposeSingleOpExpr(string expr, out int a, out OperatorType op, out int b)
    {
        a = b = 0; op = OperatorType.None;
        if (string.IsNullOrWhiteSpace(expr)) return false;

        var t = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (t.Length != 3) return false;

        if (!int.TryParse(t[0], out a)) return false;
        if (t[1] == "+") op = OperatorType.Plus;
        else if (t[1] == "-") op = OperatorType.Minus;
        else return false;

        if (!int.TryParse(t[2], out b)) return false;
        return true;
    }
    #endregion
}
