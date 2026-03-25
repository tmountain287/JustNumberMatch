using System.Collections.Generic;

public class TableDataManager : MonoSingletonDont<TableDataManager>
{
    public TableLevelData TableLevelData { get; private set; }
    public TableCharacterData TableCharacterData { get; private set; }
    public TableConfigData TableConfigData { get; private set; }
    public TableSuddaData TableSuddaData { get; private set; }
    public TableShopCharacterData TableShopCharacterData { get; private set; }
    public TableSkillData TableSkillData { get; private set; }

    public TableSlangData TableSlangData { get; private set; }
    public TableProductCatalogData TableProductCatalogData { get; private set; }
    public TableGPGAchievementData TableGPGAchievementData { get; private set; }

    private bool init = false;

    public void LoadAllCSVs()
    {
        if (init) return;

        TableLevelData = new();
        TableCharacterData = new();
        TableConfigData = new();
        TableSuddaData = new();
        TableShopCharacterData = new();
        TableSkillData = new();
        TableSlangData = new();
        TableProductCatalogData = new();
        TableGPGAchievementData = new();
        init = true;

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

            string[] fields = line.Split(',');
            if (fields.Length < 2) continue;

            list.Add(createFunc(fields));
        }

        return list;
    }
}