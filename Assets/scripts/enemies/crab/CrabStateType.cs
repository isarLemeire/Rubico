using System;
using System.Collections.Generic;

public enum CrabStateType
{
    Idle,
    Pace,
    Taunt,
    Startled,
    Stalk,
    Hurt,
    Recover,
    knockback
}

public static class CrabStatePriority
{
    public static readonly Dictionary<CrabStateType, int> priority = new()
    {
        { CrabStateType.Idle, 1 },
        { CrabStateType.Taunt, 5 },
        { CrabStateType.Pace, 10 },
        { CrabStateType.Stalk, 20 },
        { CrabStateType.Startled, 30 },
        { CrabStateType.knockback, 80 },
        { CrabStateType.Recover, 90 },
        { CrabStateType.Hurt, 100 }
    };
}