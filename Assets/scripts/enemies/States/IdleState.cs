using UnityEngine;
using System;

public class IdleState : EnemyState
{
    private float timer;

    private readonly float minDuration;
    private readonly float maxDuration;
    private readonly float nextStateOdds;
    private readonly float gravity;

    private readonly Action ResetVars;

    private readonly String animationState;
    private readonly object nextStateTagA;
    private readonly object nextStateTagB;
    

    private ContactFilter2D groundFilter;
    private readonly Rigidbody2D rb;

    public IdleState(
        EnemyContext ctx,
        IStateRequester requester,
        Rigidbody2D rb,
        float minDuration,
        float maxDuration,
        float nextStateOdds,
        float gravity,
        String animationState,
        Action ResetVars,
        object nextStateTagA,
        object nextStateTagB,
        LayerMask groundMask
    ) : base(ctx, requester)
    {
        this.minDuration = minDuration;
        this.maxDuration = maxDuration;
        this.nextStateOdds = nextStateOdds;

        this.animationState = animationState;
        this.nextStateTagA = nextStateTagA;
        this.nextStateTagB = nextStateTagB;

        this.ResetVars = ResetVars;

        this.groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };

        this.gravity = gravity;
        this.rb = rb;
    }

    public override void Enter()
    {
        ResetVars?.Invoke();
        timer  = UnityEngine.Random.Range(minDuration, maxDuration);
        ctx.speed = new Vector2(0, 0);
        requester.RequestAnimationState(animationState);
    }

    public override void Update()
    {

    }

    public override void Exit() {
        requester.CancelAnimationState(animationState);
    }
    public override void FixedUpdate()
    {
        timer -= Time.deltaTime;
        if (ctx.IsGrounded(rb, groundFilter)){
            ctx.speed.y = 0;
        }
        else{
            ctx.speed.y -= gravity * Time.deltaTime;
            return;
        }
        if (timer <= 0)
        {
            object nextState = (UnityEngine.Random.value < nextStateOdds) ? nextStateTagA : nextStateTagB;
            requester.RequestState(nextState);
        }

        
    }
    public override void LateFixedUpdate() {}
}
