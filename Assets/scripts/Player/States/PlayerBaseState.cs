using UnityEngine;

namespace Player
{
    public abstract class PlayerStateBase : State
    {
        protected PlayerController controller;
        new protected PlayerStateContext ctx;
        protected PlayerInputHandler inputHandler;

        protected PlayerStateBase(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler) : base(ctx)
        {
            this.controller = controller;
            this.ctx = ctx;
            this.inputHandler = inputHandler;
        }
    }
}