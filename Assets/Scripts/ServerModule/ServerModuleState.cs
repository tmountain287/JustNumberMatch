using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ServerModuleState
{
    public abstract IEnumerator Handle(StateMachine machine, EGoStopEventType eventType, nxGoStopEvent goStopEvent = null);
}
