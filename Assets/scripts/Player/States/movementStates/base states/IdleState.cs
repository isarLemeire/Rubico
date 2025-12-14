using UnityEngine;

namespace PlayerController
{
    public class IdleState : MovementBaseState
    {
        public IdleState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            Debug.Log("Idle");
            controller.animator.SetTrigger("Idle");
        }
        public override void Exit() 
        {
            controller.animator.ResetTrigger("Idle");
        }

        public override void Update()
        {
            checkRun();
            checkJump();
            checkDash();
            checkKnockBack();
        }

        public override void FixedUpdate()
        {
            UpdateSimpleMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                0,
                ctx.Stats.GroundDeceleration,
                ctx.Stats.GroundDeceleration,
                ctx.Stats.GroundTurnAroundAcceleration,
                ctx.Stats.GroundAcceleration,
                Time.deltaTime
                );

            ctx.speed.y -= 0.1f;
        }

        public override void LateFixedUpdate()
        {
            HandleCollision();
            checkFall();
        }
    }
}
