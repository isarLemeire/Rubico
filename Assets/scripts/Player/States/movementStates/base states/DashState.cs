using UnityEngine;

namespace Player
{
    public class DashState : MovementBaseState
    {
        public DashState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
            : base(controller, ctx, inputHandler) { }

        private float dashTimer;
        private bool wavedash;

        public override void Enter() 
        {
            Debug.Log("Dash");

            inputHandler.DashPressed.Clear();
            ctx.canDash = false;

            controller.QueueAttackState(PlayerAttackStateType.NonAttacking);
            ctx.canAttack = false;

            ctx.dashAim = inputHandler.JoyStickAim;


            ctx.speed = ScaleDash(ctx.dashAim, ctx.Stats.DashSpeedX, ctx.Stats.DashSpeedUp, ctx.Stats.DashSpeedDown);

            dashTimer = ctx.Stats.DashTime;
            wavedash = false;

            CameraController.Instance.ShakeDirectional(ctx.dashAim, ctx.JuiceStats.DashShakeIntensity, ctx.JuiceStats.DashShakeDuration, ctx.JuiceStats.ShakeFrequency);
            GameFreezeManager.Instance.Freeze(ctx.JuiceStats.DashFreeze);

            if (ctx.grounded.IsActive && ctx.dashAim.y < 0.1)
                controller.animator.SetTrigger("GroundDash");
            else
                controller.animator.SetTrigger("Dash");

            controller.shockWaveController.callShockWave(controller.transform.position, ctx.dashAim);
        }

        public override void Exit() 
        {
            ctx.dashCoolDownTimer = ctx.Stats.DashCooldown;
            ctx.dashBuffer.Set(true);
            ctx.canAttack = true;
            if (wavedash)
                controller.QueueMovementState(PlayerMovementStateType.Wavedashing);
        }

        public override void Update()
        {
            if (inputHandler.JumpPressed.IsActive && ctx.grounded.IsActive)
                wavedash = true;
        }

        public override void FixedUpdate()
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                returnToDefaultState();
        }

        public override void LateFixedUpdate()
        {
            ctx.grounded.Set(ctx.CollisionHandler.grounded);
        }

        private Vector2 ScaleDash(Vector3 aim, float speedX, float speedUp, float speedDown)
        {
            Vector2 aimNorm = aim.normalized;

            // Pick Y speed dynamically based on direction (up vs down)
            float speedY = aimNorm.y >= 0 ? speedUp : speedDown;

            // Apply anisotropic scaling
            Vector2 scaled = new Vector2(aimNorm.x * speedX, aimNorm.y * speedY);

            // Keep angle the same, but use the intended magnitude
            return aim.normalized * scaled.magnitude;
        }
    }
}
