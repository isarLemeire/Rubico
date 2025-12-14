using UnityEngine;

namespace PlayerController
{
    public class KnockBackState : MovementBaseState
    {
        public KnockBackState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
                    : base(controller, ctx, inputHandler) { }

        private float knockBackTimer;

        public override void Enter()
        {
            Debug.Log("Knock back");
            ctx.canKnockBack = false;
            ctx.speed.x = -1f * Mathf.Sign(ctx.attackAim.x) * ctx.Stats.KnockBackSpeed;
            knockBackTimer = 0;
        }
        public override void Exit()
        {
            ctx.dashCoolDownTimer = ctx.Stats.KnockBackTime;
        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {
            knockBackTimer += Time.deltaTime;
            if (knockBackTimer >= ctx.Stats.KnockBackTime)
            {
                returnToDefaultState();
            }
            
            if(ctx.grounded.IsActive)
                UpdateMoveSpeed(
                    ref ctx.speed.x,
                    inputHandler.MoveX,
                    ctx.Stats.MaxMovementSpeed,
                    ctx.Stats.KnockNoInputDeceleration,
                    ctx.Stats.KnockTurnAroundDeceleration,
                    ctx.Stats.KnockNoInputDeceleration,
                    ctx.Stats.KnockNoInputDeceleration,
                    ctx.Stats.KnockTurnAroundDeceleration,
                    ctx.Stats.KnockNoInputDeceleration,
                    Time.deltaTime);
            else
                UpdateMoveSpeed(
                    ref ctx.speed.x,
                    inputHandler.MoveX,
                    ctx.Stats.MaxMovementSpeed,
                    ctx.Stats.KnockAirNoInputDeceleration,
                    ctx.Stats.KnockAirTurnAroundDeceleration,
                    ctx.Stats.KnockAirNoInputDeceleration,
                    ctx.Stats.KnockAirNoInputDeceleration,
                    ctx.Stats.KnockAirTurnAroundDeceleration,
                    ctx.Stats.KnockAirNoInputDeceleration,
                    Time.deltaTime);


        }

        public override void LateFixedUpdate()
        {
            HandleCollision();
        }
    }
}
