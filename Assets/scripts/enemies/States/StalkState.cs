using UnityEngine;
using System;

public class StalkState : EnemyState
{
    private readonly object knockbackStateTag;
    private readonly object returnStateTag;

    private readonly float baseSpeed;
    private readonly float acceleration;
    private readonly float detectRange;
    private readonly float minimumRange;
    private readonly float detectAngle;

    private bool stuck;

    private readonly float forwardGroundCheckDistanceDistance;
    private readonly float downwardGroundCheckDistance;
    private readonly float wallCheckDistance;

    private readonly LayerMask groundMask;
    private readonly LayerMask playerMask;
    private readonly LayerMask dangerMask;

    private readonly Transform player;
    private Vector2 playerLastSeenPosition;

    private readonly float forgetDuration;
    private float timeSinceLastSeen;

    private readonly String animationState;
    private readonly String animationSeePlayerBool; 

    private readonly float gravity;
    private ContactFilter2D groundFilter;
    private ContactFilter2D playerFilter;
    private readonly Rigidbody2D rb;

    private readonly GameObject hitbox;

    private readonly Player.PlayerController playerController;

    public StalkState(
        EnemyContext ctx,
        IStateRequester requester,
        Rigidbody2D rb,
        float speed,
        float acceleration,
        float gravity,
        float detectRange,
        float minimumRange,
        float detectAngle,
        float forgetDuration,
        float forwardGroundCheckDistanceDistance,
        float downwardGroundCheckDistance,
        float wallCheckDistance,
        LayerMask groundMask,
        LayerMask playerMask,
        LayerMask dangerMask,
        Transform player,
        GameObject hitbox,
        String animationState,
        String animationSeePlayerBool,
        Player.PlayerController playerController,
        object KnockBackStateTag,
        object returnStateTag
    ) : base(ctx, requester)
    {
        this.baseSpeed = speed;
        this.acceleration = acceleration;

        this.knockbackStateTag = KnockBackStateTag;
        this.animationState = animationState;
        this.animationSeePlayerBool = animationSeePlayerBool;

        this.groundMask = groundMask;
        this.playerMask = playerMask;
        this.dangerMask = dangerMask;

        this.detectRange = detectRange;
        this.minimumRange = minimumRange;
        this.detectAngle = detectAngle;

        this.forgetDuration = forgetDuration;
        this.returnStateTag = returnStateTag;
        this.playerLastSeenPosition = Vector2.zero;

        this.forwardGroundCheckDistanceDistance = forwardGroundCheckDistanceDistance;
        this.downwardGroundCheckDistance = downwardGroundCheckDistance;
        this.wallCheckDistance = wallCheckDistance;

        this.groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = groundMask,
            useTriggers = false
        };

        this.playerFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = playerMask,
            useTriggers = true
        };

        this.gravity = gravity;
        this.rb = rb;

        this.player = player;
        this.playerController = playerController;
        this.hitbox = hitbox;
    }

    public override void Enter()
    {
        requester.RequestAnimationState(animationState);
        stuck = false;
        hitbox.SetActive(true);

        timeSinceLastSeen = 0f;
    }

    public override void Update()
    {

    }

    public override void Exit() {
        requester.CancelAnimationState(animationState);
    }

    private bool HasGround(Vector2 origin, float dir)
    {
        float downwardGroundCheckDistance = 1f;
        float forwardCheck = 1f;
        Vector2 probeOrigin = origin + Vector2.right * dir * forwardCheck;
        return Physics2D.Raycast(
            probeOrigin,
            Vector2.down,
            downwardGroundCheckDistance,
            groundMask
        );
    }

    public override void FixedUpdate()
    {
        // --- Handle vertical movement / gravity ---
        if (ctx.IsGrounded(rb, groundFilter))
        {
            ctx.speed.y = 0f;
        }
        else
        {
            ctx.speed.y -= gravity * Time.deltaTime;
            return;
        }

        if (CanSeePlayer())
        {
            timeSinceLastSeen = 0f;
            playerLastSeenPosition = player.position;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen >= forgetDuration)
            {
                requester.RequestState(returnStateTag);
                return;
            }
        }



        if (player == null) 
            return;

        Vector2 toPlayer = playerLastSeenPosition - ctx.position;
        float distance = toPlayer.magnitude;

        if (playerController.ctx.invulnerable){
            Stuck();
            return;
        }

        if(ctx.faceRight != ctx.moveRight){
            requester.RequestAnimationState("Turn");
        }

        // --- Check if player is within range ---
        if (stuck && Mathf.Abs(toPlayer.x) < minimumRange)
        {
            Stuck();
            return;
        }


        if (distance > detectRange || distance < minimumRange)
        {
            Stuck();
            return;
        }

        float angle = Mathf.Abs(Vector2.Angle(toPlayer, Vector2.right * Mathf.Sign(toPlayer.x)));
        if (angle > detectAngle)
        {
            Stuck();
            return;
        }

        float dir = ctx.moveRight ? 1f : -1f;
        bool groundAhead = CanMove(ctx.position, dir);

        if (!groundAhead)
        {
            Stuck();
            return;
        }

        // --- Move toward player ---
        
        if (ctx.AnimationBlock){
            ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, 0f, acceleration * Time.deltaTime);
        }
        else{
            float targetSpeed = ctx.moveRight ? baseSpeed : -baseSpeed;
            ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, targetSpeed, acceleration * Time.deltaTime);
        }

        

        requester.RequestAnimationBool(true, animationSeePlayerBool);
        stuck = false;
    }

    private void Stuck(){
        stuck = true;
        ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, 0f, acceleration * Time.deltaTime);
        requester.RequestAnimationBool(false, animationSeePlayerBool);
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


    public override void LateFixedUpdate() {}


    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector2 origin = ctx.position;
        Vector2 target = player.position;

        Vector2 dir = target - origin;
        float distance = dir.magnitude;

        if (distance <= Mathf.Epsilon)
            return true;

        dir /= distance; // normalize

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            dir,
            distance,
            groundMask | dangerMask | playerMask
        );

        if (!hit)
            return true;

        // Seen only if the first thing hit is the player
        return ((1 << hit.collider.gameObject.layer) & playerMask) != 0;
    }

}
