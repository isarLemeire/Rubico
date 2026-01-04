using UnityEngine;

namespace Player
{

    public abstract class DamageBaseState : PlayerStateBase
    {
        protected DamageBaseState(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        : base(controller, ctx, inputHandler) { }

        public override abstract void Enter();
        public override abstract void Exit();
        public override abstract void Update();
        public override abstract void FixedUpdate();
        public override abstract void LateFixedUpdate();

    }

}
