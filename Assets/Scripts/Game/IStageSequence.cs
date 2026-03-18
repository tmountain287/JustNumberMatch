using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IStageSequence
{
    int Count { get; }      // 총 몇 판
    int Index { get; }      // 현재 인덱스(0-based), 진행 후 증가
    EquationTableData Current { get; }   // 현재 스테이지
    bool MoveNext();         // 다음 스테이지로 진행(첫 호출 시 0으로 세팅되게 구현)
    void Reset();
}

public sealed class SingleStageSequence : IStageSequence
{
    private readonly EquationTableData _data;
    private int _index = -1;
    public SingleStageSequence(EquationTableData data) { _data = data; }
    public int Count => 1;
    public int Index => Mathf.Clamp(_index, 0, 0);
    public EquationTableData Current => _data;
    public bool MoveNext() { _index++; return _index == 0; } // 한 번만 true
    public void Reset() { _index = -1; }
}

public sealed class ListStageSequence : IStageSequence
{
    private readonly List<EquationTableData> _list;
    private int _index = -1;

    public ListStageSequence(IEnumerable<EquationTableData> datas)
    {
        _list = datas.ToList();
    }

    public int Count => _list.Count;
    public int Index => Mathf.Clamp(_index, 0, _list.Count - 1);
    public EquationTableData Current => (_index >= 0 && _index < _list.Count) ? _list[_index] : null;

    public List<EquationTableData> List => _list;

    public bool MoveNext()
    {
        if (_index + 1 >= _list.Count) return false;
        _index++;
        return true;
    }
    public void Reset() { _index = -1; }
}