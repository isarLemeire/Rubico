using UnityEngine;


public class StateMachine
{
    private State current;
    private StateContext ctx;
    
    public StateMachine(StateContext ctx)
    {
        this.ctx = ctx;
    }


    public void Update()
    {
        current?.Update();
    }


    public void FixedUpdate()
    {
        current?.FixedUpdate();
    }

    public void LateFixedUpdate()
    {
        current?.LateFixedUpdate();
    }


    public void ChangeState(State newState)
    {
        current?.Exit();
        current = newState;
        current?.Enter();
    }
}
