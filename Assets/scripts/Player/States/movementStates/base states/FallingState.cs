using UnityEngine;

namespace PlayerController
{
    public class FallingState : MovementBaseState
    {

        public FallingState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }
        public override void Enter()
        {
            Debug.Log("Falling");
        }

        public override void Exit() 
        {
            controller.animator.ResetTrigger("Fall");
        }

        public override void Update()
        {
            checkJump();
            checkDash();
            checkPogo();
            checkKnockBack();
        }

        public override void FixedUpdate() 
        {
            ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime, inputHandler.JoyStickAim.y < -0.1);

            if (ctx.speed.y <= 0)
                controller.animator.SetTrigger("Fall");

            // horizontal move
            UpdateMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                ctx.Stats.MaxMovementSpeed,
                ctx.Stats.DashDecelerationNoInput,
                ctx.Stats.DashTurnAround,
                ctx.Stats.DashDeceleration,
                ctx.Stats.AirDeceleration,
                ctx.Stats.AirTurnAroundAcceleration,
                ctx.Stats.AirAcceleration,
                Time.deltaTime);
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
