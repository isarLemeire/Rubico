using UnityEngine;
using System;

public class HurtState : EnemyState
{
    private readonly float hurtDuration;
    private readonly float bounceReduction;
    private readonly int maxBounces;
    private int bounces;
    private readonly float hitAngle;
    private readonly float gravity;

    private readonly String animationState;
    private readonly LayerMask groundMask;
    private ContactFilter2D groundFilter;
    private readonly Rigidbody2D rb;
    private readonly object nextStateTag;

    private readonly GameObject hitbox;

    private float timer;
    private float velocity;

    public HurtState(
        EnemyContext ctx,
        IStateRequester requester,
        Rigidbody2D rb,
        float initialVelocity,
        float gravity,
        float hurtDuration,
        float bounceReduction,
        float hitAngle,
        int maxBounces,
        LayerMask groundMask,
        GameObject hitbox,
        String animationState,
        object nextStateTag
    ) : base(ctx, requester)
    {
        this.rb = rb;
        this.velocity = initialVelocity;
        this.gravity = gravity;
        this.hurtDuration = hurtDuration;
        this.bounceReduction = bounceReduction;
        this.hitAngle = hitAngle;
        this.maxBounces = maxBounces;
        this.groundMask = groundMask;
        this.nextStateTag = nextStateTag;

        this.hitbox = hitbox;

        this.animationState = animationState;
        this.groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };
    }

    public override void Enter()
    {
        timer = hurtDuration;
        setSpeed();
        requester.RequestAnimationState(animationState);
        bounces = 0;

        hitbox.SetActive(false);
    }

    public override void Update()
    {
        
    }

    public void setSpeed(){
        Vector2 dir = ctx.ReceiveHitAim.normalized;

        float minAngleRad = hitAngle * Mathf.Deg2Rad;

        if (Mathf.Abs(dir.y) < 0.1f)
        {
            float signX = Mathf.Sign(dir.x);
            dir = new Vector2(
                signX * Mathf.Cos(minAngleRad),
                Mathf.Sin(minAngleRad)
            );
        }

        ctx.speed = dir * velocity;
    }

    public override void FixedUpdate()
    {
        timer -= Time.deltaTime;
        ctx.speed.y -= gravity * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + (Vector2)ctx.speed * Time.fixedDeltaTime);

        ctx.speed = ResolveVelocityFromContacts(
            rb,
            groundFilter,
            ctx.speed,
            bounceReduction
        );

        if (ctx.IsGrounded(rb, groundFilter))
        {
            if (timer <= 0f || bounces >= maxBounces){
                ctx.speed = Vector2.zero;
                requester.RequestState(nextStateTag);
            }
        }
    }

    public override void Exit() {
        requester.CancelAnimationState(animationState);
    }

    public override void LateFixedUpdate() {}

    // ---------- Helpers ----------

    public Vector2 ResolveVelocityFromContacts(
        Rigidbody2D rb,
        ContactFilter2D filter,
        Vector2 velocity,
        float restitution,
        float minNormalDot = 0.05f
    )
    {
        ContactPoint2D[] contacts = new ContactPoint2D[2];
        int count = rb.GetContacts(filter, contacts);

        for (int i = 0; i < count; i++)
        {
            Vector2 n = contacts[i].normal;

            if (Mathf.Abs(n.x) > Mathf.Abs(n.y))
                n = new Vector2(Mathf.Sign(n.x), 0f);   // left / right
            else
                n = new Vector2(0f, Mathf.Sign(n.y));  // up / down

            // Ignore near-parallel contacts
            float vn = Vector2.Dot(velocity, n);
            if (vn >= -minNormalDot)
                continue;

            // Decompose velocity
            Vector2 vNormal = vn * n;
            Vector2 vTangent = velocity - vNormal;

            // Reflect normal component
            velocity = restitution * (vTangent - vNormal);
            bounces += 1;
        }

        return  velocity;
    }
}