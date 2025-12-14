using UnityEngine;

namespace PlayerController
{
    public class RunningState : MovementBaseState
    {
        public RunningState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            Debug.Log("Running");
            controller.animator.SetTrigger("Run");
        }
        public override void Exit() 
        {
            controller.animator.ResetTrigger("Run");
        }

        public override void Update() 
        {
            checkIdle();
            checkJump();
            checkDash();
            checkKnockBack();
        }

        public override void FixedUpdate()
        {
            // horizontal move
            UpdateMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                ctx.Stats.MaxMovementSpeed,
                ctx.Stats.DashDecelerationNoInput,
                ctx.Stats.DashTurnAround,
                ctx.Stats.DashDeceleration,
                ctx.Stats.GroundDeceleration,
                ctx.Stats.GroundTurnAroundAcceleration,
                ctx.Stats.GroundAcceleration,
                Time.deltaTime);
        }

        public override void LateFixedUpdate() 
        {
            HandleCollision();
            checkFall();
        }
    }
}
