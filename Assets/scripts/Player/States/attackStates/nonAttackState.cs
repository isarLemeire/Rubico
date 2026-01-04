using UnityEngine;
namespace Player
{
    public class NonAttackState : AttackBaseState
    {
        public NonAttackState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
                : base(controller, ctx, inputHandler) { }

        public bool engage;

        public override void Enter()
        {
            controller.animator.SetLayerWeight(1, 0f);
        }
        public override void Exit()
        {
            controller.animator.SetLayerWeight(1, 1f);
        }

        public override void Update()
        {
           if (inputHandler.AttackPressed.IsActive && ctx.canAttack)
           {
                controller.QueueAttackState(PlayerAttackStateType.Attacking);
            }
        }

        public override void FixedUpdate()
        {
        }

        public override void LateFixedUpdate()
        {
        }
    }
}
