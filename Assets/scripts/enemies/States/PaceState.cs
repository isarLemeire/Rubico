using UnityEngine;
using System;

public class PaceState : EnemyState
{
    private float timer;
    private readonly float stoppingTime;

    private readonly float minDuration;
    private readonly float maxDuration;
    private readonly float maxDistance;

    private readonly object nextStateTag;

    private readonly float baseSpeed;
    private readonly float acceleration;

    private readonly float forwardGroundCheckDistanceDistance;
    private readonly float downwardGroundCheckDistance;
    private readonly float wallCheckDistance;   


    private readonly LayerMask groundMask;
    private readonly LayerMask dangerMask;

    private String animationState;

    private Vector2 initialPosition;

    public PaceState(
        EnemyContext ctx,
        IStateRequester requester,
        Vector2 initialPosition,
        float minDuration,
        float maxDuration,
        float maxDistance,
        float speed,
        float acceleration,
        float forwardGroundCheckDistanceDistance,
        float downwardGroundCheckDistance,  
        float wallCheckDistance,
        LayerMask groundMask,
        LayerMask dangerMask,
        String animationState,
        object nextStateTag
    ) : base(ctx, requester)
    {
        this.initialPosition = initialPosition;

        this.minDuration = minDuration;
        this.maxDuration = maxDuration;
        this.maxDistance = maxDistance;

        this.baseSpeed = speed;
        this.acceleration = acceleration;
        this.nextStateTag = nextStateTag;
        this.groundMask = groundMask;
        this.dangerMask = dangerMask;

        this.animationState = animationState;

        this.forwardGroundCheckDistanceDistance = forwardGroundCheckDistanceDistance;
        this.downwardGroundCheckDistance = downwardGroundCheckDistance;
        this.wallCheckDistance = wallCheckDistance;

        this.stoppingTime = speed / acceleration;
    }

    public override void Enter()
    {
        if (!TryChoosePace(ctx.position, out float dir, out float moveTime))
        {
            requester.RequestState(nextStateTag);
            return;
        }

        timer = moveTime + stoppingTime;
        ctx.speed.x = dir * baseSpeed;

        requester.RequestAnimationState(animationState);
    }

    public override void Update()
    {

    }

    public override void Exit() {
        requester.CancelAnimationState(animationState);
    }

    private bool CanMove(Vector2 origin, float dir)
    {
        return ctx.CanSafeMove(
            origin,
            dir,
            forwardGroundCheckDistanceDistance,
            downwardGroundCheckDistance,
            wallCheckDistance,
            groundMask,
            dangerMask
        );
    }

    public override void FixedUpdate()
    {
        timer -= Time.deltaTime;

        Vector2 pos = ctx.position;
        float dir = Mathf.Sign(ctx.speed.x);
        
        bool groundAhead = CanMove(pos, dir);

        if (!groundAhead)
        {
            requester.RequestState(nextStateTag);
            return;
        }

        if (timer <= 0f)
        {
            requester.RequestState(nextStateTag);
            return;
        }
        if (timer <= stoppingTime)
            ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, 0f, acceleration * Time.deltaTime);
        else
            ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, dir * baseSpeed, acceleration * Time.deltaTime);
    }

    public override void LateFixedUpdate() {}

    private bool TryChoosePace(
        Vector2 position,
        out float dir,
        out float moveTime
    )
    {
        float x  = position.x;
        float x0 = initialPosition.x;

        float dist     = x - x0;
        float absDist  = Mathf.Abs(dist);

        dir = 0f;
        moveTime = 0f;

        // ---------- Direction ----------
        if (absDist > maxDistance)
        {
            float dirToCenter = -Mathf.Sign(dist);

            dir = CanMove(position, dirToCenter)
                ? dirToCenter
                : -dirToCenter;
        }
        else
        {
            float roomLeft  = x - (x0 - maxDistance);
            float roomRight = (x0 + maxDistance) - x;

            bool canLeft  = roomLeft  > 0f && CanMove(position, -1f);
            bool canRight = roomRight > 0f && CanMove(position,  1f);

            if (canLeft && canRight)
                dir = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            else if (canLeft)
                dir = -1f;
            else if (canRight)
                dir = 1f;
            else
                return false; // nowhere safe
        }

        // ---------- Duration ----------
        moveTime = UnityEngine.Random.Range(minDuration, maxDuration);

        if (absDist <= maxDistance)
        {
            float room = dir < 0f
                ? x - (x0 - maxDistance)
                : (x0 + maxDistance) - x;

            moveTime = Mathf.Min(moveTime, room / baseSpeed);
        }

        return moveTime > 0f;
    }

}
