using UnityEngine;

namespace Player
{
    public class RespawnState : MovementBaseState
    {
        public RespawnState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        float respawnTimer;

        public override void Enter()
        {
            Debug.Log("Respawn");
            respawnTimer = 0;

            
            ctx.HP = ctx.Stats.MaxHP;
            controller.UIanimator.SetInteger("HP", ctx.HP);
            controller.animator.SetTrigger("Respawn");
        }

        public override void Exit()
        {
            ctx.DeathInputBlock = false;
            ctx.movement_invulnerable = false;
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
