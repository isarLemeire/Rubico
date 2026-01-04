using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;


public abstract class Controller : MonoBehaviour
{
    protected StateContext ctx;
    protected StateMachine stateMachine;

    protected virtual StateContext CreateContext()
    {
        return new StateContext();
    }

    protected virtual void Awake()
    {
        ctx = CreateContext();
        stateMachine = new StateMachine(ctx);
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.FixedUpdate();
        ProcessMovement();
    }

    protected abstract void ProcessMovement();
}

