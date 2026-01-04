using System;
using System.Collections.Generic;

public class CrabStateFactory : IStateFactory<CrabStateType>
{
    private readonly Dictionary<CrabStateType, State> stateCache;

    public CrabStateFactory(CrabController controller)
    {
        stateCache = new()
        {
            {
                CrabStateType.Pace,
                new PaceState(
                    controller.enemyCtx,
                    controller,
                    controller.startingLocation,
                    controller.stats.PaceMinDuration,
                    controller.stats.PaceMaxDuration,
                    controller.stats.maxPaceDistance,
                    controller.stats.speed,
                    controller.stats.acceleration,
                    controller.stats.forwardGroundCheckDistance,
                    controller.stats.downwardGroundCheckDistance,
                    controller.stats.wallCheckDistance,
                    controller.stats.groundMask,
                    controller.stats.dangerMask,
                    "Pace",
                    CrabStateType.Idle
                )
            },
            {
                CrabStateType.Idle,
                new IdleState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.MinIdleDuration,
                    controller.stats.MaxIdleDuration,
                    controller.stats.tauntOdds,
                    controller.stats.gravity,
                    "Idle",
                    controller.ResetAnger,
                    CrabStateType.Taunt,
                    CrabStateType.Pace,
                    controller.stats.groundMask)
            },
            {
                CrabStateType.Taunt,
                new AnimateState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.gravity,
                    controller.stats.tauntTime,
                    controller.stats.groundMask,
                    "Taunt",
                    CrabStateType.Idle)
            },
            {
                CrabStateType.Startled,
                new AnimateState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.gravity,
                    controller.stats.StartledTime,
                    controller.stats.groundMask,
                    "Startled",
                    CrabStateType.Stalk)
            },
            {
                CrabStateType.Stalk,
                new StalkState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.speed,
                    controller.stats.acceleration,
                    controller.stats.gravity,
                    controller.stats.holdDetectionRange,
                    controller.stats.detectMinimumRange,
                    controller.stats.detectAngle,
                    controller.stats.forgetDuration,
                    controller.stats.forwardGroundCheckDistance,
                    controller.stats.downwardGroundCheckDistanceAttack,
                    controller.stats.wallCheckDistance,
                    controller.stats.groundMask,
                    controller.stats.playerMask,
                    controller.stats.dangerMask,
                    controller.trackedPlayer,
                    controller.HitBoxObject,
                    "Attack",
                    "CanSeePlayer",
                    controller.playerController,
                    CrabStateType.knockback,
                    CrabStateType.Idle)
            },
            {
                CrabStateType.Hurt,
                new HurtState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.initialVelocity,
                    controller.stats.gravity,
                    controller.stats.hurtDuration,
                    controller.stats.bounceReduction,
                    controller.stats.hitAngle,
                    controller.stats.maxBounces,
                    controller.stats.groundMask,
                    controller.HitBoxObject,
                    "Hurt",
                    CrabStateType.Recover
                )
            },
            {
                CrabStateType.Recover,
                new AnimateState(
                    controller.enemyCtx,
                    controller,
                    controller.Rigidbody,
                    controller.stats.gravity,
                    controller.stats.recoverTime,
                    controller.stats.groundMask,
                    "Recover",
                    CrabStateType.Stalk)
            }
        };
    }

    public State GetState(CrabStateType type) => stateCache[type];
}
