using UnityEngine;
using System;

public class AnimateState : EnemyState
{
    private float timer;
    private readonly float duration;
    private readonly float gravity;
    private readonly object nextStateTag;
    private readonly String animationState;

    private ContactFilter2D groundFilter;
    private readonly Rigidbody2D rb;

    public AnimateState(
        EnemyContext ctx,
        IStateRequester requester,
        Rigidbody2D rb,
        float gravity,
        float duration,
        LayerMask groundMask,
        String animationState,
        object nextStateTag
    ) : base(ctx, requester)
    {
        this.duration = duration;
        this.nextStateTag = nextStateTag;
        this.animationState = animationState;

        this.groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };

        this.rb = rb;
        this.gravity = gravity;
    }

    public override void Enter()
    {
        timer = 0f;
        ctx.speed = new Vector2(0, 0);
        requester.RequestAnimationState(animationState);

    }

    public override void Update()
    {

    }

    public override void Exit() {
        requester.CancelAnimationState(animationState);
    }
    public override void FixedUpdate() {
        timer += Time.deltaTime;

        if (ctx.IsGrounded(rb, groundFilter)){
            ctx.speed.y = 0;
        }
        else{
            ctx.speed.y -= gravity * Time.deltaTime;
        }
        
        if (timer >= duration)
        {
            requester.RequestState(nextStateTag);
        }
    }
    public override void LateFixedUpdate() {}
}
