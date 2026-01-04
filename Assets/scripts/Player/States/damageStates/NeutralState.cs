using UnityEngine;
using System.Collections;
using System.Collections.Generic;   


namespace Player
{
    public class NeutralState : DamageBaseState
    {
        public NeutralState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            controller.UIanimator.SetInteger("HP", ctx.HP);
        }
        public override void Exit() 
        { 
        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            hits.AddRange(ctx.CollisionHandler.EnemyHits);
            hits.AddRange(ctx.CollisionHandler.HazardHits);
            
            foreach (var hit in hits)
            {
                if (!ctx.movement_invulnerable)
                {
                    ctx.lastHazardHit = hit;
                    Damage();
                    return;
                }
            }
            if (ctx.heal == ctx.Stats.MaxHeal && ctx.HP < ctx.Stats.MaxHP){
                ctx.heal = 0;
                ctx.HP++;
                controller.UIanimator.SetInteger("HP", ctx.HP);
                controller.UIanimator.SetInteger("Heal", ctx.heal);
            }
        }

        public override void LateFixedUpdate()
        {
        }

        public void Damage()
        {
            ctx.HP--;
            ctx.heal = 0;
            controller.UIanimator.SetInteger("Heal", ctx.heal);
            if (ctx.HP <= 0)
            {
                Respawn();
            }
            else
            {
                controller.QueueMovementState(PlayerMovementStateType.Hurt);
                controller.QueueDamageState(PlayerDamageStateType.Invulnerable);
                controller.UIanimator.SetInteger("HP", ctx.HP);
            }
            controller.animator.SetTrigger("Hurt");
            CameraController.Instance.Shake(ctx.JuiceStats.HitShakeIntensity, ctx.JuiceStats.HitShakeDuration, ctx.JuiceStats.ShakeFrequency);
            GameFreezeManager.Instance.Freeze(ctx.JuiceStats.HitFreeze);
        }

        public void Respawn()
        {
            controller.UIanimator.SetInteger("HP", 0);
            controller.QueueMovementState(PlayerMovementStateType.Death);
        }
    }
}
