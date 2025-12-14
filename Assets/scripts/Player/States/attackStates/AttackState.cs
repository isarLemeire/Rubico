using UnityEngine;

namespace PlayerController
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
        private Coroutine _freezeRoutine;

        private AttackType currentAttackType;

        private AnimatorStateInfo _currentStateInfo;
        private bool canTransition;

        public override void Enter() 
        {
            ctx.canKnockBack = true;
            feedback = false;
            smallFeedback = false;

            ctx.attackAim = inputHandler.JoyStickAim;

            if (ctx.grounded.IsTrue)
                ctx.attackCombo++;
            else
                ctx.attackCombo = 0;

            attackTimer = 0;
            phase = PlayerAttackPhase.Startup;
            createHitbox(ctx.Stats.AttackWidth, ctx.Stats.AttackHeight);



            if (ctx.canEngage || ctx.grounded.IsActive || ctx.attackAim.y <= -0.3f)
            {
                controller.QueueMovementState(PlayerMovementStateType.Engaging);
            }

            startAnimation();
            ctx.inputBlock = true;
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
            ctx.inputBlock = false;
            disableHitBox();
        }

        public override void Update()
        {
            if (controller.HitTarget && !feedback)
            {
                feedback = true;
                GameFreezeManager.Instance.Freeze(ctx.Stats.AttackFreeze);
                CameraController.Instance.ShakeDirectional(ctx.attackAim, ctx.Stats.AttackShakeIntensity, ctx.Stats.AttackShakeDuration);
            }
            if (controller.HitNonTarget && !smallFeedback)
            {
                smallFeedback = true;
                GameFreezeManager.Instance.Freeze(ctx.Stats.AttackFreezeNonTarget);
                CameraController.Instance.ShakeDirectional(ctx.attackAim, ctx.Stats.AttackShakeIntensityNonTarget, ctx.Stats.AttackShakeDurationNonTarget);
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
                controller.QueueMovementState(PlayerMovementStateType.Idle);
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

        public override void LateFixedUpdate()
        {

        }




    }


}