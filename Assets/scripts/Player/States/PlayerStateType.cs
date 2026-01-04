using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    public enum PlayerMovementStateType
    {
        Idle,
        Running,
        Jumping,
        Wavedashing,
        Falling,
        Engaging,
        Dashing,
        KnockBack,
        Pogoing,
        Hurt,
        Death,
        Respawning,
    }


    public enum PlayerAttackStateType
    {
        NonAttacking,
        Attacking,
        ComboAttacking,
    }

    public enum PlayerDamageStateType
    {
        Neutral,
        Invulnerable,
    }


    public static class PlayerStatePriority
    {
        public static readonly Dictionary<PlayerMovementStateType, int> MovementPriority = new()
        {
            { PlayerMovementStateType.Respawning, 101 },
            { PlayerMovementStateType.Death, 100 },
            { PlayerMovementStateType.Hurt, 90 },
            { PlayerMovementStateType.Dashing, 80 },
            { PlayerMovementStateType.Pogoing, 76 },
            { PlayerMovementStateType.KnockBack, 75 },
            { PlayerMovementStateType.Wavedashing, 71 },
            { PlayerMovementStateType.Jumping, 60 },
            { PlayerMovementStateType.Engaging, 55 },
            { PlayerMovementStateType.Falling, 50 },
            { PlayerMovementStateType.Running, 20 },
            { PlayerMovementStateType.Idle, 10 },
        };

        public static readonly Dictionary<PlayerAttackStateType, int> AttackPriority = new()
        {
            { PlayerAttackStateType.ComboAttacking, 51 },
            { PlayerAttackStateType.Attacking, 50 },
            { PlayerAttackStateType.NonAttacking, 55 },
        };

        public static readonly Dictionary<PlayerDamageStateType, int> DamagePriority = new()
        {
            { PlayerDamageStateType.Invulnerable, 50 },
            { PlayerDamageStateType.Neutral, 0 },

        };
    }


}
