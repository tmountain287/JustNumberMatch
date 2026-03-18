
using System;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    public static string FormatKMBT<T>(this T _number, int _count, bool _useDot) where T : struct, IConvertible
    {
        double numericValue = Convert.ToDouble(_number);
        if (numericValue == 0) return "0";
        string formattedNumber;

        // _count가 -1인 경우 축약하지 않고 그대로 반환
        if (_count == -1)
        {
            return numericValue.ToString("N0");
        }

        // 숫자의 자리수 계산
        int digitCount = (int)Math.Floor(Math.Log10(Math.Abs(numericValue)) + 1);

        // 자리수가 _count보다 큰 경우에만 축약
        if (digitCount > _count)
        {
            if (numericValue >= 1_000_000_000_000)  // 1조 이상이면 T로 축약
            {
                formattedNumber = (numericValue / 1_000_000_000_000).ToString("0.00") + "T";
            }
            else if (numericValue >= 1_000_000_000)  // 10억 이상이면 B로 축약
            {
                formattedNumber = (numericValue / 1_000_000_000).ToString("0.00") + "B";
            }
            else if (numericValue >= 1_000_000)  // 100만 이상이면 M으로 축약
            {
                formattedNumber = (numericValue / 1_000_000).ToString("0.00") + "M";
            }
            else if (numericValue >= 1_000)  // 1천 이상이면 K로 축약
            {
                formattedNumber = (numericValue / 1_000).ToString("0.00") + "K";
            }
            else
            {
                formattedNumber = _useDot ? numericValue.ToString("#,0.00") : numericValue.ToString("#,0");
            }
        } 
        else
        {
            formattedNumber = _useDot && numericValue < 1000 ? numericValue.ToString("#,0.00") : numericValue.ToString("#,0");
        }

        return formattedNumber;
    }


    public static string FormatKoreanUnits<T>(this T _number, bool _useDot = false) where T : struct, IConvertible
    {
        double number = Convert.ToDouble(_number);
        if (number == 0) return "0냥";

        string[] units = { "", "만", "억", "조", "경" };
        List<string> rawParts = new();

        int unitIndex = 0;
        while (number > 0)
        {
            double part = number % 10000;
            rawParts.Add($"{Math.Floor(part)}|{unitIndex}"); // 뒤에서 역순으로 쓰기 위해
            number = Math.Floor(number / 10000);
            unitIndex++;
        }

        rawParts.Reverse();

        // 상위 유효한 블록부터 최대 2개만 추출
        List<string> resultParts = new();
        int taken = 0;
        foreach (var raw in rawParts)
        {
            var split = raw.Split('|');
            double value = double.Parse(split[0]);
            int index = int.Parse(split[1]);

            if (value > 0)
            {
                string formatted = (index == 0 && _useDot && value < 1000) ?
                    value.ToString("#,0.00") :
                    value.ToString("0") + units[index];

                resultParts.Add(formatted);
                taken++;

                if (taken >= 2)
                    break;
            }
        }

        return $"{string.Join(" ", resultParts)}냥";
    }

    public static string FormatComma<T>(this T _number)
    {
        string formattedNumber = string.Format("{0:#,0}", _number);
        return formattedNumber;
    }

    /// <summary>밀리초를 03'20''365 형식(분'초''밀리초)으로 변환</summary>
    public static string FormatFromMs(this long totalMs)
    {
        if (totalMs == 0)
            return "--'--''---";

        totalMs = Math.Max(0, totalMs);
        long minutes = totalMs / 60000;
        long seconds = (totalMs % 60000) / 1000;
        long millis = totalMs % 1000;
        return string.Format("{0:00}'{1:00}''{2:000}", minutes, seconds, millis);
    }

    /// <summary>밀리초를 03'20''365 형식(분'초''밀리초)으로 변환</summary>
    public static string FormatFromMs_HMS(this long totalMs)
    {
        if (totalMs <= 0)
            return "--'--''---";

        totalMs = Math.Max(0, totalMs);
        long minutes = totalMs / 60000;
        long seconds = (totalMs % 60000) / 1000;
       // long millis = totalMs % 1000;
        return string.Format("{0:00}'{1:00}''", minutes, seconds);
    }
}