using UnityEngine;

namespace Player
{
    public class DeathState : MovementBaseState
    {
        public DeathState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        private float _timer;

        public override void Enter()
        {
            // Spawn particle effect on death
            Debug.Log("Death");
            _timer = ctx.Stats.DeathTime;
            ctx.movement_invulnerable = true;
            ctx.DeathInputBlock = true;

            getKnockback();
            controller.animator.SetTrigger("Death");
            controller.animator.SetLayerWeight(1, 0f);
            controller.QueueAttackState(PlayerAttackStateType.NonAttacking);
        }
        public override void Exit() 
        {
            controller.transform.position = new Vector3(controller.checkpoint.lastCheckpoint.x, controller.checkpoint.lastCheckpoint.y, controller.transform.position.z);
            ctx.speed = Vector3.zero;
            controller.roomManager.ReloadRoom();            
        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {
            ctx.speed.y = Mathf.MoveTowards(ctx.speed.y, 0, Mathf.Abs(ctx.speed.y) * ctx.Stats.DeathDeceleration * Time.deltaTime);
            ctx.speed.x = Mathf.MoveTowards(ctx.speed.x, 0, Mathf.Abs(ctx.speed.x) * ctx.Stats.DeathDeceleration * Time.deltaTime);

            
            _timer -= Time.deltaTime;
            if (_timer <= 0)
                controller.QueueMovementState(PlayerMovementStateType.Respawning);
            
        }

        public override void LateFixedUpdate()
        {
        }

        public void getKnockback()
        {
            Vector3 oldSpeed = ctx.speed;
            Vector3 normal = ctx.lastHazardHit.normal;

            // --- NEW LOGIC: Create an asymmetrical knockback vector ---

            // 1. Define the asymmetrical knockback vector (e.g., more horizontal or vertical).
            // This is the desired magnitude of the knockback in world-space axes.
            Vector3 asymmetricalKnockbackVector = new Vector3(
                ctx.Stats.DamageKnockbackSpeedX,
                ctx.Stats.DamageKnockbackSpeedY,
                0f // Assuming 2D/XY plane
            );

            // 2. Determine the final knockback magnitude by projecting the asymmetrical vector 
            //    onto the normal. This ensures the magnitude is scaled correctly 
            //    based on the direction of impact.
            // The Abs() ensures the resulting speed is always positive (knockback away from the hazard).
            float knockbackMagnitude = Mathf.Abs(Vector3.Dot(asymmetricalKnockbackVector, normal));

            // --- REMAINDER OF ORIGINAL LOGIC ---

            // Ensure the normal is normalized (unit length), though RaycastHit2D.normal usually is.
            normal.Normalize();

            // 1. Calculate the magnitude of the old speed component parallel to the normal (v_old . n).
            float projectionMagnitude = Vector3.Dot(oldSpeed, normal);

            // 2. Calculate the parallel component vector (v_parallel).
            Vector3 v_parallel = normal * projectionMagnitude;

            // 3. Calculate the perpendicular/tangential component vector (v_perp).
            Vector3 v_perp = oldSpeed - v_parallel;

            // 4. Calculate the new parallel component (the knockback velocity).
            // Use the calculated knockbackMagnitude from the new logic.
            Vector3 v_knockback = normal * knockbackMagnitude;

            // 5. Combine the tangential component and the new knockback component.
            ctx.speed = v_perp + v_knockback;
        }
    }
}
