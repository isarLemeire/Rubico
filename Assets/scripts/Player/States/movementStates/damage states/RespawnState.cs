using UnityEngine;

namespace PlayerController
{
    public class RespawnState : MovementBaseState
    {
        public RespawnState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        float respawnTimer;

        public override void Enter()
        {
            Debug.Log("Respawn");
            controller.animator.SetTrigger("Idle");
            respawnTimer = 0;
        }

        public override void Exit()
        {

        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= ctx.Stats.RespawnTime)
            {
                returnToDefaultState();
                return;
            }
            ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime);
        }

        public override void LateFixedUpdate() { }
    }
}
