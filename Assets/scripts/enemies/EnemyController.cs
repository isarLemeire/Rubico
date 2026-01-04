using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyController<TState> : Controller, IStateRequester, IEnemyController
    where TState : Enum
{
    public Rigidbody2D Rigidbody;

    public EnemyContext enemyCtx;

    public EnemyContext Ctx => enemyCtx;

    protected IStateFactory<TState> stateFactory;
    protected Dictionary<TState, int> statePriority;

    private TState queuedState;
    private bool hasQueuedState;

    private Animator _animator;

    protected override StateContext CreateContext()
    {
        return new EnemyContext();
    }

    protected override void Awake()
    {
        base.Awake();
        enemyCtx = (EnemyContext)ctx;

        enemyCtx.faceRight = false;
        enemyCtx.AnimationBlock = false;

        Rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>(true);
    }

    protected override void FixedUpdate()
    {
        ctx.position = transform.position;
        UpdateQueuedState();
        stateMachine.FixedUpdate();
        ProcessMovement();
    }

    protected override void ProcessMovement()
    {
        Vector3 position = transform.position;
        Rigidbody.MovePosition(position + ctx.speed * Time.deltaTime);
    }

    protected void QueueState(TState newState)
    {
        if (!hasQueuedState || statePriority[newState] > statePriority[queuedState])
        {
            queuedState = newState;
            hasQueuedState = true;
        }
    }

    private void UpdateQueuedState()
    {
        if (!hasQueuedState)
            return;

        stateMachine.ChangeState(stateFactory.GetState(queuedState));
        hasQueuedState = false;
    }

    public void RequestState(object stateTag)
    {
        if (stateTag is TState typed)
            QueueState(typed);
    }

    public void RequestAnimationState(String triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    public void CancelAnimationState(String triggerName)
    {
        _animator.ResetTrigger(triggerName);
    }

    public void RequestAnimationBool(bool value, String boolName)
    {
        _animator.SetBool(boolName, value);
    }
}


public interface IEnemyController
{
    EnemyContext Ctx { get; }
}