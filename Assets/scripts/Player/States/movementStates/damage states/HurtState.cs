using UnityEngine;

namespace PlayerController
{
    public class HurtState : MovementBaseState
    {
        public HurtState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
                : base(controller, ctx, inputHandler) { }

        private float stateTimer;

        public override void Enter()
        {
            Debug.Log("Hurt");
            stateTimer = ctx.Stats.DamagedTime;
            getKnockback();

            ctx.canDash = true;
            ctx.canEngage = true;
            ctx.dashCoolDownTimer = 0f;
            ctx.Invulnerable = true;
        }

        public override void Exit()
        {
            ctx.Invulnerable = false;
        }

        public override void Update()
        {
            
        }

        public override void FixedUpdate()
        {
            ctx.speed.y = ApplyGravity(ctx.speed.y, Time.deltaTime);


            UpdateSimpleMoveSpeed(
                ref ctx.speed.x,
                inputHandler.MoveX,
                ctx.Stats.MaxMovementSpeed,
                ctx.Stats.DamageDeceleration,
                ctx.Stats.DamageDeceleration,
                ctx.Stats.DamageTurnAroundDeceleration,
                ctx.Stats.AirAcceleration,
                Time.deltaTime);

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
                returnToDefaultState();
        }

        public override void LateFixedUpdate()
        {

        }

        // Assuming this method is part of a class that has access to 'ctx'
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