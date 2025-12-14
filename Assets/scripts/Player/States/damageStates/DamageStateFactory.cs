using UnityEngine;
using System.Collections.Generic;

namespace PlayerController
{
    public interface IPlayerDamageStateFactory
    {
        DamageBaseState GetState(PlayerDamageStateType type);
    }

    public class PlayerDamageStateFactory : IPlayerDamageStateFactory
    {
        private readonly PlayerController controller;
        private readonly PlayerStateContext ctx;
        private readonly PlayerInputHandler inputHandler;

        private readonly Dictionary<PlayerDamageStateType, DamageBaseState> stateCache;

        public PlayerDamageStateFactory(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        {
            this.controller = controller;
            this.ctx = ctx;
            this.inputHandler = inputHandler;

            stateCache = new Dictionary<PlayerDamageStateType, DamageBaseState>
            {
                { PlayerDamageStateType.Neutral, new NeutralState(controller, ctx, inputHandler) },
                { PlayerDamageStateType.Invulnerable, new InvulnerableState(controller, ctx, inputHandler) },
            };
        }

        public DamageBaseState GetState(PlayerDamageStateType type) => stateCache[type];
    }
}