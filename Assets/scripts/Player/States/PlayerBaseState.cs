using UnityEngine;

namespace PlayerController
{

    public abstract class PlayerStateBase
    {
        protected PlayerController controller;
        protected PlayerStateContext ctx;
        protected PlayerInputHandler inputHandler;

        protected PlayerStateBase(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        {
            this.controller = controller;
            this.ctx = ctx;
            this.inputHandler = inputHandler;
        }

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void LateFixedUpdate();

    }
}