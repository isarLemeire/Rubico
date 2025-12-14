using UnityEngine;


namespace PlayerController
{
    public class NeutralState : DamageBaseState
    {
        public NeutralState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
        }
        public override void Exit() 
        { 
        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
            foreach (var hit in ctx.CollisionHandler.Hits)
            {
                if (hit.collider.CompareTag("Spike") && !ctx.Invulnerable)
                {
                    ctx.lastHazardHit = hit;
                    damage();
                    return;
                }
            }
        }

        public override void LateFixedUpdate()
        {
        }

        public void damage()
        {
            ctx.HP--;
            if (ctx.HP <= 0)
            {
                controller.QueueMovementState(PlayerMovementStateType.Death);
            }
            else
            {
                controller.QueueMovementState(PlayerMovementStateType.Hurt);
                controller.QueueDamageState(PlayerDamageStateType.Invulnerable);
            }
        }
    }
}
