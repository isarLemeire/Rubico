using UnityEngine;

namespace Player
{
    public class InvulnerableState : DamageBaseState
    {
        public InvulnerableState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        private float _timer;
        private SpriteRenderer sr;
        private LineRenderer lr;
        private Color baseBodyColor;
        private Color flashBodyColor;
        private Color baseHairColor;
        private Color flashHairColor;

        private bool was_grounded;

        public override void Enter()
        {
            _timer = 0;
            was_grounded = false;

            sr = controller.Sprite;
            lr = controller.HairRenderer;
            baseBodyColor = sr.color;
            baseHairColor = lr.startColor;
            float dark = 0.3f;
            flashBodyColor = new Color(
                baseBodyColor.r * dark,
                baseBodyColor.g * dark,
                baseBodyColor.b * dark,
                1f
            );

            flashHairColor = new Color(
                baseHairColor.r * dark,
                baseHairColor.g * dark,
                baseHairColor.b * dark,
                1f
            );

            ctx.invulnerable = true;
        }
        public override void Exit()
        {
            sr.color = baseBodyColor;
            lr.startColor = baseHairColor;
            lr.endColor = lr.startColor;

            ctx.invulnerable = false;
        }

        public override void Update()
        {
            float t = Mathf.PingPong(_timer * ctx.JuiceStats.FlashFrequency, 1f);
            sr.color = Color.Lerp(baseBodyColor, flashBodyColor, t);
            lr.startColor = Color.Lerp(baseHairColor, flashHairColor, t);
            lr.endColor   = lr.startColor;
        }

        public override void FixedUpdate()
        {
            _timer += Time.deltaTime;
            if (ctx.grounded.IsTrue)
            {
                was_grounded = true;
            }

            foreach (var hit in ctx.CollisionHandler.HazardHits)
            {                
                if (!ctx.movement_invulnerable){
                    ctx.lastHazardHit = hit;
                    controller.QueueMovementState(PlayerMovementStateType.Hurt);
                    controller.animator.SetTrigger("Hurt");
                    CameraController.Instance.Shake(ctx.JuiceStats.InvulnerableShakeIntensity, ctx.JuiceStats.InvulnerableShakeDuration, ctx.JuiceStats.ShakeFrequency);
                    GameFreezeManager.Instance.Freeze(ctx.JuiceStats.InvulnerableFreeze);
                    return;
                }
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
        }

        public override void LateFixedUpdate()
        {
        }
    }
}
