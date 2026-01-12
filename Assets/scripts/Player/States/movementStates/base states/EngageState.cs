using UnityEngine;

namespace Player
{
    public class EngageState : MovementBaseState
    {
        public EngageState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
            : base(controller, ctx, inputHandler) { }

        private float timer;
        private bool engageStopped;
        Vector2 engageAim;

        public override void Enter()
        {
            Debug.Log("Engage");
            if (ctx.attackAim.y > -0.3f)
                ctx.canEngage = false;

            if (ctx.grounded.IsTrue)
            {
                engageAim = ScaleEngage(ctx.attackAim, ctx.Stats.engageSpeedX, ctx.Stats.engageSpeedUp, ctx.Stats.engageSpeedDU, 0, 0);
            }
            else
            {
                engageAim = ScaleEngage(ctx.attackAim, ctx.Stats.engageAirSpeedX, ctx.Stats.engageAirSpeedUp, ctx.Stats.engageAirSpeedDU, ctx.Stats.engageAirSpeedDown, ctx.Stats.engageAirSpeedDD);
            }
            
            if (Mathf.Abs(engageAim.x) > 0.1f)
                ctx.speed = new Vector2(
                    Mathf.Max(Mathf.Min(Mathf.Abs(ctx.speed.x), ctx.Stats.MaxMovementSpeed), Mathf.Abs(engageAim.x)) * Mathf.Sign(engageAim.x),
                    Mathf.Max(ctx.speed.y * Mathf.Sign(engageAim.y) , Mathf.Abs(engageAim.y)) * Mathf.Sign(engageAim.y)
                );
            else
                ctx.speed = new Vector2(
                    0,
                    Mathf.Max(ctx.speed.y * Mathf.Sign(engageAim.y), Mathf.Abs(engageAim.y)) * Mathf.Sign(engageAim.y)
                );

            engageStopped = false;
            timer = (!ctx.grounded.IsTrue && engageAim.y < -0.1f) ? ctx.Stats.AttackStartupTime + ctx.Stats.AttackActiveTime : ctx.Stats.AttackStartupTime;
        }
        public override void Exit()
        {
            if (!ctx.grounded.IsTrue)
                controller.animator.SetTrigger("FallEnd");
        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {
            checkStop(ctx.speed * Time.deltaTime);
            if (controller.HitTarget && ctx.canKnockBack && ctx.attackAim.y <= 0 && !ctx.grounded.IsActive)
            {
                controller.QueueMovementState(PlayerMovementStateType.Pogoing);
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                returnToDefaultState();
            }
            if ((engageAim.y < 0f) && ctx.grounded.IsTrue)
            {
                returnToDefaultState();
            }
            if (Mathf.Sign(engageAim.y) > 0)
                ctx.speed.y = Mathf.Max(engageAim.y, ctx.speed.y - ctx.Stats.JumpGravity * Time.deltaTime);
        }

        public void checkStop(Vector2 distance)
        {
            if (engageStopped) return;

            var col = controller.engageHitBoxObject.GetComponent<BoxCollider2D>();
            if (col == null) return;

            // Build contact filter
            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = ctx.Stats.AttackTargetMask;
            filter.useTriggers = true; 

            // --- 1) Check if we are already inside something ---
            Collider2D[] overlap = new Collider2D[10];
            int overlapCount = col.Overlap(filter, overlap);

            if (overlapCount > 0)
            {
                ctx.speed = Vector2.zero;
                engageStopped = true;
                return;
            }
        }

        public override void LateFixedUpdate()
        {
            ctx.grounded.Set(ctx.CollisionHandler.grounded);
        }

        private Vector2 ScaleEngage(Vector3 aim,
                                    float speedX,
                                    float speedUp,
                                    float speedDU,
                                    float speedDown,
                                    float speedDD)
        {
            // Determine direction
            bool up = aim.y > 0.5f;
            bool down = aim.y < -0.5f;
            bool diagonal = Mathf.Abs(aim.y) > 0.5f && Mathf.Abs(aim.x) > 0.5f; // anything not flat is diagonal

            float speed;

            if (up && diagonal)
                speed = speedDU;     // diagonal up
            else if (down && diagonal)
                speed = speedDD;     // diagonal down
            else if (up)
                speed = speedUp;     // straight up
            else if (down)
                speed = speedDown;   // straight down
            else
                speed = speedX;      // horizontal

            return new Vector2(aim.x, aim.y) * speed;
        }

    }
}

