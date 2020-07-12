using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReciever
{
    void RecieveInput(InputGroup inputs);
}
public struct InputGroup {
    public readonly VectorInput[] VectorInputs; public readonly ButtonInput[] ButtonInputs;
    public InputGroup(VectorInput[] vectorInputs, ButtonInput[] buttonInputs)
    {
        VectorInputs = vectorInputs;
        ButtonInputs = buttonInputs;
    }
}
public struct VectorInput {
    public readonly string name; public readonly Vector3 dir; public readonly bool started; public readonly bool continuous; public readonly bool stopped;
    public VectorInput(string name, Vector3 dir, bool started, bool continuous, bool stopped)
    {
        this.name = name;
        this.dir = dir;
        this.started = started;
        this.continuous = continuous;
        this.stopped = stopped;
    }
    public bool IsIdleState()
    {
        return !started && !continuous && !stopped;
    }
}
public struct ButtonInput {
    public readonly string name; public readonly bool started; public readonly bool continuous; public readonly bool stopped;
    public ButtonInput(string name, bool started, bool continuous, bool stopped)
    {
        this.name = name;
        this.started = started;
        this.continuous = continuous;
        this.stopped = stopped;
    }
    public bool IsIdleState() {
        return !started && !continuous && !stopped;
    }
}