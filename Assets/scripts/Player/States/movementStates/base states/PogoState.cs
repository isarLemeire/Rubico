using UnityEngine;

namespace Player
{
    public class PogoState : MovementBaseState
    {
        public PogoState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        private bool faceRight;

        public override void Enter()
        {
            
            Debug.Log("Pogo");
            ctx.canDash = true;
            ctx.canEngage = true;

            // vertical
            if (ctx.attackAim.y <= -0.9)
            {
                ctx.speed.y = ctx.Stats.PogoSpeed;
                ctx.speed.x = 0f;
            }
            else
            {
                // diagonal
                if (Mathf.Abs(Mathf.Abs(ctx.attackAim.y) - Mathf.Abs(ctx.attackAim.x)) <= 0.1)
                {
                    ctx.speed.y = ctx.Stats.DiagonalPogoSpeed;
                    ctx.speed.x = ctx.Stats.MaxMovementSpeed * Mathf.Sign(ctx.attackAim.x);
                }
                // horizontal
                else
                {
                    ctx.speed.y = ctx.Stats.HorizonalPogoSpeed;
                    ctx.speed.x = - Mathf.Sign(ctx.attackAim.x) * ctx.Stats.MaxMovementSpeed;
                    //horizonalPogo = true;
                }
            }
            ctx.canKnockBack = false;
        }
        public override void Exit() 
        {
            controller.animator.SetBool("Fall", false);
        }

        public override void Update()
        {
            checkDash();
            checkPogo();
        }

        public override void FixedUpdate()
        {
            ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime);

            UpdateSimpleMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                ctx.Stats.MaxMovementSpeed,
                ctx.Stats.PogoDeceleration,
                ctx.Stats.PogoDeceleration,
                ctx.Stats.PogoTurnAroundDeceleration,
                ctx.Stats.AirAcceleration,
                Time.deltaTime);

            if (ctx.speed.y <= 0)
                controller.animator.SetBool("Fall", true);
        }

        public override void LateFixedUpdate()
        {
            HandleCollision();
            if (ctx.CollisionHandler.grounded)
            {
                returnToDefaultState();
            }
        }
    }
}
