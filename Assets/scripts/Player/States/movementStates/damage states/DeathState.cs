using UnityEngine;

namespace PlayerController
{
    public class DeathState : MovementBaseState
    {
        public DeathState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override void Enter()
        {
            // Spawn particle effect on death
            if (ctx.Stats.DeathParticleEffect != null)
            {
                GameObject particles = Object.Instantiate(ctx.Stats.DeathParticleEffect, controller.transform.position, Quaternion.identity);
                ParticleSystem ps = particles.GetComponent<ParticleSystem>();
                if (ps != null)
                    Object.Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
                else
                    Object.Destroy(particles, 2f); // fallback
            }
            controller.transform.position = new Vector3(controller.checkpoint.lastCheckpoint.x, controller.checkpoint.lastCheckpoint.y, controller.transform.position.z);
            ctx.speed = Vector3.zero;
            controller.roomManager.ReloadRoom();

            
        }
        public override void Exit() 
        {
            ctx.HP = ctx.Stats.MaxHP;
        }

        public override void Update()
        {
        }

        public override void FixedUpdate()
        {
            controller.QueueMovementState(PlayerMovementStateType.Respawning);
        }

        public override void LateFixedUpdate()
        {
        }
    }
}
