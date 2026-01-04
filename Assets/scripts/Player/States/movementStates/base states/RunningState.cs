using UnityEngine;

namespace Player
{
    public class RunningState : MovementBaseState
    {
        public RunningState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            Debug.Log("Running");
            controller.animator.SetBool("Run", true);
        }
        public override void Exit() 
        {
            controller.animator.SetBool("Run", false);
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
