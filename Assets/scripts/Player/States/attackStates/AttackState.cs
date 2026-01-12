using UnityEngine;

namespace Player
{

    public enum AttackType
    {
        Neutral,
        Up,
        Down,
        DiagonalUp,
        DiagonalDown
    }

    public class AttackState : AttackBaseState
    {
        public AttackState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        private float attackTimer;
        private PlayerAttackPhase phase;

        private bool feedback;
        private bool smallFeedback;
        private bool heal;
        private Coroutine _freezeRoutine;

        private AttackType currentAttackType;

        private AnimatorStateInfo _currentStateInfo;
        private bool canTransition;

        public override void Enter() 
        {
            ctx.canKnockBack = true;
            feedback = false;
            smallFeedback = false;
            heal = false;

            ctx.attackAim = inputHandler.JoyStickAim;

            if (ctx.grounded.IsTrue)
                ctx.attackCombo++;
            else
                ctx.attackCombo = 0;

            attackTimer = 0;
            phase = PlayerAttackPhase.Startup;
            createHitbox(ctx.Stats.AttackWidth, ctx.Stats.AttackHeight);
            controller.attackHitbox.StartBuffer(ctx.attackAim);

            if (!ctx.DeathInputBlock && (ctx.canEngage || ctx.grounded.IsActive || ctx.attackAim.y <= -0.3f))
            {
                controller.QueueMovementState(PlayerMovementStateType.Engaging);
            }

            startAnimation();
            ctx.attackInputBlock = true;
            canTransition = !ctx.grounded.IsActive;
        }

        private void startAnimation()
        {
            if (ctx.attackAim.y == 1)
            {
                currentAttackType = AttackType.Up;
                controller.animator.SetTrigger("AttackUp");
                return;
            }

            if (ctx.attackAim.y == -1)
            {
                currentAttackType = AttackType.Down;
                controller.animator.SetTrigger("AttackDown");
                return;
            }

            if (ctx.attackAim.y > 0.5 && Mathf.Abs(ctx.attackAim.x) > 0.5)
            {
                currentAttackType = AttackType.DiagonalUp;
                controller.animator.SetTrigger("AttackDU");
                return;
            }

            if (ctx.attackAim.y < -0.5 && Mathf.Abs(ctx.attackAim.x) > 0.5)
            {
                currentAttackType = AttackType.DiagonalDown;
                controller.animator.SetTrigger("AttackDD");
                return;
            }

            if (ctx.grounded.IsTrue)
                controller.animator.SetTrigger("Attack");
            else
                controller.animator.SetTrigger("AttackAir");
            currentAttackType = AttackType.Neutral;

        }

        public override void Exit() 
        {
            ctx.attackInputBlock = false;
            disableHitBox();
        }

        public override void Update()
        {
            if (controller.HitTarget && !feedback)
            {
                feedback = true;
                GameFreezeManager.Instance.Freeze(ctx.JuiceStats.AttackFreeze);
                CameraController.Instance.ShakeDirectional(ctx.attackAim, ctx.JuiceStats.AttackShakeIntensity, ctx.JuiceStats.AttackShakeDuration, ctx.JuiceStats.ShakeFrequency);
            }
            if(controller.HitTarget && !heal){
                foreach (GameObject go in controller.attackHitbox.HitTargets)
                {
                    if (go.CompareTag("Enemy"))
                    {
                        heal = true;
                        ctx.heal ++;
                        controller.UIanimator.SetInteger("Heal", ctx.heal);
                    }
                }
            }
            if (controller.HitNonTarget && !smallFeedback)
            {
                smallFeedback = true;
                GameFreezeManager.Instance.Freeze(ctx.JuiceStats.AttackFreezeNonTarget);
                CameraController.Instance.ShakeDirectional(ctx.attackAim, ctx.JuiceStats.AttackShakeIntensityNonTarget, ctx.JuiceStats.AttackShakeDurationNonTarget, ctx.JuiceStats.ShakeFrequency);
            }
        }

        public override void FixedUpdate()
        {

            attackTimer += Time.deltaTime;
            HandleAttachPhase(
                attackTimer,
                ref phase,
                ctx.Stats.AttackStartupTime,
                ctx.Stats.AttackActiveTime,
                ctx.Stats.AttackRecoveryTime,
                ctx.Stats.AttackAnimationTime,
                ctx.Stats.AttackDamage
            );

            if ((currentAttackType == AttackType.Down || currentAttackType == AttackType.DiagonalDown) && ctx.grounded.IsTrue)
            {
                controller.animator.SetLayerWeight(1, 0f);
                controller.QueueAttackState(PlayerAttackStateType.NonAttacking);
            }
            if (canTransition && ctx.grounded.IsActive)
            {
                transitionAttackAnimation();
                canTransition = false;
            }
        }

        private void transitionAttackAnimation()
        {
            AnimatorStateInfo state = controller.animator.GetCurrentAnimatorStateInfo(1);
            string nextAnimation = GetAttackAnimationName(ctx.attackAim, ctx.grounded.IsTrue);
            float currentNormalizedTime = state.normalizedTime % 1f;
            controller.animator.CrossFade(nextAnimation, 0, 1, currentNormalizedTime);
        }


        private string GetAttackAnimationName(Vector2 aim, bool grounded)
        {

            // Determine attack type
            if (aim.y == 1)
                currentAttackType = AttackType.Up;
            else if (aim.y == -1)
                currentAttackType = AttackType.Down;
            else if (aim.y > 0.5f && Mathf.Abs(aim.x) > 0.5f)
                currentAttackType = AttackType.DiagonalUp;
            else if (aim.y < -0.5f && Mathf.Abs(aim.x) > 0.5f)
                currentAttackType = AttackType.DiagonalDown;
            else
                currentAttackType = AttackType.Neutral;

            switch (currentAttackType)
            {
                case AttackType.Up: return grounded ? "attackUp" : "AttackUpAir";
                case AttackType.Down: return "attackDown" ;
                case AttackType.DiagonalUp: return grounded ? "attackDU" : "AttackDUAIr"; 
                case AttackType.DiagonalDown: return "AttackDD";
                default: return grounded ? "attack" : "attackAir";
            }
        }

        public override void LateFixedUpdate(){}


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
                ctx.attackInputBlock = false;
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
            controller.attackHitbox.StartAttackDetection(damage, ctx.attackAim);
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