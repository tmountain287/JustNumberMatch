using UnityEngine;

public abstract class GameSessionController : MonoBehaviour
{
    public StageTableData StageTableData { get; set; }

    public abstract void ReadySession(IStageSequence sequence);

    public abstract void StartSession();
    public abstract void StopSession(); // 강제 종료(메뉴/나가기)

    public abstract void ChangeSequence();
}
