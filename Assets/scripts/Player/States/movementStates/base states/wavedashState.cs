using UnityEngine;

namespace PlayerController
{
    public class WavedashState : MovementBaseState
    {

        public WavedashState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            controller.animator.SetTrigger("WaveDash");
            Debug.Log("WaveDash");
            ctx.speed.y = ctx.Stats.JumpSpeed;
            ctx.speed.x = Mathf.Sign(ctx.speed.x) * ctx.Stats.WavedashSpeed;
            inputHandler.JumpPressed.Clear();
            ctx.canDash = true;
        }
        public override void Exit() { }

        public override void Update()
        {
            checkDash();
            checkPogo();
        }

        public override void FixedUpdate()
        {
            ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime);

            // horizontal move
            UpdateSimpleMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                ctx.Stats.MaxMovementSpeed,
                ctx.Stats.WavedashDeceleration,
                ctx.Stats.WavedashNoInputDeceleration,
                ctx.Stats.WavedashTurnAroundDeceleration,
                ctx.Stats.AirAcceleration,
                Time.deltaTime);
        }

        public override void LateFixedUpdate()
        {
            HandleCollision();
            if (ctx.grounded.IsTrue)
                returnToDefaultGroundState();
        }
    }
}
