using UnityEngine;

namespace Player
{
    public class JumpingState : MovementBaseState
    {

        public JumpingState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            Debug.Log("Jump");
            controller.animator.SetTrigger("Jump");
            ctx.speed.y = ctx.Stats.JumpSpeed;
            inputHandler.JumpPressed.Clear();
            ctx.grounded.Set(false);
        }
        public override void Exit() 
        {
            controller.animator.ResetTrigger("Jump");
        }

        public override void Update()
        {
            checkDash();
            checkPogo();
            checkKnockBack();
        }

        public override void FixedUpdate()
        {
            // Early stop
            if (!inputHandler.JumpHeld && ctx.speed.y > 0)
            {
                float newSpeed = ctx.speed.y - ctx.Stats.EarlyReleaseFloatGravity * Time.deltaTime;
                ctx.speed.y = Mathf.Min(newSpeed, ctx.Stats.EarlyReleaseEndVelocity);
            }
            else
            {
                ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime);
            }
            

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

            if (ctx.speed.y <= 0)
                controller.QueueMovementState(PlayerMovementStateType.Falling);
        }

        public override void LateFixedUpdate()
        {
            HandleCollision();
            if (ctx.grounded.IsTrue)
                returnToDefaultGroundState();
        }
    }
}
