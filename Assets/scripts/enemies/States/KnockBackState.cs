using UnityEngine;

public class KnockBackState : EnemyState
{
    private float timer;
    private readonly float duration;
    private readonly float initialVelocity;
    private readonly float deceleration;
    private readonly float gravity;
    private readonly object nextStateTag;
    private ContactFilter2D groundFilter;

    private readonly Rigidbody2D rb;

    public KnockBackState(
        EnemyContext ctx,
        IStateRequester requester,
        Rigidbody2D rb,
        float duration,
        float initialVelocity,
        float deceleration,
        float gravity,
        LayerMask groundMask,
        object nextStateTag
    ) : base(ctx, requester)
    {
        this.duration = duration;
        this.initialVelocity = initialVelocity;
        this.deceleration = deceleration;
        this.gravity = gravity;
        this.rb = rb;
        this.groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };
        this.nextStateTag = nextStateTag;
    }

    public override void Enter()
    {
        timer = 0f;
        ctx.speed = new Vector2(initialVelocity * -Mathf.Sign(ctx.speed.x), 0);
    }

    public override void Update()
    {

    }

    public override void Exit() {
    }

    public override void FixedUpdate() {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            requester.RequestState(nextStateTag);
            return;
        }

        if(ctx.IsGrounded(rb, groundFilter))
        {
            ctx.speed.y = 0;
        }
        else
        {
            ctx.speed.y -= gravity * Time.deltaTime;
        }

        // Apply deceleration
        float decelAmount = deceleration * Time.deltaTime;
        ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, 0, decelAmount);
        rb.linearVelocity = new Vector2(ctx.speed.x, rb.linearVelocity.y);
    }

    public override void LateFixedUpdate() {}
}