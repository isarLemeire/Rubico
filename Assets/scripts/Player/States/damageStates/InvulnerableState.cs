using UnityEngine;



namespace PlayerController
{
    public class InvulnerableState : DamageBaseState
    {
        public InvulnerableState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        private float _timer;
        private SpriteRenderer sr;
        private Color baseColor;
        private Color flashColor; // Dark + transparent

        private bool was_grounded;

        public override void Enter()
        {
            _timer = 0;
            was_grounded = false;

            sr = controller._sprite.GetComponent<SpriteRenderer>();
            baseColor = sr.color;
            flashColor = new Color(baseColor.r * 0.4f, baseColor.g * 0.4f, baseColor.b * 0.4f, 0.4f);



            CameraController.Instance.Shake(ctx.Stats.HitShakeIntensity, ctx.Stats.HitShakeDuration);
            GameFreezeManager.Instance.Freeze(ctx.Stats.HitFreeze);
        }
        public override void Exit()
        {
           
            sr.color = baseColor;
        }

        public override void Update()
        {
            float t = Mathf.PingPong(_timer * ctx.Stats.FlashFrequency, 1f);
            sr.color = Color.Lerp(baseColor, flashColor, t);
        }

        public override void FixedUpdate()
        {
            _timer += Time.deltaTime;
            if (ctx.grounded.IsTrue)
            {
                was_grounded = true;
            }
            if (_timer >= ctx.Stats.InvulnerableTime && was_grounded)
            {
                controller.QueueDamageState(PlayerDamageStateType.Neutral);
                return;
            }

            if (_timer >= ctx.Stats.InvulnerableAirTime && !was_grounded)
            {
                
                controller.QueueDamageState(PlayerDamageStateType.Neutral);
                return;
            }

            foreach (var hit in ctx.CollisionHandler.Hits)
            {
                if (hit.collider.CompareTag("Spike") && !ctx.Invulnerable)
                {
                    ctx.lastHazardHit = hit;
                    controller.QueueMovementState(PlayerMovementStateType.Hurt);

                    CameraController.Instance.Shake(ctx.Stats.InvulnerableIntensity, ctx.Stats.InvulnerableDuration);
                    GameFreezeManager.Instance.Freeze(ctx.Stats.InvulnerableFreeze);

                    return;
                }
            }
        }

        public override void LateFixedUpdate()
        {
        }
    }
}
