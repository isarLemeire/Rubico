using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    public class PlayerAttackStateFactory : IStateFactory<PlayerAttackStateType>
    {
        private readonly PlayerController controller;
        private readonly PlayerStateContext ctx;
        private readonly PlayerInputHandler inputHandler;

        private readonly Dictionary<PlayerAttackStateType, AttackBaseState> stateCache;

        public PlayerAttackStateFactory(PlayerController controller, PlayerStateContext ctx, PlayerInputHandler inputHandler)
        {
            this.controller = controller;
            this.ctx = ctx;
            this.inputHandler = inputHandler;

            stateCache = new Dictionary<PlayerAttackStateType, AttackBaseState>
            {
                { PlayerAttackStateType.NonAttacking, new NonAttackState(controller, ctx, inputHandler) },
                { PlayerAttackStateType.Attacking, new AttackState(controller, ctx, inputHandler) },
            };
        }

        public State GetState(PlayerAttackStateType type) => stateCache[type];
    }
}