using UnityEngine;
using System.Collections.Generic;

namespace PlayerController
{
    public interface IPlayerMovementStateFactory
    {
        MovementBaseState GetState(PlayerMovementStateType type);
    }

    public class PlayerMovementStateFactory : IPlayerMovementStateFactory
    {
        private readonly PlayerController controller;
        private readonly PlayerStateContext ctx;
        private readonly PlayerInputHandler inputHandler;

        private readonly Dictionary<PlayerMovementStateType, MovementBaseState> stateCache;

        public PlayerMovementStateFactory(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        {
            this.controller = controller;
            this.ctx = ctx;
            this.inputHandler = inputHandler;

            stateCache = new Dictionary<PlayerMovementStateType, MovementBaseState>
        {
            { PlayerMovementStateType.Idle, new IdleState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Running, new RunningState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Jumping, new JumpingState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Wavedashing, new WavedashState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Falling, new FallingState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Engaging, new EngageState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Dashing, new DashState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.KnockBack, new KnockBackState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Pogoing, new PogoState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Hurt, new HurtState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Death, new DeathState(controller, ctx, inputHandler) },
            { PlayerMovementStateType.Respawning, new RespawnState(controller, ctx, inputHandler) },
        };
        }

        public MovementBaseState GetState(PlayerMovementStateType type) => stateCache[type];
    }
}