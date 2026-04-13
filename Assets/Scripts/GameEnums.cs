/// <summary>스테이지 난이도 (슬라이더 인덱스와 동일: 0=Easy, 1=Normal, 2=Hard).</summary>
public enum DifficultyType
{
    Easy = 0,
    Normal = 1,
    Hard = 2,
}

/// <summary>뺏기 스킬 종류 (CardMainType + 1 과 대응).</summary>
public enum StealType
{
    None = 0,
    KwangSteal = 1,
    MungSteal = 2,
    DDiSteal = 3,
    PeeSteal = 4,
}
