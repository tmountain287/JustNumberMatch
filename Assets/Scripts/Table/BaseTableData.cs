using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public abstract class BaseTableData
{
    public BaseTableData()
    {
        Load();
    }

    public List<T> Parse<T>(EncryptedCSVData csvData, System.Func<string[], T> createFunc)
    {
        var list = new List<T>();

        string decrypted = csvData.GetDecryptedText();
        string[] lines = decrypted.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 필드 파싱: 쉼표로 나누되, 큰따옴표로 감싼 부분은 하나로 유지
            var fields = ParseCsvLine(line);

            if (fields.Length < 2) continue;
            list.Add(createFunc(fields));
        }

        return list;
    }

    private string[] ParseCsvLine(string line)
    {
        var pattern = @"(?:^|,)(?:""(?<val>(?:[^""]|"""")*)""|(?<val>[^,]*))";
        var matches = Regex.Matches(line, pattern);
        return matches.Cast<Match>()
                      .Select(m => m.Groups["val"].Value.Replace("\"\"", "\"")) // "" → "
                      .ToArray();
    }

    public abstract void Load();
}