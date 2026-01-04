using UnityEngine;


public abstract class State
{
    protected StateContext ctx;

    protected State(StateContext ctx)
    {
        this.ctx = ctx;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void LateFixedUpdate();
}

