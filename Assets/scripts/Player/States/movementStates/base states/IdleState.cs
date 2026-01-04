using UnityEngine;

namespace Player
{
    public class IdleState : MovementBaseState
    {
        public IdleState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            Debug.Log("Idle");
            controller.animator.SetBool("Idle", true);
        }
        public override void Exit() 
        {
            controller.animator.SetBool("Idle", false);
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
