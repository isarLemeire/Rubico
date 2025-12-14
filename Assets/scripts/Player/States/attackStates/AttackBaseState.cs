using UnityEngine;

namespace PlayerController
{

    public abstract class AttackBaseState : PlayerStateBase
    {
        protected AttackBaseState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override abstract void Enter();
        public override abstract void Exit();
        public override abstract void Update();
        public override abstract void FixedUpdate();
        public override abstract void LateFixedUpdate();



        public void HandleAttachPhase(
            float timer,
            ref PlayerAttackPhase phase,
            float startUpTime,
            float attackActiveTime,
            float recoveryTime,
            float animationTime,
            float damage
            )
        {
            if (timer >= animationTime)
                controller.animator.SetLayerWeight(1, 0f);
            // end attack
            if (timer >= startUpTime + attackActiveTime + recoveryTime && phase == PlayerAttackPhase.Recovery)
            {

                controller.QueueAttackState(PlayerAttackStateType.NonAttacking);
                return;
            }
            // go to recovery phase
            if (timer >= startUpTime + attackActiveTime && phase == PlayerAttackPhase.Attack)
            {
                ctx.inputBlock = false;
                disableHitBox();
                phase = PlayerAttackPhase.Recovery;
                return;
            }
            // go to attack phase
            if (timer >= startUpTime && phase == PlayerAttackPhase.Startup)
            {

                enableHitBox(damage);
                phase = PlayerAttackPhase.Attack;
                return;
            }
        }

        public void createHitbox(float width, float height)
        {
            inputHandler.AttackPressed.Clear();

            Vector2 attackDir = inputHandler.JoyStickAim.normalized;

            float projectedPlayerExtent =
                Mathf.Abs(attackDir.x) * (ctx.Stats.PlayerWidth / 2f) +
                Mathf.Abs(attackDir.y) * (ctx.Stats.PlayerHeight / 2f);

            float extendedWidth = width + projectedPlayerExtent;

            //--- ROTATE attack hitbox ---
            float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            Transform atk = controller.attackHitBoxObject.transform;

            atk.localScale = new Vector3(extendedWidth, height, 1f);
            atk.rotation = Quaternion.Euler(0f, 0f, angle);

            //--- POSITION attack hitbox ---
            float attackOffset = (extendedWidth / 2f) - projectedPlayerExtent;
            atk.localPosition = attackDir * attackOffset;

            ctx.attackAim = attackDir;

            //  ENGAGE HITBOX: scale + rotate + offset

            float engageWidth = ctx.Stats.engageStopDistance;
            Transform eng = controller.engageHitBoxObject.transform;

            // Same height, same rotation as main attack hitbox
            eng.localScale = new Vector3(engageWidth, height, 1f);
            eng.rotation = atk.rotation;

            float engageOffset = projectedPlayerExtent + (engageWidth / 2f);
            eng.localPosition = attackDir * engageOffset;

            //controller.attackHitBoxObject.GetComponent<Renderer>().enabled = true;  // invisible
        }


        public void enableHitBox(float damage)
        {
            controller.attackHitbox.StartAttackDetection(damage);
        }

        public void disableHitBox()
        {
            //controller.attackHitBoxObject.GetComponent<Renderer>().enabled = false;  // invisible
            controller.attackHitbox.StopAttackDetection();
        }
    }

    public enum PlayerAttackPhase
    {
        Startup,
        Attack,
        Recovery,
    }
}