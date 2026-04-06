using UnityEngine;

public interface IGameSessionController
{
    void StartSession(IStageSequence sequence);
    void StopSession(); // 강제 종료(메뉴/나가기)
}
